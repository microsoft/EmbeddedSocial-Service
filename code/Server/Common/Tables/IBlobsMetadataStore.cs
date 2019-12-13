// <copyright file="IBlobsMetadataStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Blobs metadata interface
    /// </summary>
    public interface IBlobsMetadataStore
    {
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
        Task InsertBlobMetadata(StorageConsistencyMode storageConsistencyMode, string appHandle, string userHandle, string blobHandle, long length, string contentType, BlobType blobType);

        /// <summary>
        /// Delete blob metadata
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Delete blob metadata task</returns>
        Task DeleteBlobMetadata(StorageConsistencyMode storageConsistencyMode, string appHandle, string userHandle, string blobHandle);

        /// <summary>
        /// Query blob metadata
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Blob metadata entity</returns>
        Task<IBlobMetadataEntity> QueryBlobMetadata(string appHandle, string userHandle, string blobHandle);

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
        Task InsertImageMetadata(StorageConsistencyMode storageConsistencyMode, string appHandle, string userHandle, string blobHandle, long length, string contentType, ImageType imageType, ReviewStatus reviewStatus);

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
        Task UpdateImageReviewStatus(StorageConsistencyMode storageConsistencyMode, string appHandle, string userHandle, string blobHandle, ReviewStatus reviewStatus, IImageMetadataEntity imageMetadataEntity);

        /// <summary>
        /// Delete image metadata
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Delete image metadata task</returns>
        Task DeleteImageMetadata(StorageConsistencyMode storageConsistencyMode, string appHandle, string userHandle, string blobHandle);

        /// <summary>
        /// Query image metadata
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Image metadata entity</returns>
        Task<IImageMetadataEntity> QueryImageMetadata(string appHandle, string userHandle, string blobHandle);
    }
}
