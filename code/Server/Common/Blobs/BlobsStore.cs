// <copyright file="BlobsStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Blobs
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using SocialPlus.Server.CBStore;

    /// <summary>
    /// Blobs store interface
    /// </summary>
    public class BlobsStore : IBlobsStore
    {
        /// <summary>
        /// CBStore manager
        /// </summary>
        private readonly ICBStoreManager blobStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobsStore"/> class
        /// </summary>
        /// <param name="blobStoreManager">cached blob store manager</param>
        public BlobsStore(ICBStoreManager blobStoreManager)
        {
            this.blobStoreManager = blobStoreManager;
        }

        /// <summary>
        /// Insert blob in blobs store
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="stream">Blob stream</param>
        /// <param name="contentType">Content type</param>
        /// <param name="cacheTTL">Time to live in cache</param>
        /// <returns>Insert blob task</returns>
        public async Task InsertBlob(string blobHandle, Stream stream, string contentType, TimeSpan cacheTTL)
        {
            CBStore store = await this.blobStoreManager.GetStore(ContainerIdentifier.Blobs);
            string containerName = this.blobStoreManager.GetContainerName(ContainerIdentifier.Blobs);
            await store.CreateBlobAsync(containerName, blobHandle, stream, contentType, cacheTTL);
        }

        /// <summary>
        /// Delete blob from blobs store
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Delete blob task</returns>
        public async Task DeleteBlob(string blobHandle)
        {
            CBStore store = await this.blobStoreManager.GetStore(ContainerIdentifier.Blobs);
            string containerName = this.blobStoreManager.GetContainerName(ContainerIdentifier.Blobs);
            await store.DeleteBlobAsync(containerName, blobHandle);
        }

        /// <summary>
        /// Query blob
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Blob item</returns>
        public async Task<IBlobItem> QueryBlob(string blobHandle)
        {
            CBStore store = await this.blobStoreManager.GetStore(ContainerIdentifier.Blobs);
            string containerName = this.blobStoreManager.GetContainerName(ContainerIdentifier.Blobs);
            Blob blob = await store.QueryBlobAsync(containerName, blobHandle);
            return new BlobItem()
            {
                Stream = blob.Stream,
                ContentType = blob.ContentType
            };
        }

        /// <summary>
        /// Insert image in blobs store
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="stream">Blob stream</param>
        /// <param name="contentType">Content type</param>
        /// <param name="cacheTTL">Time to live in cache</param>
        /// <returns>Insert image task</returns>
        public async Task InsertImage(string blobHandle, Stream stream, string contentType, TimeSpan cacheTTL)
        {
            CBStore store = await this.blobStoreManager.GetStore(ContainerIdentifier.Images);
            string containerName = this.blobStoreManager.GetContainerName(ContainerIdentifier.Images);
            await store.CreateBlobAsync(containerName, blobHandle, stream, contentType, cacheTTL);
        }

        /// <summary>
        /// Delete image from blobs store
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Delete image task</returns>
        public async Task DeleteImage(string blobHandle)
        {
            CBStore store = await this.blobStoreManager.GetStore(ContainerIdentifier.Images);
            string containerName = this.blobStoreManager.GetContainerName(ContainerIdentifier.Images);
            await store.DeleteBlobAsync(containerName, blobHandle);
        }

        /// <summary>
        /// Query image
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Blob item</returns>
        public async Task<IBlobItem> QueryImage(string blobHandle)
        {
            CBStore store = await this.blobStoreManager.GetStore(ContainerIdentifier.Images);
            string containerName = this.blobStoreManager.GetContainerName(ContainerIdentifier.Images);
            Blob blob = await store.QueryBlobAsync(containerName, blobHandle);
            return new BlobItem()
            {
                Stream = blob.Stream,
                ContentType = blob.ContentType
            };
        }

        /// <summary>
        /// Check if blob exists
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>A value indicating whether the blob exists</returns>
        public async Task<bool> BlobExists(string blobHandle)
        {
            CBStore store = await this.blobStoreManager.GetStore(ContainerIdentifier.Blobs);
            string containerName = this.blobStoreManager.GetContainerName(ContainerIdentifier.Blobs);
            return await store.BlobExists(containerName, blobHandle);
        }

        /// <summary>
        /// Check if image exists
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>A value indicating whether the image exists</returns>
        public async Task<bool> ImageExists(string blobHandle)
        {
            CBStore store = await this.blobStoreManager.GetStore(ContainerIdentifier.Images);
            string containerName = this.blobStoreManager.GetContainerName(ContainerIdentifier.Images);
            return await store.BlobExists(containerName, blobHandle);
        }

        /// <summary>
        /// Query CDN url for blob
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>CDN url</returns>
        public async Task<Uri> QueryBlobCdnUrl(string blobHandle)
        {
            // return null if input is null
            if (string.IsNullOrWhiteSpace(blobHandle))
            {
                return null;
            }

            CBStore store = await this.blobStoreManager.GetStore(ContainerIdentifier.Blobs);
            string containerName = this.blobStoreManager.GetContainerName(ContainerIdentifier.Blobs);
            return store.QueryCdnUrl(containerName, blobHandle);
        }

        /// <summary>
        /// Query CDN url for image
        /// </summary>
        /// <param name="blobHandle">Image handle</param>
        /// <returns>CDN url</returns>
        public async Task<Uri> QueryImageCdnUrl(string blobHandle)
        {
            // return null if input is null
            if (string.IsNullOrWhiteSpace(blobHandle))
            {
                return null;
            }

            CBStore store = await this.blobStoreManager.GetStore(ContainerIdentifier.Images);
            string containerName = this.blobStoreManager.GetContainerName(ContainerIdentifier.Images);
            return store.QueryCdnUrl(containerName, blobHandle);
        }
    }
}
