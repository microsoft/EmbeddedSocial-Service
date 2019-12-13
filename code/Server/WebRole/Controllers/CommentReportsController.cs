// <copyright file="CommentReportsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon;
    using SocialPlus.Utils;

    /// <summary>
    /// API to report comment
    /// </summary>
    [RoutePrefix("comments/{commentHandle}/reports")]
    public class CommentReportsController : ReportsControllerBase
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
        /// Initializes a new instance of the <see cref="CommentReportsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="reportsManager">Reports manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="commentsManager">Comments manager</param>
        /// <param name="repliesManager">Replies manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public CommentReportsController(
            ILog log,
            IReportsManager reportsManager,
            ITopicsManager topicsManager,
            ICommentsManager commentsManager,
            IRepliesManager repliesManager,
            IUsersManager usersManager,
            IHandleGenerator handleGenerator)
            : base(log, reportsManager, topicsManager, commentsManager, repliesManager, usersManager, handleGenerator)
        {
            this.log = log;
            this.commentsManager = commentsManager;
        }

        /// <summary>
        /// Report a comment as spam, offensive, etc.
        /// </summary>
        /// <param name="commentHandle">Comment handle for the comment being reported on</param>
        /// <param name="postReportRequest">Post report request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized to make this request.</response>
        /// <response code="404">Not Found. The comment being reported on is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PostReport(string commentHandle, [FromBody]PostReportRequest postReportRequest)
        {
            string className = "CommentReportsController";
            string methodName = "PostReport";
            string logEntry = $"CommentHandle = {commentHandle}, Reason = {postReportRequest?.Reason}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // check if the content exists
            var commentEntity = await this.commentsManager.ReadComment(commentHandle);
            if (commentEntity == null)
            {
                return this.NotFound(ResponseStrings.CommentNotFound);
            }

            // Call the ReportControllerBase's method for posting the report. This method of the base class takes care of tracing also.
            return await this.UpdateContentReport(className, methodName, ContentType.Comment, commentHandle, commentEntity.UserHandle, commentEntity.AppHandle, postReportRequest.Reason);
        }
    }
}
