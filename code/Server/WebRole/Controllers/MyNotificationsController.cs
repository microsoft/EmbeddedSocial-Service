//-----------------------------------------------------------------------
// <copyright file="MyNotificationsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class MyNotificationsController.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.Controllers
{
    using System.Linq;
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
    /// API calls to get notifications, get notifications count, and update notification status
    /// </summary>
    [RoutePrefix("users/me/notifications")]
    public class MyNotificationsController : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Notifications manager
        /// </summary>
        private readonly INotificationsManager notificationsManager;

        /// <summary>
        /// Views manager
        /// </summary>
        private readonly IViewsManager viewsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyNotificationsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="notificationsManager">Notifications manager</param>
        /// <param name="viewsManager">Views manager</param>
        public MyNotificationsController(
            ILog log,
            INotificationsManager notificationsManager,
            IViewsManager viewsManager)
        {
            this.log = log;
            this.notificationsManager = notificationsManager;
            this.viewsManager = viewsManager;
        }

        /// <summary>
        /// Update notifications status
        /// </summary>
        /// <remarks>
        /// This API call records the most recent notification that the user has read (or seen).
        /// In the GET notifications API call, each notification will have an unread status.
        /// Any notifications that are newer than this ReadActivityHandle will have an unread status of true.
        /// Any notifications that correspond to this ReadActivityHandle or older will have an unread status of false.
        /// If this API call has never been made, then all notifications will have an unread status of true.
        /// </remarks>
        /// <param name="request">Put notifications status request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="409">Conflict. Newer item exists.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("status")]
        [HttpPut]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PutNotificationsStatus([FromBody]PutNotificationsStatusRequest request)
        {
            string className = "MyNotificationsController";
            string methodName = "PutNotificationsStatus";
            string logEntry = $"ActivityHandle = {request?.ReadActivityHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            INotificationsStatusEntity notificationsStatusEntity = await this.notificationsManager.ReadNotificationsStatus(this.UserHandle, this.AppHandle);
            if (notificationsStatusEntity != null)
            {
                if (string.CompareOrdinal(request.ReadActivityHandle, notificationsStatusEntity.ReadActivityHandle) > 0)
                {
                    return this.Conflict(ResponseStrings.NewerItemExists);
                }
            }

            await this.notificationsManager.UpdateNotificationsStatus(this.UserHandle, this.AppHandle, request.ReadActivityHandle, notificationsStatusEntity);
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Get my notifications
        /// </summary>
        /// <remarks>
        /// This gets a feed of activities.
        /// This feed is time ordered, with the most recent activity first.
        /// An activity is added to this feed when any user other than myself does one of the following 6 actions:
        /// (a) creates a comment to my topic; (b) creates a reply to my comment; (c) likes my topic; (d) follows me;
        /// (e) requests to follow me when I'm a private user; (f) accepts my request to follow them when they are a private user.
        /// Each activity has an unread status, which is controlled by doing a PUT on the status API call.
        /// If a user that performed the activity is deleted, then that activity will no longer appear in this feed.
        /// If an activity is performed on content that is then deleted, that activity will no longer appear in this feed.
        /// If a user has un-done an activity (e.g. unlike a previous like), then that activity will no longer appear in this feed.
        /// When activityType is Like, the activityHandle is the likeHandle that uniquely identifies the new like.
        /// When activityType is Comment, the activityHandle is the commentHandle that uniquely identifies the new comment.
        /// When activityType is Reply, the activityHandle is the replyHandle that uniquely identifies the new reply.
        /// ActivityType values of CommentPeer and ReplyPeer are currently not used.
        /// When activityType is Following or FollowRequest or FollowAccept, the activityHandle is the relationshipHandle
        /// that uniquely identifies the relationship between the two users.
        /// </remarks>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Activity feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app or user is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<ActivityView>))]
        public async Task<IHttpActionResult> GetNotifications(string cursor = null, int limit = ApiDefaultValues.GetActivitiesPageLimit)
        {
            string className = "MyNotificationsController";
            string methodName = "GetNotifications";
            this.LogControllerStart(this.log, className, methodName);

            var taskNotificationsStatus = this.notificationsManager.ReadNotificationsStatus(this.UserHandle, this.AppHandle);
            var taskNotifications = this.notificationsManager.ReadNotifications(this.UserHandle, this.AppHandle, cursor, limit + 1);
            var activityFeedEntities = await taskNotifications;
            FeedResponse<ActivityView> response = new FeedResponse<ActivityView>();
            if (activityFeedEntities.Count == limit + 1)
            {
                activityFeedEntities.Remove(activityFeedEntities.Last());
                response.Cursor = activityFeedEntities.Last().ActivityHandle;
            }

            string readActivityHandle = null;
            var notificationsStatusEntity = await taskNotificationsStatus;
            if (notificationsStatusEntity != null)
            {
                readActivityHandle = notificationsStatusEntity.ReadActivityHandle;
            }

            response.Data = await this.viewsManager.GetActivityViews(activityFeedEntities, readActivityHandle, this.UserHandle);

            // Concatenate all handles of the activities in response data into long strings, delimited by ','
            string activityHandles = null;
            if (response.Data != null)
            {
                activityHandles = string.Join(",", response.Data.Select(v => v.ActivityHandle).ToArray());
            }

            string logEntry = $"CountActivityHandles = {response.Data?.Count}, ActivityHandlesList = [{activityHandles}]";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(response);
        }

        /// <summary>
        /// Get unread notifications count
        /// </summary>
        /// <remarks>
        /// This returns a count of activities in my notification feed that have an unread status of true.
        /// </remarks>
        /// <returns>Unread notifications count</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app or user is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("count")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(CountResponse))]
        public async Task<IHttpActionResult> GetNotificationsCount()
        {
            string className = "MyNotificationsController";
            string methodName = "GetNotificationsCount";
            this.LogControllerStart(this.log, className, methodName);

            long? count = await this.notificationsManager.ReadNotificationsCount(this.UserHandle, this.AppHandle);
            var countResponse = new CountResponse()
            {
                Count = count.HasValue ? count.Value : 0
            };

            string logEntry = $"CountActivityHandles = {countResponse?.Count}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(countResponse);
        }
    }
}
