// <copyright file="IBlobsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Blobs;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Blobs manager interface
    /// </summary>
    public interface IBlobsManager
    {
        /// <summary>
        /// Create blob in blob store and blob metadata in table store
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="stream">Image stream</param>
        /// <param name="contentType">Content type</param>
        /// <param name="blobType">Blob type</param>
        /// <returns>Create blob task</returns>
        Task CreateBlob(string appHandle, string userHandle, string blobHandle, Stream stream, string contentType, BlobType blobType);

        /// <summary>
        /// Delete blob using its blob handle
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Delete blob task</returns>
        Task DeleteBlob(string appHandle, string userHandle, string blobHandle);

        /// <summary>
        /// Read blob using blob handle
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Blob item</returns>
        Task<IBlobItem> ReadBlob(string blobHandle);

        /// <summary>
        /// Read blob metadata
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Blob metadata entity</returns>
        Task<IBlobMetadataEntity> ReadBlobMetadata(string appHandle, string userHandle, string blobHandle);

        /// <summary>
        /// Create image in blob store, image metadata in table store, and resize task to the queue
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="stream">Image stream</param>
        /// <param name="contentType">Content type</param>
        /// <param name="imageType">Image type</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Create image task</returns>
        Task CreateImage(string appHandle, string userHandle, string blobHandle, Stream stream, string contentType, ImageType imageType, ReviewStatus reviewStatus);

        /// <summary>
        /// Create image resizes in blob store and update the image metadata in table store
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="imageType">Image type</param>
        /// <returns>Create image resizes task</returns>
        Task CreateImageResizes(ProcessType processType, string blobHandle, ImageType imageType);

        /// <summary>
        /// Update image review status
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="imageMetadataEntity">Image metadata entity</param>
        /// <returns>Update image metadata task</returns>
        Task UpdateImageReviewStatus(string appHandle, string userHandle, string blobHandle, ReviewStatus reviewStatus, IImageMetadataEntity imageMetadataEntity);

        /// <summary>
        /// Delete image using its blob handle
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="imageType">Image type</param>
        /// <returns>Delete image task</returns>
        Task DeleteImage(string appHandle, string userHandle, string blobHandle, ImageType imageType);

        /// <summary>
        /// Read image using blob handle
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Blob item</returns>
        Task<IBlobItem> ReadImage(string blobHandle);

        /// <summary>
        /// Read image metadata
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Image metadata entity</returns>
        Task<IImageMetadataEntity> ReadImageMetadata(string appHandle, string userHandle, string blobHandle);

        /// <summary>
        /// Get CDN url
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>CDN url for the blob handle</returns>
        Task<Uri> ReadBlobCdnUrl(string blobHandle);

        /// <summary>
        /// Get CDN url for image
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>CDN url for the blob handle</returns>
        Task<Uri> ReadImageCdnUrl(string blobHandle);

        /// <summary>
        /// Check if blob exists
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>A value indicating whether blob exists</returns>
        Task<bool> BlobExists(string blobHandle);

        /// <summary>
        /// Check if image exists
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>A value indicating whether image exists</returns>
        Task<bool> ImageExists(string blobHandle);

        /// <summary>
        /// Resizes an image and places the new image in blob storage
        /// </summary>
        /// <param name="blobHandle">original blob handle</param>
        /// <param name="originalImage">original image</param>
        /// <param name="newSize">new size for image</param>
        /// <returns>create resized image task</returns>
        Task CreateResizedImage(string blobHandle, Image originalImage, ImageSize newSize);

        /// <summary>
        /// Converts an image to the specified size using the jpeg encoder
        /// </summary>
        /// <param name="originalImage">original image</param>
        /// <param name="newSize">new size for the image</param>
        /// <returns>a memory stream containing the jpeg image, with the stream position set to the start of the image</returns>
        MemoryStream EncodeAndResizeImage(Image originalImage, ImageSize newSize);
    }
}
