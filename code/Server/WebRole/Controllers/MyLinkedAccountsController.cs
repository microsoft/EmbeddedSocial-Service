// <copyright file="MyLinkedAccountsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Exceptions;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Principal;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon;

    /// <summary>
    /// APIs to create, delete and query my linked accounts
    /// </summary>
    [RoutePrefix("users/me/linked_accounts")]
    public class MyLinkedAccountsController : BaseController
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
        /// Users manager
        /// </summary>
        private readonly IUsersManager usersManager;

        /// <summary>
        /// Apps manager
        /// </summary>
        private readonly IAppsManager appsManager;

        /// <summary>
        /// Views manager
        /// </summary>
        private readonly IViewsManager viewsManager;

        /// <summary>
        /// Session token manager
        /// </summary>
        private readonly ISessionTokenManager sessionTokenManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyLinkedAccountsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="identitiesManager">Identities manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="appsManager">Apps manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="sessionTokenManager">Session token manager</param>
        public MyLinkedAccountsController(ILog log, IIdentitiesManager identitiesManager, IUsersManager usersManager, IAppsManager appsManager, IViewsManager viewsManager, ISessionTokenManager sessionTokenManager)
        {
            this.log = log;
            this.identitiesManager = identitiesManager;
            this.usersManager = usersManager;
            this.appsManager = appsManager;
            this.viewsManager = viewsManager;
            this.sessionTokenManager = sessionTokenManager;
        }

        /// <summary>
        /// Create a new linked account.
        /// The account to be linked must appear in the Auth header of the request. This new third-party account
        /// will be linked against the credentials appearing in the session token passed in the body of the request.
        /// </summary>
        /// <param name="request">Post linked account request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="409">Conflict. Item already exists.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PostLinkedAccount([FromBody]PostLinkedAccountRequest request)
        {
            string className = "MyLinkedAccountsController";
            string methodName = "PostLinkedAccount";
            string logEntry = $"IdentityProvider = {this.UserPrincipal?.IdentityProvider}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // 1. Check that the auth header has no user handle. If it does, it means that the auth filter already found a user handle linked
            // to this credential
            if (this.UserHandle != null)
            {
                this.log.LogError(string.Format("User already has account linked. UserHandle: {0}", this.UserHandle));
                return this.Conflict(ResponseStrings.LinkedAccountExists);
            }

            // 2. Validate the session token. If token is invalid we return BadRequest (400) and not unauthorized (401).
            List<IPrincipal> principals;
            try
            {
                principals = await this.sessionTokenManager.ValidateToken(request.SessionToken);
            }
            catch (Exception e)
            {
                // Catch exception and log it
                this.log.LogError(string.Format("Session token {0} invalid in PostLinkedAccount", request.SessionToken), e);
                return this.BadRequest(ResponseStrings.SessionTokenInvalid);
            }

            // Extract app and user principals from session token.
            AppPrincipal sessionTokenAppPrincipal = null;
            UserPrincipal sessionTokenUserPrincipal = null;
            foreach (IPrincipal p in principals)
            {
                if (p is AppPrincipal)
                {
                    sessionTokenAppPrincipal = p as AppPrincipal;
                }
                else
                {
                    sessionTokenUserPrincipal = p as UserPrincipal;
                }
            }

            // 3. Check that the app principal extracted from session token matches the one in the auth filter.
            if (sessionTokenAppPrincipal != this.AppPrincipal)
            {
                this.log.LogError($"Session token belongs to app {sessionTokenAppPrincipal.ToString()} whereas the request's token belongs to app {this.AppPrincipal.ToString()}");
                return this.BadRequest(ResponseStrings.SessionTokenInvalid);
            }

            // 4. Check if the account is linked already. For this we use the user handle from the session token and the identity provider from the Auth header
            var linkedAccountEntity = await this.usersManager.ReadLinkedAccount(sessionTokenUserPrincipal.UserHandle, this.UserPrincipal.IdentityProvider);
            if (linkedAccountEntity != null)
            {
                this.log.LogError($"User already has account linked. UserHandle: {sessionTokenUserPrincipal.UserHandle}, IdentityProvider: {this.UserPrincipal.IdentityProvider}");
                return this.Conflict(ResponseStrings.LinkedAccountExists);
            }

            // 5. Finally link account
            UserPrincipal linkedAccountUserPrincipal = new UserPrincipal(this.log, sessionTokenUserPrincipal.UserHandle, this.UserPrincipal.IdentityProvider, this.UserPrincipal.IdentityProviderAccountId);
            await this.usersManager.CreateLinkedAccount(ProcessType.Frontend, linkedAccountUserPrincipal);

            logEntry += $", SessionTokenAppHandle = {sessionTokenAppPrincipal?.AppHandle}, SessionTokenUserHandle = {sessionTokenUserPrincipal?.UserHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Delete linked account
        /// </summary>
        /// <param name="identityProvider">Identity provider type</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The linked account is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{identityProvider}")]
        [HttpDelete]
        [VersionRange("All")]
        public async Task<IHttpActionResult> DeleteLinkedAccount(IdentityProviderType identityProvider)
        {
            string className = "MyLinkedAccountsController";
            string methodName = "DeleteLinkedAccount";
            string logEntry = $"IdentityProvider = {identityProvider.ToString()}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // 1. If user handle does not exist, return unauthorized
            if (this.UserHandle == null)
            {
                this.log.LogError("Unauthorized because DeleteLinkedAccount called without a user handle");
                return this.Unauthorized(ResponseStrings.GenericUnauthorizedError);
            }

            // 2. Check that the identity provider is not SocialPlus. You cannot delete a linked account of SocialPlus-type
            if (identityProvider == IdentityProviderType.SocialPlus)
            {
                this.log.LogError("DeleteLinkedAccount cannot be called for a SocialPlus linked account identity");
                return this.BadRequest(ResponseStrings.LinkedAccountOperationInvalid);
            }

            // 3. Lookup linked account
            var linkedAccountEntity = await this.usersManager.ReadLinkedAccount(this.UserHandle, identityProvider);
            if (linkedAccountEntity == null)
            {
                this.log.LogError($"DeletedLinkedAccount of a user without such a linked account. UserHandle: {this.UserHandle}, IdentityProviderType: {identityProvider}");
                return this.NotFound(ResponseStrings.LinkedAccountNotFound);
            }

            // 4. Delete linked account
            await this.usersManager.DeleteLinkedAccount(
                ProcessType.Frontend,
                this.UserHandle,
                identityProvider,
                linkedAccountEntity.AccountId);

            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Get linked accounts. Each user has at least two linked accounts: one SocialPlus account, and one (or more) third-party account.
        /// </summary>
        /// <returns>User linked accounts</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(List<LinkedAccountView>))]
        public async Task<IHttpActionResult> GetLinkedAccounts()
        {
            string className = "MyLinkedAccountsController";
            string methodName = "GetLinkedAccounts";
            this.LogControllerStart(this.log, className, methodName);

            var linkedAccountEntities = await this.usersManager.ReadLinkedAccounts(this.UserHandle);
            var linkedAccountViews = this.viewsManager.GetLinkedAccountViews(linkedAccountEntities);

            // Concatenate all linked accounts into a long string, delimited by ','
            string linkedAccounts = null;
            if (linkedAccountViews != null)
            {
                linkedAccounts = string.Join(",", linkedAccountViews.Select(v => v.IdentityProvider.ToString()).ToArray());
            }

            string logEntry = $"CountLinkedAccounts = {linkedAccountViews?.Count}, LinkedAccountsList = [{linkedAccounts}]";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(linkedAccountViews);
        }
    }
}
