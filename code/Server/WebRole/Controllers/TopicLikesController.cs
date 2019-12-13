//-----------------------------------------------------------------------
// <copyright file="TopicLikesController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class TopicLikesController.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Metrics;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Utils;

    /// <summary>
    /// APIs to add, remove and query topic likes
    /// </summary>
    [RoutePrefix("topics/{topicHandle}/likes")]
    public class TopicLikesController : LikesControllerBase
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicLikesController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="likesManager">Likes manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="commentsManager">Comments manager</param>
        /// <param name="repliesManager">Replies manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        /// <param name="applicationMetrics">Application metrics logger</param>
        public TopicLikesController(
            ILog log,
            ILikesManager likesManager,
            ITopicsManager topicsManager,
            ICommentsManager commentsManager,
            IRepliesManager repliesManager,
            IViewsManager viewsManager,
            IHandleGenerator handleGenerator,
            IApplicationMetrics applicationMetrics)
            : base(log, likesManager, topicsManager, commentsManager, repliesManager, viewsManager, handleGenerator, applicationMetrics)
        {
            this.log = log;
        }

        /// <summary>
        /// Add like to topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic is not found.</response>
        /// <response code="409">Conflict. Newer item exists.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PostLike(string topicHandle)
        {
            string className = "TopicLikesController";
            string methodName = "PostLike";
            string logEntry = $"TopicHandle = {topicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // Call the LikesControllerBase's method for updating likes. This method of the base class takes care of tracing also.
            return await this.UpdateLike(className, methodName, ContentType.Topic, topicHandle, true);
        }

        /// <summary>
        /// Remove like from topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic is not found.</response>
        /// <response code="409">Conflict. Newer item exists.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("me")]
        [HttpDelete]
        [VersionRange("All")]
        public async Task<IHttpActionResult> DeleteLike(string topicHandle)
        {
            string className = "TopicLikesController";
            string methodName = "DeleteLike";
            string logEntry = $"TopicHandle = {topicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // Call the LikesControllerBase's method for updating likes. This method of the base class takes care of tracing also.
            return await this.UpdateLike(className, methodName, ContentType.Topic, topicHandle, false);
        }

        /// <summary>
        /// Get likes for topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>User feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app is not authorized.</response>
        /// <response code="404">Not Found. The topic is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<UserCompactView>))]
        public async Task<IHttpActionResult> GetLikes(string topicHandle, string cursor = null, int limit = ApiDefaultValues.GetLikesPageLimit)
        {
            string className = "TopicLikesController";
            string methodName = "GetLikes";
            string logEntry = $"TopicHandle = {topicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // Call the LikesControllerBase's method for getting likes. This method of the base class takes care of tracing also.
            return await this.GetContentLikes(className, methodName, topicHandle, cursor, limit);
        }
    }
}
