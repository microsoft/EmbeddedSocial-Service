// <copyright file="BlobResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon
{
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    using SocialPlus.Server.Blobs;

    /// <summary>
    /// This class implements an IHttpActionResult that returns a blob
    /// </summary>
    public class BlobResult : IHttpActionResult
    {
        private readonly IBlobItem blob;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobResult"/> class
        /// </summary>
        /// <param name="blob">blob to return in an http response</param>
        public BlobResult(IBlobItem blob)
        {
            this.blob = blob;
        }

        /// <summary>
        /// Creates the http response message and sets the content and content type
        /// </summary>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>an http response message</returns>
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            MemoryStream memoryStream = this.blob.Stream as MemoryStream;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(memoryStream.ToArray())
            };

            var contentType = this.blob.ContentType;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            return Task.FromResult(response);
        }
    }
}