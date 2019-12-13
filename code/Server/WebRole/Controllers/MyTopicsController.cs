// <copyright file="MyTopicsController.cs" company="Microsoft">
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

    /// <summary>
    /// APIs to query my topics
    /// </summary>
    [RoutePrefix("users/me/topics")]
    public class MyTopicsController : TopicsControllerBase
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
        /// Initializes a new instance of the <see cref="MyTopicsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="popularTopicsManager">Popular topics manager</param>
        /// <param name="viewsManager">Views manager</param>
        public MyTopicsController(
            ILog log,
            ITopicsManager topicsManager,
            IPopularTopicsManager popularTopicsManager,
            IViewsManager viewsManager)
            : base(log, viewsManager)
        {
            this.log = log;
            this.topicsManager = topicsManager;
            this.popularTopicsManager = popularTopicsManager;
        }

        /// <summary>
        /// Get my topics sorted by creation time
        /// </summary>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<TopicView>))]
        public async Task<IHttpActionResult> GetTopics(string cursor = null, int limit = ApiDefaultValues.GetTopicsPageLimit)
        {
            string className = "MyTopicsController";
            string methodName = "GetTopics";
            this.LogControllerStart(this.log, className, methodName);

            var topicFeedEntities = await this.topicsManager.ReadUserTopics(this.UserHandle, this.AppHandle, cursor, limit + 1);

            // Call the TopicsControllerBase's method for getting topics. This method of the base class takes care of tracing also.
            return await this.GetResponse(className, methodName, topicFeedEntities, limit);
        }

        /// <summary>
        /// Get my topics sorted by popularity
        /// </summary>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("popular")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<TopicView>))]
        public async Task<IHttpActionResult> GetPopularTopics(int cursor = 0, int limit = ApiDefaultValues.GetTopicsPageLimit)
        {
            string className = "MyTopicsController";
            string methodName = "GetPopularTopics";
            this.LogControllerStart(this.log, className, methodName);

            var topicFeedEntities = await this.popularTopicsManager.ReadPopularUserTopics(this.UserHandle, this.AppHandle, cursor, limit + 1);

            // Call the TopicsControllerBase's method for getting topics. This method of the base class takes care of tracing also.
            return await this.GetResponse(className, methodName, topicFeedEntities, cursor, limit);
        }
    }
}
