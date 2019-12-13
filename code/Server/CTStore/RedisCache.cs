//-----------------------------------------------------------------------
// <copyright file="RedisCache.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class RedisCache.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using StackExchange.Redis;

    /// <summary>
    /// <c>Redis</c> cache as cache
    /// </summary>
    public class RedisCache : ICache
    {
        /// <summary>
        /// Separator character in key
        /// </summary>
        private const char KeySeparator = ':';

        /// <summary>
        /// Lazy <c>Redis</c> Connection Multiplexer
        /// </summary>
        private Lazy<ConnectionMultiplexer> lazyConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCache"/> class
        /// </summary>
        /// <param name="connectionString"><c>Redis</c> cache connection string</param>
        public RedisCache(string connectionString)
        {
            this.lazyConnection = new Lazy<ConnectionMultiplexer>(() => { return ConnectionMultiplexer.Connect(connectionString); });
        }

        /// <summary>
        /// Gets <c>Redis</c> Connection Multiplexer
        /// </summary>
        private ConnectionMultiplexer Connection
        {
            get
            {
                return this.lazyConnection.Value;
            }
        }

        /// <summary>
        /// Gets <c>Redis</c> database
        /// </summary>
        private IDatabase Cache
        {
            get
            {
                return this.Connection.GetDatabase();
            }
        }

        /// <summary>
        /// Execute operation async
        /// </summary>
        /// <param name="operation">Table operation</param>
        /// <returns>Operation result</returns>
        public async Task<Result> ExecuteOperationAsync(Operation operation)
        {
            Transaction transaction = new Transaction();
            transaction.Add(operation);
            var results = await this.ExecuteTransactionAsync(transaction);
            return results.First();
        }

        /// <summary>
        /// Execute transaction async
        /// </summary>
        /// <param name="transaction">Table transaction</param>
        /// <returns>List of operation results</returns>
        public async Task<IList<Result>> ExecuteTransactionAsync(Transaction transaction)
        {
            List<Operation> operations = transaction.Operations;
            RedisLuaScript script = this.BuildLuaScript(operations);
            RedisResult[] redisResults = null;
            redisResults = (RedisResult[])await this.Cache.ScriptEvaluateAsync(script.Script, script.Keys.ToArray(), script.Values.ToArray());
            return this.ParseResults(operations, script, redisResults);
        }

        /// <summary>
        /// Query object async
        /// </summary>
        /// <typeparam name="T">Object entity</typeparam>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <returns>Object entity in store</returns>
        public async Task<T> QueryObjectAsync<T>(ObjectTable table, string partitionKey, string objectKey)
            where T : ObjectEntity, new()
        {
            string key = this.GetKey(table, partitionKey, objectKey);
            HashEntry[] hashEntries = await this.Cache.HashGetAllAsync(key);
            if (hashEntries.Length == 0)
            {
                return null;
            }

            T entity = this.GetEntityFromHash<T>(hashEntries);
            entity.PartitionKey = partitionKey;
            entity.ObjectKey = objectKey;
            return entity;
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
            string key = this.GetKey(table, partitionKey, objectKey);
            List<string> queryFields = this.NormalizePartialObjectFields(fields);
            RedisValue[] hashFields = queryFields.Select(f => (RedisValue)f).ToArray();
            RedisValue[] hashValues = await this.Cache.HashGetAsync(key, hashFields);

            if (hashFields.Length != hashValues.Length)
            {
                throw new UnexpectedException("Partial object result length does not match query length", null);
            }

            T entity = this.GetEntityFromHash<T>(hashFields, hashValues);
            if (entity.ETag == null)
            {
                return null;
            }

            entity.PartitionKey = partitionKey;
            entity.ObjectKey = objectKey;
            return entity;
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
            string key = this.GetKey(table, partitionKey, objectKey);
            RedisValue value = await this.Cache.StringGetAsync(key);
            if (value == (byte[])null)
            {
                return null;
            }

            T entity = default(T);
            try
            {
                entity = this.GetEntityFromBytes<T>(value);
            }
            catch (Exception e)
            {
                throw new FormatException("Unable to construct entity from value in cache", e);
            }

            entity.PartitionKey = partitionKey;
            entity.ObjectKey = objectKey;
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
            string key = this.GetKey(table, partitionKey, feedKey);
            RedisValue[] result = (RedisValue[])await this.Cache.ScriptEvaluateAsync(RedisLuaCommands.FeedItemQuery, new RedisKey[] { key }, new RedisValue[] { this.GetRangeMinItemKey(itemKey), this.GetRangeMaxItemKey(itemKey) });
            if (result == null || result.Length == 0)
            {
                return null;
            }

            if (result.Length > 1)
            {
                throw new UnexpectedException("More than one feed item returned", null);
            }

            T entity = default(T);
            try
            {
                entity = this.GetEntityFromBytes<T>(result.First());
            }
            catch (Exception e)
            {
                throw new FormatException("Unable to construct entity from value in cache", e);
            }

            entity.PartitionKey = partitionKey;
            entity.FeedKey = feedKey;
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
            string key = this.GetKey(table, partitionKey, feedKey);
            byte[] minItemKey = cursor != null ? this.GetRangeCursorItemKey(cursor) : new byte[] { Convert.ToByte('-') };
            RedisValue[] results = (RedisValue[])await this.Cache.ScriptEvaluateAsync(RedisLuaCommands.FeedQuery, new RedisKey[] { key }, new RedisValue[] { minItemKey, limit });
            if (results == null)
            {
                return null;
            }

            List<T> entities = new List<T>();
            foreach (RedisValue result in results)
            {
                T entity = default(T);
                try
                {
                    entity = this.GetEntityFromBytes<T>(result);
                }
                catch (Exception e)
                {
                    throw new FormatException("Unable to construct entity from value in cache", e);
                }

                entity.PartitionKey = partitionKey;
                entity.FeedKey = feedKey;
                entity.Cursor = entity.ItemKey;
                entities.Add(entity);
            }

            return entities;
        }

        /// <summary>
        /// Query count async
        /// </summary>
        /// <param name="table">Count table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="countKey">Key for entity</param>
        /// <returns>Count entity</returns>
        public async Task<CountEntity> QueryCountAsync(CountTable table, string partitionKey, string countKey)
        {
            string key = this.GetKey(table, partitionKey, countKey);
            HashEntry[] hashEntries = await this.Cache.HashGetAllAsync(key);
            if (hashEntries.Length == 0)
            {
                return null;
            }

            CountEntity entity = this.GetEntityFromHash<CountEntity>(hashEntries);
            entity.PartitionKey = partitionKey;
            entity.CountKey = countKey;
            return entity;
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
            string key = this.GetKey(table, partitionKey, feedKey);
            double? score = await this.Cache.SortedSetScoreAsync(key, itemKey);
            if (!score.HasValue)
            {
                return null;
            }

            RankFeedEntity entity = new RankFeedEntity()
            {
                PartitionKey = partitionKey,
                FeedKey = feedKey,
                ItemKey = itemKey,
                Score = score.Value,
                Cursor = null
            };

            return entity;
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
            return await this.QueryRankFeedAsync(table, partitionKey, feedKey, cursor, limit, false);
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
            return await this.QueryRankFeedAsync(table, partitionKey, feedKey, cursor, limit, true);
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
            string key = this.GetKey(table, partitionKey, feedKey);
            SortedSetEntry[] values = await this.Cache.SortedSetRangeByScoreWithScoresAsync(key, startScore, endScore, Exclude.None, Order.Ascending);
            List<RankFeedEntity> entities = new List<RankFeedEntity>();
            foreach (SortedSetEntry value in values)
            {
                entities.Add(this.GetRankFeedEntity(value, partitionKey, feedKey, null));
            }

            return entities;
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
            string key = this.GetKey(table, partitionKey, feedKey);
            return await this.Cache.SortedSetLengthAsync(key);
        }

        /// <summary>
        /// Query rank feed async in a given order
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="cursor">Feed cursor</param>
        /// <param name="limit">Feed count limit</param>
        /// <param name="reverseOrder">A value indicating whether to query the feed in reverse order</param>
        /// <returns>List of rank feed entities</returns>
        private async Task<IList<RankFeedEntity>> QueryRankFeedAsync(
            RankFeedTable table,
            string partitionKey,
            string feedKey,
            string cursor,
            int limit,
            bool reverseOrder)
        {
            string key = this.GetKey(table, partitionKey, feedKey);
            int startRank = 0;
            if (cursor != null)
            {
                // Cursor represents the rank of the item last retrieved.
                // We need to start querying from the next rank
                // ---
                // Can throw exception
                startRank = Convert.ToInt32(cursor) + 1;
            }

            Order order = Order.Ascending;
            if ((table.Order == FeedOrder.Descending && !reverseOrder)
                || (table.Order == FeedOrder.Ascending && reverseOrder))
            {
                order = Order.Descending;
            }

            SortedSetEntry[] values = await this.Cache.SortedSetRangeByRankWithScoresAsync(key, startRank, startRank + limit - 1, order);
            List<RankFeedEntity> entities = new List<RankFeedEntity>();
            int rank = startRank;
            foreach (SortedSetEntry value in values)
            {
                entities.Add(this.GetRankFeedEntity(value, partitionKey, feedKey, rank.ToString()));
                rank++;
            }

            return entities;
        }

        /// <summary>
        /// Build <c>Lua</c> script form operations
        /// </summary>
        /// <param name="operations">List of operations</param>
        /// <returns><c>Redis</c> <c>Lua</c> script</returns>
        private RedisLuaScript BuildLuaScript(List<Operation> operations)
        {
            RedisLuaScript script = new RedisLuaScript();
            script.Script = string.Empty;
            script.Conditions = new List<string>();
            script.Actions = new List<string>();
            script.Keys = new List<RedisKey>();
            script.Values = new List<RedisValue>();
            script.ETags = new List<string>();
            script.Exceptions = new Dictionary<int, List<OperationFailedException>>();
            Dictionary<string, Table> feedTrims = new Dictionary<string, Table>();

            int keyIndex = 1;
            int valueIndex = 1;
            int errorCode = -1;
            int resultIndex = 2;
            foreach (Operation operation in operations)
            {
                string key = this.GetKey(operation);
                string condition = null;
                List<RedisValue> conditionValues = new List<RedisValue>();
                bool checkConditionValue = false;
                string action = null;
                List<RedisValue> actionValues = new List<RedisValue>();
                List<OperationFailedException> exceptions = new List<OperationFailedException>();
                string resultETag = null;

                if (operation.Table is ObjectTable)
                {
                    switch (operation.OperationType)
                    {
                        case OperationType.Insert:
                            condition = string.Format(RedisLuaCommands.ObjectInsertCondition, keyIndex, errorCode);
                            actionValues.AddRange(this.GetValueHash(operation.Entity));
                            action = string.Format(RedisLuaCommands.ObjectInsertAction, resultIndex, keyIndex, script.Values.Count + 1, script.Values.Count + actionValues.Count);
                            exceptions.Add(new ConflictException(Strings.Conflict, -errorCode - 1, null));
                            resultETag = operation.Entity.CustomETag;
                            break;
                        case OperationType.Delete:
                            if (operation.Entity == null || operation.Entity.ETag == "*")
                            {
                                condition = string.Format(RedisLuaCommands.ObjectDeleteCondition, keyIndex, errorCode);
                                action = string.Format(RedisLuaCommands.ObjectDeleteAction, resultIndex, keyIndex);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                            }
                            else
                            {
                                condition = string.Format(RedisLuaCommands.ObjectETagCondition, keyIndex, valueIndex, errorCode);
                                conditionValues.Add(operation.Entity.ETag);
                                checkConditionValue = true;
                                action = string.Format(RedisLuaCommands.ObjectDeleteAction, resultIndex, keyIndex);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                exceptions.Add(new PreconditionFailedException(Strings.PreconditionFailed, -errorCode - 1, null));
                            }

                            break;
                        case OperationType.DeleteIfExists:
                            action = string.Format(RedisLuaCommands.ObjectDeleteAction, resultIndex, keyIndex);
                            break;
                        case OperationType.Replace:
                            if (operation.Entity.ETag == "*")
                            {
                                condition = string.Format(RedisLuaCommands.ObjectWilcardReplaceCondition, keyIndex, errorCode);
                                actionValues.AddRange(this.GetValueHash(operation.Entity));
                                action = string.Format(RedisLuaCommands.ObjectInsertOrReplaceAction, resultIndex, keyIndex, script.Values.Count + 1, script.Values.Count + actionValues.Count);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                resultETag = operation.Entity.CustomETag;
                            }
                            else
                            {
                                condition = string.Format(RedisLuaCommands.ObjectETagCondition, keyIndex, valueIndex, errorCode);
                                conditionValues.Add(operation.Entity.ETag);
                                checkConditionValue = true;
                                actionValues.AddRange(this.GetValueHash(operation.Entity));
                                action = string.Format(RedisLuaCommands.ObjectReplaceAction, resultIndex, keyIndex, script.Values.Count + 2, script.Values.Count + actionValues.Count + 1);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                exceptions.Add(new PreconditionFailedException(Strings.PreconditionFailed, -errorCode - 1, null));
                                resultETag = operation.Entity.CustomETag;
                            }

                            break;
                        case OperationType.InsertOrReplace:
                            actionValues.AddRange(this.GetValueHash(operation.Entity));
                            action = string.Format(RedisLuaCommands.ObjectInsertOrReplaceAction, resultIndex, keyIndex, script.Values.Count + 1, script.Values.Count + actionValues.Count);
                            resultETag = operation.Entity.CustomETag;
                            break;
                        case OperationType.Merge:
                            if (operation.Entity.ETag == "*")
                            {
                                condition = string.Format(RedisLuaCommands.ObjectWilcardMergeCondition, keyIndex, errorCode);
                                actionValues.AddRange(this.GetValueHash(operation.Entity));
                                action = string.Format(RedisLuaCommands.ObjectInsertOrMergeAction, resultIndex, keyIndex, script.Values.Count + 1, script.Values.Count + actionValues.Count);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                resultETag = operation.Entity.CustomETag;
                            }
                            else
                            {
                                condition = string.Format(RedisLuaCommands.ObjectETagCondition, keyIndex, valueIndex, errorCode);
                                conditionValues.Add(operation.Entity.ETag);
                                checkConditionValue = true;
                                actionValues.AddRange(this.GetValueHash(operation.Entity));
                                action = string.Format(RedisLuaCommands.ObjectMergeAction, resultIndex, keyIndex, script.Values.Count + 2, script.Values.Count + actionValues.Count + 1);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                exceptions.Add(new PreconditionFailedException(Strings.PreconditionFailed, -errorCode - 1, null));
                                resultETag = operation.Entity.CustomETag;
                            }

                            break;
                        case OperationType.InsertOrMerge:
                            actionValues.AddRange(this.GetValueHash(operation.Entity));
                            action = string.Format(RedisLuaCommands.ObjectInsertOrMergeAction, resultIndex, keyIndex, script.Values.Count + 1, script.Values.Count + actionValues.Count);
                            resultETag = operation.Entity.CustomETag;
                            break;
                    }
                }
                else if (operation.Table is FixedObjectTable)
                {
                    switch (operation.OperationType)
                    {
                        case OperationType.Insert:
                            condition = string.Format(RedisLuaCommands.FixedObjectInsertCondition, keyIndex, errorCode);
                            action = string.Format(RedisLuaCommands.FixedObjectInsertAction, resultIndex, keyIndex, valueIndex);
                            actionValues.Add(this.GetValueBytes(operation));
                            exceptions.Add(new ConflictException(Strings.Conflict, -errorCode - 1, null));
                            resultETag = this.ConvertValueToETag(actionValues.Last());
                            break;
                        case OperationType.Delete:
                            if (operation.Entity == null || operation.Entity.ETag == "*")
                            {
                                condition = string.Format(RedisLuaCommands.FixedObjectDeleteCondition, keyIndex, errorCode);
                                action = string.Format(RedisLuaCommands.FixedObjectDeleteAction, resultIndex, keyIndex);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                            }
                            else
                            {
                                condition = string.Format(RedisLuaCommands.FixedObjectETagCondition, keyIndex, valueIndex, errorCode);
                                conditionValues.Add(this.ConvertETagToValue(operation.Entity.ETag));
                                checkConditionValue = true;
                                action = string.Format(RedisLuaCommands.FixedObjectDeleteAction, resultIndex, keyIndex);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                exceptions.Add(new PreconditionFailedException(Strings.PreconditionFailed, -errorCode - 1, null));
                            }

                            break;
                        case OperationType.DeleteIfExists:
                            action = string.Format(RedisLuaCommands.FixedObjectDeleteAction, resultIndex, keyIndex);
                            break;
                        case OperationType.Replace:
                            if (operation.Entity.ETag == "*")
                            {
                                condition = string.Format(RedisLuaCommands.FixedObjectWilcardReplaceCondition, keyIndex, errorCode);
                                action = string.Format(RedisLuaCommands.FixedObjectInsertOrReplaceAction, resultIndex, keyIndex, valueIndex);
                                actionValues.Add(this.GetValueBytes(operation));
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                resultETag = this.ConvertValueToETag(actionValues.Last());
                            }
                            else
                            {
                                condition = string.Format(RedisLuaCommands.FixedObjectETagCondition, keyIndex, valueIndex, errorCode);
                                conditionValues.Add(this.ConvertETagToValue(operation.Entity.ETag));
                                checkConditionValue = true;
                                action = string.Format(RedisLuaCommands.FixedObjectReplaceAction, resultIndex, keyIndex, valueIndex + 1);
                                actionValues.Add(this.GetValueBytes(operation));
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                exceptions.Add(new PreconditionFailedException(Strings.PreconditionFailed, -errorCode - 1, null));
                                resultETag = this.ConvertValueToETag(actionValues.Last());
                            }

                            break;
                        case OperationType.InsertOrReplace:
                            action = string.Format(RedisLuaCommands.FixedObjectInsertOrReplaceAction, resultIndex, keyIndex, valueIndex);
                            actionValues.Add(this.GetValueBytes(operation));
                            resultETag = this.ConvertValueToETag(actionValues.Last());
                            break;
                    }
                }
                else if (operation.Table is FeedTable)
                {
                    bool trimCheck = false;
                    switch (operation.OperationType)
                    {
                        case OperationType.Insert:
                            condition = string.Format(RedisLuaCommands.FeedInsertCondition, keyIndex, valueIndex, valueIndex + 1, errorCode);
                            conditionValues.Add(this.GetRangeMinItemKey(operation.ItemKey));
                            conditionValues.Add(this.GetRangeMaxItemKey(operation.ItemKey));
                            action = string.Format(RedisLuaCommands.FeedInsertAction, resultIndex, keyIndex, valueIndex + 2);
                            actionValues.Add(this.GetValueBytes(operation));
                            exceptions.Add(new ConflictException(Strings.Conflict, -errorCode - 1, null));
                            resultETag = this.ConvertValueToETag(actionValues.Last());
                            trimCheck = true;
                            break;
                        case OperationType.Delete:
                            if (operation.Entity == null || operation.Entity.ETag == "*")
                            {
                                condition = string.Format(RedisLuaCommands.FeedDeleteCondition, keyIndex, valueIndex, valueIndex + 1, errorCode);
                                conditionValues.Add(this.GetRangeMinItemKey(operation.ItemKey));
                                conditionValues.Add(this.GetRangeMaxItemKey(operation.ItemKey));
                                action = string.Format(RedisLuaCommands.FeedDeleteAction, resultIndex, keyIndex, valueIndex, valueIndex + 1);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                            }
                            else
                            {
                                condition = string.Format(RedisLuaCommands.FeedETagCondition, keyIndex, valueIndex, valueIndex + 1, valueIndex + 2, errorCode);
                                conditionValues.Add(this.GetRangeMinItemKey(operation.ItemKey));
                                conditionValues.Add(this.GetRangeMaxItemKey(operation.ItemKey));
                                conditionValues.Add(this.ConvertETagToValue(operation.Entity.ETag));
                                checkConditionValue = true;
                                action = string.Format(RedisLuaCommands.FeedDeleteAction, resultIndex, keyIndex, valueIndex, valueIndex + 1);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                exceptions.Add(new PreconditionFailedException(Strings.PreconditionFailed, -errorCode - 1, null));
                            }

                            break;
                        case OperationType.DeleteIfExists:
                            action = string.Format(RedisLuaCommands.FeedDeleteAction, resultIndex, keyIndex, valueIndex, valueIndex + 1);
                            actionValues.Add(this.GetRangeMinItemKey(operation.ItemKey));
                            actionValues.Add(this.GetRangeMaxItemKey(operation.ItemKey));
                            break;
                        case OperationType.Replace:
                            if (operation.Entity.ETag == "*")
                            {
                                condition = string.Format(RedisLuaCommands.FeedWildcardReplaceCondition, keyIndex, valueIndex, valueIndex + 1, errorCode);
                                conditionValues.Add(this.GetRangeMinItemKey(operation.ItemKey));
                                conditionValues.Add(this.GetRangeMaxItemKey(operation.ItemKey));
                                action = string.Format(RedisLuaCommands.FeedReplaceAction, resultIndex, keyIndex, valueIndex, valueIndex + 1, valueIndex + 2);
                                actionValues.Add(this.GetValueBytes(operation));
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                resultETag = this.ConvertValueToETag(actionValues.Last());
                            }
                            else
                            {
                                condition = string.Format(RedisLuaCommands.FeedETagCondition, keyIndex, valueIndex, valueIndex + 1, valueIndex + 2, errorCode);
                                conditionValues.Add(this.GetRangeMinItemKey(operation.ItemKey));
                                conditionValues.Add(this.GetRangeMaxItemKey(operation.ItemKey));
                                conditionValues.Add(this.ConvertETagToValue(operation.Entity.ETag));
                                checkConditionValue = true;
                                action = string.Format(RedisLuaCommands.FeedReplaceAction, resultIndex, keyIndex, valueIndex, valueIndex + 1, valueIndex + 3);
                                actionValues.Add(this.GetValueBytes(operation));
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                exceptions.Add(new PreconditionFailedException(Strings.PreconditionFailed, -errorCode - 1, null));
                                resultETag = this.ConvertValueToETag(actionValues.Last());
                            }

                            break;
                        case OperationType.InsertOrReplace:
                            action = string.Format(RedisLuaCommands.FeedInsertOrReplaceAction, resultIndex, keyIndex, valueIndex, valueIndex + 1, valueIndex + 2);
                            actionValues.Add(this.GetRangeMinItemKey(operation.ItemKey));
                            actionValues.Add(this.GetRangeMaxItemKey(operation.ItemKey));
                            actionValues.Add(this.GetValueBytes(operation));
                            resultETag = this.ConvertValueToETag(actionValues.Last());
                            trimCheck = true;
                            break;
                        case OperationType.InsertOrReplaceIfNotLast:
                            action = string.Format(RedisLuaCommands.FeedInsertOrReplaceIfNotLastAction, resultIndex, keyIndex, valueIndex, valueIndex + 1, valueIndex + 2);
                            actionValues.Add(this.GetRangeMinItemKey(operation.ItemKey));
                            actionValues.Add(this.GetRangeMaxItemKey(operation.ItemKey));
                            actionValues.Add(this.GetValueBytes(operation));
                            resultETag = this.ConvertValueToETag(actionValues.Last());
                            trimCheck = true;
                            break;
                        case OperationType.InsertIfNotEmpty:
                            condition = string.Format(RedisLuaCommands.FeedInsertCondition, keyIndex, valueIndex, valueIndex + 1, errorCode);
                            conditionValues.Add(this.GetRangeMinItemKey(operation.ItemKey));
                            conditionValues.Add(this.GetRangeMaxItemKey(operation.ItemKey));
                            action = string.Format(RedisLuaCommands.FeedInsertIfNotEmptyAction, resultIndex, keyIndex, valueIndex + 2);
                            actionValues.Add(this.GetValueBytes(operation));
                            exceptions.Add(new ConflictException(Strings.Conflict, -errorCode - 1, null));
                            resultETag = this.ConvertValueToETag(actionValues.Last());
                            trimCheck = true;
                            break;
                    }

                    if (trimCheck)
                    {
                        FeedTable feedTable = operation.Table as FeedTable;
                        if (feedTable.MaxFeedSizeInCache != int.MaxValue)
                        {
                            if (!feedTrims.ContainsKey(key))
                            {
                                feedTrims.Add(key, feedTable);
                            }
                        }
                    }
                }
                else if (operation.Table is CountTable)
                {
                    switch (operation.OperationType)
                    {
                        case OperationType.Insert:
                            condition = string.Format(RedisLuaCommands.CountInsertCondition, keyIndex, errorCode);
                            actionValues.AddRange(this.GetValueHash(operation.Entity));
                            action = string.Format(RedisLuaCommands.CountInsertAction, resultIndex, keyIndex, script.Values.Count + 1, script.Values.Count + actionValues.Count);
                            exceptions.Add(new ConflictException(Strings.Conflict, -errorCode - 1, null));
                            resultETag = operation.Entity.CustomETag;
                            break;
                        case OperationType.Delete:
                            if (operation.Entity == null || operation.Entity.ETag == "*")
                            {
                                condition = string.Format(RedisLuaCommands.CountDeleteCondition, keyIndex, errorCode);
                                action = string.Format(RedisLuaCommands.CountDeleteAction, resultIndex, keyIndex);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                            }
                            else
                            {
                                condition = string.Format(RedisLuaCommands.CountETagCondition, keyIndex, valueIndex, errorCode);
                                conditionValues.Add(operation.Entity.ETag);
                                checkConditionValue = true;
                                action = string.Format(RedisLuaCommands.CountDeleteAction, resultIndex, keyIndex);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                exceptions.Add(new PreconditionFailedException(Strings.PreconditionFailed, -errorCode - 1, null));
                            }

                            break;
                        case OperationType.DeleteIfExists:
                            action = string.Format(RedisLuaCommands.CountDeleteAction, resultIndex, keyIndex);
                            break;
                        case OperationType.InsertOrReplace:
                            actionValues.AddRange(this.GetValueHash(operation.Entity));
                            action = string.Format(RedisLuaCommands.CountInsertOrReplaceAction, resultIndex, keyIndex, script.Values.Count + 1, script.Values.Count + actionValues.Count);
                            resultETag = operation.Entity.CustomETag;
                            break;
                        case OperationType.Replace:
                            if (operation.Entity.ETag == "*")
                            {
                                condition = string.Format(RedisLuaCommands.CountWildcardReplaceCondition, keyIndex, errorCode);
                                actionValues.AddRange(this.GetValueHash(operation.Entity));
                                action = string.Format(RedisLuaCommands.CountInsertOrReplaceAction, resultIndex, keyIndex, script.Values.Count + 1, script.Values.Count + actionValues.Count);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                resultETag = operation.Entity.CustomETag;
                            }
                            else
                            {
                                condition = string.Format(RedisLuaCommands.CountETagCondition, keyIndex, valueIndex, errorCode);
                                conditionValues.Add(operation.Entity.ETag);
                                checkConditionValue = true;
                                actionValues.AddRange(this.GetValueHash(operation.Entity));
                                action = string.Format(RedisLuaCommands.CountReplaceAction, resultIndex, keyIndex, script.Values.Count + 2, script.Values.Count + actionValues.Count + 1);
                                exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                                exceptions.Add(new PreconditionFailedException(Strings.PreconditionFailed, -errorCode - 1, null));
                                resultETag = operation.Entity.CustomETag;
                            }

                            break;
                        case OperationType.Increment:
                            condition = string.Format(RedisLuaCommands.CountIncrementCondition, keyIndex, errorCode);
                            action = string.Format(RedisLuaCommands.CountIncrementAction, resultIndex, keyIndex, valueIndex, valueIndex + 1);
                            actionValues.Add(SpecialFieldNames.Count);
                            actionValues.Add(operation.Score);
                            exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                            break;
                        case OperationType.InsertOrIncrement:
                            actionValues.AddRange(this.GetValueHash(operation.Entity));
                            action = string.Format(RedisLuaCommands.CountInsertOrIncrementAction, resultIndex, keyIndex, script.Values.Count + 1, script.Values.Count + actionValues.Count, script.Values.Count + actionValues.Count + 1, script.Values.Count + actionValues.Count + 2);
                            actionValues.Add(SpecialFieldNames.Count);
                            actionValues.Add(operation.Score);
                            resultETag = operation.Entity.CustomETag;
                            break;
                    }
                }
                else if (operation.Table is RankFeedTable)
                {
                    bool trimCheck = false;
                    switch (operation.OperationType)
                    {
                        case OperationType.Insert:
                            condition = string.Format(RedisLuaCommands.RankFeedInsertCondition, keyIndex, valueIndex, errorCode);
                            conditionValues.Add(operation.ItemKey);
                            action = string.Format(RedisLuaCommands.RankFeedInsertAction, resultIndex, keyIndex, valueIndex, valueIndex + 1);
                            actionValues.Add(operation.Score);
                            exceptions.Add(new ConflictException(Strings.Conflict, -errorCode - 1, null));
                            trimCheck = true;
                            break;
                        case OperationType.Delete:
                            condition = string.Format(RedisLuaCommands.RankFeedDeleteCondition, keyIndex, valueIndex, errorCode);
                            conditionValues.Add(operation.ItemKey);
                            action = string.Format(RedisLuaCommands.RankFeedDeleteAction, resultIndex, keyIndex, valueIndex);
                            exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                            break;
                        case OperationType.DeleteIfExists:
                            action = string.Format(RedisLuaCommands.RankFeedDeleteAction, resultIndex, keyIndex, valueIndex);
                            actionValues.Add(operation.ItemKey);
                            break;
                        case OperationType.InsertOrReplace:
                            action = string.Format(RedisLuaCommands.RankFeedInsertOrReplaceAction, resultIndex, keyIndex, valueIndex, valueIndex + 1);
                            actionValues.Add(operation.ItemKey);
                            actionValues.Add(operation.Score);
                            trimCheck = true;
                            break;
                        case OperationType.Increment:
                            condition = string.Format(RedisLuaCommands.RankFeedIncrementCondition, keyIndex, valueIndex, errorCode);
                            conditionValues.Add(operation.ItemKey);
                            action = string.Format(RedisLuaCommands.RankFeedIncrementAction, resultIndex, keyIndex, valueIndex, valueIndex + 1);
                            actionValues.Add(operation.Score);
                            exceptions.Add(new NotFoundException(Strings.NotFound, -errorCode - 1, null));
                            break;
                        case OperationType.InsertOrIncrement:
                            action = string.Format(RedisLuaCommands.RankFeedIncrementAction, resultIndex, keyIndex, valueIndex, valueIndex + 1);
                            actionValues.Add(operation.ItemKey);
                            actionValues.Add(operation.Score);
                            trimCheck = true;
                            break;
                    }

                    if (trimCheck)
                    {
                        RankFeedTable rankFeedTable = operation.Table as RankFeedTable;
                        if (rankFeedTable.MaxFeedSizeInCache != int.MaxValue)
                        {
                            if (!feedTrims.ContainsKey(key))
                            {
                                feedTrims.Add(key, rankFeedTable);
                            }
                        }
                    }
                }

                script.Keys.Add(key);
                keyIndex++;

                if (condition != null)
                {
                    script.Conditions.Add(condition);
                }

                if (conditionValues.Count > 0)
                {
                    script.Values.AddRange(conditionValues);
                    valueIndex += conditionValues.Count;
                }
                else
                {
                    if (checkConditionValue)
                    {
                        throw new ArgumentException(string.Format("Operation requires an ETag (which may be the '*' wildcard)."));
                    }
                }

                script.Actions.Add(action);
                if (actionValues.Count > 0)
                {
                    script.Values.AddRange(actionValues);
                    valueIndex += actionValues.Count;
                }

                script.ETags.Add(resultETag);

                if (exceptions.Count > 0)
                {
                    script.Exceptions.Add(errorCode, exceptions);
                }

                errorCode--;
                resultIndex++;
            }

            foreach (string key in feedTrims.Keys)
            {
                Table table = feedTrims[key];
                string actionString = null;
                int maxFeedSizeInCache = int.MaxValue;
                if (table is FeedTable)
                {
                    FeedTable feedTable = table as FeedTable;
                    actionString = RedisLuaCommands.FeedTrimAction;
                    maxFeedSizeInCache = feedTable.MaxFeedSizeInCache;
                }
                else if (table is RankFeedTable)
                {
                    RankFeedTable rankFeedTable = table as RankFeedTable;
                    actionString = rankFeedTable.Order == FeedOrder.Ascending ? RedisLuaCommands.RankFeedAscendingTrimAction : RedisLuaCommands.RankFeedDescendingTrimAction;
                    maxFeedSizeInCache = rankFeedTable.MaxFeedSizeInCache;
                }

                if (actionString != null)
                {
                    script.Actions.Add(string.Format(actionString, keyIndex, valueIndex));
                    script.Keys.Add(key);
                    script.Values.Add(maxFeedSizeInCache);
                    keyIndex++;
                    valueIndex++;
                }
            }

            script.Script = string.Join(
                " ",
                string.Join(" ", script.Conditions),
                RedisLuaCommands.ResultArray,
                string.Join(" ", script.Actions),
                RedisLuaCommands.LuaReturnSuccess);

            return script;
        }

        /// <summary>
        /// Get key for operation
        /// </summary>
        /// <param name="operation">Cache operation</param>
        /// <returns>Key for operation</returns>
        private string GetKey(Operation operation)
        {
            return this.GetKey(operation.Table, operation.PartitionKey, operation.Key);
        }

        /// <summary>
        /// Get key from table, partition key and key
        /// </summary>
        /// <param name="table">Store table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="key">Cache key</param>
        /// <returns>Key for operation</returns>
        private string GetKey(Table table, string partitionKey, string key)
        {
            return string.Join(
                KeySeparator.ToString(),
                table.ContainerInitial + table.TableInitial,
                partitionKey,
                key);
        }

        /// <summary>
        /// Get field names and values as a list of <c>Redis</c> values.
        /// The format is field1, value1, field2, value2.
        /// If the entity has N fields, this method returns 2*N items
        /// </summary>
        /// <param name="entity">Table entity</param>
        /// <returns>List of <c>Redis</c> values containing field names and their values</returns>
        private List<RedisValue> GetValueHash(Entity entity)
        {
            List<RedisValue> redisValues = new List<RedisValue>();
            if (entity.CustomETag != null)
            {
                redisValues.Add(SpecialFieldNames.ETag);
                redisValues.Add(entity.CustomETag);
            }

            if (entity.CacheFlags != CacheFlags.None)
            {
                redisValues.Add(SpecialFieldNames.CacheFlags);
                redisValues.Add((byte)entity.CacheFlags);
            }

            if (entity.CacheInvalid)
            {
                redisValues.Add(SpecialFieldNames.CacheExpiry);
                redisValues.Add(entity.CacheExpiry.ToBinary());
            }

            var properties = entity.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (SpecialFieldNames.IsSpecialFieldName(property.Name, entity.GetType()))
                {
                    continue;
                }

                object value = property.GetValue(entity);
                if (value == null)
                {
                    continue;
                }

                redisValues.Add(property.Name);
                if (property.PropertyType == typeof(string))
                {
                    redisValues.Add((string)value);
                }
                else if (property.PropertyType == typeof(int))
                {
                    redisValues.Add((int)value);
                }
                else if (property.PropertyType == typeof(long))
                {
                    redisValues.Add((long)value);
                }
                else if (property.PropertyType == typeof(double))
                {
                    redisValues.Add((double)value);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    redisValues.Add((bool)value);
                }
                else if (property.PropertyType == typeof(DateTime))
                {
                    redisValues.Add(((DateTime)value).ToBinary());
                }
                else if (property.PropertyType.BaseType == typeof(Enum))
                {
                    redisValues.Add(value.ToString());
                }
                else if (property.PropertyType == typeof(byte[]))
                {
                    redisValues.Add((byte[])value);
                }
                else if (property.PropertyType == typeof(byte))
                {
                    redisValues.Add((byte)value);
                }
            }

            return redisValues;
        }

        /// <summary>
        /// Get entity from hash fields and values
        /// </summary>
        /// <typeparam name="T">Store entity</typeparam>
        /// <param name="fields">Hash fields</param>
        /// <param name="values">Hash values</param>
        /// <returns>Store entity generated from hash values</returns>
        private T GetEntityFromHash<T>(RedisValue[] fields, RedisValue[] values)
            where T : Entity, new()
        {
            Dictionary<string, RedisValue> fieldValues = new Dictionary<string, RedisValue>();
            for (int i = 0; i < fields.Length; i++)
            {
                fieldValues.Add((string)fields[i], values[i]);
            }

            T result = new T();
            if (fieldValues.ContainsKey(SpecialFieldNames.CacheFlags))
            {
                result.CacheFlags = (CacheFlags)(byte)fieldValues[SpecialFieldNames.CacheFlags];
            }

            if (fieldValues.ContainsKey(SpecialFieldNames.CacheExpiry))
            {
                result.CacheExpiry = DateTime.FromBinary((long)fieldValues[SpecialFieldNames.CacheExpiry]);
            }

            var properties = result.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (fieldValues.ContainsKey(property.Name))
                {
                    if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(result, (string)fieldValues[property.Name]);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(result, (int)fieldValues[property.Name]);
                    }
                    else if (property.PropertyType == typeof(long))
                    {
                        property.SetValue(result, (long)fieldValues[property.Name]);
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        property.SetValue(result, (double)fieldValues[property.Name]);
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        property.SetValue(result, (bool)fieldValues[property.Name]);
                    }
                    else if (property.PropertyType == typeof(DateTime))
                    {
                        property.SetValue(result, DateTime.FromBinary((long)fieldValues[property.Name]));
                    }
                    else if (property.PropertyType.BaseType == typeof(Enum))
                    {
                        property.SetValue(result, Enum.Parse(property.PropertyType, (string)fieldValues[property.Name]));
                    }
                    else if (property.PropertyType == typeof(byte[]))
                    {
                        property.SetValue(result, (byte[])fieldValues[property.Name]);
                    }
                    else if (property.PropertyType == typeof(byte))
                    {
                        property.SetValue(result, (byte)fieldValues[property.Name]);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get entity from hash entries
        /// </summary>
        /// <typeparam name="T">Store entity</typeparam>
        /// <param name="hashEntries">Hash entries</param>
        /// <returns>Store entity generated from hash values</returns>
        private T GetEntityFromHash<T>(HashEntry[] hashEntries)
            where T : Entity, new()
        {
            List<RedisValue> fields = new List<RedisValue>();
            List<RedisValue> values = new List<RedisValue>();
            foreach (HashEntry hashEntry in hashEntries)
            {
                fields.Add(hashEntry.Name);
                values.Add(hashEntry.Value);
            }

            return this.GetEntityFromHash<T>(fields.ToArray(), values.ToArray());
        }

        /// <summary>
        /// Get value as byte array
        /// </summary>
        /// <param name="operation">Table operation</param>
        /// <returns>Value as byte array</returns>
        private byte[] GetValueBytes(Operation operation)
        {
            Entity entity = operation.Entity;
            if (entity == null)
            {
                return null;
            }

            byte[] result = null;
            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                if (entity is FeedEntity)
                {
                    FeedEntity feedEntity = entity as FeedEntity;
                    byte[] itemKeyBytes = UTF8Encoding.UTF8.GetBytes(operation.ItemKey);
                    writer.Write(itemKeyBytes);
                    writer.Write((byte)0);
                }

                writer.Write((byte)entity.CacheFlags);
                if (entity.CacheInvalid)
                {
                    writer.Write(entity.CacheExpiry.ToBinary());
                }

                if (!entity.NoETag)
                {
                    writer.Write(entity.CustomETag);
                }

                var properties = entity.GetType().GetProperties();
                var sortedProperties = properties.OrderBy(p => p.Name);
                foreach (var property in sortedProperties)
                {
                    if (SpecialFieldNames.IsSpecialFieldName(property.Name, entity.GetType()))
                    {
                        continue;
                    }

                    object value = property.GetValue(entity);
                    if (property.PropertyType == typeof(string))
                    {
                        writer.Write(value != null);
                        if (value != null)
                        {
                            writer.Write((string)value);
                        }
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        writer.Write((int)value);
                    }
                    else if (property.PropertyType == typeof(long))
                    {
                        writer.Write((long)value);
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        writer.Write((double)value);
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        writer.Write((bool)value);
                    }
                    else if (property.PropertyType == typeof(DateTime))
                    {
                        writer.Write(((DateTime)value).ToBinary());
                    }
                    else if (property.PropertyType.BaseType == typeof(Enum))
                    {
                        writer.Write(value.ToString());
                    }
                    else if (property.PropertyType == typeof(byte[]))
                    {
                        if (value == null)
                        {
                            writer.Write(-1);
                        }
                        else
                        {
                            writer.Write(((byte[])value).Length);
                            writer.Write((byte[])value);
                        }
                    }
                    else if (property.PropertyType == typeof(byte))
                    {
                        writer.Write((byte)value);
                    }
                }
            }

            result = stream.ToArray();
            return result;
        }

        /// <summary>
        /// Get entity from key and value byte array
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="value">Cache value</param>
        /// <returns>Table entity</returns>
        private T GetEntityFromBytes<T>(byte[] value)
            where T : Entity, new()
        {
            if (value == null)
            {
                return null;
            }

            T entity = new T();
            MemoryStream stream = new MemoryStream(value);
            using (BinaryReader reader = new BinaryReader(stream))
            {
                if (typeof(FeedEntity).IsAssignableFrom(typeof(T)))
                {
                    List<byte> itemKeyBytes = new List<byte>();
                    byte itemKeyByte = reader.ReadByte();
                    while (itemKeyByte != 0)
                    {
                        itemKeyBytes.Add(itemKeyByte);
                        itemKeyByte = reader.ReadByte();
                    }

                    if (itemKeyBytes.Count > 0)
                    {
                        ((FeedEntity)(object)entity).ItemKey = UTF8Encoding.UTF8.GetString(itemKeyBytes.ToArray());
                    }
                }

                entity.CacheFlags = (CacheFlags)reader.ReadByte();
                if (entity.CacheInvalid)
                {
                    entity.CacheExpiry = DateTime.FromBinary(reader.ReadInt64());
                }

                if (!entity.NoETag)
                {
                    entity.ETag = reader.ReadString();
                }
                else
                {
                    entity.ETag = this.ConvertValueToETag(value);
                }

                if (entity.CacheInvalid)
                {
                    return entity;
                }

                var properties = entity.GetType().GetProperties();
                var sortedProperties = properties.OrderBy(p => p.Name);

                foreach (var property in sortedProperties)
                {
                    if (SpecialFieldNames.IsSpecialFieldName(property.Name, typeof(T)))
                    {
                        continue;
                    }

                    if (property.PropertyType == typeof(string))
                    {
                        bool notNull = reader.ReadBoolean();
                        if (notNull)
                        {
                            property.SetValue(entity, reader.ReadString());
                        }
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(entity, reader.ReadInt32());
                    }
                    else if (property.PropertyType == typeof(long))
                    {
                        property.SetValue(entity, reader.ReadInt64());
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        property.SetValue(entity, reader.ReadDouble());
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        property.SetValue(entity, reader.ReadBoolean());
                    }
                    else if (property.PropertyType == typeof(DateTime))
                    {
                        property.SetValue(entity, DateTime.FromBinary(reader.ReadInt64()));
                    }
                    else if (property.PropertyType.BaseType == typeof(Enum))
                    {
                        property.SetValue(entity, Enum.Parse(property.PropertyType, reader.ReadString()));
                    }
                    else if (property.PropertyType == typeof(byte[]))
                    {
                        int size = reader.ReadInt32();
                        if (size == 0)
                        {
                            property.SetValue(entity, new byte[0]);
                        }

                        if (size > 0)
                        {
                            property.SetValue(entity, reader.ReadBytes(size));
                        }
                    }
                    else if (property.PropertyType == typeof(byte))
                    {
                        property.SetValue(entity, reader.ReadByte());
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// To query items after the item key.
        /// This method returns the min parameter for the RANGEBY queries.
        /// </summary>
        /// <param name="itemKey">Item key</param>
        /// <returns>Value prefix as byte array</returns>
        private byte[] GetRangeCursorItemKey(string itemKey)
        {
            string nextItemKey = this.GetNextLexString(itemKey);
            byte[] result = null;
            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write('[');
                byte[] itemKeyBytes = UTF8Encoding.UTF8.GetBytes(nextItemKey);
                writer.Write(itemKeyBytes);
                writer.Write((byte)0);
            }

            result = stream.ToArray();
            return result;
        }

        /// <summary>
        /// To query a single item matching a prefix, we need to use RANGEBY sorted set queries.
        /// This method returns the min parameter for the RANGEBY queries.
        /// </summary>
        /// <param name="itemKey">Item key</param>
        /// <returns>Value prefix as byte array</returns>
        private byte[] GetRangeMinItemKey(string itemKey)
        {
            byte[] result = null;
            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write('[');
                byte[] itemKeyBytes = UTF8Encoding.UTF8.GetBytes(itemKey);
                writer.Write(itemKeyBytes);
                writer.Write((byte)0);
            }

            result = stream.ToArray();
            return result;
        }

        /// <summary>
        /// To query a single item matching a prefix, we need to use RANGEBY sorted set queries.
        /// This method returns the max parameter for the RANGEBY queries.
        /// </summary>
        /// <param name="itemKey">Item key</param>
        /// <returns>Value prefix as byte array</returns>
        private byte[] GetRangeMaxItemKey(string itemKey)
        {
            string nextItemKey = this.GetNextLexString(itemKey);
            byte[] result = null;
            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write('(');
                byte[] itemKeyBytes = UTF8Encoding.UTF8.GetBytes(nextItemKey);
                writer.Write(itemKeyBytes);
                writer.Write((byte)0);
            }

            result = stream.ToArray();
            return result;
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
        /// Convert cache value to ETag
        /// </summary>
        /// <param name="value">Cache value</param>
        /// <returns>ETag for cache value</returns>
        private string ConvertValueToETag(byte[] value)
        {
            if (value == null)
            {
                return null;
            }

            char[] chars = new char[value.Length];
            Buffer.BlockCopy(value, 0, chars, 0, value.Length);
            return new string(chars);
        }

        /// <summary>
        /// Convert ETag to cache value
        /// </summary>
        /// <param name="etag">ETag value</param>
        /// <returns>Cache value</returns>
        private byte[] ConvertETagToValue(string etag)
        {
            if (etag == null)
            {
                return null;
            }

            byte[] bytes = new byte[etag.Length];
            Buffer.BlockCopy(etag.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// Parse <c>Redis</c> <c>Lua</c> results
        /// </summary>
        /// <param name="operations">List of operations</param>
        /// <param name="script"><c>Redis</c> <c>Lua</c> script</param>
        /// <param name="redisResults">Results from <c>Redis</c></param>
        /// <returns>Store results</returns>
        private IList<Result> ParseResults(List<Operation> operations, RedisLuaScript script, RedisResult[] redisResults)
        {
            if (redisResults == null || (redisResults.Length != 2 && redisResults.Length != operations.Count + 1))
            {
                throw new UnexpectedException("Redis operation returned unexpected results.", null);
            }

            int resultCode = (int)redisResults[0];
            if (resultCode < 0)
            {
                if (!script.Exceptions.ContainsKey(resultCode))
                {
                    throw new UnexpectedException("Redis operation returned unexpected error code.", null);
                }

                throw script.Exceptions[resultCode][(int)redisResults[1]];
            }

            IList<Result> results = new List<Result>();
            int i = 0;
            foreach (Operation operation in operations)
            {
                Result result = new Result()
                {
                    ETag = script.ETags[i],
                    EntitiesAffected = 1
                };

                RedisResult redisResult = redisResults[i + 1];
                switch (operation.OperationType)
                {
                    case OperationType.DeleteIfExists:
                        if ((int)redisResult == 0)
                        {
                            result.EntitiesAffected = 0;
                        }

                        break;
                    case OperationType.InsertOrReplaceIfNotLast:
                    case OperationType.InsertIfNotEmpty:
                        if ((string)redisResult == RedisResults.NotOK)
                        {
                            result.EntitiesAffected = 0;
                            result.ETag = null;
                        }

                        break;
                    case OperationType.Increment:
                    case OperationType.InsertOrIncrement:
                        if (operation.Table is CountTable)
                        {
                            string valueAndETag = (string)redisResult;
                            string[] splits = valueAndETag.Split(new char[] { ',' });
                            result.Value = Convert.ToDouble(splits[0]);
                            result.ETag = splits[1];
                        }
                        else if (operation.Table is RankFeedTable)
                        {
                            result.Value = (double)redisResult;
                        }

                        break;
                }

                results.Add(result);
                i++;
            }

            return results;
        }

        /// <summary>
        /// Normalize partial object fields. Remove fields that are unnecessary.
        /// Add fields that need to be queried.
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

            // It is important to retrieve ETag with even partial objects.
            // We return ETag for all objects retrieved from the store.
            // A null ETag suggests that the key is not found in RedisCache.
            if (!queryFields.Contains(SpecialFieldNames.ETag))
            {
                queryFields.Add(SpecialFieldNames.ETag);
            }

            return queryFields.ToList();
        }

        /// <summary>
        /// Get rank feed entity
        /// </summary>
        /// <param name="value">Sorted set entry</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="feedKey">Feed key</param>
        /// <param name="cursor">Item cursor</param>
        /// <returns>Rank feed entity</returns>
        private RankFeedEntity GetRankFeedEntity(
            SortedSetEntry value,
            string partitionKey,
            string feedKey,
            string cursor)
        {
            RankFeedEntity entity = new RankFeedEntity()
            {
                PartitionKey = partitionKey,
                FeedKey = feedKey,
                ItemKey = value.Element,
                Score = value.Score,
                Cursor = cursor
            };

            return entity;
        }
    }
}
