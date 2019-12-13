// <copyright file="BlobsMetadataStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// Blobs metadata class
    /// </summary>
    public class BlobsMetadataStore : IBlobsMetadataStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobsMetadataStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public BlobsMetadataStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Insert blob metadata
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="length">Blob length</param>
        /// <param name="contentType">Content type</param>
        /// <param name="blobType">Blob type</param>
        /// <returns>Insert blob metadata task</returns>
        public async Task InsertBlobMetadata(StorageConsistencyMode storageConsistencyMode, string appHandle, string userHandle, string blobHandle, long length, string contentType, BlobType blobType)
        {
            BlobMetadataEntity blobMetadataEntity = new BlobMetadataEntity()
            {
                AppHandle = appHandle,
                UserHandle = userHandle,
                BlobHandle = blobHandle,
                Length = length,
                ContentType = contentType,
                BlobType = blobType
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Blobs);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Blobs, TableIdentifier.BlobsMetadata) as ObjectTable;
            string partitionKey = this.GetPartitionKey(appHandle, userHandle);
            Operation operation = Operation.Insert(table, partitionKey, blobHandle, blobMetadataEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete blob metadata
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Delete blob metadata task</returns>
        public async Task DeleteBlobMetadata(StorageConsistencyMode storageConsistencyMode, string appHandle, string userHandle, string blobHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Blobs);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Blobs, TableIdentifier.BlobsMetadata) as ObjectTable;
            string partitionKey = this.GetPartitionKey(appHandle, userHandle);
            Operation operation = Operation.Delete(table, partitionKey, blobHandle);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query blob metadata
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Blob metadata entity</returns>
        public async Task<IBlobMetadataEntity> QueryBlobMetadata(string appHandle, string userHandle, string blobHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Blobs);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Blobs, TableIdentifier.BlobsMetadata) as ObjectTable;
            string partitionKey = this.GetPartitionKey(appHandle, userHandle);
            BlobMetadataEntity blobMetadataEntity = await store.QueryObjectAsync<BlobMetadataEntity>(table, partitionKey, blobHandle);
            return blobMetadataEntity;
        }

        /// <summary>
        /// Insert image metadata
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="length">Image length</param>
        /// <param name="contentType">Content type</param>
        /// <param name="imageType">Image type</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Insert image metadata task</returns>
        public async Task InsertImageMetadata(StorageConsistencyMode storageConsistencyMode, string appHandle, string userHandle, string blobHandle, long length, string contentType, ImageType imageType, ReviewStatus reviewStatus)
        {
            ImageMetadataEntity imageMetadataEntity = new ImageMetadataEntity()
            {
                AppHandle = appHandle,
                UserHandle = userHandle,
                BlobHandle = blobHandle,
                Length = length,
                ContentType = contentType,
                ImageType = imageType,
                ReviewStatus = reviewStatus
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Images);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Images, TableIdentifier.ImagesMetadata) as ObjectTable;
            string partitionKey = this.GetPartitionKey(appHandle, userHandle);
            Operation operation = Operation.Insert(table, partitionKey, blobHandle, imageMetadataEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete image metadata
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Delete image metadata task</returns>
        public async Task DeleteImageMetadata(StorageConsistencyMode storageConsistencyMode, string appHandle, string userHandle, string blobHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Images);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Images, TableIdentifier.ImagesMetadata) as ObjectTable;
            string partitionKey = this.GetPartitionKey(appHandle, userHandle);
            Operation operation = Operation.Delete(table, partitionKey, blobHandle);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query image metadata
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Image metadata entity</returns>
        public async Task<IImageMetadataEntity> QueryImageMetadata(string appHandle, string userHandle, string blobHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Images);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Images, TableIdentifier.ImagesMetadata) as ObjectTable;
            string partitionKey = this.GetPartitionKey(appHandle, userHandle);
            ImageMetadataEntity imageMetadataEntity = await store.QueryObjectAsync<ImageMetadataEntity>(table, partitionKey, blobHandle);
            return imageMetadataEntity;
        }

        /// <summary>
        /// Updates image review status
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="imageMetadataEntity">Image metadata entity</param>
        /// <returns>Update image review status task</returns>
        public async Task UpdateImageReviewStatus(StorageConsistencyMode storageConsistencyMode, string appHandle, string userHandle, string blobHandle, ReviewStatus reviewStatus, IImageMetadataEntity imageMetadataEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Images);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Images, TableIdentifier.ImagesMetadata) as ObjectTable;
            string partitionKey = this.GetPartitionKey(appHandle, userHandle);

            imageMetadataEntity.ReviewStatus = reviewStatus;

            Operation operation = Operation.Replace(table, partitionKey, blobHandle, imageMetadataEntity as ImageMetadataEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Get partition key for metadata tables
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Partition key</returns>
        private string GetPartitionKey(string appHandle, string userHandle)
        {
            return string.Join("+", appHandle, userHandle);
        }
    }
}
