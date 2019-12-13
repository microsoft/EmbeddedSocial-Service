//-----------------------------------------------------------------------
// <copyright file="UserTopicsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class UserTopicsController.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon;

    /// <summary>
    /// APIs to query user topics
    /// </summary>
    [RoutePrefix("users/{userHandle:regex(^(?!me$).*)}/topics")]
    public class UserTopicsController : TopicsControllerBase
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Topics manager
        /// </summary>
        private readonly ITopicsManager topicsManager;

        /// <summary>
        /// Popular topics manager
        /// </summary>
        private readonly IPopularTopicsManager popularTopicsManager;

        /// <summary>
        /// Users manager
        /// </summary>
        private readonly IUsersManager usersManager;

        /// <summary>
        /// Relationships manager
        /// </summary>
        private readonly IRelationshipsManager relationshipsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTopicsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="popularTopicsManager">Popular topics manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="relationshipsManager">Relationships manager</param>
        /// <param name="viewsManager">Views manager</param>
        public UserTopicsController(
            ILog log,
            ITopicsManager topicsManager,
            IPopularTopicsManager popularTopicsManager,
            IUsersManager usersManager,
            IRelationshipsManager relationshipsManager,
            IViewsManager viewsManager)
            : base(log, viewsManager)
        {
            this.log = log;
            this.topicsManager = topicsManager;
            this.popularTopicsManager = popularTopicsManager;
            this.usersManager = usersManager;
            this.relationshipsManager = relationshipsManager;
        }

        /// <summary>
        /// Get user topics sorted by creation time
        /// </summary>
        /// <param name="userHandle">Handle of queried user</param>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app is not authorized.</response>
        /// <response code="404">Not Found. The user is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<TopicView>))]
        public async Task<IHttpActionResult> GetTopics(string userHandle, string cursor = null, int limit = ApiDefaultValues.GetTopicsPageLimit)
        {
            string className = "UserTopicsController";
            string methodName = "GetTopics";
            string logEntry = $"QueriedUserHandle = {userHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            IUserProfileEntity userProfileEntity = await this.usersManager.ReadUserProfile(userHandle, this.AppHandle);
            if (userProfileEntity == null)
            {
                this.NotFound(ResponseStrings.UserNotFound);
            }

            bool visibility = await this.relationshipsManager.ReadRelationshipVisibility(userProfileEntity, userHandle, this.UserHandle, this.AppHandle);
            if (!visibility)
            {
                this.Unauthorized(ResponseStrings.NotAllowed);
            }

            var topicFeedEntities = await this.topicsManager.ReadUserTopics(userHandle, this.AppHandle, cursor, limit + 1);

            // Call the TopicsControllerBase's method for getting topics. This method of the base class takes care of tracing also.
            return await this.GetResponse(className, methodName, topicFeedEntities, limit);
        }

        /// <summary>
        /// Get user topics sorted by popularity
        /// </summary>
        /// <param name="userHandle">Handle of queried user</param>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app is not authorized.</response>
        /// <response code="404">Not Found. The user is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("popular")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<TopicView>))]
        public async Task<IHttpActionResult> GetPopularTopics(string userHandle, int cursor = 0, int limit = ApiDefaultValues.GetTopicsPageLimit)
        {
            string className = "UserTopicsController";
            string methodName = "GetPopularTopics";
            string logEntry = $"QueriedUserHandle = {userHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            IUserProfileEntity userProfileEntity = await this.usersManager.ReadUserProfile(userHandle, this.AppHandle);
            if (userProfileEntity == null)
            {
                this.NotFound(ResponseStrings.UserNotFound);
            }

            bool visibility = await this.relationshipsManager.ReadRelationshipVisibility(userProfileEntity, userHandle, this.UserHandle, this.AppHandle);
            if (!visibility)
            {
                this.Unauthorized(ResponseStrings.NotAllowed);
            }

            var topicFeedEntities = await this.popularTopicsManager.ReadPopularUserTopics(userHandle, this.AppHandle, cursor, limit + 1);

            // Call the TopicsControllerBase's method for getting topics. This method of the base class takes care of tracing also.
            return await this.GetResponse(className, methodName, topicFeedEntities, cursor, limit);
        }
    }
}
