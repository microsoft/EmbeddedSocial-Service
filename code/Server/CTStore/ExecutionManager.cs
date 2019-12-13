//-----------------------------------------------------------------------
// <copyright file="ExecutionManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class ExecutionManager.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Execution manager class
    /// </summary>
    public class ExecutionManager
    {
        /// <summary>
        /// Persistent store for CT store
        /// </summary>
        private IPersistentStore persistentStore;

        /// <summary>
        /// Cache for CT store
        /// </summary>
        private ICache cache;

        /// <summary>
        /// Store configuration
        /// </summary>
        private Configuration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionManager"/> class
        /// </summary>
        /// <param name="persistentStore">Persistent store for CT store</param>
        /// <param name="cache">Cache for CT store</param>
        /// <param name="config">Store configuration</param>
        public ExecutionManager(IPersistentStore persistentStore, ICache cache, Configuration config)
        {
            this.persistentStore = persistentStore;
            this.cache = cache;
            this.config = config;
        }

        /// <summary>
        /// Execute operation async
        /// </summary>
        /// <param name="operation">Table operation</param>
        /// <param name="consistencyMode">Consistency mode</param>
        /// <returns>Operation result</returns>
        public async Task<Result> ExecuteOperationAsync(Operation operation, ConsistencyMode consistencyMode)
        {
            if (operation.Table.StorageMode == StorageMode.PersistentOnly)
            {
                return await this.persistentStore.ExecuteOperationAsync(operation);
            }

            if (operation.Table.StorageMode == StorageMode.CacheOnly)
            {
                this.SetupCacheEntityETag(operation);
                return await this.cache.ExecuteOperationAsync(operation);
            }

            return await this.ExecuteStrongConsistentOperationAsync(operation);
        }

        /// <summary>
        /// Execute transaction async
        /// </summary>
        /// <param name="transaction">Table transaction</param>
        /// <param name="consistencyMode">Consistency mode</param>
        /// <returns>List of operation results</returns>
        public async Task<IList<Result>> ExecuteTransactionAsync(Transaction transaction, ConsistencyMode consistencyMode)
        {
            if (transaction.PersistentOnlyOperations.Count == transaction.Operations.Count)
            {
                return await this.persistentStore.ExecuteTransactionAsync(transaction);
            }

            if (transaction.CacheOnlyOperations.Count == transaction.Operations.Count)
            {
                this.SetupCacheEntityETags(transaction);
                return await this.cache.ExecuteTransactionAsync(transaction);
            }

            return await this.ExecuteStrongConsistentTransactionAsync(transaction);
        }

        /// <summary>
        /// Execute strong consistent operation
        /// </summary>
        /// <param name="operation">Store operation</param>
        /// <returns>Store result</returns>
        private async Task<Result> ExecuteStrongConsistentOperationAsync(Operation operation)
        {
            // -------------
            // Pre operation
            // -------------
            Operation preOperation = this.GetPreOperation(operation);
            Result preResult = null;
            if (preOperation != null)
            {
                preResult = await this.cache.ExecuteOperationAsync(preOperation);
            }

            // ---------
            // Operation
            // ---------
            Result result = null;
            Exception exception = null;

            try
            {
                result = await this.persistentStore.ExecuteOperationAsync(operation);
            }
            catch (Exception e)
            {
                exception = e;
            }

            // --------
            // Rollback
            // --------
            if (exception != null)
            {
                if (preOperation != null)
                {
                    Operation rollbackOperation = this.GetRollbackOperation(preOperation, preResult);
                    try
                    {
                        await this.cache.ExecuteOperationAsync(rollbackOperation);
                    }
                    catch
                    {
                        // best effort
                    }
                }

                throw exception;
            }

            // --------------
            // Post operation
            // --------------
            Operation postOperation = this.GetPostOperation(operation, preResult, result);
            if (postOperation != null)
            {
                try
                {
                    await this.cache.ExecuteOperationAsync(postOperation);
                }
                catch (Exception e)
                {
                    // best effort
                    Console.WriteLine(e.Message);
                }
            }

            return result;
        }

        /// <summary>
        /// Execute strong consistent transaction
        /// </summary>
        /// <param name="transaction">Store transaction</param>
        /// <returns>List of operation results</returns>
        private async Task<IList<Result>> ExecuteStrongConsistentTransactionAsync(Transaction transaction)
        {
            // ---------------
            // Pre transaction
            // ---------------
            List<Operation> preOperations = this.GetPreOperations(transaction.DefaultOperations);
            Transaction preTransaction = new Transaction();
            List<int> nullPreOperationIndices = new List<int>();
            for (int i = 0; i < preOperations.Count; i++)
            {
                Operation preOperation = preOperations[i];
                if (preOperation != null)
                {
                    preTransaction.Add(preOperation);
                }
                else
                {
                    nullPreOperationIndices.Add(i);
                }
            }

            IList<Result> preResults = new List<Result>();
            if (preTransaction.Operations.Count > 0)
            {
                preResults = await this.cache.ExecuteTransactionAsync(preTransaction);
            }

            nullPreOperationIndices.ForEach(index => preResults.Insert(index, null));

            // -----------
            // Transaction
            // -----------
            IList<Result> results = null;
            Exception exception = null;
            try
            {
                results = await this.persistentStore.ExecuteTransactionAsync(transaction);
            }
            catch (Exception e)
            {
                exception = e;
            }

            // --------
            // Rollback
            // --------
            if (exception != null)
            {
                List<Operation> rollbackOperations = this.GetRollbackOperations(preOperations, preResults);
                if (rollbackOperations.Count > 0)
                {
                    Transaction rollbackTransaction = new Transaction(rollbackOperations);
                    try
                    {
                        await this.cache.ExecuteTransactionAsync(rollbackTransaction);
                    }
                    catch (Exception e)
                    {
                        // best effort
                        Console.WriteLine(e.Message);
                    }
                }

                throw exception;
            }

            // ----------------
            // Post transaction
            // ----------------
            IList<Result> defaultOperationResults = new List<Result>();
            transaction.DefaultOperationIndices.ForEach(i => defaultOperationResults.Add(results[transaction.DefaultOperationIndices[i]]));
            List<Operation> postOperations = this.GetPostOperations(transaction.DefaultOperations, preResults, defaultOperationResults);
            if (postOperations.Count > 0)
            {
                Transaction postTransaction = new Transaction(postOperations);
                try
                {
                    await this.cache.ExecuteTransactionAsync(postTransaction);
                }
                catch
                {
                    // best effort
                }
            }

            return results;
        }

        /// <summary>
        /// Get list of pre operations for a store operations in default mode.
        /// </summary>
        /// <param name="operations">List of operations</param>
        /// <returns>List of pre operations</returns>
        private List<Operation> GetPreOperations(List<Operation> operations)
        {
            return operations.Select(op => { return this.GetPreOperation(op); }).ToList();
        }

        /// <summary>
        /// Get rollback operations
        /// </summary>
        /// <param name="preOperations">Pre operations</param>
        /// <param name="preResults">Results for pre operations</param>
        /// <returns>List of rollback operations</returns>
        private List<Operation> GetRollbackOperations(List<Operation> preOperations, IList<Result> preResults)
        {
            List<Operation> rollbackOperations = new List<Operation>();
            for (int i = 0; i < preOperations.Count; i++)
            {
                if (preOperations[i] == null)
                {
                    continue;
                }

                Operation rollbackOperation = this.GetRollbackOperation(preOperations[i], preResults[i]);
                if (rollbackOperation != null)
                {
                    rollbackOperations.Add(rollbackOperation);
                }
            }

            return rollbackOperations;
        }

        /// <summary>
        /// Get list of pre operations for a store operations in default mode.
        /// </summary>
        /// <param name="operations">List of operations</param>
        /// <param name="preResults">Results from pre operations</param>
        /// <param name="results">Results from operations</param>
        /// <returns>List of post operations</returns>
        private List<Operation> GetPostOperations(List<Operation> operations, IList<Result> preResults, IList<Result> results)
        {
            List<Operation> postOperations = new List<Operation>();
            for (int i = 0; i < operations.Count; i++)
            {
                Operation postOperation = this.GetPostOperation(operations[i], preResults[i], results[i]);
                if (postOperation != null)
                {
                    postOperations.Add(postOperation);
                }
            }

            return postOperations;
        }

        /// <summary>
        /// Get pre operation for a store operation in default mode.
        /// The pre operation is typically a cache invalidation operation.
        /// </summary>
        /// <param name="operation">Store operation</param>
        /// <returns>Pre operation for default mode</returns>
        private Operation GetPreOperation(Operation operation)
        {
            Table table = operation.Table;
            OperationType operationType = operation.OperationType;
            Operation preOperation = null;
            if (table is ObjectTable)
            {
                if (operationType != OperationType.Insert)
                {
                    ObjectEntity entity = this.GenerateInvalidCacheEntity<ObjectEntity>(this.config.CacheExpiryInSeconds);
                    preOperation = Operation.InsertOrReplace(table as ObjectTable, operation.PartitionKey, operation.Key, entity);
                }
            }
            else if (table is FixedObjectTable)
            {
                if (operationType != OperationType.Insert)
                {
                    ObjectEntity entity = this.GenerateInvalidCacheEntity<ObjectEntity>(this.config.CacheExpiryInSeconds);
                    preOperation = Operation.InsertOrReplace(table as FixedObjectTable, operation.PartitionKey, operation.Key, entity);
                }
            }
            else if (table is FeedTable)
            {
                FeedEntity entity = this.GenerateInvalidCacheEntity<FeedEntity>(this.config.CacheExpiryInSeconds);
                preOperation = Operation.InsertOrReplaceIfNotLast(table as FeedTable, operation.PartitionKey, operation.Key, operation.ItemKey, entity);
            }
            else if (table is CountTable)
            {
                if (operationType != OperationType.Insert)
                {
                    preOperation = Operation.InsertOrReplace(table as CountTable, operation.PartitionKey, operation.Key, 0);
                    preOperation.Entity = this.GenerateInvalidCacheEntity<CountEntity>(this.config.CacheExpiryInSeconds);
                }
            }

            if (preOperation != null)
            {
                this.SetupCacheEntityETag(preOperation);
            }

            return preOperation;
        }

        /// <summary>
        /// Get rollback operation for store operation.
        /// The rollback operation typically deletes the invalid cache entity
        /// </summary>
        /// <param name="preOperation">Pre operation</param>
        /// <param name="preResult">Result of pre operation</param>
        /// <returns>Rollback operation</returns>
        private Operation GetRollbackOperation(Operation preOperation, Result preResult)
        {
            Table table = preOperation.Table;
            Operation rollbackOperation = null;
            if (table is ObjectTable)
            {
                ObjectEntity deleteEntity = new ObjectEntity();
                deleteEntity.ETag = preResult.ETag;
                rollbackOperation = Operation.Delete(preOperation.Table as ObjectTable, preOperation.PartitionKey, preOperation.Key, deleteEntity);
            }
            else if (table is FixedObjectTable)
            {
                ObjectEntity deleteEntity = new ObjectEntity();
                deleteEntity.ETag = preResult.ETag;
                rollbackOperation = Operation.Delete(preOperation.Table as FixedObjectTable, preOperation.PartitionKey, preOperation.Key, deleteEntity);
            }
            else if (table is FeedTable)
            {
                if (preResult.EntitiesAffected == 0)
                {
                    return null;
                }

                // To rollback, we cannot delete the invalidated item in a feed.
                // This will violate the cache feed invariant.
                // The best we can do is to expire the item right away so that the next read can correct the cache.
                // Not the negative sign in the parameter.
                FeedEntity invalidEntity = this.GenerateInvalidCacheEntity<FeedEntity>(-this.config.CacheExpiryInSeconds);
                invalidEntity.ETag = preResult.ETag;
                rollbackOperation = Operation.Replace(preOperation.Table as FeedTable, preOperation.PartitionKey, preOperation.Key, preOperation.ItemKey, invalidEntity);
            }
            else if (table is CountTable)
            {
                CountEntity deleteEntity = new CountEntity();
                deleteEntity.ETag = preResult.ETag;
                rollbackOperation = Operation.Delete(preOperation.Table as CountTable, preOperation.PartitionKey, preOperation.Key, deleteEntity);
            }

            if (rollbackOperation != null)
            {
                this.SetupCacheEntityETag(rollbackOperation);
            }

            return rollbackOperation;
        }

        /// <summary>
        /// Get post operation for a store operation in default mode.
        /// The post operation typically makes the cache consistent with the store.
        /// There are exceptions: For merge operations, we simple delete the cache entry
        /// A read operation on the whole item should bring it back to the cache.
        /// </summary>
        /// <param name="operation">Store operation</param>
        /// <param name="preResult">Result of pre operation</param>
        /// <param name="result">Result of operations</param>
        /// <returns>Post operation</returns>
        private Operation GetPostOperation(Operation operation, Result preResult, Result result)
        {
            Table table = operation.Table;
            OperationType operationType = operation.OperationType;
            Entity entity = operation.Entity;
            Operation postOperation = null;
            if (table is ObjectTable)
            {
                switch (operationType)
                {
                    case OperationType.Insert:
                        postOperation = Operation.Insert(table as ObjectTable, operation.PartitionKey, operation.Key, entity as ObjectEntity);
                        postOperation.Entity.CustomETag = result.ETag;
                        return postOperation;
                    case OperationType.Replace:
                    case OperationType.InsertOrReplace:
                        postOperation = Operation.Replace(table as ObjectTable, operation.PartitionKey, operation.Key, entity as ObjectEntity);
                        postOperation.Entity.ETag = preResult.ETag;
                        postOperation.Entity.CustomETag = result.ETag;
                        return postOperation;
                    case OperationType.Delete:
                    case OperationType.DeleteIfExists:
                    case OperationType.Merge:
                    case OperationType.InsertOrMerge:
                        ObjectEntity deleteEntity = new ObjectEntity();
                        deleteEntity.ETag = preResult.ETag;
                        return Operation.Delete(table as ObjectTable, operation.PartitionKey, operation.Key, deleteEntity);
                }
            }
            else if (table is FixedObjectTable)
            {
                switch (operationType)
                {
                    case OperationType.Insert:
                        postOperation = Operation.Insert(table as FixedObjectTable, operation.PartitionKey, operation.Key, entity as ObjectEntity);
                        postOperation.Entity.CustomETag = result.ETag;
                        return postOperation;
                    case OperationType.Replace:
                    case OperationType.InsertOrReplace:
                        postOperation = Operation.Replace(table as FixedObjectTable, operation.PartitionKey, operation.Key, entity as ObjectEntity);
                        postOperation.Entity.ETag = preResult.ETag;
                        postOperation.Entity.CustomETag = result.ETag;
                        return postOperation;
                    case OperationType.Delete:
                    case OperationType.DeleteIfExists:
                        ObjectEntity deleteEntity = new ObjectEntity();
                        deleteEntity.ETag = preResult.ETag;
                        return Operation.Delete(table as FixedObjectTable, operation.PartitionKey, operation.Key, deleteEntity);
                }
            }
            else if (table is FeedTable)
            {
                if (preResult.EntitiesAffected == 0)
                {
                    return null;
                }

                switch (operationType)
                {
                    case OperationType.Insert:
                    case OperationType.Replace:
                    case OperationType.InsertOrReplace:
                        postOperation = Operation.Replace(table as FeedTable, operation.PartitionKey, operation.Key, operation.ItemKey, entity as FeedEntity);
                        postOperation.Entity.ETag = preResult.ETag;
                        postOperation.Entity.CustomETag = result.ETag;
                        return postOperation;
                    case OperationType.Delete:
                    case OperationType.DeleteIfExists:
                        FeedEntity deleteEntity = new FeedEntity();
                        deleteEntity.ETag = preResult.ETag;
                        return Operation.Delete(table as FeedTable, operation.PartitionKey, operation.Key, operation.ItemKey, deleteEntity);
                }
            }
            else if (table is CountTable)
            {
                switch (operationType)
                {
                    case OperationType.Insert:
                        postOperation = Operation.Insert(table as CountTable, operation.PartitionKey, operation.Key, (entity as CountEntity).Count);
                        postOperation.Entity.CustomETag = result.ETag;
                        return postOperation;
                    case OperationType.Replace:
                    case OperationType.InsertOrReplace:
                        postOperation = Operation.Replace(table as CountTable, operation.PartitionKey, operation.Key, entity as CountEntity);
                        postOperation.Entity.ETag = preResult.ETag;
                        postOperation.Entity.CustomETag = result.ETag;
                        return postOperation;
                    case OperationType.Increment:
                    case OperationType.InsertOrIncrement:
                        postOperation = Operation.Replace(table as CountTable, operation.PartitionKey, operation.Key, new CountEntity() { Count = (double)result.Value });
                        postOperation.Entity.ETag = preResult.ETag;
                        postOperation.Entity.CustomETag = result.ETag;
                        break;
                    case OperationType.Delete:
                    case OperationType.DeleteIfExists:
                        CountEntity deleteEntity = new CountEntity();
                        deleteEntity.ETag = preResult.ETag;
                        return Operation.Delete(table as CountTable, operation.PartitionKey, operation.Key, deleteEntity);
                }
            }

            return postOperation;
        }

        /// <summary>
        /// Prepare cache only transaction
        /// </summary>
        /// <param name="transaction">Store transaction</param>
        private void SetupCacheEntityETags(Transaction transaction)
        {
            foreach (Operation operation in transaction.Operations)
            {
                this.SetupCacheEntityETag(operation);
            }
        }

        /// <summary>
        /// Prepare cache only operation. Modify entity to include new ETag when appropriate.
        /// </summary>
        /// <param name="operation">Store operation</param>
        private void SetupCacheEntityETag(Operation operation)
        {
            if (operation.Table is ObjectTable
                || operation.Table is CountTable)
            {
                if (operation.Entity != null)
                {
                    operation.Entity.NoETag = false;
                    operation.Entity.CustomETag = this.GenerateETag();
                }
            }
            else
            {
                if (operation.Entity != null)
                {
                    operation.Entity.NoETag = true;
                }
            }
        }

        /// <summary>
        /// Generate invalid cache entity
        /// </summary>
        /// <typeparam name="T">Store entity</typeparam>
        /// <param name="expiryInSeconds">Expiration time in seconds</param>
        /// <returns>Invalid cache entity</returns>
        private T GenerateInvalidCacheEntity<T>(int expiryInSeconds)
            where T : Entity, new()
        {
            T entity = new T();
            entity.CacheFlags |= CacheFlags.Invalid;
            entity.CacheExpiry = this.config.GetTimeMethod().AddSeconds(expiryInSeconds);
            return entity;
        }

        /// <summary>
        /// Generate a ETag
        /// </summary>
        /// <returns>New ETag</returns>
        private string GenerateETag()
        {
            string etag = this.config.GetTimeMethod().ToString("yyyy-MM-dd HH:mm:ss.ffffff");
            return etag;
        }
    }
}
