// <copyright file="UserReportsController.cs" company="Microsoft">
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
    /// API to report a user
    /// </summary>
    [RoutePrefix("users/{userHandle}/reports")]
    public class UserReportsController : ReportsControllerBase
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Users manager
        /// </summary>
        private readonly IUsersManager usersManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserReportsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="reportsManager">Reports manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="commentsManager">Comments manager</param>
        /// <param name="repliesManager">Replies manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public UserReportsController(
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
            this.usersManager = usersManager;
        }

        /// <summary>
        /// Report a user as spam, offensive, etc.
        /// </summary>
        /// <remarks>
        /// This call allows a user to complain about another user's profile content
        /// (photo, bio, name) as containing spam, offensive material, etc.
        /// </remarks>
        /// <param name="userHandle">User handle being reported on</param>
        /// <param name="postReportRequest">Post report request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user is not authorized to make this request.</response>
        /// <response code="404">Not Found. The user being reported on is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PostReport(string userHandle, [FromBody]PostReportRequest postReportRequest)
        {
            string className = "UserReportsController";
            string methodName = "PostReport";
            string logEntry = $"UserHandle = {userHandle}, Reason = {postReportRequest?.Reason}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // check if the user exists
            var reportedUserEntity = await this.usersManager.ReadUserProfile(userHandle, this.AppHandle);
            if (reportedUserEntity == null)
            {
                return this.NotFound(ResponseStrings.UserNotFound);
            }

            // Call the ReportControllerBase's method for posting the report. This method of the base class takes care of tracing also.
            return await this.UpdateUserReport(className, methodName, userHandle, postReportRequest.Reason);
        }
    }
}
