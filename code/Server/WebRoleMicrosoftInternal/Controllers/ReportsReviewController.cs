// <copyright file="ReportsReviewController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleMicrosoftInternal.Controllers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;

    using SocialPlus.Logging;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRoleMicrosoftInternal.Versioning;
    using SocialPlus.Utils;

    /// <summary>
    /// API call for the AVERT service to provide the results of a review
    /// </summary>
    [RoutePrefix("reports/{reportHandle}/review")]
    public class ReportsReviewController : BaseController
    {
        /// <summary>
        /// Reports manager
        /// </summary>
        private readonly IReportsManager reportsManager;

        /// <summary>
        /// Connection string provider
        /// </summary>
        private readonly IConnectionStringProvider connectionStringProvider;

        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsReviewController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="reportsManager">Reports manager</param>
        /// <param name="connectionStringProvider">connection string provider</param>
        public ReportsReviewController(ILog log, IReportsManager reportsManager, IConnectionStringProvider connectionStringProvider)
        {
            this.log = log;
            this.reportsManager = reportsManager;
            this.connectionStringProvider = connectionStringProvider;
        }

        /// <summary>
        /// Response from the AVERT service
        /// </summary>
        /// <param name="reportHandle">report handle that this review result is for</param>
        /// <returns>No content on success</returns>
        /// <response code="202">Accepted. Embedded Social has successfully received the review results.</response>
        /// <response code="400">Bad Request. The CsReview body is not valid.</response>
        /// <response code="401">Unauthorized. The X509 certificate is not valid or not authorized to call this method.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("v0.2-Cur")]
        public async Task<IHttpActionResult> PostReportReview(string reportHandle)
        {
            string className = "ReportsReviewController";
            string methodName = "PostReportReview";
            string logEntry = $"ReportHandle = {reportHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // check the X509 client certificate
            string thumbprint = await this.connectionStringProvider.GetAVERTCertThumbprint(AVERTInstanceType.Default);
            try
            {
                X509CertificateChecker.ValidateX509Certificate(thumbprint, this.RequestContext.ClientCertificate);
            }
            catch (Exception e)
            {
                this.log.LogError(e);
                return this.Unauthorized();
            }

            // Check the body of the HTTP Request.
            // The body is read directly from the Request instead of as an input parameter to this method.
            // That is because we want to treat it as a string instead of referring to an AVERT model buried deep
            // in the AVERT NuGet package. This string will eventually make its way to code in the AVERT project
            // which will convert it.
            string reviewResults = await this.Request.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(reviewResults))
            {
                this.log.LogError("got empty review results from AVERT service for reportHandle: " + reportHandle);
                return this.BadRequest();
            }

            await this.reportsManager.ProcessReportResult(reportHandle, reviewResults);
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.StatusCode(HttpStatusCode.Accepted);
        }
    }
}
