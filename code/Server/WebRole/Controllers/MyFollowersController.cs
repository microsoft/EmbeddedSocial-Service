// <copyright file="MyFollowersController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Utils;

    /// <summary>
    /// APIs to accept, delete and query my followers
    /// </summary>
    [RoutePrefix("users/me/followers")]
    public class MyFollowersController : RelationshipsControllerBase
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyFollowersController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="relationshipsManager">Relationships manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public MyFollowersController(
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
        /// Accept follower request
        /// </summary>
        /// <param name="request">Post follower request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="403">Forbidden. The request cannot be performed.</response>
        /// <response code="404">Not Found. The user is not found.</response>
        /// <response code="409">Conflict. Newer item exists.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PostFollower([FromBody]PostFollowerRequest request)
        {
            string className = "MyFollowersController";
            string methodName = "PostFollower";
            string logEntry = $"FollowerUserHandle = {request?.UserHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // Call the RelationshipControllerBase's method for updating the relationship. This method of the base class takes care of tracing also.
            return await this.UpdateRelationshipToUser(className, methodName, RelationshipOperation.AcceptUser, request.UserHandle);
        }

        /// <summary>
        /// Remove follower
        /// </summary>
        /// <param name="userHandle">Handle of follower user</param>
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
        public async Task<IHttpActionResult> DeleteFollower(string userHandle)
        {
            string className = "MyFollowersController";
            string methodName = "DeleteFollower";
            string logEntry = $"FollowerUserHandle = {userHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // Call the RelationshipControllerBase's method for updating the relationship. This method of the base class takes care of tracing also.
            return await this.UpdateRelationshipToUser(className, methodName, RelationshipOperation.DeleteFollower, userHandle);
        }

        /// <summary>
        /// Get my followers
        /// </summary>
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
        public async Task<IHttpActionResult> GetFollowers(string cursor = null, int limit = ApiDefaultValues.GetUsersPageLimit)
        {
            string className = "MyFollowersController";
            string methodName = "GetFollowers";
            this.LogControllerStart(this.log, className, methodName);

            // Call the RelationshipControllerBase's method for getting the users. This method of the base class takes care of tracing also.
            return await this.GetRelationshipUsers(className, methodName, RelationshipType.Follower, this.UserHandle, this.AppHandle, cursor, limit);
        }
    }
}
