//-----------------------------------------------------------------------
// <copyright file="ConfigController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class ConfigController.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon;
    using SocialPlus.Server.WebRoleCommon.Versioning;

    /// <summary>
    /// Responds to requests for the config information
    /// </summary>
    [RoutePrefix("config")]
    public class ConfigController : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// apps manager
        /// </summary>
        private readonly IAppsManager appsManager;

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
        /// <param name="log">Log</param>
        /// <param name="appsManager">apps manager</param>
        /// <param name="serviceVersionInfo">info about version of service</param>
        /// <param name="buildVersionInfo">build version info</param>
        public ConfigController(ILog log, IAppsManager appsManager, IServiceVersionInfo serviceVersionInfo, IBuildVersionInfo buildVersionInfo)
        {
            this.log = log;
            this.appsManager = appsManager;
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
        [VersionRange("v0.8-Cur")]
        [ResponseType(typeof(GetBuildInfoResponse))]
        public IHttpActionResult GetBuildInfo()
        {
            string className = "ConfigController";
            string methodName = "GetBuildInfo";
            this.LogControllerStart(this.log, className, methodName);

            var minApiVersion = this.serviceVersionInfo.GetMinVersion();
            var maxApiVersion = this.serviceVersionInfo.GetMaxVersion();
            var response = new GetBuildInfoResponse
            {
                DateAndTime = this.buildVersionInfo.BuildDateAndTime,
                CommitHash = this.buildVersionInfo.CommitHash,
                DirtyFiles = this.buildVersionInfo.DirtyFiles,
                Hostname = this.buildVersionInfo.Hostname,
                BranchName = this.buildVersionInfo.BranchName,
                Tag = this.buildVersionInfo.Tag,
            };

            string logEntry = $"DateAndTime = {response.DateAndTime}, CommitHash = {response.CommitHash}, DirtyFiles = {response.DirtyFiles}, ";
            logEntry += $"Hostname = {response.Hostname}, BranchName = {response.BranchName}, Tag = {response.Tag}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
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
        [VersionRange("v0.8-Cur")]
        [ResponseType(typeof(GetServiceInfoResponse))]
        public IHttpActionResult GetServiceInfo()
        {
            string className = "ConfigController";
            string methodName = "GetServiceInfo";
            this.LogControllerStart(this.log, className, methodName);

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

            string logEntry = $"ServiceApiVersion = {response.ServiceApiVersion}, ServiceApiAllVersions = {response.ServiceApiAllVersions}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(response);
        }

        /// <summary>
        /// Get the configuration of a client for a given developer id and client name.
        /// </summary>
        /// <remarks>
        /// Applications can use a developer id and a client name to lookup client configurations. A client configuration comprises a
        /// ServerSideAppKey and a ClientConfigJson.
        ///
        /// An application can split its app key into a client-side and server-side component. On each launch, the application can query
        /// its server-side app key component by using its developer id and a client name. The server-side component is combined with
        /// the client-side to recreate a valid app key. This split-key mechanism lets app developers rotate an app key quickly
        /// without having to update client-side code. Rotating a server-side app key component will effectively rotate an app key.
        ///
        /// ClientConfigJson provides a container for any other configuration that developers want to avoid hard-coding into their apps.
        ///
        /// For a given developer id, a client name maps to a single client configuration (i.e., a ServerSideAppKey and a ClientConfigJson).
        /// However, the same configuration can map to different client names.
        ///
        /// To lookup a particular client configuration, you must provide the developer id and client name associated with that app key.
        /// Examples:
        /// 1. If the application name is "Starbucks", the client name could be "Starbucks:1.0", or "Starbucks:1.1", or both.
        /// 2. When multiple app keys are split, each server-side component of the app keys must be registered
        ///    under different client names, such as "Starbucks:USA:1.0" and "Starbucks:EU:1.0", or "Starbucks:1.0" and "Starbucks:2.0".
        ///
        /// Although we do not require it, we recommend that applications split their app keys into a client-side and server-side component.
        /// </remarks>
        /// <param name="developerId">developer id</param>
        /// <param name="clientName">client name</param>
        /// <returns>client configuration</returns>
        /// <response code="200">success</response>
        /// <response code="404">Not Found. The client name is not found.</response>
        [Route("client_config/{developerId}/{clientName}")]
        [HttpGet]
        [OverrideAuthentication]
        [VersionRange("v0.8-Cur")]
        [ResponseType(typeof(GetClientConfigResponse))]
        public async Task<IHttpActionResult> GetClientConfig(string developerId, string clientName)
        {
            string className = "ConfigController";
            string methodName = "GetClientConfig";
            string logEntry = $"ClientName = {clientName}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var clientConfig = await this.appsManager.ReadClientConfig(developerId, clientName);
            if (clientConfig == null)
            {
                return this.NotFound(ResponseStrings.ClientNameNotFound);
            }

            var response = new GetClientConfigResponse
            {
                ServerSideAppKey = clientConfig.ServerSideAppKey,
                ClientConfigJson = clientConfig.ClientConfigJson
            };

            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(response);
        }
    }
}