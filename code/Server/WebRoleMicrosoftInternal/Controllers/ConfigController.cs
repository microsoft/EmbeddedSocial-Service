//-----------------------------------------------------------------------
// <copyright file="ConfigController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class ConfigController.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.WebRoleMicrosoftInternal.Controllers
{
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Models;
    using SocialPlus.Server.WebRoleCommon.Versioning;
    using SocialPlus.Server.WebRoleMicrosoftInternal.Versioning;

    /// <summary>
    /// Responds to requests for the config information
    /// </summary>
    [RoutePrefix("config")]
    public class ConfigController : BaseController
    {
        /// <summary>
        /// Info about service version
        /// </summary>
        private readonly IServiceVersionInfo serviceVersionInfo;

        /// <summary>
        /// Info about build version
        /// </summary>
        private readonly IBuildVersionInfo buildVersionInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigController"/> class
        /// </summary>
        /// <param name="serviceVersionInfo">info about version of service</param>
        /// <param name="buildVersionInfo">build version info</param>
        public ConfigController(IServiceVersionInfo serviceVersionInfo, IBuildVersionInfo buildVersionInfo)
        {
            this.serviceVersionInfo = serviceVersionInfo;
            this.buildVersionInfo = buildVersionInfo;
        }

        /// <summary>
        /// Get build information
        /// </summary>
        /// <returns>The build response</returns>
        /// <response code="200">success</response>
        [Route("build_info")]
        [HttpGet]
        [OverrideAuthentication]
        [VersionRange("v0.3-Cur")]
        [ResponseType(typeof(GetBuildInfoResponse))]
        public IHttpActionResult GetBuildInfo()
        {
            var response = new GetBuildInfoResponse
            {
                DateAndTime = this.buildVersionInfo.BuildDateAndTime,
                CommitHash = this.buildVersionInfo.CommitHash,
                DirtyFiles = this.buildVersionInfo.DirtyFiles,
                Hostname = this.buildVersionInfo.Hostname,
                BranchName = this.buildVersionInfo.BranchName,
                Tag = this.buildVersionInfo.Tag,
            };

            return this.Ok(response);
        }

        /// <summary>
        /// Get service information
        /// </summary>
        /// <returns>The build response</returns>
        /// <response code="200">success</response>
        [Route("service_info")]
        [HttpGet]
        [OverrideAuthentication]
        [VersionRange("v0.3-Cur")]
        [ResponseType(typeof(GetServiceInfoResponse))]
        public IHttpActionResult GetServiceInfo()
        {
            // Retrieve the API Version Info min, current, and max
            var minApiVersion = this.serviceVersionInfo.GetMinVersion();
            var currentApiVersion = this.serviceVersionInfo.GetCurrentVersion();
            var maxApiVersion = this.serviceVersionInfo.GetMaxVersion();

            // Convert them to strings paying attention to handling null cases. Note that currentApiVersion cannot be null.
            string currentApiVersionString = currentApiVersion.ToString();
            List<string> validApiVersionsList = this.serviceVersionInfo.EnumerateVersions(minApiVersion, maxApiVersion);
            string validApiVersionsString = (validApiVersionsList != null) ? string.Join(",", validApiVersionsList) : default(string);

            var response = new GetServiceInfoResponse
            {
                ServiceApiVersion = currentApiVersionString,
                ServiceApiAllVersions = validApiVersionsString
            };

            return this.Ok(response);
        }
    }
}