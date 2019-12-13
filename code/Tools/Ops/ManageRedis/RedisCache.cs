// <copyright file="RedisCache.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageRedis
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using StackExchange.Redis;

    /// <summary>
    /// <c>Redis</c> cache
    /// </summary>
    public class RedisCache
    {
        /// <summary>
        /// Lazy <c>Redis</c> Connection Multiplexer
        /// </summary>
        private readonly Lazy<ConnectionMultiplexer> lazyConnection;

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
        /// Reads an object from Redis
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>list of key value pairs for the object properties</returns>
        public async Task<List<KeyValuePair<string, string>>> QueryObjectAsync(string key)
        {
            var kvpList = new List<KeyValuePair<string, string>>();
            HashEntry[] hashEntries = await this.Cache.HashGetAllAsync(key);
            List<RedisValue> fields = new List<RedisValue>();
            List<RedisValue> values = new List<RedisValue>();
            foreach (HashEntry hashEntry in hashEntries)
            {
                var kvp = new KeyValuePair<string, string>(hashEntry.Name, hashEntry.Value);
                kvpList.Add(kvp);
            }

            return kvpList;
        }

        /// <summary>
        /// Deletes an object from Redis
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>boolean indicating success or failure</returns>
        public async Task<bool> DeleteObjectAsync(string key)
        {
            string script;
            RedisKey[] keys = new RedisKey[1];
            RedisResult[] redisResults = null;

            string objectDeleteCondition = "if redis.call('exists', KEYS[1]) == 0 then return {-1, 0} end";
            string resultArray = "local result={1}";
            string objectDeleteAction = "result[2] = redis.call('del', KEYS[1])";
            string final = "return result";
            script = string.Join(" ", objectDeleteCondition, resultArray, objectDeleteAction, final);
            keys[0] = key;
            redisResults = (RedisResult[])await this.Cache.ScriptEvaluateAsync(script, keys);

            if (redisResults == null)
            {
                return false;
            }

            if (redisResults.Length != 2)
            {
                return false;
            }

            int resultCode1 = (int)redisResults[0];
            int resultCode2 = (int)redisResults[1];
            if (resultCode1 == 1 && resultCode2 == 1)
            {
                // return true if the condition and the action are both successful
                return true;
            }

            return false;
        }

        /// <summary>
        /// queries a rank feed from redis
        /// </summary>
        /// <param name="key">rank feed key</param>
        /// <param name="limit">limit on number of items to fetch</param>
        /// <param name="order">ordering of the feed (descending is the default)</param>
        /// <returns>list of sorted set entries</returns>
        public async Task<IList<SortedSetEntry>> QueryRankFeedAsync(string key, long limit, Order order = Order.Descending)
        {
            long startRank = 0;
            SortedSetEntry[] values = await this.Cache.SortedSetRangeByRankWithScoresAsync(key, startRank, startRank + limit - 1, order);
            var results = new List<SortedSetEntry>();
            foreach (SortedSetEntry value in values)
            {
                results.Add(value);
            }

            return results;
        }

        /// <summary>
        ///  Query rank feed length
        /// </summary>
        /// <param name="key">rank feed key</param>
        /// <returns>length of the feed</returns>
        public async Task<long> QueryRankFeedLengthAsync(string key)
        {
            return await this.Cache.SortedSetLengthAsync(key);
        }

        /// <summary>
        /// Get info from server
        /// </summary>
        /// <returns>string containing info result</returns>
        public async Task<string> GetInfoAsync()
        {
            var endpoints = this.Connection.GetEndPoints();

            // currently we are not using redis replication, we are running with a single redis server
            // so we expect the number of endpoints to always be 1
            if (endpoints.Length != 1)
            {
                return string.Format($"Connection.GetEndPoints() returned more than one endpoint: endpoints.Length = {endpoints.Length}");
            }

            var server = this.Connection.GetServer(endpoints[0]);
            return await server.InfoRawAsync();
        }
    }
}
