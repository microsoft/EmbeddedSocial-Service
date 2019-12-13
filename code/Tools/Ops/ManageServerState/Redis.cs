// <copyright file="Redis.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageServerState
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Tables;
    using StackExchange.Redis;

    /// <summary>
    /// portion of Program class that deals with deleting Redis caches
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// Delete the contents of the redis caches
        /// </summary>
        /// <param name="volatileCacheConnectionString">connection string for volatile cache</param>
        /// <param name="persistentCacheConnectionString">connection string for persistent cache</param>
        private static void DeleteRedisCaches(string volatileCacheConnectionString, string persistentCacheConnectionString)
        {
            Console.WriteLine("Deleting Redis Caches...");
            var conn = ConnectionMultiplexer.Connect(volatileCacheConnectionString);
            var ep = conn.GetEndPoints();
            if (ep.Length == 1)
            {
                var redisServer = conn.GetServer(ep[0]);
                redisServer.FlushAllDatabases();
                Console.WriteLine("  Redis Volatile Cache - Deleted");
            }
            else
            {
                Console.WriteLine("Fatal error: expected 1 redis cache endpoint, found {0}", ep.Length);
                Console.WriteLine("Redis Cache contents were not deleted");
                return;
            }

            conn = ConnectionMultiplexer.Connect(persistentCacheConnectionString);
            ep = conn.GetEndPoints();
            if (ep.Length == 1)
            {
                var redisServer = conn.GetServer(ep[0]);
                redisServer.FlushAllDatabases();
                Console.WriteLine("  Redis Persistent Cache - Deleted");
            }
            else
            {
                Console.WriteLine("Fatal error: expected 1 redis cache endpoint, found {0}", ep.Length);
                Console.WriteLine("Redis Cache contents were not deleted");
                return;
            }
        }

        /// <summary>
        /// Provision the contents of the redis caches
        /// </summary>
        /// <param name="persistentCacheConnectionString">connection string for persistent cache</param>
        /// <param name="tableStoreManager">table store manager</param>
        /// <returns>provisioning task</returns>
        private static async Task ProvisionRedisCaches(string persistentCacheConnectionString, CTStoreManager tableStoreManager)
        {
            Console.WriteLine("Provisioning Redis Caches...");

            // insert the store version number into table storage
            var versionEntity = new StoreVersionEntity { Version = tableStoreManager.StoreVersionString };

            ObjectTable versionTable = Table.GetObjectTable(
                ContainerIdentifier.ServiceConfig.ToString(),
                tableStoreManager.ServiceConfigContainerInitials,
                tableStoreManager.StoreVersionTableName,
                tableStoreManager.StoreVersionTableInitials,
                StorageMode.CacheOnly);
            var operation = Operation.Insert(versionTable, tableStoreManager.StoreVersionKey, tableStoreManager.StoreVersionKey, versionEntity);

            RedisCache redisCachePersistent = new RedisCache(persistentCacheConnectionString);
            CTStore persistentCache = new CTStore(null, redisCachePersistent);

            // perform the insert operation on persistent redis
            // note that we only insert the version number into persistent redis,
            // because with volatile redis each time the cache restarts the version number will be lost
            await persistentCache.ExecuteOperationAsync(operation, ConsistencyMode.Strong);
        }

        /// <summary>
        /// Upgrade the store version number in persistent redis
        /// </summary>
        /// <param name="ctStoreMananger">ctstore manager</param>
        /// <param name="persistentCacheConnectionString">connection string for persistent cache</param>
        /// <returns>true if upgrade is successful</returns>
        private static async Task<bool> UpgradeStoreVersionRedis(CTStoreManager ctStoreMananger, string persistentCacheConnectionString)
        {
            RedisCache redisCachePersistent = new RedisCache(persistentCacheConnectionString);
            CTStore persistentCacheStore = new CTStore(null, redisCachePersistent);

            ObjectTable versionTable = Table.GetObjectTable(
                ContainerIdentifier.ServiceConfig.ToString(),
                ctStoreMananger.ServiceConfigContainerInitials,
                ctStoreMananger.StoreVersionTableName,
                ctStoreMananger.StoreVersionTableInitials,
                StorageMode.CacheOnly);

            var persistentCacheVersion = await persistentCacheStore.QueryObjectAsync<StoreVersionEntity>(versionTable, ctStoreMananger.StoreVersionKey, ctStoreMananger.StoreVersionKey);

            // if version in store does not match oldVersion, then refuse to upgrade
            if (persistentCacheVersion.Version != oldVersion)
            {
                Console.WriteLine("Version mismatch in persistent Redis: original version in store is {0}", persistentCacheVersion.Version);
                return false;
            }

            persistentCacheVersion.Version = newVersion;
            var operation = Operation.Replace(versionTable, ctStoreMananger.StoreVersionKey, ctStoreMananger.StoreVersionKey, persistentCacheVersion);

            // perform the insert operation on persistent redis
            await persistentCacheStore.ExecuteOperationAsync(operation, ConsistencyMode.Strong);

            return true;
        }
    }
}
