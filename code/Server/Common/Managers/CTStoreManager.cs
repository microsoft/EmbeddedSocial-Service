// <copyright file="CTStoreManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// CT store manager
    /// </summary>
    public class CTStoreManager : ICTStoreManager
    {
        /// <summary>
        /// Connection string provider
        /// </summary>
        private readonly IConnectionStringProvider connectionStringProvider;

        /// <summary>
        /// Cached store objects
        /// </summary>
        private readonly ConcurrentDictionary<string, CTStore> cachedStoreObjects = new ConcurrentDictionary<string, CTStore>();

        /// <summary>
        /// flag used to indicate if the CTStore Manager has been initialized (i.e. has checked the storage version numbers).
        /// </summary>
        /// <remarks>
        /// This flag is used for synchronization. Marking it volatile is important because it makes it not subject to
        /// compiler optimizations that assume access by a single thread. This ensures the flag has
        /// the most up-to-date value present at all times.
        /// </remarks>
        private volatile bool initialized = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CTStoreManager"/> class.
        /// </summary>
        /// <param name="connectionStringProvider">connection string provider</param>
        public CTStoreManager(IConnectionStringProvider connectionStringProvider)
        {
            this.connectionStringProvider = connectionStringProvider;
        }

        /// <summary>
        /// Gets the key used as the partition key and object key for storing the social plus storage version number.
        /// </summary>
        public string StoreVersionKey { get; } = "SocialPlusStoreVersion";

        /// <summary>
        /// Gets the version string represents the version number of the data stored by the social plus service.
        /// Each time there is an incompatible change to our storage representation, we must bump the version number
        /// here and write code to perform format conversion.
        ///
        /// Note that adding a new table does NOT require incrementing the version number, because this will not cause
        /// a newer server to misinterpret data created by an older server.
        /// </summary>
        public string StoreVersionString { get; } = "1.1";

        /// <summary>
        /// Gets the initials for the service config container
        /// </summary>
        public string ServiceConfigContainerInitials { get; } = "SC";

        /// <summary>
        /// Gets the table name for the storage version table
        /// </summary>
        public string StoreVersionTableName { get; } = "StoreVersion";

        /// <summary>
        /// Gets the table initials for the storage version table
        /// </summary>
        public string StoreVersionTableInitials { get; } = "V";

        /// <summary>
        /// Gets the default count key for tables that don't need a unique count key. CTStore requires a non-null count key.
        /// </summary>
        public string DefaultCountKey { get; } = "c";

        /// <summary>
        /// Gets the default feed key for tables that don't need a unique feed key. CTStore requires a non-null feed key.
        /// </summary>
        public string DefaultFeedKey { get; } = "f";

        /// <summary>
        /// Gets the default object key for tables that don't need a unique object key. CTStore requires a non-null object key.
        /// </summary>
        public string DefaultObjectKey { get; } = "o";

        /// <summary>
        /// Initialization routine performs a version check for each storage component: the table store, persistent redis, and volatile redis.
        /// Fails if the version numbers don't match for any of the stores.
        /// </summary>
        /// <returns>true if the version checks pass</returns>
        public async Task<bool> Initialize()
        {
            string azureStorageConnectionString = await this.connectionStringProvider.GetTablesAzureStorageConnectionString(AzureStorageInstanceType.Default);
            AzureTableStorage azureTableStorage = new AzureTableStorage(azureStorageConnectionString);

            string redisConnectionStringPersistent = await this.connectionStringProvider.GetRedisConnectionString(RedisInstanceType.Persistent);
            RedisCache redisCachePersistent = new RedisCache(redisConnectionStringPersistent);

            CTStore tableStore = new CTStore(azureTableStorage, null);
            CTStore persistentCacheStore = new CTStore(null, redisCachePersistent);

            ObjectTable versionTable = Table.GetObjectTable(
                ContainerIdentifier.ServiceConfig.ToString(),
                this.ServiceConfigContainerInitials,
                this.StoreVersionTableName,
                this.StoreVersionTableInitials,
                StorageMode.PersistentOnly);
            var tableVersion = await tableStore.QueryObjectAsync<StoreVersionEntity>(versionTable, this.StoreVersionKey, this.StoreVersionKey);

            // if version table was not found or the version number is not current, return false indicating checks don't pass
            if (tableVersion == null || tableVersion.Version != this.StoreVersionString)
            {
                return false;
            }

            versionTable = Table.GetObjectTable(
                ContainerIdentifier.ServiceConfig.ToString(),
                this.ServiceConfigContainerInitials,
                this.StoreVersionTableName,
                this.StoreVersionTableInitials,
                StorageMode.CacheOnly);
            var persistentCacheVersion = await persistentCacheStore.QueryObjectAsync<StoreVersionEntity>(versionTable, this.StoreVersionKey, this.StoreVersionKey);

            // check version for persistent redis cache
            if (persistentCacheVersion.Version != this.StoreVersionString)
            {
                return false;
            }

            this.initialized = true;
            return true;
        }

        /// <summary>
        /// Get store from container identifier
        /// </summary>
        /// <param name="containerIdentifier">Container identifier</param>
        /// <returns>CT store</returns>
        public async Task<CTStore> GetStore(ContainerIdentifier containerIdentifier)
        {
            // refuse to provide the store if the version check fails, or if it has not been executed
            if (this.initialized == false)
            {
                return null;
            }

            ContainerDescriptor containerDescriptor = ContainerTableDescriptorProvider.Containers[containerIdentifier];
            string azureStorageConnectionString = await this.connectionStringProvider.GetTablesAzureStorageConnectionString(containerDescriptor.AzureStorageInstanceType);
            string redisConnectionString = await this.connectionStringProvider.GetRedisConnectionString(containerDescriptor.RedisInstanceType);
            string uniqueStoreIdentity = string.Join(":", azureStorageConnectionString, redisConnectionString);

            // cachedStoreObjects is a thread-safe dictionary (ConcurrentDictionary). If uniqueStoreIdentity is not present
            // in cachedStoreObects, try adding it. Since GetStore can be called concurrently by
            // different threads, it is possible for two (or more) threads to attempt inserting uniqueStoreIdentity
            // concurrently in the cachedStoreObjects. That's ok, because the call to TryAdd is guaranteed to be thread-safe.
            // One of the threads will not be able to insert (i.e., TryAdd will return false), but the code will happily execute
            // and fall through to the return statement.
            // This code makes no use of locking on the common path (i.e., reads of cachedStoreObjects).
            if (!this.cachedStoreObjects.ContainsKey(uniqueStoreIdentity))
            {
                AzureTableStorage azureTableStorage = new AzureTableStorage(azureStorageConnectionString);
                azureTableStorage.TableRequestOptions = AzureStorageConfiguration.GetTableRequestOptions();
                RedisCache redisCache = new RedisCache(redisConnectionString);

                CTStore store = new CTStore(azureTableStorage, redisCache);
                this.cachedStoreObjects.TryAdd(uniqueStoreIdentity, store);
            }

            return this.cachedStoreObjects[uniqueStoreIdentity];
        }

        /// <summary>
        /// Get table from container identifier and table identifier
        /// </summary>
        /// <param name="containerIdentifier">Container identifier</param>
        /// <param name="tableIdentifier">Table identifier</param>
        /// <returns>Store table</returns>
        public Table GetTable(ContainerIdentifier containerIdentifier, TableIdentifier tableIdentifier)
        {
            // refuse to provide the table if the version check fails, or if it has not been executed
            if (this.initialized == false)
            {
                return null;
            }

            ContainerDescriptor containerDescriptor = ContainerTableDescriptorProvider.Containers[containerIdentifier];
            TableDescriptor tableDescriptor = containerDescriptor.Tables[tableIdentifier];
            if (tableDescriptor.TableType == TableType.Object)
            {
                return Table.GetObjectTable(
                    containerDescriptor.ContainerName,
                    containerDescriptor.ContainerInitial,
                    tableDescriptor.TableName,
                    tableDescriptor.TableInitial,
                    tableDescriptor.StorageMode);
            }

            if (tableDescriptor.TableType == TableType.Count)
            {
                return Table.GetCountTable(
                    containerDescriptor.ContainerName,
                    containerDescriptor.ContainerInitial,
                    tableDescriptor.TableName,
                    tableDescriptor.TableInitial,
                    tableDescriptor.StorageMode);
            }

            if (tableDescriptor.TableType == TableType.Feed)
            {
                return Table.GetFeedTable(
                    containerDescriptor.ContainerName,
                    containerDescriptor.ContainerInitial,
                    tableDescriptor.TableName,
                    tableDescriptor.TableInitial,
                    tableDescriptor.StorageMode,
                    tableDescriptor.MaxFeedSizeInCache);
            }

            if (tableDescriptor.TableType == TableType.RankFeed)
            {
                return Table.GetRankFeedTable(
                    containerDescriptor.ContainerName,
                    containerDescriptor.ContainerInitial,
                    tableDescriptor.TableName,
                    tableDescriptor.TableInitial,
                    tableDescriptor.StorageMode,
                    tableDescriptor.MaxFeedSizeInCache);
            }

            return null;
        }
    }
}
