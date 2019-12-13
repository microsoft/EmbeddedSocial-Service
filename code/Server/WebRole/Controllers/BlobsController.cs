//-----------------------------------------------------------------------
// <copyright file="BlobsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class BlobsController.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.Controllers
{
    using System;
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
    /// API to upload blobs
    /// </summary>
    [RoutePrefix("blobs")]
    public class BlobsController : BaseController
    {
        /// <summary>
        /// Maximum blob size allowed by PostBlob (~ 1 GB)
        /// </summary>
        private readonly int maxBlobSizeInBytes = 1024 * 1024 * 1000;

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
        /// Initializes a new instance of the <see cref="BlobsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="blobsManager">Blobs manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public BlobsController(ILog log, IBlobsManager blobsManager, IHandleGenerator handleGenerator)
        {
            this.log = log;
            this.blobsManager = blobsManager;
            this.handleGenerator = handleGenerator;
        }

        /// <summary>
        /// Upload a blob
        /// </summary>
        /// <remarks>
        /// If your blob is an image, use image APIs. For all other blob types, use this API.
        /// </remarks>
        /// <returns>Post blob response</returns>
        /// <response code="201">Created. The response contains the blob handle.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        [ResponseType(typeof(PostBlobResponse))]
        public async Task<IHttpActionResult> PostBlob()
        {
            string className = "BlobsController";
            string methodName = "PostBlob";
            string logEntry = $"ContentType = {HttpContext.Current?.Request?.ContentType}, ContentLength = {HttpContext.Current?.Request?.ContentLength}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // make sure the necessary fields in the request are present
            if (HttpContext.Current.Request == null ||
                HttpContext.Current.Request.InputStream == null ||
                HttpContext.Current.Request.ContentType == null)
            {
                this.log.LogError("got invalid request");
                return this.BadRequest(ResponseStrings.BlobRequestNull);
            }

            HttpRequest request = HttpContext.Current.Request;

            // check content length range
            if (request.ContentLength <= 0)
            {
                this.log.LogError("got unexpected size of blob: " + request.ContentLength);
                return this.BadRequest(ResponseStrings.BlobTooSmall);
            }

            if (request.ContentLength > this.maxBlobSizeInBytes)
            {
                this.log.LogError("got unexpected size of blob: " + request.ContentLength);
                return this.BadRequest(ResponseStrings.BlobTooBig);
            }

            if (string.IsNullOrWhiteSpace(request.ContentType))
            {
                this.log.LogError("got invalid blob content type");
                return this.BadRequest(ResponseStrings.BlobRequestNull);
            }

            string blobHandle = this.handleGenerator.GenerateLongHandle();
            await this.blobsManager.CreateBlob(this.AppHandle, this.UserHandle, blobHandle, request.InputStream, request.ContentType, BlobType.Unknown);
            PostBlobResponse response = new PostBlobResponse()
            {
                BlobHandle = blobHandle
            };

            logEntry += $", {blobHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Created<PostBlobResponse>(new Uri(this.Request.RequestUri.AbsoluteUri + "/" + blobHandle), response);
        }

        /// <summary>
        /// Get blob
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <returns>Blob data</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The blob is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{blobHandle}")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetBlob(string blobHandle)
        {
            string className = "BlobsController";
            string methodName = "GetBlob";
            string logEntry = $"BlobHandle = {blobHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            bool exists = await this.blobsManager.BlobExists(blobHandle);
            if (!exists)
            {
                return this.NotFound(ResponseStrings.BlobNotFound);
            }

            IBlobItem blobItem = await this.blobsManager.ReadBlob(blobHandle);
            logEntry += $", {blobItem?.ContentType}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return new BlobResult(blobItem);
        }
    }
}
