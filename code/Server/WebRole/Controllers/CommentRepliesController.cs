//-----------------------------------------------------------------------
// <copyright file="CommentRepliesController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class CommentRepliesController.
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
    /// APIs to create and query replies for a comment
    /// </summary>
    [RoutePrefix("comments/{commentHandle}/replies")]
    public class CommentRepliesController : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

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
        /// Initializes a new instance of the <see cref="CommentRepliesController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="commentsManager">Comments manager</param>
        /// <param name="repliesManager">Replies manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public CommentRepliesController(
            ILog log,
            ICommentsManager commentsManager,
            IRepliesManager repliesManager,
            IViewsManager viewsManager,
            IHandleGenerator handleGenerator)
        {
            this.log = log;
            this.commentsManager = commentsManager;
            this.repliesManager = repliesManager;
            this.viewsManager = viewsManager;
            this.handleGenerator = handleGenerator;
        }

        /// <summary>
        /// Create a new reply
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="request">Post reply request</param>
        /// <returns>Post reply response</returns>
        /// <response code="201">Created. The response contains the reply handle.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The comment is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        [ResponseType(typeof(PostReplyResponse))]
        public async Task<IHttpActionResult> PostReply(string commentHandle, [FromBody]PostReplyRequest request)
        {
            string className = "CommentRepliesController";
            string methodName = "PostReply";
            string logEntry = $"CommentHandle = {commentHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var commentEntity = await this.commentsManager.ReadComment(commentHandle);
            if (commentEntity == null)
            {
                return this.NotFound(ResponseStrings.CommentNotFound);
            }

            if (commentEntity.AppHandle != this.AppHandle)
            {
                return this.Unauthorized(ResponseStrings.NotAllowed);
            }

            string replyHandle = this.handleGenerator.GenerateShortHandle();
            await this.repliesManager.CreateReply(
                ProcessType.Frontend,
                replyHandle,
                request.Text,
                request.Language,
                this.UserHandle,
                commentHandle,
                commentEntity.UserHandle,
                commentEntity.TopicHandle,
                DateTime.UtcNow,
                ReviewStatus.Active,
                this.AppHandle,
                null);

            var response = new PostReplyResponse()
            {
                ReplyHandle = replyHandle
            };

            logEntry += $", ReplyHandle = {replyHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Created<PostReplyResponse>(new Uri(this.Request.RequestUri.AbsoluteUri + "/" + commentHandle), response);
        }

        /// <summary>
        /// Get replies for a comment
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Reply feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The comment is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<ReplyView>))]
        public async Task<IHttpActionResult> GetReplies(string commentHandle, string cursor = null, int limit = ApiDefaultValues.GetRepliesPageLimit)
        {
            string className = "CommentRepliesController";
            string methodName = "GetReplies";
            string logEntry = $"CommentHandle = {commentHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var commentEntity = await this.commentsManager.ReadComment(commentHandle);
            if (commentEntity == null)
            {
                return this.NotFound(ResponseStrings.CommentNotFound);
            }

            var replyFeedEntities = await this.repliesManager.ReadCommentReplies(commentHandle, cursor, limit + 1);

            var response = new FeedResponse<ReplyView>();
            if (replyFeedEntities.Count == limit + 1)
            {
                replyFeedEntities.Remove(replyFeedEntities.Last());
                response.Cursor = replyFeedEntities.Last().ReplyHandle;
            }

            response.Data = await this.viewsManager.GetReplyViews(replyFeedEntities, commentEntity.AppHandle, this.UserHandle);

            // Concatenate all handles of the replies in response data into long strings, delimited by ','
            string replyHandles = null;
            if (response.Data != null)
            {
                replyHandles = string.Join(",", response.Data.Select(v => v.ReplyHandle).ToArray());
            }

            logEntry += $", CountReplyHandles = {response.Data?.Count}, ReplyHandlesList = [{replyHandles}]";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(response);
        }
    }
}
