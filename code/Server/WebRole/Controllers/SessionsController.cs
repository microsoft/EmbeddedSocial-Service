// <copyright file="SessionsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Exceptions;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Metrics;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon;
    using SocialPlus.Utils;

    /// <summary>
    /// APIs to create session (sign in) and delete session (sign out)
    /// </summary>
    [RoutePrefix("sessions")]
    public class SessionsController : BaseController
    {
        /// <summary>
        /// Session token duration.
        /// </summary>
        private readonly TimeSpan sessionTokenDuration = TimeSpan.FromDays(180);

        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Identities manager
        /// </summary>
        private readonly IIdentitiesManager identitiesManager;

        /// <summary>
        /// Tokens manager
        /// </summary>
        private readonly ISessionTokenManager tokenManager;

        /// <summary>
        /// Users manager
        /// </summary>
        private readonly IUsersManager usersManager;

        /// <summary>
        /// Apps manager
        /// </summary>
        private readonly IAppsManager appsManager;

        /// <summary>
        /// Application metrics logger
        /// </summary>
        private readonly IApplicationMetrics applicationMetrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="identitiesManager">Identities manager</param>
        /// <param name="tokensManager">Token manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="appsManager">Apps manager</param>
        /// <param name="applicationMetrics">Application metrics logger</param>
        public SessionsController(
            ILog log,
            IIdentitiesManager identitiesManager,
            ISessionTokenManager tokensManager,
            IUsersManager usersManager,
            IAppsManager appsManager,
            IApplicationMetrics applicationMetrics)
        {
            this.log = log;
            this.identitiesManager = identitiesManager;
            this.tokenManager = tokensManager;
            this.usersManager = usersManager;
            this.appsManager = appsManager;
            this.applicationMetrics = applicationMetrics;
        }

        /// <summary>
        /// Create a new session (sign in)
        /// </summary>
        /// <param name="request">Post session request</param>
        /// <returns>Post session response</returns>
        /// <response code="201">Created. The response contains user handle and session token.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The user is not found. The app needs to call create user to create a new user.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        [ResponseType(typeof(PostSessionResponse))]
        public async Task<IHttpActionResult> PostSession([FromBody]PostSessionRequest request)
        {
            string className = "SessionsController";
            string methodName = "PostSession";
            string logEntry = $"SessionUserHandle = {request?.UserHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // Check whether user handle is null
            if (this.UserHandle == null)
            {
                this.log.LogError("Unauthorized because PostSession called without a user handle");
                return this.Unauthorized(ResponseStrings.GenericUnauthorizedError);
            }

            // The auth's user principal must have the same user handle as the one in PostSession request
            if (this.UserHandle != request.UserHandle)
            {
                this.log.LogError(string.Format("Unauthorized because one user handle called PostSession on behalf of another user handle. Auth's UserHandle: {0}, Request's UserHandle: {1}", this.User, request.UserHandle));
                return this.Unauthorized(ResponseStrings.UserUnauthorized);
            }

            // Is user handle registered with this app?
            var userProfileEntity = await this.usersManager.ReadUserProfile(this.UserHandle, this.AppHandle);
            if (userProfileEntity == null)
            {
                this.log.LogError(string.Format("No user profile found for this app. UserHandle: {0}, AppHandle {1}", this.UserHandle, this.AppHandle));
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            // Generate session token
            string sessionToken = await this.tokenManager.CreateToken(this.AppPrincipal, this.UserPrincipal, this.sessionTokenDuration);
            PostSessionResponse response = new PostSessionResponse()
            {
                UserHandle = this.UserHandle,
                SessionToken = sessionToken
            };

            // Log user session start to app metrics
            this.applicationMetrics.AddActiveUser();

            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Created<PostSessionResponse>(this.UserHandle, response);
        }

        /// <summary>
        /// Delete the current session (sign out)
        /// </summary>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("current")]
        [HttpDelete]
        [VersionRange("All")]
        public IHttpActionResult DeleteSession()
        {
            string className = "SessionsController";
            string methodName = "DeleteSession";
            this.LogControllerStart(this.log, className, methodName);

            // Log user session start to app metrics
            this.applicationMetrics.DeleteActiveUser();

            var result = this.NoContent();
            this.LogControllerEnd(this.log, className, methodName);
            return result;
        }
    }
}
