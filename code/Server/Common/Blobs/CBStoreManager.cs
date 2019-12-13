// <copyright file="CBStoreManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Blobs
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using SocialPlus.Server.CBStore;

    /// <summary>
    /// CB store manager
    /// </summary>
    public class CBStoreManager : ICBStoreManager
    {
        /// <summary>
        /// Cached store objects
        /// </summary>
        private static ConcurrentDictionary<string, CBStore> cachedStoreObjects = new ConcurrentDictionary<string, CBStore>();

        /// <summary>
        /// Connection string provider
        /// </summary>
        private readonly IConnectionStringProvider connectionStringProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CBStoreManager"/> class.
        /// </summary>
        /// <param name="connectionStringProvider">connection string provider</param>
        public CBStoreManager(IConnectionStringProvider connectionStringProvider)
        {
            this.connectionStringProvider = connectionStringProvider;
        }

        /// <summary>
        /// Get store from container identifier
        /// </summary>
        /// <param name="containerIdentifier">Container identifier</param>
        /// <returns>CB store</returns>
        public async Task<CBStore> GetStore(ContainerIdentifier containerIdentifier)
        {
            ContainerDescriptor containerDescriptor = ContainerDescriptorProvider.Containers[containerIdentifier];
            string azureStorageConnectionString = await this.connectionStringProvider.GetBlobsAzureStorageConnectionString(containerDescriptor.AzureStorageInstanceType);
            string azureCdnUrl = await this.connectionStringProvider.GetAzureCdnUrl(containerDescriptor.AzureCdnInstanceType);
            string uniqueStoreIdentity = string.Join(":", azureStorageConnectionString, azureCdnUrl);

            // cachedStoreObjects is a thread-safe dictionary (ConcurrentDictionary). If uniqueStoreIdentity is not present
            // in cachedStoreObjects, try adding it. Since GetStore can be called concurrently by
            // different threads, it is possible for two (or more) threads to attempt inserting uniqueStoreIdentity
            // concurrently in the cachedStoreObjects. That's ok, because the call to TryAdd is guaranteed to be thread-safe.
            // One of the threads will not be able to insert (i.e., TryAdd will return false), but the code will happily execute
            // and fall through to the return statement.
            // This code makes no use of locking on the common path (i.e., reads of cachedStoreObjects).
            if (!cachedStoreObjects.ContainsKey(uniqueStoreIdentity))
            {
                AzureBlobStorage azureBlobStorage = new AzureBlobStorage(azureStorageConnectionString);
                azureBlobStorage.BlobRequestOptions = AzureStorageConfiguration.GetBlobRequestOptions();
                AzureCdn azureCdn = new AzureCdn(azureCdnUrl);

                CBStore store = new CBStore(azureBlobStorage, azureCdn);
                cachedStoreObjects.TryAdd(uniqueStoreIdentity, store);
            }

            return cachedStoreObjects[uniqueStoreIdentity];
        }

        /// <summary>
        /// Get container name for container identifier
        /// </summary>
        /// <param name="containerIdentifier">Container identifier</param>
        /// <returns>Container name</returns>
        public string GetContainerName(ContainerIdentifier containerIdentifier)
        {
            ContainerDescriptor containerDescriptor = ContainerDescriptorProvider.Containers[containerIdentifier];
            return containerDescriptor.ContainerName;
        }
    }
}
