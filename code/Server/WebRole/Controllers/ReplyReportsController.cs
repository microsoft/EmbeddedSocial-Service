// <copyright file="ReplyReportsController.cs" company="Microsoft">
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
    /// API to report reply
    /// </summary>
    [RoutePrefix("replies/{replyHandle}/reports")]
    public class ReplyReportsController : ReportsControllerBase
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
        /// Initializes a new instance of the <see cref="ReplyReportsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="reportsManager">Reports manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="commentsManager">Comments manager</param>
        /// <param name="repliesManager">Replies manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public ReplyReportsController(
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
            this.repliesManager = repliesManager;
        }

        /// <summary>
        /// Report a reply as spam, offensive, etc.
        /// </summary>
        /// <param name="replyHandle">Reply handle for the reply being reported on</param>
        /// <param name="postReportRequest">Post report request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized to make this request.</response>
        /// <response code="404">Not Found. The reply being reported on is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PostReport(string replyHandle, [FromBody]PostReportRequest postReportRequest)
        {
            string className = "ReplyReportsController";
            string methodName = "PostReport";
            string logEntry = $"ReplyHandle = {replyHandle}, Reason = {postReportRequest?.Reason}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // check if the content exists
            var replyEntity = await this.repliesManager.ReadReply(replyHandle);
            if (replyEntity == null)
            {
                return this.NotFound(ResponseStrings.ReplyNotFound);
            }

            // Call the ReportControllerBase's method for posting the report. This method of the base class takes care of tracing also.
            return await this.UpdateContentReport(className, methodName, ContentType.Reply, replyHandle, replyEntity.UserHandle, replyEntity.AppHandle, postReportRequest.Reason);
        }
    }
}
