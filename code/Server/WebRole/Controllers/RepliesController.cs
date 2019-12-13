// <copyright file="RepliesController.cs" company="Microsoft">
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
    using SocialPlus.Server.WebRoleCommon;

    /// <summary>
    /// APIs to delete and query replies
    /// </summary>
    [RoutePrefix("replies")]
    public class RepliesController : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Replies manager
        /// </summary>
        private readonly IRepliesManager repliesManager;

        /// <summary>
        /// Views manager
        /// </summary>
        private readonly IViewsManager viewsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepliesController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="repliesManager">Replies manager</param>
        /// <param name="viewsManager">Views manager</param>
        public RepliesController(
            ILog log,
            IRepliesManager repliesManager,
            IViewsManager viewsManager)
        {
            this.log = log;
            this.repliesManager = repliesManager;
            this.viewsManager = viewsManager;
        }

        /// <summary>
        /// Delete reply
        /// </summary>
        /// <param name="replyHandle">Reply handle</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The reply is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{replyHandle}")]
        [HttpDelete]
        [VersionRange("All")]
        public async Task<IHttpActionResult> DeleteReply(string replyHandle)
        {
            string className = "RepliesController";
            string methodName = "DeleteReply";
            string logEntry = $"ReplyHandle = {replyHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var replyEntity = await this.repliesManager.ReadReply(replyHandle);
            if (replyEntity == null)
            {
                return this.NotFound(ResponseStrings.ReplyNotFound);
            }

            if (replyEntity.UserHandle != this.UserHandle || replyEntity.AppHandle != this.AppHandle)
            {
                return this.Unauthorized(ResponseStrings.UserUnauthorized);
            }

            await this.repliesManager.DeleteReply(
                ProcessType.Frontend,
                replyHandle,
                replyEntity.CommentHandle);

            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Get reply
        /// </summary>
        /// <param name="replyHandle">Reply handle</param>
        /// <returns>Comment data</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The reply is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{replyHandle}")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(ReplyView))]
        public async Task<IHttpActionResult> GetReply(string replyHandle)
        {
            string className = "RepliesController";
            string methodName = "GetReply";
            string logEntry = $"QueriedReplyHandle = {replyHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var replyEntity = await this.repliesManager.ReadReply(replyHandle);
            if (replyEntity == null)
            {
                return this.NotFound(ResponseStrings.ReplyNotFound);
            }

            var replyView = await this.viewsManager.GetReplyView(replyHandle, replyEntity, this.UserHandle);
            logEntry += $", RetrievedReplyHandle = {replyView?.ReplyHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(replyView);
        }
    }
}
