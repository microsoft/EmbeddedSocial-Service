// <copyright file="CBStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.CBStore
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Cached blob store class
    /// </summary>
    public class CBStore : ICBStore
    {
        /// <summary>
        /// Persistent store for CB store
        /// </summary>
        private IPersistentStore persistentStore;

        /// <summary>
        /// CDN for CT store
        /// </summary>
        private ICdn cdn;

        /// <summary>
        /// Initializes a new instance of the <see cref="CBStore"/> class
        /// </summary>
        /// <param name="persistentStore">Persistent store for CB store</param>
        /// <param name="cdn">CDN for CB store</param>
        public CBStore(IPersistentStore persistentStore, ICdn cdn)
        {
            this.persistentStore = persistentStore;
            this.cdn = cdn;
        }

        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True on success</returns>
        public async Task<bool> CreateContainerAsync(string containerName)
        {
            return await this.persistentStore.CreateContainerAsync(containerName);
        }

        /// <summary>
        /// Delete container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True on success</returns>
        public async Task<bool> DeleteContainerAsync(string containerName)
        {
            return await this.persistentStore.DeleteContainerAsync(containerName);
        }

        /// <summary>
        /// Create blob async
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="stream">Blob stream</param>
        /// <param name="contentType">Content type</param>
        /// <param name="cacheTTL">Cache time to live</param>
        /// <returns>Create blob task</returns>
        public async Task CreateBlobAsync(string containerName, string blobName, Stream stream, string contentType, TimeSpan cacheTTL)
        {
            await this.persistentStore.CreateBlobAsync(containerName, blobName, stream, contentType, cacheTTL);
        }

        /// <summary>
        /// Delete blob async
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Delete blob task</returns>
        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            await this.persistentStore.DeleteBlobAsync(containerName, blobName);
        }

        /// <summary>
        /// Query blob async
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Blob stream</returns>
        public async Task<Blob> QueryBlobAsync(string containerName, string blobName)
        {
            return await this.persistentStore.QueryBlobAsync(containerName, blobName);
        }

        /// <summary>
        /// Check if container exits
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>A value indicating whether container exists</returns>
        public async Task<bool> ContainerExists(string containerName)
        {
            return await this.persistentStore.ContainerExists(containerName);
        }

        /// <summary>
        /// Check if blob exits
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>A value indicating whether blob exists</returns>
        public async Task<bool> BlobExists(string containerName, string blobName)
        {
            return await this.persistentStore.BlobExists(containerName, blobName);
        }

        /// <summary>
        /// Query persistent url
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Persistent url</returns>
        public Uri QueryPersistentUrl(string containerName, string blobName)
        {
            return this.persistentStore.QueryPersistentUrl(containerName, blobName);
        }

        /// <summary>
        /// Query CDN url
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>CDN url</returns>
        public Uri QueryCdnUrl(string containerName, string blobName)
        {
            return this.cdn.QueryCdnUrl(containerName, blobName);
        }
    }
}
