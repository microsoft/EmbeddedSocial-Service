// <copyright file="ChallengeOnUnauthorizedResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    /// <summary>
    /// Challenge of unauthorized result class
    /// </summary>
    public class ChallengeOnUnauthorizedResult : IHttpActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChallengeOnUnauthorizedResult"/> class
        /// </summary>
        /// <param name="challenge">Authentication challenge</param>
        /// <param name="innerResult">Inner http action result</param>
        public ChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, IHttpActionResult innerResult)
        {
            this.Challenge = challenge;
            this.InnerResult = innerResult;
        }

        /// <summary>
        /// Gets authentication challenge
        /// </summary>
        public AuthenticationHeaderValue Challenge { get; private set; }

        /// <summary>
        /// Gets inner http action result
        /// </summary>
        public IHttpActionResult InnerResult { get; private set; }

        /// <summary>
        /// Execute async method
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Http response message</returns>
        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await this.InnerResult.ExecuteAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!response.Headers.WwwAuthenticate.Any((h) => h.Scheme == this.Challenge.Scheme))
                {
                    response.Headers.WwwAuthenticate.Add(this.Challenge);
                }
            }

            return response;
        }
    }
}