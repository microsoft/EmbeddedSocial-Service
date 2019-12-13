//-----------------------------------------------------------------------
// <copyright file="CTStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class CTStore.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// CT Store class
    /// </summary>
    public class CTStore : ICTStore
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
        /// Execution manager
        /// </summary>
        private ExecutionManager executionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CTStore"/> class
        /// </summary>
        /// <param name="persistentStore">Persistent store for CT store</param>
        /// <param name="cache">Cache for CT store</param>
        public CTStore(IPersistentStore persistentStore, ICache cache)
        {
            this.persistentStore = persistentStore;
            this.cache = cache;
            this.config = new Configuration();
            this.executionManager = new ExecutionManager(this.persistentStore, this.cache, this.config);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CTStore"/> class
        /// </summary>
        /// <param name="persistentStore">Persistent store for CT store</param>
        /// <param name="cache">Cache for CT store</param>
        /// <param name="config">Store configuration</param>
        public CTStore(IPersistentStore persistentStore, ICache cache, Configuration config)
        {
            this.persistentStore = persistentStore;
            this.cache = cache;
            this.config = config;
            this.executionManager = new ExecutionManager(persistentStore, cache, config);
        }

        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True of success</returns>
        public async Task<bool> CreateContainerAsync(string containerName)
        {
            return await this.persistentStore.CreateContainer(containerName);
        }

        /// <summary>
        /// Delete container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True of success</returns>
        public async Task<bool> DeleteContainerAsync(string containerName)
        {
            return await this.persistentStore.DeleteContainer(containerName);
        }

        /// <summary>
        /// Execute operation async
        /// </summary>
        /// <param name="operation">Table operation</param>
        /// <param name="consistencyMode">Consistency mode</param>
        /// <returns>Operation result</returns>
        public async Task<Result> ExecuteOperationAsync(Operation operation, ConsistencyMode consistencyMode)
        {
            this.ValidateExecuteOperationParameters(operation);
            return await this.executionManager.ExecuteOperationAsync(operation, consistencyMode);
        }

        /// <summary>
        /// Execute transaction async
        /// </summary>
        /// <param name="transaction">Table transaction</param>
        /// <param name="consistencyMode">Consistency mode</param>
        /// <returns>List of operation results</returns>
        public async Task<IList<Result>> ExecuteTransactionAsync(Transaction transaction, ConsistencyMode consistencyMode)
        {
            this.ValidateExecuteTransactionParameters(transaction);
            return await this.executionManager.ExecuteTransactionAsync(transaction, consistencyMode);
        }

        /// <summary>
        /// Query object async
        /// </summary>
        /// <typeparam name="T">Object entity</typeparam>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <returns>Object entity result. Returns null if entity does not exist.</returns>
        public async Task<T> QueryObjectAsync<T>(ObjectTable table, string partitionKey, string objectKey)
            where T : ObjectEntity, new()
        {
            this.ValidateQueryObjectParameters(table, partitionKey, objectKey);
            switch (table.StorageMode)
            {
                case StorageMode.PersistentOnly:
                    return await this.persistentStore.QueryObjectAsync<T>(table, partitionKey, objectKey);
                case StorageMode.CacheOnly:
                    return await this.cache.QueryObjectAsync<T>(table, partitionKey, objectKey);
                case StorageMode.Default:
                    T cacheEntity = await this.cache.QueryObjectAsync<T>(table, partitionKey, objectKey);
                    if (this.IsCacheEntityNullOrInvalid(cacheEntity))
                    {
                        T persistentEntity = await this.persistentStore.QueryObjectAsync<T>(table, partitionKey, objectKey);
                        Operation cachingOperation = this.GetObjectCachingOperation(table, partitionKey, objectKey, cacheEntity, persistentEntity);
                        await this.ExecuteCachingOperationAsync(cachingOperation);
                        return persistentEntity;
                    }

                    return cacheEntity;
                default:
                    throw new NotSupportedException();
            }
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
            this.ValidateQueryObjectParameters(table, partitionKey, objectKey);
            switch (table.StorageMode)
            {
                case StorageMode.PersistentOnly:
                    return await this.persistentStore.QueryObjectAsync<T>(table, partitionKey, objectKey);
                case StorageMode.CacheOnly:
                    return await this.cache.QueryObjectAsync<T>(table, partitionKey, objectKey);
                case StorageMode.Default:
                    T cacheEntity = await this.cache.QueryObjectAsync<T>(table, partitionKey, objectKey);
                    if (this.IsCacheEntityNullOrInvalid(cacheEntity))
                    {
                        T persistentEntity = await this.persistentStore.QueryObjectAsync<T>(table, partitionKey, objectKey);
                        Operation cachingOperation = this.GetObjectCachingOperation(table, partitionKey, objectKey, cacheEntity, persistentEntity);
                        await this.ExecuteCachingOperationAsync(cachingOperation);

                        return persistentEntity;
                    }

                    return cacheEntity;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Query partial object async
        /// </summary>
        /// <typeparam name="T">Partial object entity</typeparam>
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
            this.ValidateQueryObjectParameters(table, partitionKey, objectKey, fields);
            switch (table.StorageMode)
            {
                case StorageMode.PersistentOnly:
                    return await this.persistentStore.QueryPartialObjectAsync<T>(table, partitionKey, objectKey, fields);
                case StorageMode.CacheOnly:
                    return await this.cache.QueryPartialObjectAsync<T>(table, partitionKey, objectKey, fields);
                case StorageMode.Default:
                    T cacheEntity = await this.cache.QueryPartialObjectAsync<T>(table, partitionKey, objectKey, fields);
                    if (this.IsCacheEntityNullOrInvalid(cacheEntity))
                    {
                        T persistentEntity = await this.persistentStore.QueryPartialObjectAsync<T>(table, partitionKey, objectKey, fields);
                        return persistentEntity;
                    }

                    return cacheEntity;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Query feed item async
        /// </summary>
        /// <typeparam name="T">Feed entity</typeparam>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key for feed item</param>
        /// <returns>Feed entity in feed</returns>
        public async Task<T> QueryFeedItemAsync<T>(FeedTable table, string partitionKey, string feedKey, string itemKey)
            where T : FeedEntity, new()
        {
            this.ValidateQueryFeedItemParameters(table, partitionKey, feedKey, itemKey);
            switch (table.StorageMode)
            {
                case StorageMode.PersistentOnly:
                    return await this.persistentStore.QueryFeedItemAsync<T>(table, partitionKey, feedKey, itemKey);
                case StorageMode.CacheOnly:
                    return await this.cache.QueryFeedItemAsync<T>(table, partitionKey, feedKey, itemKey);
                case StorageMode.Default:
                    T cacheEntity = await this.cache.QueryFeedItemAsync<T>(table, partitionKey, feedKey, itemKey);
                    if (this.IsCacheEntityNullOrInvalid(cacheEntity))
                    {
                        T persistentEntity = await this.persistentStore.QueryFeedItemAsync<T>(table, partitionKey, feedKey, itemKey);
                        return persistentEntity;
                    }

                    return cacheEntity;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Query feed async
        /// </summary>
        /// <typeparam name="T">Feed entity</typeparam>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition for entity</param>
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
            this.ValidateQueryFeedParameters(table, partitionKey, feedKey, limit);
            switch (table.StorageMode)
            {
                case StorageMode.PersistentOnly:
                    return await this.persistentStore.QueryFeedAsync<T>(table, partitionKey, feedKey, cursor, limit);
                case StorageMode.CacheOnly:
                    return await this.cache.QueryFeedAsync<T>(table, partitionKey, feedKey, cursor, limit);
                case StorageMode.Default:
                    IList<T> cacheEntities = await this.cache.QueryFeedAsync<T>(table, partitionKey, feedKey, cursor, limit);
                    IList<T> invalidCacheEntities = cacheEntities.Where(e => this.IsCacheEntityNullOrInvalid(e)).ToList();
                    IEnumerable<Task<T>> persistentInvalidEntitiesTasks = null;
                    if (invalidCacheEntities.Count > 0)
                    {
                        persistentInvalidEntitiesTasks = from entity in invalidCacheEntities select this.persistentStore.QueryFeedItemAsync<T>(table, partitionKey, feedKey, entity.ItemKey);
                    }

                    IList<T> persistentEntities = new List<T>();
                    if (cacheEntities.Count < limit)
                    {
                        string continuationCursor = cacheEntities.Count == 0 ? cursor : cacheEntities.Last().ItemKey;

                        // We add count of invalid cache entities to be conservative in case all the invalid cache entities are null
                        // We add one more to the limit so there is at least item in the cache when the client queries with last item key as cursor
                        // If there are no items in the cache for a query, the entities in persistent store is not cached since we don't know the
                        // the range of the items and whether it continues from items in the cache.
                        int continuationLimit = limit - cacheEntities.Count;
                        if (int.MaxValue - continuationLimit < invalidCacheEntities.Count + 1)
                        {
                            continuationLimit = int.MaxValue;
                        }
                        else
                        {
                            continuationLimit += invalidCacheEntities.Count + 1;
                        }

                        persistentEntities = await this.persistentStore.QueryFeedAsync<T>(table, partitionKey, feedKey, continuationCursor, continuationLimit);
                    }

                    List<T> entities = new List<T>();
                    List<Operation> cachingOperations = new List<Operation>();
                    T[] persistentInvalidEntities = null;
                    if (persistentInvalidEntitiesTasks != null)
                    {
                        persistentInvalidEntities = await Task.WhenAll(persistentInvalidEntitiesTasks.ToArray());
                    }

                    int persistentInvalidEntityIndex = 0;
                    foreach (T cacheEntity in cacheEntities)
                    {
                        if (this.IsCacheEntityNullOrInvalid(cacheEntity))
                        {
                            T invalidPersistentEntity = persistentInvalidEntities[persistentInvalidEntityIndex++];
                            if (invalidPersistentEntity != null)
                            {
                                entities.Add(invalidPersistentEntity);
                            }

                            Operation cachingOperation = this.GetFeedCachingOperation(table, partitionKey, feedKey, cacheEntity.ItemKey, cacheEntity, invalidPersistentEntity, false);
                            if (cachingOperation != null)
                            {
                                cachingOperations.Add(cachingOperation);
                            }
                        }
                        else
                        {
                            entities.Add(cacheEntity);
                        }
                    }

                    foreach (T persistentEntity in persistentEntities)
                    {
                        entities.Add(persistentEntity);

                        // Add caching operations only if the persistent feed items are continuation
                        // of items retrieved from cache or if cursor is null (starting of the feed)
                        if (cacheEntities.Count > 0 || cursor == null)
                        {
                            // The cache could evict the entire feed before we run the caching operations.
                            // If the cache already has feed items, the eviction combined with the caching operations
                            // can violate the feed cache invariant.
                            // Hence, we need to make sure the the feed is not empty
                            // when newer items are added to the cache.
                            // We don't need to make this check if the cursor is null and there are
                            // no items in the cache to start with.
                            bool checkIfNotEmpty = cacheEntities.Count != 0;
                            Operation cachingOperation = this.GetFeedCachingOperation(table, partitionKey, feedKey, persistentEntity.ItemKey, null, persistentEntity, checkIfNotEmpty);
                            cachingOperations.Add(cachingOperation);
                        }
                    }

                    await this.ExecuteCachingOperationsAsync(cachingOperations);
                    return entities.Take(limit).ToList();
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Query count async
        /// </summary>
        /// <param name="table">Count table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="countKey">Key for entity</param>
        /// <returns>Count entity for key</returns>
        public async Task<CountEntity> QueryCountAsync(CountTable table, string partitionKey, string countKey)
        {
            this.ValidateQueryCountParameters(table, partitionKey, countKey);
            switch (table.StorageMode)
            {
                case StorageMode.PersistentOnly:
                    return await this.persistentStore.QueryCountAsync(table, partitionKey, countKey);
                case StorageMode.CacheOnly:
                    return await this.cache.QueryCountAsync(table, partitionKey, countKey);
                case StorageMode.Default:
                    CountEntity cacheEntity = await this.cache.QueryCountAsync(table, partitionKey, countKey);
                    if (this.IsCacheEntityNullOrInvalid(cacheEntity))
                    {
                        CountEntity persistentEntity = await this.persistentStore.QueryCountAsync(table, partitionKey, countKey);
                        Operation cachingOperation = this.GetCountCachingOperation(table, partitionKey, countKey, cacheEntity, persistentEntity);
                        await this.ExecuteCachingOperationAsync(cachingOperation);
                        return persistentEntity;
                    }

                    return cacheEntity;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Query rank feed item async
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key</param>
        /// <returns>List of rank feed entities</returns>
        public async Task<RankFeedEntity> QueryRankFeedItemAsync(
            RankFeedTable table,
            string partitionKey,
            string feedKey,
            string itemKey)
        {
            this.ValidateQueryRankFeedItemParameters(table, partitionKey, feedKey, itemKey);
            switch (table.StorageMode)
            {
                case StorageMode.CacheOnly:
                    return await this.cache.QueryRankFeedItemAsync(table, partitionKey, feedKey, itemKey);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Query rank feed async
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="cursor">Feed cursor</param>
        /// <param name="limit">Feed count limit</param>
        /// <returns>List of rank feed entities</returns>
        public async Task<IList<RankFeedEntity>> QueryRankFeedAsync(
            RankFeedTable table,
            string partitionKey,
            string feedKey,
            string cursor,
            int limit)
        {
            this.ValidateQueryRankFeedParameters(table, partitionKey, feedKey);
            switch (table.StorageMode)
            {
                case StorageMode.CacheOnly:
                    return await this.cache.QueryRankFeedAsync(table, partitionKey, feedKey, cursor, limit);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Query rank feed in reverse async
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="cursor">Feed cursor</param>
        /// <param name="limit">Feed count limit</param>
        /// <returns>List of rank feed entities</returns>
        public async Task<IList<RankFeedEntity>> QueryRankFeedReverseAsync(
            RankFeedTable table,
            string partitionKey,
            string feedKey,
            string cursor,
            int limit)
        {
            this.ValidateQueryRankFeedParameters(table, partitionKey, feedKey);
            switch (table.StorageMode)
            {
                case StorageMode.CacheOnly:
                    return await this.cache.QueryRankFeedReverseAsync(table, partitionKey, feedKey, cursor, limit);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Query rank feed by score async
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="startScore">Start score</param>
        /// <param name="endScore">End score</param>
        /// <returns>List of rank feed entities</returns>
        public async Task<IList<RankFeedEntity>> QueryRankFeedByScoreAsync(
            RankFeedTable table,
            string partitionKey,
            string feedKey,
            double startScore,
            double endScore)
        {
            this.ValidateQueryRankFeedParameters(table, partitionKey, feedKey);
            switch (table.StorageMode)
            {
                case StorageMode.CacheOnly:
                    return await this.cache.QueryRankFeedByScoreAsync(table, partitionKey, feedKey, startScore, endScore);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Query rank feed length
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <returns>Rank feed length</returns>
        public async Task<long> QueryRankFeedLengthAsync(
            RankFeedTable table,
            string partitionKey,
            string feedKey)
        {
            this.ValidateQueryRankFeedParameters(table, partitionKey, feedKey);
            switch (table.StorageMode)
            {
                case StorageMode.CacheOnly:
                    return await this.cache.QueryRankFeedLengthAsync(table, partitionKey, feedKey);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Verify if cache entity is null or invalid
        /// </summary>
        /// <param name="entity">Cache entity</param>
        /// <returns>A value indicating whether cache entity is invalid</returns>
        private bool IsCacheEntityNullOrInvalid(Entity entity)
        {
            if (entity == null || entity.CacheInvalid)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Verify if cache entity is expired
        /// </summary>
        /// <param name="entity">Cache entity</param>
        /// <returns>A value indicating whether cache entity is expired</returns>
        private bool IsCacheEntityExpired(Entity entity)
        {
            return this.config.GetTimeMethod() > entity.CacheExpiry;
        }

        /// <summary>
        /// Execute caching operations
        /// </summary>
        /// <param name="cachingOperations">Caching operations</param>
        /// <returns>Caching operations task</returns>
        private async Task ExecuteCachingOperationsAsync(List<Operation> cachingOperations)
        {
            if (cachingOperations.Count > 0)
            {
                Transaction cachingTransaction = new Transaction(cachingOperations);
                try
                {
                    await this.cache.ExecuteTransactionAsync(cachingTransaction);
                }
                catch (Exception e)
                {
                    // best effort
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Execute caching operation
        /// </summary>
        /// <param name="cachingOperation">Caching operation</param>
        /// <returns>Caching operation task</returns>
        private async Task ExecuteCachingOperationAsync(Operation cachingOperation)
        {
            if (cachingOperation != null)
            {
                try
                {
                    await this.cache.ExecuteOperationAsync(cachingOperation);
                }
                catch
                {
                    // best effort
                }
            }
        }

        /// <summary>
        /// Get object caching operation
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="key">Key for object or feed</param>
        /// <param name="cacheEntity">Cache entity</param>
        /// <param name="persistentEntity">Persistent store entity</param>
        /// <returns>Caching operation</returns>
        private Operation GetObjectCachingOperation(Table table, string partitionKey, string key, Entity cacheEntity, Entity persistentEntity)
        {
            Operation cachingOperation = null;
            if (cacheEntity != null && this.IsCacheEntityExpired(cacheEntity))
            {
                if (persistentEntity != null)
                {
                    if (table is ObjectTable)
                    {
                        cachingOperation = Operation.Replace(table as ObjectTable, partitionKey, key, persistentEntity as ObjectEntity);
                        cachingOperation.Entity.ETag = cacheEntity.ETag;
                        cachingOperation.Entity.CustomETag = persistentEntity.ETag;
                    }
                    else if (table is FixedObjectTable)
                    {
                        cachingOperation = Operation.Replace(table as FixedObjectTable, partitionKey, key, persistentEntity as ObjectEntity);
                        cachingOperation.Entity.ETag = cacheEntity.ETag;
                        cachingOperation.Entity.CustomETag = persistentEntity.ETag;
                    }
                }
                else
                {
                    if (table is ObjectTable)
                    {
                        ObjectEntity deleteEntity = new ObjectEntity();
                        deleteEntity.ETag = cacheEntity.ETag;
                        cachingOperation = Operation.Delete(table as ObjectTable, partitionKey, key, deleteEntity);
                    }
                    else if (table is FixedObjectTable)
                    {
                        ObjectEntity deleteEntity = new ObjectEntity();
                        deleteEntity.ETag = cacheEntity.ETag;
                        cachingOperation = Operation.Delete(table as FixedObjectTable, partitionKey, key, deleteEntity);
                    }
                }
            }
            else if (cacheEntity == null)
            {
                if (persistentEntity != null)
                {
                    if (table is ObjectTable)
                    {
                        cachingOperation = Operation.Insert(table as ObjectTable, partitionKey, key, persistentEntity as ObjectEntity);
                        cachingOperation.Entity.CustomETag = persistentEntity.ETag;
                    }
                    else if (table is FixedObjectTable)
                    {
                        cachingOperation = Operation.Insert(table as FixedObjectTable, partitionKey, key, persistentEntity as ObjectEntity);
                        cachingOperation.Entity.CustomETag = persistentEntity.ETag;
                    }
                }
            }

            return cachingOperation;
        }

        /// <summary>
        /// Get count caching operation
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="key">Key for object or feed</param>
        /// <param name="cacheEntity">Cache entity</param>
        /// <param name="persistentEntity">Persistent store entity</param>
        /// <returns>Caching operation</returns>
        private Operation GetCountCachingOperation(Table table, string partitionKey, string key, Entity cacheEntity, Entity persistentEntity)
        {
            Operation cachingOperation = null;
            if (cacheEntity != null && this.IsCacheEntityExpired(cacheEntity))
            {
                if (persistentEntity != null)
                {
                    if (table is CountTable)
                    {
                        cachingOperation = Operation.Replace(table as CountTable, partitionKey, key, persistentEntity as CountEntity);
                        cachingOperation.Entity.ETag = cacheEntity.ETag;
                        cachingOperation.Entity.CustomETag = persistentEntity.ETag;
                    }
                }
                else
                {
                    if (table is CountTable)
                    {
                        CountEntity deleteEntity = new CountEntity();
                        deleteEntity.ETag = cacheEntity.ETag;
                        cachingOperation = Operation.Delete(table as CountTable, partitionKey, key, deleteEntity);
                    }
                }
            }
            else if (cacheEntity == null)
            {
                if (persistentEntity != null)
                {
                    if (table is CountTable)
                    {
                        cachingOperation = Operation.Insert(table as CountTable, partitionKey, key, (persistentEntity as CountEntity).Count);
                        cachingOperation.Entity.CustomETag = persistentEntity.ETag;
                    }
                }
            }

            return cachingOperation;
        }

        /// <summary>
        /// Get feed caching operation
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="key">Key for object or feed</param>
        /// <param name="itemKey">Item key for feed</param>
        /// <param name="cacheEntity">Cache entity</param>
        /// <param name="persistentEntity">Persistent store entity</param>
        /// <param name="checkNotEmpty">A value indicating whether we should check if the feed is not empty in cache during the operation</param>
        /// <returns>Caching operation</returns>
        private Operation GetFeedCachingOperation(Table table, string partitionKey, string key, string itemKey, Entity cacheEntity, Entity persistentEntity, bool checkNotEmpty)
        {
            Operation cachingOperation = null;
            if (cacheEntity != null && this.IsCacheEntityExpired(cacheEntity))
            {
                if (persistentEntity != null)
                {
                    if (table is FeedTable)
                    {
                        cachingOperation = Operation.Replace(table as FeedTable, partitionKey, key, itemKey, persistentEntity as FeedEntity);
                        cachingOperation.Entity.ETag = cacheEntity.ETag;
                        cachingOperation.Entity.CustomETag = persistentEntity.ETag;
                    }
                }
                else
                {
                    if (table is FeedTable)
                    {
                        FeedEntity deleteEntity = new FeedEntity();
                        deleteEntity.ETag = cacheEntity.ETag;
                        cachingOperation = Operation.Delete(table as FeedTable, partitionKey, key, itemKey, deleteEntity);
                    }
                }
            }
            else if (cacheEntity == null)
            {
                if (persistentEntity != null)
                {
                    if (table is FeedTable)
                    {
                        if (checkNotEmpty)
                        {
                            cachingOperation = Operation.InsertIfNotEmpty(table as FeedTable, partitionKey, key, itemKey, persistentEntity as FeedEntity);
                            cachingOperation.Entity.CustomETag = persistentEntity.ETag;
                        }
                        else
                        {
                            cachingOperation = Operation.Insert(table as FeedTable, partitionKey, key, itemKey, persistentEntity as FeedEntity);
                            cachingOperation.Entity.CustomETag = persistentEntity.ETag;
                        }
                    }
                }
            }

            return cachingOperation;
        }

        /// <summary>
        /// Validate execute operation parameters
        /// </summary>
        /// <param name="operation">Store operation</param>
        private void ValidateExecuteOperationParameters(Operation operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException("Operation cannot be null");
            }
        }

        /// <summary>
        /// Validate execute transaction parameters
        /// </summary>
        /// <param name="transaction">Store transaction</param>
        private void ValidateExecuteTransactionParameters(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("Transaction cannot be null");
            }

            if (transaction.Operations.Count == 0)
            {
                throw new ArgumentException("Transaction cannot be empty");
            }
        }

        /// <summary>
        /// Validate query object parameters
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="objectKey">Object key</param>
        /// <param name="fields">Partial object query fields</param>
        private void ValidateQueryObjectParameters(Table table, string partitionKey, string objectKey, List<string> fields)
        {
            this.ValidateQueryObjectParameters(table, partitionKey, objectKey);
            if (fields == null)
            {
                throw new ArgumentNullException("Fields cannot be null");
            }
        }

        /// <summary>
        /// Validate query object parameters
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="objectKey">Object key</param>
        private void ValidateQueryObjectParameters(Table table, string partitionKey, string objectKey)
        {
            if (table == null)
            {
                throw new ArgumentNullException("Table cannot be null");
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentNullException("Partition key cannot be null or empty");
            }

            if (string.IsNullOrEmpty(objectKey))
            {
                throw new ArgumentNullException("Object key cannot be null or empty");
            }
        }

        /// <summary>
        /// Validate query feed item parameters
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="feedKey">Feed key</param>
        /// <param name="itemKey">Item key</param>
        private void ValidateQueryFeedItemParameters(Table table, string partitionKey, string feedKey, string itemKey)
        {
            this.ValidateQueryFeedParameters(table, partitionKey, feedKey);
            if (string.IsNullOrEmpty(itemKey))
            {
                throw new ArgumentNullException("Item key cannot be null or empty");
            }
        }

        /// <summary>
        /// Validate query feed parameters
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="feedKey">Feed key</param>
        /// <param name="limit">Item limit</param>
        private void ValidateQueryFeedParameters(Table table, string partitionKey, string feedKey, int limit)
        {
            this.ValidateQueryFeedParameters(table, partitionKey, feedKey);
            if (limit <= 0)
            {
                throw new ArgumentException("Limit should be a positive number");
            }
        }

        /// <summary>
        /// Validate query feed parameters
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="feedKey">Feed key</param>
        private void ValidateQueryFeedParameters(Table table, string partitionKey, string feedKey)
        {
            if (table == null)
            {
                throw new ArgumentNullException("Table cannot be null");
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentNullException("Partition key cannot be null or empty");
            }

            if (string.IsNullOrEmpty(feedKey))
            {
                throw new ArgumentNullException("Object key cannot be null or empty");
            }
        }

        /// <summary>
        /// Validate query count parameters
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="countKey">Count key</param>
        private void ValidateQueryCountParameters(Table table, string partitionKey, string countKey)
        {
            if (table == null)
            {
                throw new ArgumentNullException("Table cannot be null");
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentNullException("Partition key cannot be null or empty");
            }

            if (string.IsNullOrEmpty(countKey))
            {
                throw new ArgumentNullException("Count key cannot be null or empty");
            }
        }

        /// <summary>
        /// Validate query rank feed item parameters
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="feedKey">Feed key</param>
        /// <param name="itemKey">Item key</param>
        private void ValidateQueryRankFeedItemParameters(Table table, string partitionKey, string feedKey, string itemKey)
        {
            this.ValidateQueryRankFeedParameters(table, partitionKey, feedKey);
            if (string.IsNullOrEmpty(itemKey))
            {
                throw new ArgumentNullException("Item key cannot be null or empty");
            }
        }

        /// <summary>
        /// Validate query rank feed parameters
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="feedKey">Count key</param>
        /// <param name="limit">Item limit</param>
        private void ValidateQueryRankFeedParameters(Table table, string partitionKey, string feedKey, int limit)
        {
            this.ValidateQueryRankFeedParameters(table, partitionKey, feedKey);
            if (limit <= 0)
            {
                throw new ArgumentException("Limit should be a positive number");
            }
        }

        /// <summary>
        /// Validate query rank feed parameters
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="feedKey">Count key</param>
        private void ValidateQueryRankFeedParameters(Table table, string partitionKey, string feedKey)
        {
            if (table == null)
            {
                throw new ArgumentNullException("Table cannot be null");
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentNullException("Partition key cannot be null or empty");
            }

            if (string.IsNullOrEmpty(feedKey))
            {
                throw new ArgumentNullException("Feed key cannot be null or empty");
            }
        }
    }
}
