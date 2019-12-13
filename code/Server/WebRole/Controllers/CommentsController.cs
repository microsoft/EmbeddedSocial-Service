// <copyright file="CommentsController.cs" company="Microsoft">
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
    using SocialPlus.Server.WebRoleCommon;

    /// <summary>
    /// APIs to delete and query comments
    /// </summary>
    [RoutePrefix("comments")]
    public class CommentsController : BaseController
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
        /// Views manager
        /// </summary>
        private readonly IViewsManager viewsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="commentsManager">Comments manager</param>
        /// <param name="viewsManager">Views manager</param>
        public CommentsController(
            ILog log,
            ICommentsManager commentsManager,
            IViewsManager viewsManager)
        {
            this.log = log;
            this.commentsManager = commentsManager;
            this.viewsManager = viewsManager;
        }

        /// <summary>
        /// Delete comment
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The comment is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{commentHandle}")]
        [HttpDelete]
        [VersionRange("All")]
        public async Task<IHttpActionResult> DeleteComment(string commentHandle)
        {
            string className = "CommentsController";
            string methodName = "DeleteComment";
            string logEntry = $"CommentHandle = {commentHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var commentEntity = await this.commentsManager.ReadComment(commentHandle);
            if (commentEntity == null)
            {
                return this.NotFound(ResponseStrings.CommentNotFound);
            }

            if (commentEntity.UserHandle != this.UserHandle || commentEntity.AppHandle != this.AppHandle)
            {
                return this.Unauthorized(ResponseStrings.UserUnauthorized);
            }

            await this.commentsManager.DeleteComment(
                ProcessType.Frontend,
                commentHandle,
                commentEntity.TopicHandle);

            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Get comment
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Comment data</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The comment is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{commentHandle}")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(CommentView))]
        public async Task<IHttpActionResult> GetComment(string commentHandle)
        {
            string className = "CommentsController";
            string methodName = "GetComment";
            string logEntry = $"QueriedCommentHandle = {commentHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var commentEntity = await this.commentsManager.ReadComment(commentHandle);
            if (commentEntity == null)
            {
                return this.NotFound(ResponseStrings.CommentNotFound);
            }

            var commentView = await this.viewsManager.GetCommentView(commentHandle, commentEntity, this.UserHandle);
            logEntry += $", RetrievedCommentHandle = {commentView?.CommentHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(commentView);
        }
    }
}
