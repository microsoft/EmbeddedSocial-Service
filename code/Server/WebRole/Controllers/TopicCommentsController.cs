//-----------------------------------------------------------------------
// <copyright file="TopicCommentsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class TopicCommentsController.
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
    using SocialPlus.Server.Metrics;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon;
    using SocialPlus.Utils;

    /// <summary>
    /// APIs to create and query comments for a topic
    /// </summary>
    [RoutePrefix("topics/{topicHandle}/comments")]
    public class TopicCommentsController : BaseController
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
        /// Comments manager
        /// </summary>
        private readonly ICommentsManager commentsManager;

        /// <summary>
        /// Views manager
        /// </summary>
        private readonly IViewsManager viewsManager;

        /// <summary>
        /// Handle generator
        /// </summary>
        private readonly IHandleGenerator handleGenerator;

        /// <summary>
        /// Application metrics logger
        /// </summary>
        private readonly IApplicationMetrics applicationMetrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicCommentsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="commentsManager">Comments manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        /// <param name="applicationMetrics">Application metrics logger</param>
        public TopicCommentsController(
            ILog log,
            ITopicsManager topicsManager,
            ICommentsManager commentsManager,
            IViewsManager viewsManager,
            IHandleGenerator handleGenerator,
            IApplicationMetrics applicationMetrics)
        {
            this.log = log;
            this.topicsManager = topicsManager;
            this.commentsManager = commentsManager;
            this.viewsManager = viewsManager;
            this.handleGenerator = handleGenerator;
            this.applicationMetrics = applicationMetrics;
        }

        /// <summary>
        /// Create a new comment
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="request">Post comment request</param>
        /// <returns>Post comment response</returns>
        /// <response code="201">Created. The response contains the comment handle.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        [ResponseType(typeof(PostCommentResponse))]
        public async Task<IHttpActionResult> PostComment(string topicHandle, [FromBody]PostCommentRequest request)
        {
            string className = "TopicCommentsController";
            string methodName = "PostComment";
            string logEntry = $"TopicHandle = {topicHandle}, BlobHandle = {request?.BlobHandle}, BlobType = {request?.BlobType}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var topicEntity = await this.topicsManager.ReadTopic(topicHandle);
            if (topicEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNotFound);
            }

            if (topicEntity.AppHandle != this.AppHandle)
            {
                return this.Unauthorized(ResponseStrings.NotAllowed);
            }

            string commentHandle = this.handleGenerator.GenerateShortHandle();
            await this.commentsManager.CreateComment(
                ProcessType.Frontend,
                commentHandle,
                request.Text,
                request.BlobType,
                request.BlobHandle,
                request.Language,
                this.UserHandle,
                topicHandle,
                topicEntity.PublisherType,
                topicEntity.UserHandle,
                DateTime.UtcNow,
                ReviewStatus.Active,
                this.AppHandle,
                null);

            // Log new comment to app metrics
            this.applicationMetrics.Comment(
                ProcessType.Frontend.ToString(),
                commentHandle,
                request.Text,
                request.BlobType.ToString(),
                request.BlobHandle,
                request.Language,
                this.UserHandle,
                topicHandle,
                topicEntity.PublisherType.ToString(),
                topicEntity.UserHandle,
                DateTime.UtcNow,
                ReviewStatus.Active.ToString(),
                this.AppHandle);

            var response = new PostCommentResponse()
            {
                CommentHandle = commentHandle
            };

            logEntry += $", CommentHandle = {commentHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Created<PostCommentResponse>(new Uri(this.Request.RequestUri.AbsoluteUri + "/" + commentHandle), response);
        }

        /// <summary>
        /// Get comments for a topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Comment feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<CommentView>))]
        public async Task<IHttpActionResult> GetTopicComments(string topicHandle, string cursor = null, int limit = ApiDefaultValues.GetCommentsPageLimit)
        {
            string className = "TopicCommentsController";
            string methodName = "GetTopicComments";
            string logEntry = $"TopicHandle = {topicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var topicEntity = await this.topicsManager.ReadTopic(topicHandle);
            if (topicEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNotFound);
            }

            var commentFeedEntities = await this.commentsManager.ReadTopicComments(topicHandle, cursor, limit + 1);

            var response = new FeedResponse<CommentView>();
            if (commentFeedEntities.Count == limit + 1)
            {
                commentFeedEntities.Remove(commentFeedEntities.Last());
                response.Cursor = commentFeedEntities.Last().CommentHandle;
            }

            response.Data = await this.viewsManager.GetCommentViews(commentFeedEntities, topicEntity.AppHandle, this.UserHandle);

            // Concatenate all handles of the comments in response data into long strings, delimited by ','
            string commentHandles = null;
            if (response.Data != null)
            {
                commentHandles = string.Join(",", response.Data.Select(v => v.CommentHandle).ToArray());
            }

            logEntry = $"TopicHandle = {topicHandle}, CountCommentHandles = {response.Data?.Count}, CommentHandlesList = [{commentHandles}]";
            this.LogControllerEnd(this.log, className, methodName, logEntry);

            return this.Ok(response);
        }
    }
}
