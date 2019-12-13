//-----------------------------------------------------------------------
// <copyright file="MyPinsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class MyPinsController.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon;
    using SocialPlus.Utils;

    /// <summary>
    /// APIs to create, delete and query my pins
    /// </summary>
    [RoutePrefix("users/me/pins")]
    public class MyPinsController : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Pins manager
        /// </summary>
        private readonly IPinsManager pinsManager;

        /// <summary>
        /// Topics manager
        /// </summary>
        private readonly ITopicsManager topicsManager;

        /// <summary>
        /// Views manager
        /// </summary>
        private readonly IViewsManager viewsManager;

        /// <summary>
        /// Handle generator
        /// </summary>
        private readonly IHandleGenerator handleGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyPinsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="pinsManager">Pins manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public MyPinsController(ILog log, IPinsManager pinsManager, ITopicsManager topicsManager, IViewsManager viewsManager, IHandleGenerator handleGenerator)
        {
            this.log = log;
            this.pinsManager = pinsManager;
            this.topicsManager = topicsManager;
            this.viewsManager = viewsManager;
            this.handleGenerator = handleGenerator;
        }

        /// <summary>
        /// Pin a topic
        /// </summary>
        /// <param name="request">Post pin request</param>
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
        public async Task<IHttpActionResult> PostPin([FromBody]PostPinRequest request)
        {
            string className = "MyPinsController";
            string methodName = "PostPin";
            string logEntry = $"PinnedTopicHandle = {request?.TopicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var topicEntity = await this.topicsManager.ReadTopic(request.TopicHandle);
            if (topicEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNotFound);
            }

            var pinLookupEntity = await this.pinsManager.ReadPin(this.UserHandle, request.TopicHandle);
            DateTime currentTime = DateTime.UtcNow;
            if (pinLookupEntity != null && pinLookupEntity.LastUpdatedTime > currentTime)
            {
                return this.Conflict(ResponseStrings.NewerItemExists);
            }

            string pinHandle = this.handleGenerator.GenerateShortHandle();
            await this.pinsManager.UpdatePin(
                ProcessType.Frontend,
                pinHandle,
                this.UserHandle,
                request.TopicHandle,
                true,
                topicEntity.PublisherType,
                topicEntity.UserHandle,
                topicEntity.AppHandle,
                currentTime,
                pinLookupEntity);

            logEntry += $", PinHandle = {pinHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Unpin a topic
        /// </summary>
        /// <param name="topicHandle">Handle of pinned topic</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic is not found.</response>
        /// <response code="409">Conflict. Newer item exists.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{topicHandle}")]
        [HttpDelete]
        [VersionRange("All")]
        public async Task<IHttpActionResult> DeletePin(string topicHandle)
        {
            string className = "MyPinsController";
            string methodName = "DeletePin";
            string logEntry = $"PinnedTopicHandle = {topicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var topicEntity = await this.topicsManager.ReadTopic(topicHandle);
            if (topicEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNotFound);
            }

            var pinLookupEntity = await this.pinsManager.ReadPin(this.UserHandle, topicHandle);
            DateTime currentTime = DateTime.UtcNow;
            if (pinLookupEntity != null && pinLookupEntity.LastUpdatedTime > currentTime)
            {
                return this.Conflict(ResponseStrings.NewerItemExists);
            }

            await this.pinsManager.UpdatePin(
                ProcessType.Frontend,
                null,
                this.UserHandle,
                topicHandle,
                false,
                topicEntity.PublisherType,
                topicEntity.UserHandle,
                topicEntity.AppHandle,
                currentTime,
                pinLookupEntity);

            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Get my pins
        /// </summary>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<TopicView>))]
        public async Task<IHttpActionResult> GetPins(string cursor = null, int limit = ApiDefaultValues.GetPinsPageLimit)
        {
            string className = "MyPinsController";
            string methodName = "GetPins";
            this.LogControllerStart(this.log, className, methodName);

            var pinFeedEntities = await this.pinsManager.ReadPins(this.UserHandle, this.AppHandle, cursor, limit + 1);
            FeedResponse<TopicView> response = new FeedResponse<TopicView>();
            if (pinFeedEntities.Count == limit + 1)
            {
                pinFeedEntities.Remove(pinFeedEntities.Last());
                response.Cursor = pinFeedEntities.Last().PinHandle;
            }

            response.Data = await this.viewsManager.GetTopicViews(pinFeedEntities, this.UserHandle);

            // Concatenate all handles of the topics in response data into long strings, delimited by ','
            string topicHandles = null;
            if (response.Data != null)
            {
                topicHandles = string.Join(",", response.Data.Select(v => v.TopicHandle).ToArray());
            }

            string logEntry = $"CountPinHandles = {response.Data?.Count}, TopicHandlesList = [{topicHandles}]";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(response);
        }
    }
}
