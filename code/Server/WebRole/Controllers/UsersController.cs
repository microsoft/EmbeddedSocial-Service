// <copyright file="UsersController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Exceptions;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Metrics;
    using SocialPlus.Server.Principal;
    using SocialPlus.Server.WebRoleCommon;
    using SocialPlus.Utils;
    using WebRole.Versioning;

    /// <summary>
    /// APIs to create, delete, update and query users
    /// </summary>
    [RoutePrefix("users")]
    public class UsersController : BaseController
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
        /// Popular users manager
        /// </summary>
        private readonly IPopularUsersManager popularUsersManager;

        /// <summary>
        /// Apps manager
        /// </summary>
        private readonly IAppsManager appsManager;

        /// <summary>
        /// Views manager
        /// </summary>
        private readonly IViewsManager viewsManager;

        /// <summary>
        /// Handle generator
        /// </summary>
        private readonly IHandleGenerator handleGenerator;

        /// <summary>
        /// Application metrics logger
        /// </summary>
        private readonly IApplicationMetrics applicationMetrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="identitiesManager">Identities manager</param>
        /// <param name="tokenManager">Token manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="popularUsersManager">Popular users manager</param>
        /// <param name="appsManager">Apps manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        /// <param name="applicationMetrics">Application metrics logger</param>
        public UsersController(
            ILog log,
            IIdentitiesManager identitiesManager,
            ISessionTokenManager tokenManager,
            IUsersManager usersManager,
            IPopularUsersManager popularUsersManager,
            IAppsManager appsManager,
            IViewsManager viewsManager,
            IHandleGenerator handleGenerator,
            IApplicationMetrics applicationMetrics)
        {
            this.log = log;
            if (identitiesManager == null || tokenManager == null || popularUsersManager == null || appsManager == null || viewsManager == null || handleGenerator == null)
            {
                this.log.LogException(
                    string.Format(
                        "Calling UserController constructure with null parameters. identitiesManager is {0}, tokenManager is {1}," +
                        "usersManager is {2}, popularUsersManager is {3}, appsManager is {4}, viewsManager is {5}, handleGenerator is {6}",
                        identitiesManager == null ? "null" : "not null",
                        tokenManager == null ? "null" : "not null",
                        usersManager == null ? "null" : "not null",
                        popularUsersManager == null ? "null" : "not null",
                        appsManager == null ? "null" : "not null",
                        viewsManager == null ? "null" : "not null",
                        handleGenerator == null ? "null" : "not null"));
            }

            this.identitiesManager = identitiesManager;
            this.tokenManager = tokenManager;
            this.usersManager = usersManager;
            this.popularUsersManager = popularUsersManager;
            this.appsManager = appsManager;
            this.viewsManager = viewsManager;
            this.handleGenerator = handleGenerator;
            this.applicationMetrics = applicationMetrics;
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <remarks>
        /// Create a new user and return a fresh session token
        /// </remarks>
        /// <param name="request">Post user request</param>
        /// <returns>Post user response</returns>
        /// <response code="201">Created. The response contains user handle and session token.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="409">Conflict. Item already exists.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        [ResponseType(typeof(PostUserResponse))]
        public async Task<IHttpActionResult> PostUser([FromBody]PostUserRequest request)
        {
            string className = "UsersController";
            string methodName = "PostUser";
            this.LogControllerStart(this.log, className, methodName);

            // 1. Construct the user principal for the new user. An incoming request always has a user principal. However the user
            // principal might or might not have an empty user handle. For example, Beihai requests have non-empty user handles.
            UserPrincipal userPrincipal = this.UserPrincipal;
            if (userPrincipal.UserHandle == null)
            {
                // Use the identity provider and account id from the user principal except if the identity provider type is AADS2S.
                // In that case, the userhandle also acts as the account id
                string userHandle = this.handleGenerator.GenerateShortHandle();
                if (this.UserPrincipal.IdentityProvider == IdentityProviderType.AADS2S)
                {
                    userPrincipal = new UserPrincipal(this.log, userHandle, IdentityProviderType.AADS2S, userHandle);
                }
                else
                {
                    userPrincipal = new UserPrincipal(this.log, userHandle, this.UserPrincipal.IdentityProvider, this.UserPrincipal.IdentityProviderAccountId);
                }
            }

            // 2. Check whether a user profile for the application calling us exists. If so, return a conflict error message
            var userProfileEntity = await this.usersManager.ReadUserProfile(userPrincipal.UserHandle, this.AppHandle);
            if (userProfileEntity != null)
            {
                this.log.LogError(string.Format("Conflict on PostUser: user profile for this app already exists.  UserHandle={0}, AppHandle={1}", userPrincipal.UserHandle, this.AppHandle));
                return this.Conflict(ResponseStrings.UserExists);
            }

            // 3. Check whether this user has profiles registered with any applications other than the one calling us.
            //    In that case, we shouldn't need to create the user (we just need to create a user profile).
            //    Othwerwise, create the user (which also creates the user profile).
            var linkedAccountEntities = await this.usersManager.ReadLinkedAccounts(userPrincipal.UserHandle);
            if (linkedAccountEntities.Count == 0)
            {
                // Create the user using SocialPlus identity provider space
                await this.usersManager.CreateUserAndUserProfile(
                    ProcessType.Frontend,
                    userPrincipal.UserHandle,
                    IdentityProviderType.SocialPlus,
                    userPrincipal.UserHandle,
                    this.AppHandle,
                    request.FirstName,
                    request.LastName,
                    request.Bio,
                    request.PhotoHandle,
                    DateTime.UtcNow,
                    null);

                // Also create a linked account in the third-party identity provider space
                await this.usersManager.CreateLinkedAccount(ProcessType.Frontend, userPrincipal);
            }
            else
            {
                await this.usersManager.CreateUserProfile(
                        ProcessType.Frontend,
                        userPrincipal.UserHandle,
                        this.AppHandle,
                        request.FirstName,
                        request.LastName,
                        request.Bio,
                        request.PhotoHandle,
                        DateTime.UtcNow,
                        null);
            }

            // 4. Generate session token
            string sessionToken = await this.tokenManager.CreateToken(this.AppPrincipal, userPrincipal, this.sessionTokenDuration);
            PostUserResponse response = new PostUserResponse()
            {
                UserHandle = userPrincipal.UserHandle,
                SessionToken = sessionToken
            };

            // Log added user to app metrics
            this.applicationMetrics.AddUser();

            string logEntry = $"UserHandle = {response.UserHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Created<PostUserResponse>(userPrincipal.UserHandle, response);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not found. User is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("me")]
        [HttpDelete]
        [VersionRange("All")]
        public async Task<IHttpActionResult> DeleteUser()
        {
            string className = "UsersController";
            string methodName = "DeleteUser";
            this.LogControllerStart(this.log, className, methodName);

            // If user handle is null, return 404
            if (this.UserHandle == null)
            {
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            // Lookup the user profile. If not found, return 404
            var userProfileEntity = await this.usersManager.ReadUserProfile(this.UserHandle, this.AppHandle);
            if (userProfileEntity == null)
            {
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            await this.usersManager.DeleteUserProfile(ProcessType.Frontend, this.UserHandle, this.AppHandle);

            // Log user deletion to app metrics
            this.applicationMetrics.DeleteUser();

            this.LogControllerEnd(this.log, className, methodName);
            return this.NoContent();
        }

        /// <summary>
        /// Update user info
        /// </summary>
        /// <param name="request">Put user info request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not found. User is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("me/info")]
        [HttpPut]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PutUserInfo([FromBody]PutUserInfoRequest request)
        {
            string className = "UsersController";
            string methodName = "PutUserInfo";
            this.LogControllerStart(this.log, className, methodName);

            // If user handle is null, return 404
            if (this.UserHandle == null)
            {
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            var userProfileEntity = await this.usersManager.ReadUserProfile(this.UserHandle, this.AppHandle);
            if (userProfileEntity == null)
            {
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            await this.usersManager.UpdateUserInfo(
                ProcessType.Frontend,
                this.UserHandle,
                this.AppHandle,
                request.FirstName,
                request.LastName,
                request.Bio,
                DateTime.UtcNow,
                userProfileEntity);

            this.LogControllerEnd(this.log, className, methodName);
            return this.NoContent();
        }

        /// <summary>
        /// Update user photo
        /// </summary>
        /// <param name="request">Put user photo request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not found. User is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("me/photo")]
        [HttpPut]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PutUserPhoto([FromBody]PutUserPhotoRequest request)
        {
            string className = "UsersController";
            string methodName = "PutUserPhoto";
            string logEntry = $"PhotoHandle = {request?.PhotoHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // If user handle is null, return 404
            if (this.UserHandle == null)
            {
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            var userProfileEntity = await this.usersManager.ReadUserProfile(this.UserHandle, this.AppHandle);
            if (userProfileEntity == null)
            {
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            await this.usersManager.UpdateUserPhoto(
                this.UserHandle,
                this.AppHandle,
                request.PhotoHandle,
                DateTime.UtcNow,
                userProfileEntity);

            logEntry = $"PhotoHandle = {request?.PhotoHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Update user visibility
        /// </summary>
        /// <param name="request">Put user visibility request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not found. User is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("me/visibility")]
        [HttpPut]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PutUserVisibility([FromBody]PutUserVisibilityRequest request)
        {
            string className = "UsersController";
            string methodName = "PutUserVisibility";
            string logEntry = $"NewVisibility = {request?.Visibility}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // If user handle is null, return 404
            if (this.UserHandle == null)
            {
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            var userProfileEntity = await this.usersManager.ReadUserProfile(this.UserHandle, this.AppHandle);
            if (userProfileEntity == null)
            {
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            await this.usersManager.UpdateUserVisibility(
                this.UserHandle,
                this.AppHandle,
                request.Visibility,
                DateTime.UtcNow,
                userProfileEntity);

            logEntry = $"OldVisibility = {userProfileEntity?.Visibility}, NewVisibility = {request?.Visibility}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Get user profile
        /// </summary>
        /// <param name="userHandle">Handle of queried user</param>
        /// <returns>User profile</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The user is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{userHandle}")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(UserProfileView))]
        public async Task<IHttpActionResult> GetUser(string userHandle)
        {
            string className = "UsersController";
            string methodName = "GetUser";
            string logEntry = $"QueriedUserHandle = {userHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // If user handle is null, return 404
            if (userHandle == null)
            {
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            var userProfileView = await this.viewsManager.GetUserProfileView(userHandle, this.AppHandle, this.UserHandle);
            if (userProfileView == null)
            {
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            logEntry = $"QueriedUserHandle = {userHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(userProfileView);
        }

        /// <summary>
        /// Get my profile
        /// </summary>
        /// <returns>User profile</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The user is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("me")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(UserProfileView))]
        public async Task<IHttpActionResult> GetMyProfile()
        {
            string className = "UsersController";
            string methodName = "GetMyProfile";
            this.LogControllerStart(this.log, className, methodName);

            // If user handle is null, return 404
            if (this.UserHandle == null)
            {
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            var userProfileView = await this.viewsManager.GetUserProfileView(this.UserHandle, this.AppHandle, this.UserHandle);
            if (userProfileView == null)
            {
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            string logEntry = $"PhotoHandle = {userProfileView?.PhotoHandle}, Visibility = {userProfileView?.Visibility}, TotalTopics = {userProfileView?.TotalTopics}";
            logEntry += $", TotalFollowers = {userProfileView?.TotalFollowers}, TotalFollowing = {userProfileView?.TotalFollowing}, FollowerStatus = {userProfileView?.FollowerStatus}";
            logEntry += $", FollowingStatus = {userProfileView?.FollowingStatus}, ReviewStatus = {userProfileView?.ProfileStatus}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(userProfileView);
        }

        /// <summary>
        /// Get popular users
        /// </summary>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>User profile feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("popular")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<UserProfileView>))]
        public async Task<IHttpActionResult> GetPopularUsers(int cursor = 0, int limit = ApiDefaultValues.GetPopularUsersPageLimit)
        {
            string className = "UsersController";
            string methodName = "GetPopularUsers";
            this.LogControllerStart(this.log, className, methodName);

            var userFeedEntities = await this.popularUsersManager.ReadPopularUsers(this.AppHandle, cursor, limit + 1);
            var response = new FeedResponse<UserProfileView>();
            if (userFeedEntities.Count == limit + 1)
            {
                userFeedEntities.Remove(userFeedEntities.Last());
                response.Cursor = (cursor + limit).ToString();
            }

            response.Data = await this.viewsManager.GetUserProfileViews(userFeedEntities, this.AppHandle, this.UserHandle);

            // Concatenate all handles of the users in response data into long strings, delimited by ','
            string userHandles = null;
            if (response.Data != null)
            {
                userHandles = string.Join(",", response.Data.Select(v => v.UserHandle).ToArray());
            }

            string logEntry = $"CountUserHandles = {response.Data?.Count}, UserHandlesList = [{userHandles}]";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(response);
        }
    }
}
