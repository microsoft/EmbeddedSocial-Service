// <copyright file="ImagesController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Blobs;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon;
    using SocialPlus.Utils;

    /// <summary>
    /// API to upload images
    /// </summary>
    [RoutePrefix("images")]
    public class ImagesController : BaseController
    {
        /// <summary>
        /// Maximum image size allowed by PostImage
        /// </summary>
        private const int MaxImageSizeInBytes = 1024 * 1024 * 100;

        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Blobs manager
        /// </summary>
        private IBlobsManager blobsManager;

        /// <summary>
        /// Handle generator
        /// </summary>
        private IHandleGenerator handleGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagesController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="blobsManager">Blobs manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public ImagesController(ILog log, IBlobsManager blobsManager, IHandleGenerator handleGenerator)
        {
            this.log = log;
            this.blobsManager = blobsManager;
            this.handleGenerator = handleGenerator;
        }

        /// <summary>
        /// Upload a new image
        /// </summary>
        /// <remarks>
        /// Images will be resized. To access a resized image, append the 1 character size identifier to the blobHandle that is returned.
        ///
        /// - d is 25 pixels wide
        /// - h is 50 pixels wide
        /// - l is 100 pixels wide
        /// - p is 250 pixels wide
        /// - t is 500 pixels wide
        /// - x is 1000 pixels wide
        ///
        /// - ImageType.UserPhoto supports d,h,l,p,t,x
        /// - ImageType.ContentBlob supports d,h,l,p,t,x
        /// - ImageType.AppIcon supports l
        ///
        /// All resized images will maintain their aspect ratio. Any orientation specified in the EXIF headers will be honored.
        /// </remarks>
        /// <param name="imageType">Image type</param>
        /// <returns>Post image response</returns>
        /// <response code="201">Created. The response contains the image handle.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{imageType}")]
        [HttpPost]
        [VersionRange("All")]
        [ResponseType(typeof(PostImageResponse))]
        public async Task<IHttpActionResult> PostImage(ImageType imageType)
        {
            string className = "ImagesController";
            string methodName = "PostImage";
            string logEntry = $"ImageType = {imageType}, ContentType = {HttpContext.Current?.Request?.ContentType}, ContentLength = {HttpContext.Current?.Request?.ContentLength}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // make sure the necessary fields in the request are present
            if (HttpContext.Current.Request == null ||
                HttpContext.Current.Request.InputStream == null ||
                HttpContext.Current.Request.ContentType == null)
            {
                this.log.LogError("got invalid request");
                return this.BadRequest(ResponseStrings.ImageRequestNull);
            }

            HttpRequest request = HttpContext.Current.Request;

            // is the content type appropriate?
            if (!request.ContentType.StartsWith("image", true, System.Globalization.CultureInfo.InvariantCulture))
            {
                this.log.LogError("got unexpected content type: " + request.ContentType);
                return this.BadRequest(ResponseStrings.ImageBadType);
            }

            // check content length range
            if (request.ContentLength <= 0)
            {
                this.log.LogError("got unexpected size of image: " + request.ContentLength);
                return this.BadRequest(ResponseStrings.ImageTooSmall);
            }

            if (request.ContentLength > MaxImageSizeInBytes)
            {
                this.log.LogError("got unexpected size of image: " + request.ContentLength);
                return this.BadRequest(ResponseStrings.ImageTooBig);
            }

            // check content parses correctly by .NET libraries
            // Image receivedImage = Image.FromStream(HttpContext.Current.Request.InputStream);
            // (commented this out to improve speed)

            // create a unique handle for this image
            string blobHandle = this.handleGenerator.GenerateLongHandle();

            // store the image
            request.InputStream.Position = 0L;
            await this.blobsManager.CreateImage(this.AppHandle, this.UserHandle, blobHandle, request.InputStream, request.ContentType, imageType, ReviewStatus.Active);

            // all done
            this.log.LogInformation("all done with: " + blobHandle);

            // send back response
            PostImageResponse response = new PostImageResponse()
            {
                BlobHandle = blobHandle
            };

            logEntry += $", BlobHandle = {blobHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Created<PostImageResponse>(new Uri(this.Request.RequestUri.AbsoluteUri + "/" + blobHandle), response);
        }

        /// <summary>
        /// Get image
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Image content</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The image is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{blobHandle}")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetImage(string blobHandle)
        {
            string className = "ImagesController";
            string methodName = "GetImage";
            string logEntry = $"BlobHandle = {blobHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            bool exists = await this.blobsManager.ImageExists(blobHandle);
            if (!exists)
            {
                return this.NotFound(ResponseStrings.ImageNotFound);
            }

            IBlobItem blobItem = await this.blobsManager.ReadImage(blobHandle);
            logEntry += $", BlobType = {blobItem?.ContentType}, BlobContentLength = {blobItem?.Stream?.Length}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return new BlobResult(blobItem);
        }
    }
}
