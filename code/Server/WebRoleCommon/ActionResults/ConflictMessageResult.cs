// <copyright file="ConflictMessageResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    /// <summary>
    /// Conflict message result class
    /// </summary>
    public class ConflictMessageResult : IHttpActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictMessageResult"/> class
        /// </summary>
        /// <param name="message">Message string</param>
        /// <param name="request">Http request</param>
        public ConflictMessageResult(string message, HttpRequestMessage request)
        {
            this.Message = message;
            this.Request = request;
        }

        /// <summary>
        /// Gets message string
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets http request
        /// </summary>
        public HttpRequestMessage Request { get; private set; }

        /// <summary>
        /// Execute async
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Http response message</returns>
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(this.Execute());
        }

        /// <summary>
        /// Execute action result
        /// </summary>
        /// <returns>Http response message</returns>
        private HttpResponseMessage Execute()
        {
            return this.Request.CreateResponse(HttpStatusCode.Conflict, new { message = this.Message });
        }
    }
}