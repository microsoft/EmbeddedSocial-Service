// <copyright file="Actions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageRedis
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Server.Tables;
    using StackExchange.Redis;

    /// <summary>
    /// Implements manual control of operations on redis for a store implemented with CTstore.
    /// </summary>
    /// <remarks>Currently only get and delete are supported.  Need to implement insert.</remarks>
    public class Actions
    {
        /// <summary>
        /// redis cache
        /// </summary>
        private readonly RedisCache redisCache;

        /// <summary>
        /// container descriptor
        /// </summary>
        private readonly ContainerDescriptor container;

        /// <summary>
        /// table descriptor
        /// </summary>
        private readonly TableDescriptor table;

        /// <summary>
        /// Initializes a new instance of the <see cref="Actions"/> class.
        /// </summary>
        /// <param name="cache">redis cache</param>
        /// <param name="container">valid container</param>
        /// <param name="table">valid table</param>
        public Actions(RedisCache cache, ContainerDescriptor container, TableDescriptor table)
        {
            this.redisCache = cache;
            this.container = container;
            this.table = table;
        }

        /// <summary>
        /// Get an object from Redis
        /// </summary>
        /// <param name="partitionkey">partition key</param>
        /// <param name="objectkey">object key</param>
        /// <returns>get object task</returns>
        public async Task GetObject(string partitionkey, string objectkey)
        {
            string cacheKey = this.ConstructCacheKey(partitionkey, objectkey);
            Console.WriteLine($"Looking up object with key {cacheKey} in Redis");
            var resultList = await this.redisCache.QueryObjectAsync(cacheKey);
            if (resultList.Count == 0)
            {
                Console.WriteLine("Object not found.");
            }

            foreach (var element in resultList)
            {
                Console.WriteLine($"{element.Key}:{element.Value}");
            }
        }

        /// <summary>
        /// Delete an object from Redis
        /// </summary>
        /// <param name="partitionkey">partition key</param>
        /// <param name="objectkey">object key</param>
        /// <returns>delete object task</returns>
        public async Task DeleteObject(string partitionkey, string objectkey)
        {
            string cacheKey = this.ConstructCacheKey(partitionkey, objectkey);
            Console.WriteLine($"Deleting object with key {cacheKey} from Redis");
            var result = await this.redisCache.DeleteObjectAsync(cacheKey);
            if (result == true)
            {
                Console.WriteLine("Object deleted.");
            }
            else
            {
                Console.WriteLine("Object deletion failed - object not found.");
            }
        }

        /// <summary>
        /// Get the contents of a rank feed from Redis
        /// </summary>
        /// <param name="partitionkey">parition key</param>
        /// <param name="feedkey">feed key</param>
        /// <param name="order">ordering of rank feed</param>
        /// <returns>get rank feed task</returns>
        public async Task GetRankFeed(string partitionkey, string feedkey, Order order)
        {
            string cacheKey = this.ConstructCacheKey(partitionkey, feedkey);
            Console.WriteLine($"Fetching rank feed with key {cacheKey} from Redis");
            var count = await this.redisCache.QueryRankFeedLengthAsync(cacheKey);
            Console.WriteLine($"Length of rank feed = {count}");
            var result = await this.redisCache.QueryRankFeedAsync(cacheKey, count, order);
            foreach (var entry in result)
            {
                Console.WriteLine($"Item score = {entry.Score}, value = {entry.Element}");
            }
        }

        /// <summary>
        /// Gets info from the redis server
        /// </summary>
        /// <returns>get info task</returns>
        public async Task GetInfo()
        {
            string info = await this.redisCache.GetInfoAsync();
            Console.WriteLine(info);
        }

        /// <summary>
        /// Creates a key to find the item in the cache from the partition key and object key.
        /// Uses the same formatting convention that CTStore uses for Redis.
        /// </summary>
        /// <param name="partitionkey">partition key</param>
        /// <param name="objectkey">object key</param>
        /// <returns>string containing cache key</returns>
        private string ConstructCacheKey(string partitionkey, string objectkey)
        {
            return string.Join(":", this.container.ContainerInitial + this.table.TableInitial, partitionkey, objectkey);
        }
    }
}
