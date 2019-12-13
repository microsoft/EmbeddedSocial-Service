//-----------------------------------------------------------------------
// <copyright file="AzureTableStorage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class AzureTableStorage.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Azure table storage as persistent store
    /// </summary>
    public class AzureTableStorage : IPersistentStore
    {
        /// <summary>
        /// Separator character in row key
        /// </summary>
        private const char RowKeySeparator = ':';

        /// <summary>
        /// Connection string to Azure table storage
        /// </summary>
        private string connectionString;

        /// <summary>
        /// Cloud storage account
        /// </summary>
        private CloudStorageAccount cloudStorageAccount;

        /// <summary>
        /// Default table request options
        /// </summary>
        private TableRequestOptions tableRequestOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorage"/> class
        /// </summary>
        /// <param name="connectionString">Azure table storage connection string</param>
        public AzureTableStorage(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Gets or sets connection string to Azure table storage
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return this.connectionString;
            }

            set
            {
                this.connectionString = value;
            }
        }

        /// <summary>
        /// Gets cloud storage account
        /// </summary>
        public CloudStorageAccount CloudStorageAccount
        {
            get
            {
                if (this.cloudStorageAccount == null)
                {
                    this.cloudStorageAccount = CloudStorageAccount.Parse(this.ConnectionString);
                }

                return this.cloudStorageAccount;
            }
        }

        /// <summary>
        /// Gets or sets table request options
        /// </summary>
        public TableRequestOptions TableRequestOptions
        {
            get
            {
                return this.tableRequestOptions;
            }

            set
            {
                this.tableRequestOptions = value;
            }
        }

        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True of success</returns>
        public async Task<bool> CreateContainer(string containerName)
        {
            CloudTableClient tableClient = this.CloudStorageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(containerName);
            return await table.CreateIfNotExistsAsync();
        }

        /// <summary>
        /// Delete container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True of success</returns>
        public async Task<bool> DeleteContainer(string containerName)
        {
            CloudTableClient tableClient = this.CloudStorageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(containerName);
            return await table.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Execute operation async
        /// </summary>
        /// <param name="operation">Table operation</param>
        /// <returns>Operation result</returns>
        public async Task<Result> ExecuteOperationAsync(Operation operation)
        {
            CloudTable cloudTable = this.GetCloudTable(operation.Table);
            IList<Result> results = await this.Execute(cloudTable, new List<Operation>() { operation }, false);
            return results.First();
        }

        /// <summary>
        /// Execute transaction async
        /// </summary>
        /// <param name="transaction">Store transaction</param>
        /// <returns>List of operation results</returns>
        public async Task<IList<Result>> ExecuteTransactionAsync(Transaction transaction)
        {
            CloudTable cloudTable = this.GetCloudTable(transaction.ContainerName);
            return await this.Execute(cloudTable, transaction.Operations, true);
        }

        /// <summary>
        /// Query object async
        /// </summary>
        /// <typeparam name="T">Object entity</typeparam>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <returns>Object entity result. Returns null if entity does not exist</returns>
        public async Task<T> QueryObjectAsync<T>(ObjectTable table, string partitionKey, string objectKey)
            where T : ObjectEntity, new()
        {
            return await this.QueryObjectAsync<T>(table, partitionKey, objectKey, null);
        }

        /// <summary>
        /// Query partial object async
        /// </summary>
        /// <typeparam name="T">Object entity</typeparam>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <returns>Object entity result. Returns null if entity does not exist.</returns>
        public async Task<T> QueryPartialObjectAsync<T>(ObjectTable table, string partitionKey, string objectKey)
            where T : ObjectEntity, new()
        {
            List<string> fields = typeof(T).GetProperties().Select(f => f.Name).ToList();
            return await this.QueryPartialObjectAsync<T>(table, partitionKey, objectKey, fields);
        }

        /// <summary>
        /// Query partial object async
        /// </summary>
        /// <typeparam name="T">Object entity</typeparam>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <param name="fields">Fields to query</param>
        /// <returns>Object entity result. Returns null if entity does not exist.</returns>
        public async Task<T> QueryPartialObjectAsync<T>(ObjectTable table, string partitionKey, string objectKey, List<string> fields)
            where T : ObjectEntity, new()
        {
            List<string> queryFields = this.NormalizePartialObjectFields(fields);
            return await this.QueryObjectAsync<T>(table, partitionKey, objectKey, queryFields);
        }

        /// <summary>
        /// Query fixed object async
        /// </summary>
        /// <typeparam name="T">Object entity</typeparam>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <returns>Object entity result. Returns null if entity does not exist.</returns>
        public async Task<T> QueryObjectAsync<T>(FixedObjectTable table, string partitionKey, string objectKey)
            where T : ObjectEntity, new()
        {
            return await this.QueryObjectAsync<T>(table, partitionKey, objectKey, null);
        }

        /// <summary>
        /// Query count async
        /// </summary>
        /// <param name="table">Count table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="countKey">Key for entity</param>
        /// <returns>Count entity result. Returns null if entity does not exist</returns>
        public async Task<CountEntity> QueryCountAsync(CountTable table, string partitionKey, string countKey)
        {
            string rowKey = this.GetRowKey(table, countKey);
            CountEntity entity = await this.QueryTableEntity<CountEntity>(table, partitionKey, rowKey);
            if (entity == null)
            {
                return null;
            }

            entity.CountKey = countKey;
            return entity;
        }

        /// <summary>
        /// Query feed item async
        /// </summary>
        /// <typeparam name="T">Feed entity</typeparam>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key for feed entity</param>
        /// <returns>Feed entity result. Returns null if entity does not exist</returns>
        public async Task<T> QueryFeedItemAsync<T>(FeedTable table, string partitionKey, string feedKey, string itemKey)
            where T : FeedEntity, new()
        {
            string rowKey = this.GetRowKey(table, feedKey, itemKey);
            T entity = await this.QueryTableEntity<T>(table, partitionKey, rowKey);
            if (entity == null)
            {
                return null;
            }

            entity.FeedKey = feedKey;
            entity.ItemKey = itemKey;
            entity.Cursor = entity.ItemKey;
            return entity;
        }

        /// <summary>
        /// Query feed async
        /// </summary>
        /// <typeparam name="T">Feed entity</typeparam>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="cursor">Feed cursor</param>
        /// <param name="limit">Feed count limit</param>
        /// <returns>List of feed entities</returns>
        public async Task<IList<T>> QueryFeedAsync<T>(
            FeedTable table,
            string partitionKey,
            string feedKey,
            string cursor,
            int limit)
            where T : FeedEntity, new()
        {
            string startRowKey = this.GetStartRowKeyForFeed(table, feedKey, cursor);
            string endRowKey = this.GetEndRowKeyForFeed(table, feedKey);

            string filterA = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            string filterB = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, startRowKey);
            string filterC = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, endRowKey);
            string filterAB = TableQuery.CombineFilters(filterA, TableOperators.And, filterB);
            string filterABC = TableQuery.CombineFilters(filterAB, TableOperators.And, filterC);

            TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where(filterABC);
            if (limit != int.MaxValue)
            {
                query = query.Take(limit);
            }

            CloudTable cloudTable = this.GetCloudTable(table);

            var feedEntities = new List<T>();
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<DynamicTableEntity> segment = await cloudTable.ExecuteQuerySegmentedAsync<DynamicTableEntity>(query, token);
                token = segment.ContinuationToken;

                foreach (var item in segment)
                {
                    T entity = this.GetEntityFromDynamicTableEntity<T>(item);
                    entity.PartitionKey = partitionKey;
                    entity.FeedKey = feedKey;
                    entity.ItemKey = this.GetItemKey(table, feedKey, item.RowKey);
                    entity.Cursor = entity.ItemKey;
                    feedEntities.Add(entity);
                }

                if (query.TakeCount != null)
                {
                    if (feedEntities.Count == query.TakeCount.Value)
                    {
                        break;
                    }
                }
            }
            while (token != null);

            return feedEntities;
        }

        /// <summary>
        /// Query object async
        /// </summary>
        /// <typeparam name="T">Object entity</typeparam>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <param name="fields">Fields to query. If null, then query all fields.</param>
        /// <returns>Object entity result. Returns null if entity does not exist.</returns>
        private async Task<T> QueryObjectAsync<T>(Table table, string partitionKey, string objectKey, List<string> fields)
            where T : ObjectEntity, new()
        {
            string rowKey = this.GetRowKey(table, objectKey);
            T entity = await this.QueryTableEntity<T>(table, partitionKey, rowKey, fields);
            if (entity == null)
            {
                return null;
            }

            entity.ObjectKey = objectKey;
            return entity;
        }

        /// <summary>
        /// Query table entity
        /// </summary>
        /// <typeparam name="T">Store entity</typeparam>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="rowKey">Row key</param>
        /// <param name="fields">Optional fields for query projection</param>
        /// <returns>Dynamic table entity</returns>
        private async Task<T> QueryTableEntity<T>(Table table, string partitionKey, string rowKey, List<string> fields = null)
            where T : Entity, new()
        {
            CloudTable cloudTable = this.GetCloudTable(table);
            TableOperation tableOperation = null;
            if (fields == null)
            {
                tableOperation = TableOperation.Retrieve(partitionKey, rowKey);
            }
            else
            {
                tableOperation = TableOperation.Retrieve(partitionKey, rowKey, fields);
            }

            TableResult tableResult = await cloudTable.ExecuteAsync(tableOperation);
            if (tableResult.Result == null)
            {
                return default(T);
            }

            DynamicTableEntity dynamicTableEntity = (DynamicTableEntity)tableResult.Result;
            T entity = this.GetEntityFromDynamicTableEntity<T>(dynamicTableEntity);

            return entity;
        }

        /// <summary>
        /// Execute table operations. Execute them as batch there are multiple operations.
        /// </summary>
        /// <param name="cloudTable">Cloud table</param>
        /// <param name="operations">List of store operations</param>
        /// <param name="isTransaction">A value indicating whether the operation should be executed as a transaction</param>
        /// <returns>Store results</returns>
        private async Task<IList<Result>> Execute(CloudTable cloudTable, List<Operation> operations, bool isTransaction)
        {
            List<int> resultIndices = new List<int>();
            List<TableOperation> tableOperations = new List<TableOperation>();
            for (int i = 0; i < operations.Count; i++)
            {
                Operation operation = operations[i];
                TableOperation tableOperation = await this.GetTableOperation(operation);
                if (tableOperation != null)
                {
                    tableOperations.Add(tableOperation);
                    resultIndices.Add(tableOperations.Count - 1);
                }
                else
                {
                    resultIndices.Add(-1);
                }
            }

            IList<TableResult> tableResults = null;
            if (tableOperations.Count > 0)
            {
                if (!isTransaction)
                {
                    try
                    {
                        tableResults = new List<TableResult>();
                        TableResult tableResult = await cloudTable.ExecuteAsync(tableOperations.First(), this.tableRequestOptions, null);
                        tableResults.Add(tableResult);
                    }
                    catch (StorageException e)
                    {
                        this.ThrowException(e);
                    }
                }
                else
                {
                    TableBatchOperation batchOperation = this.GetBatchOperation(tableOperations);
                    try
                    {
                        tableResults = await cloudTable.ExecuteBatchAsync(batchOperation, this.tableRequestOptions, null);
                    }
                    catch (StorageException e)
                    {
                        this.ThrowException(e, isTransaction);
                    }
                }
            }

            return this.GetResults(operations, tableResults, resultIndices, tableOperations);
        }

        /// <summary>
        /// Get table batch operation from list of table operations
        /// </summary>
        /// <param name="tableOperations">List of table operations</param>
        /// <returns>Table batch operation</returns>
        private TableBatchOperation GetBatchOperation(List<TableOperation> tableOperations)
        {
            TableBatchOperation batchOperation = new TableBatchOperation();
            foreach (TableOperation tableOperation in tableOperations)
            {
                batchOperation.Add(tableOperation);
            }

            return batchOperation;
        }

        /// <summary>
        /// Get store results from table results
        /// </summary>
        /// <param name="operations">Store operations</param>
        /// <param name="tableResults">Table results</param>
        /// <param name="resultIndices">Result indices</param>
        /// <param name="tableOperations">Table operations</param>
        /// <returns>Store results</returns>
        private IList<Result> GetResults(List<Operation> operations, IList<TableResult> tableResults, List<int> resultIndices, List<TableOperation> tableOperations)
        {
            List<Result> results = new List<Result>();
            for (int i = 0; i < operations.Count; i++)
            {
                Result result = new Result();
                if (resultIndices[i] >= 0)
                {
                    TableResult tableResult = tableResults[resultIndices[i]];
                    result.ETag = tableResult.Etag;
                    result.EntitiesAffected = 1;
                }
                else
                {
                    result.ETag = null;
                    result.EntitiesAffected = 0;
                }

                if (operations[i].OperationType == OperationType.Increment
                    || operations[i].OperationType == OperationType.InsertOrIncrement)
                {
                    TableResult tableResult = tableResults[resultIndices[i]];
                    DynamicTableEntity tableEntity = (DynamicTableEntity)tableResult.Result;
                    result.Value = tableEntity.Properties[SpecialFieldNames.Count].DoubleValue;
                }

                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Get entity from dynamic table entity properties
        /// </summary>
        /// <typeparam name="T">Store entity</typeparam>
        /// <param name="dynamicTableEntity">Dynamic table entity</param>
        /// <returns>Store entity created from dynamic table entity</returns>
        private T GetEntityFromDynamicTableEntity<T>(DynamicTableEntity dynamicTableEntity)
            where T : Entity, new()
        {
            T entity = new T();
            entity.PartitionKey = dynamicTableEntity.PartitionKey;
            entity.ETag = dynamicTableEntity.ETag;
            foreach (var property in entity.GetType().GetProperties())
            {
                if (dynamicTableEntity.Properties.ContainsKey(property.Name))
                {
                    if (property.PropertyType.BaseType == typeof(Enum))
                    {
                        property.SetValue(entity, Enum.Parse(property.PropertyType, dynamicTableEntity.Properties[property.Name].StringValue));
                    }
                    else
                    {
                        property.SetValue(entity, dynamicTableEntity.Properties[property.Name].PropertyAsObject);
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// Get table storage operation from store operation
        /// </summary>
        /// <param name="operation">Store operation</param>
        /// <returns>Table storage operation</returns>
        private async Task<TableOperation> GetTableOperation(Operation operation)
        {
            if (operation.OperationType == OperationType.Insert)
            {
                var tableEntity = this.GetTableEntity(operation);
                return TableOperation.Insert(tableEntity);
            }
            else if (operation.OperationType == OperationType.Delete)
            {
                var tableEntity = this.GetTableEntity(operation);
                return TableOperation.Delete(tableEntity);
            }
            else if (operation.OperationType == OperationType.DeleteIfExists)
            {
                return await this.GetDeleteIfExistsOperation(operation);
            }
            else if (operation.OperationType == OperationType.Replace)
            {
                var tableEntity = this.GetTableEntity(operation);
                return TableOperation.Replace(tableEntity);
            }
            else if (operation.OperationType == OperationType.InsertOrReplace)
            {
                var tableEntity = this.GetTableEntity(operation);
                return TableOperation.InsertOrReplace(tableEntity);
            }
            else if (operation.OperationType == OperationType.Merge)
            {
                var tableEntity = this.GetTableEntity(operation);
                return TableOperation.Merge(tableEntity);
            }
            else if (operation.OperationType == OperationType.InsertOrMerge)
            {
                var tableEntity = this.GetTableEntity(operation);
                return TableOperation.InsertOrMerge(tableEntity);
            }
            else if (operation.OperationType == OperationType.Increment
                || operation.OperationType == OperationType.InsertOrIncrement)
            {
                return await this.GetCountTableOperation(operation, operation.Score);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Get delete if exists table operation
        /// </summary>
        /// <param name="operation">Store operation</param>
        /// <returns>Table operation. Returns null if the entity does not exist.</returns>
        private async Task<TableOperation> GetDeleteIfExistsOperation(Operation operation)
        {
            string partitionKey = this.GetPartitionKey(operation);
            string rowKey = this.GetRowKey(operation);

            Entity entity = await this.QueryTableEntity<Entity>(operation.Table, partitionKey, rowKey);
            if (entity != null)
            {
                TableEntity tableEntity = new TableEntity()
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey,
                    ETag = entity.ETag
                };

                return TableOperation.Delete(tableEntity);
            }

            return null;
        }

        /// <summary>
        /// Get increment and decrement operation for count table
        /// </summary>
        /// <param name="operation">Store operation</param>
        /// <param name="value">Value to add to counter</param>
        /// <returns>Table operation</returns>
        private async Task<TableOperation> GetCountTableOperation(Operation operation, double value)
        {
            string partitionKey = this.GetPartitionKey(operation);
            string rowKey = this.GetRowKey(operation);

            CountTable countTable = operation.Table as CountTable;
            CountEntity entity = await this.QueryCountAsync(countTable, operation.PartitionKey, operation.Key);
            TableOperation tableOperation = null;

            if (entity != null)
            {
                entity.Count += value;

                // The following will generate a precondition failed exception if entity was updated
                // between the read and replace operation.
                Operation replaceOperation = Operation.Replace(countTable, operation.PartitionKey, operation.Key, entity);
                tableOperation = await this.GetTableOperation(replaceOperation);
                return tableOperation;
            }

            if (operation.OperationType == OperationType.InsertOrIncrement)
            {
                // In case an entity gets added between the read and the following operation, it will generate
                // a conflict exception.
                Operation insertOperation = Operation.Insert(countTable, operation.PartitionKey, operation.Key, value);
                tableOperation = await this.GetTableOperation(insertOperation);
                return tableOperation;
            }
            else
            {
                CountEntity replaceEntity = new CountEntity()
                {
                    Count = value,
                    ETag = "W/\"datetime'2000-01-01T00%3A00%3A00.0000000Z'\"" // Dummy ETag Value
                };

                // We want to throw a not found exception. The following will throw a not found exception.
                // In case an entity gets added between the read and the following operation,
                // it will generate a precondition failed exception.
                // TODO: Need to make this consistent with cache.
                Operation replaceOperation = Operation.Replace(countTable, operation.PartitionKey, operation.Key, replaceEntity);
                tableOperation = await this.GetTableOperation(replaceOperation);
                return tableOperation;
            }
        }

        /// <summary>
        /// Get cloud table
        /// </summary>
        /// <param name="table">Store table</param>
        /// <returns>Cloud table</returns>
        private CloudTable GetCloudTable(Table table)
        {
            return this.GetCloudTable(table.ContainerName);
        }

        /// <summary>
        /// Get cloud table
        /// </summary>
        /// <param name="cloudTableName">Cloud table name</param>
        /// <returns>Cloud table</returns>
        private CloudTable GetCloudTable(string cloudTableName)
        {
            CloudTableClient tableClient = this.CloudStorageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(cloudTableName);
            return table;
        }

        /// <summary>
        /// Get dynamic table entity from operation
        /// </summary>
        /// <param name="operation">Store operation</param>
        /// <returns>Table entity</returns>
        private DynamicTableEntity GetTableEntity(Operation operation)
        {
            DynamicTableEntity tableEntity = new DynamicTableEntity();
            tableEntity.PartitionKey = this.GetPartitionKey(operation);
            tableEntity.RowKey = this.GetRowKey(operation);
            tableEntity.ETag = "*";

            if (operation.Entity != null)
            {
                Entity entity = operation.Entity;
                if (operation.OperationType == OperationType.Delete ||
                    operation.OperationType == OperationType.Replace ||
                    operation.OperationType == OperationType.Merge)
                {
                    tableEntity.ETag = entity.ETag;
                }

                Dictionary<string, EntityProperty> entityProperties = new Dictionary<string, EntityProperty>();
                var properties = entity.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (SpecialFieldNames.IsSpecialFieldName(property.Name, entity.GetType()))
                    {
                        continue;
                    }

                    object value = property.GetValue(entity);
                    if (property.PropertyType.BaseType == typeof(Enum))
                    {
                        entityProperties.Add(property.Name, EntityProperty.CreateEntityPropertyFromObject(value.ToString()));
                    }
                    else
                    {
                        entityProperties.Add(property.Name, EntityProperty.CreateEntityPropertyFromObject(value));
                    }
                }

                tableEntity.Properties = entityProperties;
            }

            return tableEntity;
        }

        /// <summary>
        /// Get partition key from operation
        /// </summary>
        /// <param name="operation">Store operation</param>
        /// <returns>Partition key</returns>
        private string GetPartitionKey(Operation operation)
        {
            return operation.PartitionKey;
        }

        /// <summary>
        /// Get row key from operation
        /// </summary>
        /// <param name="operation">Store operation</param>
        /// <returns>Row key</returns>
        private string GetRowKey(Operation operation)
        {
            return this.GetRowKey(operation.Table, operation.Key, operation.ItemKey);
        }

        /// <summary>
        /// Get row key
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="key">Key for object, count or feed</param>
        /// <param name="itemKey">Item key for feed entity</param>
        /// <returns>Row key</returns>
        private string GetRowKey(Table table, string key, string itemKey)
        {
            string rowKey = table.TableName;
            if (key != null)
            {
                rowKey += RowKeySeparator + key;
            }

            if (itemKey != null)
            {
                rowKey += RowKeySeparator + itemKey;
            }

            return rowKey;
        }

        /// <summary>
        /// Get row key
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="key">Key for entity or feed</param>
        /// <returns>Row key</returns>
        private string GetRowKey(Table table, string key)
        {
            return this.GetRowKey(table, key, null);
        }

        /// <summary>
        /// Get item key from row key
        /// </summary>
        /// <param name="table">Feed table</param>
        /// <param name="key">Key for object, count or feed</param>
        /// <param name="rowKey">Row key</param>
        /// <returns>Item key</returns>
        private string GetItemKey(Table table, string key, string rowKey)
        {
            string rowKeyPrefix = table.TableName + RowKeySeparator + key + RowKeySeparator;
            return rowKey.Substring(rowKeyPrefix.Length);
        }

        /// <summary>
        /// Get start row key for feed tables
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="cursor">Feed cursor</param>
        /// <returns>Start row key</returns>
        private string GetStartRowKeyForFeed(Table table, string feedKey, string cursor)
        {
            return this.GetRowKey(table, feedKey) + RowKeySeparator + cursor;
        }

        /// <summary>
        /// Get end row key for feed tables
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="feedKey">Key for feed</param>
        /// <returns>End row key</returns>
        private string GetEndRowKeyForFeed(Table table, string feedKey)
        {
            string startRowKey = this.GetStartRowKeyForFeed(table, feedKey, null);
            string endRowKey = this.GetNextLexString(startRowKey);
            return endRowKey;
        }

        /// <summary>
        /// Get next row key for feed
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key for feed entity</param>
        /// <returns>Next row key for feed</returns>
        private string GetNextRowKeyForFeed(Table table, string feedKey, string itemKey)
        {
            string rowKey = this.GetRowKey(table, feedKey, itemKey);
            string nextRowKey = this.GetNextLexString(rowKey);
            return rowKey;
        }

        /// <summary>
        /// Get next string in the lexical order
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>Next string in lexical order</returns>
        private string GetNextLexString(string input)
        {
            char lastChar = input[input.Length - 1];
            char nextLastChar = (char)((int)lastChar + 1);
            string nextString = input.Substring(0, input.Length - 1) + nextLastChar;
            return nextString;
        }

        /// <summary>
        /// Extract operation index from exception for batch operations
        /// </summary>
        /// <param name="e">Storage exception</param>
        /// <returns>Operation index in the exception message</returns>
        private int ExtractOperationIndex(StorageException e)
        {
            string[] splits = e.RequestInformation.ExtendedErrorInformation.ErrorMessage.Split(new char[] { ':' });
            return Convert.ToInt32(splits[0]);
        }

        /// <summary>
        /// Throw the appropriate exception based on the storage exception
        /// </summary>
        /// <param name="e">Storage exception</param>
        /// <param name="isTransaction">A value indicating whether the exception was generated during a transaction</param>
        private void ThrowException(StorageException e, bool isTransaction = false)
        {
            int operationIndex = 0;
            if (isTransaction)
            {
                operationIndex = this.ExtractOperationIndex(e);
            }

            if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                throw new ConflictException(Strings.Conflict, operationIndex, e);
            }
            else if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new NotFoundException(Strings.NotFound, operationIndex, e);
            }
            else if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
            {
                throw new PreconditionFailedException(Strings.PreconditionFailed, operationIndex, e);
            }
            else if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.BadRequest)
            {
                throw new BadRequestException(Strings.BadRequest, operationIndex, e);
            }

            throw e;
        }

        /// <summary>
        /// Normalize partial object fields. Remove fields that are unnecessary.
        /// </summary>
        /// <param name="fields">List of fields to be cleaned.</param>
        /// <returns>Normalized list of fields</returns>
        private List<string> NormalizePartialObjectFields(List<string> fields)
        {
            HashSet<string> queryFields = new HashSet<string>(fields);
            if (queryFields.Contains(SpecialFieldNames.PartitionKey))
            {
                queryFields.Remove(SpecialFieldNames.PartitionKey);
            }

            if (queryFields.Contains(SpecialFieldNames.ObjectKey))
            {
                queryFields.Remove(SpecialFieldNames.ObjectKey);
            }

            if (queryFields.Contains(SpecialFieldNames.ETag))
            {
                queryFields.Remove(SpecialFieldNames.ETag);
            }

            // When an empty field set is passed, we want to return only the
            // Partition key, object key, and ETag.
            // Azure table storage behavior is different for empty field set.
            // It returns all the field. We want to override this behavior by
            // not providing an empty field set.
            if (queryFields.Count == 0)
            {
                queryFields.Add(SpecialFieldNames.PartitionKey);
            }

            return queryFields.ToList();
        }
    }
}
