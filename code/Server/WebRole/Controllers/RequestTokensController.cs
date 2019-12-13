// <copyright file="RequestTokensController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.OAuth;
    using SocialPlus.Server.Exceptions;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRole.Versioning;

    /// <summary>
    /// API to request token from identity providers
    /// </summary>
    [RoutePrefix("request_tokens")]
    public class RequestTokensController : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Identities manager
        /// </summary>
        private readonly IIdentitiesManager identitiesManager;

        /// <summary>
        /// Apps manager
        /// </summary>
        private readonly IAppsManager appsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestTokensController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="identitiesManager">Identities manager</param>
        /// <param name="appsManager">Apps manager</param>
        public RequestTokensController(ILog log, IIdentitiesManager identitiesManager, IAppsManager appsManager)
        {
            this.log = log;
            this.identitiesManager = identitiesManager;
            this.appsManager = appsManager;
        }

        /// <summary>
        /// Get request token
        /// </summary>
        /// <param name="identityProvider">Identity provider type</param>
        /// <returns>Get request token response</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{identityProvider}")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(GetRequestTokenResponse))]
        public async Task<IHttpActionResult> GetRequestToken(IdentityProviderType identityProvider)
        {
            string className = "RequestTokensController";
            string methodName = "GetRequestToken";
            string logEntry = $"IdentityProvider = {identityProvider}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var identityProviderCredentials = await this.appsManager.ReadIdentityProviderCredentials(this.AppHandle, identityProvider);

            // If no credentials found, or the credentials are missing material, throw internal server error
            if (identityProviderCredentials == null ||
                string.IsNullOrEmpty(identityProviderCredentials.ClientId) ||
                string.IsNullOrEmpty(identityProviderCredentials.ClientSecret) ||
                string.IsNullOrEmpty(identityProviderCredentials.ClientRedirectUri))
            {
                return this.InternalServerError(new Exception("application credentials are incorrect in the developer portal."));
            }

            string requestToken = null;
            try
            {
                requestToken = await this.identitiesManager.GetRequestToken(
                    identityProvider,
                    identityProviderCredentials.ClientId,
                    identityProviderCredentials.ClientSecret,
                    identityProviderCredentials.ClientRedirectUri);
            }
            catch (IdentityProviderException e)
            {
                Exception innerEx = e.InnerException;

                // If the innnerException is an HttpRequestException, this is a bad request
                // If it's an AuthException it's an internal server error
                if (innerEx is HttpRequestException)
                {
                    // this corresponds to a bad request
                    string badRequestMsg = ((HttpRequestException)innerEx).Message;
                    return this.BadRequest(badRequestMsg);
                }
                else if (innerEx is OAuthException)
                {
                    return this.InternalServerError(innerEx);
                }

                // Else
                return this.InternalServerError(e);
            }

            // The request token should not be null here
            GetRequestTokenResponse response = new GetRequestTokenResponse()
            {
                RequestToken = requestToken
            };

            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(response);
        }
    }
}
