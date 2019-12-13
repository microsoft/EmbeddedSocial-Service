// <copyright file="MyBlockedUsersController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Utils;

    /// <summary>
    /// APIs to block, unblock and query my blocked users
    /// </summary>
    [RoutePrefix("users/me/blocked_users")]
    public class MyBlockedUsersController : RelationshipsControllerBase
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyBlockedUsersController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="relationshipsManager">Relationships manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public MyBlockedUsersController(
            ILog log,
            IRelationshipsManager relationshipsManager,
            ITopicsManager topicsManager,
            IUsersManager usersManager,
            IViewsManager viewsManager,
            IHandleGenerator handleGenerator)
            : base(log, relationshipsManager, topicsManager, usersManager, viewsManager, handleGenerator)
        {
            this.log = log;
        }

        /// <summary>
        /// Block a user
        /// </summary>
        /// <remarks>
        /// After I block a user, that user will no longer be able to see topics authored by me.
        /// However, that user will continue to see comments and replies that I create on
        /// topics authored by other users or by the app. That user will also be able to observe
        /// that activities have been performed by users on my topics.
        /// I will no longer appear in that user's following feed, and that user will no longer
        /// appear in my followers feed.
        /// If I am following that user, that relationship will survive and I will continue to see
        /// topics and activities by that user and I will appear in that user's follower feed and
        /// that user will appear in my following feed.
        /// </remarks>
        /// <param name="request">Post blocked user request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The user is not found.</response>
        /// <response code="409">Conflict. Newer item exists.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PostBlockedUser([FromBody]PostBlockedUserRequest request)
        {
            string className = "MyBlockedUsersController";
            string methodName = "PostBlockedUser";
            string logEntry = $"BlockedUserHandle = {request?.UserHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // Call the RelationshipControllerBase's method for updating the relationship. This method of the base class takes care of tracing also.
            return await this.UpdateRelationshipToUser(className, methodName, RelationshipOperation.BlockUser, request.UserHandle);
        }

        /// <summary>
        /// Unblock a user
        /// </summary>
        /// After I unblock a user that I had previously blocked, that user will be able to
        /// see topics authored by me. That user will be able to add me to their following feed.
        /// <param name="userHandle">Handle of blocked user</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The user is not found.</response>
        /// <response code="409">Conflict. Newer item exists.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{userHandle}")]
        [HttpDelete]
        [VersionRange("All")]
        public async Task<IHttpActionResult> DeleteBlockedUser(string userHandle)
        {
            string className = "MyBlockedUsersController";
            string methodName = "DeleteBlockedUser";
            string logEntry = $"BlockedUserHandle = {userHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // Call the RelationshipControllerBase's method for updating the relationship. This method of the base class takes care of tracing also.
            return await this.UpdateRelationshipToUser(className, methodName, RelationshipOperation.UnblockUser, userHandle);
        }

        /// <summary>
        /// Get my blocked users
        /// </summary>
        /// <remarks>
        /// This is a feed of users that I have blocked. Any user on this list
        /// cannot see topics authored by me. However, any such user will see comments
        /// and replies that I create on topics authored by other users or by the app.
        /// Any such user will also be able to observe that activities have been performed
        /// by users on my topics.
        /// I will not appear in any such user's following feed, and those users will not
        /// appear in my followers feed.
        /// If I am following any user in this feed, that relationship will continue and I
        /// will continue to see topics and activities by that user and I will appear in
        /// that user's follower feed and that user will appear in my following feed.
        /// </remarks>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>User feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<UserCompactView>))]
        public async Task<IHttpActionResult> GetBlockedUsers(string cursor = null, int limit = ApiDefaultValues.GetUsersPageLimit)
        {
            string className = "MyBlockedUsersController";
            string methodName = "GetBlockedUsers";
            this.LogControllerStart(this.log, className, methodName);

            // Call the RelationshipControllerBase's method for getting the users. This method of the base class takes care of tracing also.
            return await this.GetRelationshipUsers(className, methodName, RelationshipType.BlockedUser, this.UserHandle, this.AppHandle, cursor, limit);
        }
    }
}
