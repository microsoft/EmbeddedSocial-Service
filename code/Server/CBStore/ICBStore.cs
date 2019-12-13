// <copyright file="ICBStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.CBStore
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Cached blob store interface
    /// </summary>
    public interface ICBStore
    {
        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True on success</returns>
        Task<bool> CreateContainerAsync(string containerName);

        /// <summary>
        /// Delete container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True on success</returns>
        Task<bool> DeleteContainerAsync(string containerName);

        /// <summary>
        /// Create blob async
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="stream">Blob stream</param>
        /// <param name="contentType">Content type</param>
        /// <param name="cacheTTL">Cache time to live</param>
        /// <returns>Create blob task</returns>
        Task CreateBlobAsync(string containerName, string blobName, Stream stream, string contentType, TimeSpan cacheTTL);

        /// <summary>
        /// Delete blob async
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Delete blob task</returns>
        Task DeleteBlobAsync(string containerName, string blobName);

        /// <summary>
        /// Query blob async
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Blob stream</returns>
        Task<Blob> QueryBlobAsync(string containerName, string blobName);

        /// <summary>
        /// Check if container exits
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>A value indicating whether container exists</returns>
        Task<bool> ContainerExists(string containerName);

        /// <summary>
        /// Check if blob exits
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>A value indicating whether blob exists</returns>
        Task<bool> BlobExists(string containerName, string blobName);

        /// <summary>
        /// Query persistent url
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Persistent url</returns>
        Uri QueryPersistentUrl(string containerName, string blobName);

        /// <summary>
        /// Query CDN url
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>CDN url</returns>
        Uri QueryCdnUrl(string containerName, string blobName);
    }
}
