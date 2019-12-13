//-----------------------------------------------------------------------
// <copyright file="BuildsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class BuildsController.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.WebRoleMicrosoftInternal.Controllers
{
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Models;
    using SocialPlus.Server.WebRoleCommon.Versioning;
    using SocialPlus.Server.WebRoleMicrosoftInternal.Versioning;

    /// <summary>
    /// Responds to requests for the build information of this service.
    /// </summary>
    [RoutePrefix("builds")]
    public class BuildsController : BaseController
    {
        /// <summary>
        /// Info about service version
        /// </summary>
        private IServiceVersionInfo serviceVersionInfo;

        /// <summary>
        /// Info about build version
        /// </summary>
        private IBuildVersionInfo buildVersionInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildsController"/> class
        /// </summary>
        /// <param name="serviceVersionInfo">info about version of service</param>
        /// <param name="buildVersionInfo">build version info</param>
        public BuildsController(IServiceVersionInfo serviceVersionInfo, IBuildVersionInfo buildVersionInfo)
        {
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
        [VersionRange("Min-v0.2")]
        [ResponseType(typeof(BuildsCurrentResponse))]
        public BuildsCurrentResponse GetBuildsCurrentV02()
        {
            return new BuildsCurrentResponse
            {
                DateAndTime = this.buildVersionInfo.BuildDateAndTime,
                CommitHash = this.buildVersionInfo.CommitHash,
                DirtyFiles = this.buildVersionInfo.DirtyFiles,
                Hostname = this.buildVersionInfo.Hostname,
                ServiceApiVersion = this.serviceVersionInfo.GetCurrentVersion().ToString(),
            };
        }
    }
}
