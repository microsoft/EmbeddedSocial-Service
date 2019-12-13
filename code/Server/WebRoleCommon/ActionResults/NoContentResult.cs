// <copyright file="NoContentResult.cs" company="Microsoft">
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
    /// NoContent result class
    /// </summary>
    public class NoContentResult : IHttpActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoContentResult"/> class
        /// </summary>
        /// <param name="request">Http request</param>
        public NoContentResult(HttpRequestMessage request)
        {
            this.Request = request;
        }

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
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            response.RequestMessage = this.Request;
            return response;
        }
    }
}