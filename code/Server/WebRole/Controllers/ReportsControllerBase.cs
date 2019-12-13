// <copyright file="ReportsControllerBase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRoleCommon;
    using SocialPlus.Utils;

    /// <summary>
    /// Reports base controller
    /// </summary>
    public class ReportsControllerBase : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Reports manager
        /// </summary>
        private readonly IReportsManager reportsManager;

        /// <summary>
        /// Handle generator
        /// </summary>
        private readonly IHandleGenerator handleGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsControllerBase"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="reportsManager">Reports manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="commentsManager">Comments manager</param>
        /// <param name="repliesManager">Replies manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public ReportsControllerBase(
            ILog log,
            IReportsManager reportsManager,
            ITopicsManager topicsManager,
            ICommentsManager commentsManager,
            IRepliesManager repliesManager,
            IUsersManager usersManager,
            IHandleGenerator handleGenerator)
        {
            this.log = log;
            this.reportsManager = reportsManager;
            this.handleGenerator = handleGenerator;
        }

        /// <summary>
        /// Constructs the URI for AVERT to call us back with the review results for a specified report handle
        /// </summary>
        /// <param name="reportHandle">uniquely identifies the report</param>
        /// <returns>URI that can be used by AVERT to provide a response</returns>
        public Uri ConstructReviewUri(string reportHandle)
        {
            string url;

            // check the handle is valid
            if (string.IsNullOrWhiteSpace(reportHandle))
            {
                throw new ArgumentNullException("reportHandle");
            }

            // protocol + hostname
            url = this.Request.RequestUri.Scheme + "://" + this.Request.RequestUri.Host;

            // explicitly add the port number.
            // this code executes on the WebRole, and calculates a callback uri that will execute on the WebRoleMicrosoftInternal.
            // WebRole runs on port 80 (when running locally) or 443 (when deployed in Azure).
            // WebRoleInternal runs on port 81 (when running locally) or 444 (when deployed in Azure).
            // in both cases, the WebRoleInternal port number = the WebRole port number + 1
            int webRoleInternalPort = this.Request.RequestUri.Port + 1;
            url += ":" + webRoleInternalPort;

            // note: do not use the product version that is in this.Request.Headers.UserAgent.FirstOrDefault()
            // because that indicates the version of the client, not neccessarily the version of the API call
            // that has been made.

            // hardcode version 0.2
            // this version number corresponds to the value set in the Register() method of the WebApiConfig class in the WebRoleMicrosoftInternal
            url += @"/v0.2/";

            // hardcode route to controller
            // this path corresponds to the value shown in the RoutePrefix attribute of the ReportsReviewController in the WebRoleMicrosoftInternal
            url += @"reports/" + reportHandle + @"/review";

            return new Uri(url);
        }

        /// <summary>
        /// Update report for content.
        /// </summary>
        /// <param name="callerClassName">name of the controller class of the caller</param>
        /// <param name="callerMethodName">name of method insider controller class of the caller (should correspond to an HTTP action)</param>
        /// <param name="contentType">Content type</param>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="contentUserHandle">User handle that created the content</param>
        /// <param name="contentAppHandle">App handle that the content lives in</param>
        /// <param name="reason">Report reason</param>
        /// <returns>Http response</returns>
        protected async Task<IHttpActionResult> UpdateContentReport(string callerClassName, string callerMethodName, ContentType contentType, string contentHandle, string contentUserHandle, string contentAppHandle, ReportReason reason)
        {
            string reportingUserHandle = this.UserHandle;

            // check if the app handle for the content is the app that is reporting this or the master app
            if (this.AppHandle != contentAppHandle && this.AppHandle != MasterApp.AppHandle)
            {
                return this.Unauthorized(ResponseStrings.AppUnauthorized);
            }

            // Do not allow the user that created the content to self-report.
            // If we did, it could allow an attacker to test our reporting mechanism and figure out
            // what gets blocked and what gets through.
            if (reportingUserHandle == contentUserHandle)
            {
                return this.Unauthorized(ResponseStrings.SelfReportIsNotAllowed);
            }

            // submit the request to the reports manager
            DateTime currentTime = DateTime.UtcNow;
            string reportHandle = this.handleGenerator.GenerateShortHandle();
            await this.reportsManager.CreateContentReport(
                ProcessType.Frontend,
                reportHandle,
                contentType,
                contentHandle,
                contentUserHandle,
                reportingUserHandle,
                contentAppHandle,
                reason,
                currentTime,
                this.ConstructReviewUri(reportHandle));

            string logEntry = $"ContentHandle = {contentHandle}, Reason = {reason}, ContentType = {contentType}";
            logEntry += $", ContentOwnerUserHandle = {contentUserHandle}, ContentAppHandle = {contentAppHandle}, ReportingUserHandle = {reportingUserHandle}, ReportHandle = {reportHandle}";
            this.LogControllerEnd(this.log, callerClassName, callerMethodName, logEntry);

            return this.NoContent();
        }

        /// <summary>
        /// Update report for a user.
        /// </summary>
        /// <param name="callerClassName">name of the controller class of the caller</param>
        /// <param name="callerMethodName">name of method insider controller class of the caller (should correspond to an HTTP action)</param>
        /// <param name="reportedUserHandle">User being reported on</param>
        /// <param name="reason">Report reason</param>
        /// <returns>Http response</returns>
        protected async Task<IHttpActionResult> UpdateUserReport(string callerClassName, string callerMethodName, string reportedUserHandle, ReportReason reason)
        {
            string reportingUserHandle = this.UserHandle;
            string appHandle = this.AppHandle;

            // Do not allow the user to self-report.
            // If we did, it could allow an attacker to test our reporting mechanism and figure out
            // what gets blocked and what gets through.
            if (reportingUserHandle == reportedUserHandle)
            {
                return this.Unauthorized(ResponseStrings.SelfReportIsNotAllowed);
            }

            // submit the request to the reports manager
            DateTime currentTime = DateTime.UtcNow;
            string reportHandle = this.handleGenerator.GenerateShortHandle();
            await this.reportsManager.CreateUserReport(
                ProcessType.Frontend,
                reportHandle,
                reportedUserHandle,
                reportingUserHandle,
                appHandle,
                reason,
                currentTime,
                this.ConstructReviewUri(reportHandle));

            string logEntry = $"ReportedUserHandle = {reportedUserHandle}, Reason = {reason}, ReportHandle = {reportHandle}";
            this.LogControllerEnd(this.log, callerClassName, callerMethodName, logEntry);
            return this.NoContent();
        }
    }
}
