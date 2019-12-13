//-----------------------------------------------------------------------
// <copyright file="BuildsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class BuildsController.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.Controllers
{
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon.Versioning;

    /// <summary>
    /// Responds to requests for the build information of this service.
    /// </summary>
    [RoutePrefix("builds")]
    public class BuildsController : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Info about service version
        /// </summary>
        private readonly IServiceVersionInfo serviceVersionInfo;

        /// <summary>
        /// Info about build version
        /// </summary>
        private readonly IBuildVersionInfo buildVersionInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="serviceVersionInfo">info about version of service</param>
        /// <param name="buildVersionInfo">build version info</param>
        public BuildsController(ILog log, IServiceVersionInfo serviceVersionInfo, IBuildVersionInfo buildVersionInfo)
        {
            this.log = log;
            this.serviceVersionInfo = serviceVersionInfo;
            this.buildVersionInfo = buildVersionInfo;
        }

        /// <summary>
        /// The build information for this service
        /// </summary>
        /// <remarks>This API is meant to be called by humans for debugging</remarks>
        /// <returns>The builds current response</returns>
        /// <response code="200">success</response>
        [Route("current")]
        [HttpGet]
        [OverrideAuthentication]
        [VersionRange("Min-v0.7")]
        [ResponseType(typeof(BuildsCurrentResponse))]
        public BuildsCurrentResponse GetBuildsCurrentV07()
        {
            string className = "BuildsController";
            string methodName = "GetBuildsCurrentV07";
            this.LogControllerStart(this.log, className, methodName);

            var response = new BuildsCurrentResponse
            {
                DateAndTime = this.buildVersionInfo.BuildDateAndTime,
                CommitHash = this.buildVersionInfo.CommitHash,
                DirtyFiles = this.buildVersionInfo.DirtyFiles,
                Hostname = this.buildVersionInfo.Hostname,
                ServiceApiVersion = this.serviceVersionInfo.GetCurrentVersion().ToString(),
            };

            string logEntry = $"DateAndTime = {response.DateAndTime}, CommitHash = {response.CommitHash}, DirtyFiles = {response.DirtyFiles}, ";
            logEntry += $"Hostname = {response.Hostname}, ServiceApiVersion = {response.ServiceApiVersion}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return response;
        }
    }
}
