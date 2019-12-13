// <copyright file="LikesControllerBase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Metrics;
    using SocialPlus.Server.WebRoleCommon;
    using SocialPlus.Utils;

    /// <summary>
    /// Likes base controller
    /// </summary>
    public class LikesControllerBase : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Likes manager
        /// </summary>
        private readonly ILikesManager likesManager;

        /// <summary>
        /// Topics manager
        /// </summary>
        private readonly ITopicsManager topicsManager;

        /// <summary>
        /// Comments manager
        /// </summary>
        private readonly ICommentsManager commentsManager;

        /// <summary>
        /// Replies manager
        /// </summary>
        private readonly IRepliesManager repliesManager;

        /// <summary>
        /// Views manager
        /// </summary>
        private readonly IViewsManager viewsManager;

        /// <summary>
        /// Handle generator
        /// </summary>
        private readonly IHandleGenerator handleGenerator;

        /// <summary>
        /// Application Metrics logger
        /// </summary>
        private readonly IApplicationMetrics applicationMetrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="LikesControllerBase"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="likesManager">Likes manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="commentsManager">Comments manager</param>
        /// <param name="repliesManager">Replies manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        /// <param name="applicationMetrics">Application metrics logger</param>
        public LikesControllerBase(
            ILog log,
            ILikesManager likesManager,
            ITopicsManager topicsManager,
            ICommentsManager commentsManager,
            IRepliesManager repliesManager,
            IViewsManager viewsManager,
            IHandleGenerator handleGenerator,
            IApplicationMetrics applicationMetrics)
        {
            this.log = log;
            this.likesManager = likesManager;
            this.topicsManager = topicsManager;
            this.commentsManager = commentsManager;
            this.repliesManager = repliesManager;
            this.viewsManager = viewsManager;
            this.handleGenerator = handleGenerator;
            this.applicationMetrics = applicationMetrics;
        }

        /// <summary>
        /// Update like for content
        /// </summary>
        /// <param name="callerClassName">name of the controller class of the caller</param>
        /// <param name="callerMethodName">name of method insider controller class of the caller (should correspond to an HTTP action)</param>
        /// <param name="contentType">Content type</param>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="liked">Like status</param>
        /// <returns>Http response</returns>
        protected async Task<IHttpActionResult> UpdateLike(string callerClassName, string callerMethodName, ContentType contentType, string contentHandle, bool liked)
        {
            PublisherType contentPublisherType = PublisherType.User;
            string contentUserHandle = null;
            string contentAppHandle = this.AppHandle;
            DateTime contentCreatedTime = DateTime.MinValue;
            string logEntry = null;

            if (contentType == ContentType.Topic)
            {
                var topicEntity = await this.topicsManager.ReadTopic(contentHandle);
                if (topicEntity == null)
                {
                    return this.NotFound(ResponseStrings.TopicNotFound);
                }

                contentPublisherType = topicEntity.PublisherType;
                contentUserHandle = topicEntity.UserHandle;
                contentAppHandle = topicEntity.AppHandle;
                contentCreatedTime = topicEntity.CreatedTime;
                logEntry = $"TopicHandle = {contentHandle}";
            }
            else if (contentType == ContentType.Comment)
            {
                var commentEntity = await this.commentsManager.ReadComment(contentHandle);
                if (commentEntity == null)
                {
                    return this.NotFound(ResponseStrings.CommentNotFound);
                }

                contentPublisherType = PublisherType.User;
                contentUserHandle = commentEntity.UserHandle;
                contentAppHandle = commentEntity.AppHandle;
                contentCreatedTime = commentEntity.CreatedTime;
                logEntry = $"CommentHandle = {contentHandle}";
            }
            else if (contentType == ContentType.Reply)
            {
                var replyEntity = await this.repliesManager.ReadReply(contentHandle);
                if (replyEntity == null)
                {
                    return this.NotFound(ResponseStrings.ReplyNotFound);
                }

                contentPublisherType = PublisherType.User;
                contentUserHandle = replyEntity.UserHandle;
                contentAppHandle = replyEntity.AppHandle;
                contentCreatedTime = replyEntity.CreatedTime;
                logEntry = $"ReplyHandle = {contentHandle}";
            }

            var likeLookupEntity = await this.likesManager.ReadLike(contentHandle, this.UserHandle);
            DateTime currentTime = DateTime.UtcNow;
            if (likeLookupEntity != null && likeLookupEntity.LastUpdatedTime > currentTime)
            {
                return this.Conflict(ResponseStrings.NewerItemExists);
            }

            string likeHandle = null;
            if (liked)
            {
                likeHandle = this.handleGenerator.GenerateShortHandle();
            }

            await this.likesManager.UpdateLike(
                ProcessType.Frontend,
                likeHandle,
                contentType,
                contentHandle,
                this.UserHandle,
                liked,
                contentPublisherType,
                contentUserHandle,
                contentCreatedTime,
                contentAppHandle,
                currentTime,
                likeLookupEntity);

            // Log new like to app metrics
            this.applicationMetrics.Like(
                ProcessType.Frontend.ToString(),
                likeHandle,
                contentType.ToString(),
                contentHandle,
                this.UserHandle,
                liked.ToString(),
                contentPublisherType.ToString(),
                contentUserHandle,
                contentCreatedTime,
                contentAppHandle,
                currentTime,
                likeLookupEntity?.LastUpdatedTime,
                likeLookupEntity?.Liked.ToString(),
                likeLookupEntity?.LikeHandle);

            logEntry += $", LikeHandle = {likeHandle}, OldLikedStatus = {likeLookupEntity?.Liked}, NewLikedStatus = {liked}";
            this.LogControllerEnd(this.log, callerClassName, callerMethodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Get content likes
        /// </summary>
        /// <param name="callerClassName">name of the controller class of the caller</param>
        /// <param name="callerMethodName">name of method insider controller class of the caller (should correspond to an HTTP action)</param>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Http response</returns>
        protected async Task<IHttpActionResult> GetContentLikes(string callerClassName, string callerMethodName, string contentHandle, string cursor, int limit)
        {
            var likeFeedEntities = await this.likesManager.ReadLikes(contentHandle, cursor, limit + 1);
            FeedResponse<UserCompactView> response = new FeedResponse<UserCompactView>();
            if (likeFeedEntities.Count == limit + 1)
            {
                likeFeedEntities.Remove(likeFeedEntities.Last());
                response.Cursor = likeFeedEntities.Last().LikeHandle;
            }

            response.Data = await this.viewsManager.GetUserCompactViews(likeFeedEntities, this.AppHandle, this.UserHandle);

            // Concatenate all handles of the users in response data into long strings, delimited by ','
            string userHandles = null;
            if (response.Data != null)
            {
                userHandles = string.Join(",", response.Data.Select(v => v.UserHandle).ToArray());
            }

            string logEntry = $"ContentHandle = {contentHandle}, CountLikeHandles = {response.Data?.Count}, UserHandlesList = [{userHandles}]";
            this.LogControllerEnd(this.log, callerClassName, callerMethodName, logEntry);
            return this.Ok(response);
        }
    }
}
