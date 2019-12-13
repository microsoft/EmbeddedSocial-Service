// <copyright file="NotImplementedResult.cs" company="Microsoft">
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
    /// NotImplementedResult result class
    /// </summary>
    public class NotImplementedResult : IHttpActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotImplementedResult"/> class
        /// </summary>
        /// <param name="message">Message string</param>
        /// <param name="request">Http request</param>
        public NotImplementedResult(string message, HttpRequestMessage request)
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
            return this.Request.CreateResponse(HttpStatusCode.NotImplemented, new { message = this.Message });
        }
    }
}