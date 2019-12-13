// <copyright file="TopicReportsController.cs" company="Microsoft">
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
    /// API to report topic
    /// </summary>
    [RoutePrefix("topics/{topicHandle}/reports")]
    public class TopicReportsController : ReportsControllerBase
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
        /// Initializes a new instance of the <see cref="TopicReportsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="reportsManager">Reports manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="commentsManager">Comments manager</param>
        /// <param name="repliesManager">Replies manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public TopicReportsController(
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
            this.topicsManager = topicsManager;
        }

        /// <summary>
        /// Report a topic as spam, offensive, etc.
        /// </summary>
        /// <param name="topicHandle">Topic handle being reported on</param>
        /// <param name="postReportRequest">Post report request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized to make this request.</response>
        /// <response code="404">Not Found. The topic being reported on is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PostReport(string topicHandle, [FromBody]PostReportRequest postReportRequest)
        {
            string className = "TopicReportsController";
            string methodName = "PostReport";
            string logEntry = $"TopicHandle = {topicHandle}, Reason = {postReportRequest?.Reason}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // check if the content exists
            var topicEntity = await this.topicsManager.ReadTopic(topicHandle);
            if (topicEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNotFound);
            }

            // Call the ReportControllerBase's method for posting the report. This method of the base class takes care of tracing also.
            return await this.UpdateContentReport(className, methodName, ContentType.Topic, topicHandle, topicEntity.UserHandle, topicEntity.AppHandle, postReportRequest.Reason);
        }
    }
}
