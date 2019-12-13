// <copyright file="BlobsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Blobs;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Blobs manager class
    /// </summary>
    public class BlobsManager : IBlobsManager
    {
        /// <summary>
        /// Time to live in cache
        /// </summary>
        private const int CacheTTLInSec = 60 * 60 * 24;

        /// <summary>
        /// What quality to encode resized images at
        /// </summary>
        private const long JpegQuality = 80L;

        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Blobs store
        /// </summary>
        private IBlobsStore blobsStore;

        /// <summary>
        /// Blob metadata store
        /// </summary>
        private IBlobsMetadataStore blobsMetadataStore;

        /// <summary>
        /// Resize images queue
        /// </summary>
        private IResizeImagesQueue resizeImagesQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobsManager"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="blobsStore">Blobs store</param>
        /// <param name="blobsMetadataStore">Blobs metadata store</param>
        /// <param name="resizeImagesQueue">Resize images queue</param>
        public BlobsManager(
            ILog log,
            IBlobsStore blobsStore,
            IBlobsMetadataStore blobsMetadataStore,
            IResizeImagesQueue resizeImagesQueue)
        {
            this.log = log;
            this.blobsStore = blobsStore;
            this.blobsMetadataStore = blobsMetadataStore;
            this.resizeImagesQueue = resizeImagesQueue;
        }

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
        public async Task CreateBlob(string appHandle, string userHandle, string blobHandle, Stream stream, string contentType, BlobType blobType)
        {
            await this.blobsStore.InsertBlob(blobHandle, stream, contentType, TimeSpan.FromSeconds(CacheTTLInSec));
            await this.blobsMetadataStore.InsertBlobMetadata(StorageConsistencyMode.Strong, appHandle, userHandle, blobHandle, stream.Length, contentType, blobType);
        }

        /// <summary>
        /// Delete blob using its blob handle
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Delete blob task</returns>
        public async Task DeleteBlob(string appHandle, string userHandle, string blobHandle)
        {
            await this.blobsMetadataStore.DeleteBlobMetadata(StorageConsistencyMode.Strong, appHandle, userHandle, blobHandle);
            await this.blobsStore.DeleteBlob(blobHandle);
        }

        /// <summary>
        /// Read blob using blob handle
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Blob item</returns>
        public async Task<IBlobItem> ReadBlob(string blobHandle)
        {
            return await this.blobsStore.QueryBlob(blobHandle);
        }

        /// <summary>
        /// Read blob metadata
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Blob metadata entity</returns>
        public async Task<IBlobMetadataEntity> ReadBlobMetadata(string appHandle, string userHandle, string blobHandle)
        {
            return await this.blobsMetadataStore.QueryBlobMetadata(appHandle, userHandle, blobHandle);
        }

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
        public async Task CreateImage(string appHandle, string userHandle, string blobHandle, Stream stream, string contentType, ImageType imageType, ReviewStatus reviewStatus)
        {
            await this.blobsStore.InsertImage(blobHandle, stream, contentType, TimeSpan.FromSeconds(CacheTTLInSec));
            await this.blobsMetadataStore.InsertImageMetadata(StorageConsistencyMode.Strong, appHandle, userHandle, blobHandle, stream.Length, contentType, imageType, reviewStatus);
            await this.resizeImagesQueue.SendResizeImageMessage(blobHandle, imageType);
        }

        /// <summary>
        /// Creates smaller image sizes in blob store
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="imageType">Image type</param>
        /// <returns>Create image resizes task</returns>
        public async Task CreateImageResizes(ProcessType processType, string blobHandle, ImageType imageType)
        {
            this.log.LogInformation("resizing " + blobHandle);

            // retrieve the image from blob store
            IBlobItem imageBlob = await this.blobsStore.QueryImage(blobHandle);
            if (imageBlob.Stream == null || imageBlob.Stream.Length <= 0)
            {
                this.log.LogException("did not retrieve image " + blobHandle);
            }

            Image originalImage = Image.FromStream(imageBlob.Stream);

            // rotate the image
            originalImage = RotateImage(originalImage);

            // resize the image and store it in blob store
            List<ImageSize> imageSizes = null;
            if (ImageSizesConfiguration.Sizes.ContainsKey(imageType))
            {
                imageSizes = ImageSizesConfiguration.Sizes[imageType];
            }

            if (imageSizes != null)
            {
                foreach (ImageSize newSize in imageSizes)
                {
                    await this.CreateResizedImage(blobHandle, originalImage, newSize);
                }
            }
        }

        /// <summary>
        /// Update image review status
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="imageMetadataEntity">Image metadata entity</param>
        /// <returns>Update image metadata task</returns>
        public async Task UpdateImageReviewStatus(string appHandle, string userHandle, string blobHandle, ReviewStatus reviewStatus, IImageMetadataEntity imageMetadataEntity)
        {
            await this.blobsMetadataStore.UpdateImageReviewStatus(StorageConsistencyMode.Strong, appHandle, userHandle, blobHandle, reviewStatus, imageMetadataEntity);
        }

        /// <summary>
        /// Delete image and its various sizes using its blob handle
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="imageType">Image type</param>
        /// <returns>Delete image task</returns>
        public async Task DeleteImage(string appHandle, string userHandle, string blobHandle, ImageType imageType)
        {
            this.log.LogInformation("deleting " + blobHandle);

            // delete the image metadata
            await this.blobsMetadataStore.DeleteImageMetadata(StorageConsistencyMode.Strong, appHandle, userHandle, blobHandle);

            // get the various sizes of this type of image
            List<ImageSize> imageSizes = null;
            if (ImageSizesConfiguration.Sizes.ContainsKey(imageType))
            {
                imageSizes = ImageSizesConfiguration.Sizes[imageType];
            }

            // delete each size
            if (imageSizes != null)
            {
                foreach (ImageSize imageSize in imageSizes)
                {
                    // form new image handle
                    string resizedBlobHandle = blobHandle + imageSize.Id;

                    // delete the resized image
                    try
                    {
                        await this.blobsStore.DeleteImage(resizedBlobHandle);
                    }

                    // It's ok if the resized image was not found
                    // because it may not have been resized yet.
                    // A background worker is needed anyway to periodically
                    // remove unreferenced images.
                    catch (Exception e)
                    {
                        if (!e.Message.Contains("NotFound"))
                        {
                            throw e;
                        }
                    }
                }
            }

            // delete the original image
            await this.blobsStore.DeleteImage(blobHandle);
        }

        /// <summary>
        /// Read image using blob handle
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Blob item</returns>
        public async Task<IBlobItem> ReadImage(string blobHandle)
        {
            return await this.blobsStore.QueryImage(blobHandle);
        }

        /// <summary>
        /// Read image metadata
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Image metadata entity</returns>
        public async Task<IImageMetadataEntity> ReadImageMetadata(string appHandle, string userHandle, string blobHandle)
        {
            return await this.blobsMetadataStore.QueryImageMetadata(appHandle, userHandle, blobHandle);
        }

        /// <summary>
        /// Get CDN url for blob
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>CDN url for the blob handle</returns>
        public async Task<Uri> ReadBlobCdnUrl(string blobHandle)
        {
            return await this.blobsStore.QueryBlobCdnUrl(blobHandle);
        }

        /// <summary>
        /// Get CDN url for image
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>CDN url for the blob handle</returns>
        public async Task<Uri> ReadImageCdnUrl(string blobHandle)
        {
            return await this.blobsStore.QueryImageCdnUrl(blobHandle);
        }

        /// <summary>
        /// Check if blob exists
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>A value indicating whether blob exists</returns>
        public async Task<bool> BlobExists(string blobHandle)
        {
            return await this.blobsStore.BlobExists(blobHandle);
        }

        /// <summary>
        /// Check if image exists
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>A value indicating whether image exists</returns>
        public async Task<bool> ImageExists(string blobHandle)
        {
            return await this.blobsStore.ImageExists(blobHandle);
        }

        /// <summary>
        /// Resizes an image and places the new image in blob storage
        /// </summary>
        /// <param name="blobHandle">original blob handle</param>
        /// <param name="originalImage">original image</param>
        /// <param name="newSize">new size for image</param>
        /// <returns>create resized image task</returns>
        public async Task CreateResizedImage(string blobHandle, Image originalImage, ImageSize newSize)
        {
            // form new image handle
            string newImageHandle = blobHandle + newSize.Id;

            // skip sizes that already exist in the blob store
            bool newImageExists = await this.ImageExists(newImageHandle);
            if (newImageExists)
            {
                return;
            }

            // encode image to a memory stream containing the new size jpeg image
            var newImageStream = this.EncodeAndResizeImage(originalImage, newSize);
            await this.blobsStore.InsertImage(newImageHandle, newImageStream, "image/jpeg", TimeSpan.FromSeconds(CacheTTLInSec));
        }

        /// <summary>
        /// Converts an image to the specified size using the jpeg encoder
        /// </summary>
        /// <param name="originalImage">original image</param>
        /// <param name="newSize">new size for the image</param>
        /// <returns>a memory stream containing the jpeg image, with the stream position set to the start of the image</returns>
        public MemoryStream EncodeAndResizeImage(Image originalImage, ImageSize newSize)
        {
            // resize image
            Size size = CalculateImageSize(newSize.Width, originalImage.Size);
            Bitmap newImage = new Bitmap(originalImage, size);

            // set encoding quality
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            Encoder myEncoder = Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, JpegQuality);
            myEncoderParameters.Param[0] = myEncoderParameter;

            // save to a memory stream
            MemoryStream newImageStream = new MemoryStream();
            newImage.Save(newImageStream, jpgEncoder, myEncoderParameters);

            // reset the memory stream position to the beginning
            newImageStream.Position = 0L;
            return newImageStream;
        }

        /// <summary>
        /// Given an image, will apply EXIF Orientation to the image and return it without the orientation tag
        /// </summary>
        /// <param name="inputImage">image with EXIF orientation</param>
        /// <returns>rotated image without the EXIF orientation</returns>
        private static Image RotateImage(Image inputImage)
        {
            // if the orientation is not specified in this image, then return the input image
            if (Array.IndexOf(inputImage.PropertyIdList, 274) < 0)
            {
                return inputImage;
            }

            // get the orientation
            int orientation = (int)inputImage.GetPropertyItem(274).Value[0];

            // now rotate the image based on orientation
            // http://www.impulseadventure.com/photo/exif-orientation.html
            // http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/EXIF.html
            // https://beradrian.wordpress.com/2008/11/14/rotate-exif-images/
            switch (orientation)
            {
                case 1:
                    break;
                case 2:
                    inputImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    break;
                case 3:
                    inputImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 4:
                    inputImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    break;
                case 5:
                    inputImage.RotateFlip(RotateFlipType.Rotate90FlipX);
                    break;
                case 6:
                    inputImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 7:
                    inputImage.RotateFlip(RotateFlipType.Rotate270FlipX);
                    break;
                case 8:
                    inputImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
                default:
                    break;
            }

            // remove the orientation
            inputImage.RemovePropertyItem(274);

            // all done
            return inputImage;
        }

        /// <summary>
        ///  gets image encoder for an image format
        /// </summary>
        /// <param name="format">type of image format such as jpg</param>
        /// <returns>encoder/decoder for that image format</returns>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }

        /// <summary>
        /// calculates the new size of an image
        /// </summary>
        /// <param name="newWidth">target width</param>
        /// <param name="originalSize">original image size</param>
        /// <returns>new image size</returns>
        private static Size CalculateImageSize(int newWidth, Size originalSize)
        {
            // if original image is too small, just use that and skip any resizing
            if (originalSize.Width <= newWidth)
            {
                return originalSize;
            }

            // maintain aspect ratio
            Size newSize = default(Size);
            newSize.Width = newWidth;
            double newHeight = (newWidth * originalSize.Height) / originalSize.Width;
            newSize.Height = Convert.ToInt32(Math.Round(newHeight));

            return newSize;
        }
    }
}
