//-----------------------------------------------------------------------
// <copyright file="UserFollowingController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class UserFollowingController.
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
    using SocialPlus.Utils;

    /// <summary>
    /// API to query following
    /// </summary>
    [RoutePrefix("users/{userHandle:regex(^(?!me$).*)}/following")]
    public class UserFollowingController : RelationshipsControllerBase
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Relationships manager
        /// </summary>
        private readonly IRelationshipsManager relationshipsManager;

        /// <summary>
        /// Users manager
        /// </summary>
        private readonly IUsersManager usersManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFollowingController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="relationshipsManager">Relationships manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public UserFollowingController(
            ILog log,
            IRelationshipsManager relationshipsManager,
            ITopicsManager topicsManager,
            IUsersManager usersManager,
            IViewsManager viewsManager,
            IHandleGenerator handleGenerator)
            : base(log, relationshipsManager, topicsManager, usersManager, viewsManager, handleGenerator)
        {
            this.log = log;
            this.relationshipsManager = relationshipsManager;
            this.usersManager = usersManager;
        }

        /// <summary>
        /// Get following users of a user
        /// </summary>
        /// <param name="userHandle">Handle of queried user</param>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>User feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app is not authorized.</response>
        /// <response code="404">Not Found. The user is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<UserCompactView>))]
        public async Task<IHttpActionResult> GetFollowing(string userHandle, string cursor = null, int limit = ApiDefaultValues.GetUsersPageLimit)
        {
            string className = "UserFollowingController";
            string methodName = "GetFollowing";
            this.LogControllerStart(this.log, className, methodName);

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

            // Call the RelationshipControllerBase's method for getting the users. This method of the base class takes care of tracing also.
            return await this.GetRelationshipUsers(className, methodName, RelationshipType.Following, userHandle, this.AppHandle, cursor, limit);
        }
    }
}
