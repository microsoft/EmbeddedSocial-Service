// <copyright file="IBlobsStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Blobs
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Blobs store interface
    /// </summary>
    public interface IBlobsStore
    {
        /// <summary>
        /// Insert blob in blobs store
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="stream">Blob stream</param>
        /// <param name="contentType">Content type</param>
        /// <param name="cacheTTL">Time to live in cache</param>
        /// <returns>Insert blob task</returns>
        Task InsertBlob(string blobHandle, Stream stream, string contentType, TimeSpan cacheTTL);

        /// <summary>
        /// Delete blob from blobs store
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Delete blob task</returns>
        Task DeleteBlob(string blobHandle);

        /// <summary>
        /// Query blob
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Blob item</returns>
        Task<IBlobItem> QueryBlob(string blobHandle);

        /// <summary>
        /// Insert image in blobs store
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="stream">Blob stream</param>
        /// <param name="contentType">Content type</param>
        /// <param name="cacheTTL">Time to live in cache</param>
        /// <returns>Insert image task</returns>
        Task InsertImage(string blobHandle, Stream stream, string contentType, TimeSpan cacheTTL);

        /// <summary>
        /// Delete image from blobs store
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Delete image task</returns>
        Task DeleteImage(string blobHandle);

        /// <summary>
        /// Query image
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Blob item</returns>
        Task<IBlobItem> QueryImage(string blobHandle);

        /// <summary>
        /// Check if blob exists
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>A value indicating whether the blob exists</returns>
        Task<bool> BlobExists(string blobHandle);

        /// <summary>
        /// Check if image exists
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>A value indicating whether the image exists</returns>
        Task<bool> ImageExists(string blobHandle);

        /// <summary>
        /// Query CDN url
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>CDN url</returns>
        Task<Uri> QueryBlobCdnUrl(string blobHandle);

        /// <summary>
        /// Query CDN url for image
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>CDN url</returns>
        Task<Uri> QueryImageCdnUrl(string blobHandle);
    }
}
