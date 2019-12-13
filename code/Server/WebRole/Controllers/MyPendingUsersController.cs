// <copyright file="MyPendingUsersController.cs" company="Microsoft">
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
    /// APIs to reject follower request and query my pending users
    /// </summary>
    [RoutePrefix("users/me/pending_users")]
    public class MyPendingUsersController : RelationshipsControllerBase
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
        /// Initializes a new instance of the <see cref="MyPendingUsersController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="relationshipsManager">Relationships manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public MyPendingUsersController(
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
        }

        /// <summary>
        /// Reject follower request
        /// </summary>
        /// <param name="userHandle">Handle of pending user</param>
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
        public async Task<IHttpActionResult> DeletePendingUser(string userHandle)
        {
            string className = "MyPendingUsersController";
            string methodName = "DeletePendingUser";
            string logEntry = $"PendingUserHandle = {userHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // Call the RelationshipControllerBase's method for updating the relationship. This method of the base class takes care of tracing also.
            return await this.UpdateRelationshipToUser(className, methodName, RelationshipOperation.RejectUser, userHandle);
        }

        /// <summary>
        /// Get my pending users
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
        public async Task<IHttpActionResult> GetPendingUsers(string cursor = null, int limit = ApiDefaultValues.GetUsersPageLimit)
        {
            string className = "MyPendingUsersController";
            string methodName = "GetPendingUsers";
            this.LogControllerStart(this.log, className, methodName);

            // Call the RelationshipControllerBase's method for getting the users. This method of the base class takes care of tracing also.
            return await this.GetRelationshipUsers(className, methodName, RelationshipType.PendingUser, this.UserHandle, this.AppHandle, cursor, limit);
        }

        /// <summary>
        /// Get my pending users count
        /// </summary>
        /// <returns>Count of pending users</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("count")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(CountResponse))]
        public async Task<IHttpActionResult> GetPendingUsersCount()
        {
            string className = "MyPendingUsersController";
            string methodName = "GetPendingUsersCount";
            this.LogControllerStart(this.log, className, methodName);

            long? count = await this.relationshipsManager.ReadPendingUsersCount(this.UserHandle, this.AppHandle);
            var countResponse = new CountResponse()
            {
                Count = count.HasValue ? count.Value : 0
            };

            string logEntry = $"CountPendingUsers = {count}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(countResponse);
        }
    }
}
