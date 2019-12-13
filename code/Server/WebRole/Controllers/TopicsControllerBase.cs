// <copyright file="TopicsControllerBase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Principal;

    /// <summary>
    /// Topics base controller
    /// </summary>
    public class TopicsControllerBase : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Views manager
        /// </summary>
        private IViewsManager viewsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicsControllerBase"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="viewsManager">Views manager</param>
        public TopicsControllerBase(
            ILog log,
            IViewsManager viewsManager)
        {
            this.log = log;
            this.viewsManager = viewsManager;
        }

        /// <summary>
        /// Get response
        /// </summary>
        /// <param name="callerClassName">name of the controller class of the caller</param>
        /// <param name="callerMethodName">name of method insider controller class of the caller (should correspond to an HTTP action)</param>
        /// <param name="topicFeedEntities">Topic feed entities</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Http response</returns>
        protected async Task<IHttpActionResult> GetResponse(string callerClassName, string callerMethodName, IList<ITopicFeedEntity> topicFeedEntities, int limit)
        {
            var response = new FeedResponse<TopicView>();
            if (topicFeedEntities.Count == limit + 1)
            {
                topicFeedEntities.Remove(topicFeedEntities.Last());
                response.Cursor = topicFeedEntities.Last().TopicHandle;
            }

            response.Data = await this.viewsManager.GetTopicViews(topicFeedEntities, this.UserHandle);

            // Concatenate all handles of the topics in response data into long strings, delimited by ','
            string topicHandles = null;
            if (response.Data != null)
            {
                topicHandles = string.Join(",", response.Data.Select(v => v.TopicHandle).ToArray());
            }

            string logEntry = $"CountTopicHandles = {response.Data?.Count}, TopicHandlesList = [{topicHandles}]";
            this.LogControllerEnd(this.log, callerClassName, callerMethodName, logEntry);
            return this.Ok(response);
        }

        /// <summary>
        /// Get response
        /// </summary>
        /// <param name="callerClassName">name of the controller class of the caller</param>
        /// <param name="callerMethodName">name of method insider controller class of the caller (should correspond to an HTTP action)</param>
        /// <param name="topicFeedEntities">Topic feed entities</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Http response</returns>
        protected async Task<IHttpActionResult> GetResponse(string callerClassName, string callerMethodName, IList<ITopicFeedEntity> topicFeedEntities, int cursor, int limit)
        {
            var response = new FeedResponse<TopicView>();
            if (topicFeedEntities.Count == limit + 1)
            {
                topicFeedEntities.Remove(topicFeedEntities.Last());
                response.Cursor = (cursor + limit).ToString();
            }

            response.Data = await this.viewsManager.GetTopicViews(topicFeedEntities, this.UserHandle);

            // Concatenate all handles of the topics in response data as one long string, delimited by ','
            string topicHandles = null;
            if (response.Data != null)
            {
                topicHandles = string.Join(",", response.Data.Select(v => v.TopicHandle).ToArray());
            }

            string logEntry = $"CountTopicHandles = {response.Data?.Count}, TopicHandlesList = [{topicHandles}]";
            this.LogControllerEnd(this.log, callerClassName, callerMethodName, logEntry);

            return this.Ok(response);
        }
    }
}
