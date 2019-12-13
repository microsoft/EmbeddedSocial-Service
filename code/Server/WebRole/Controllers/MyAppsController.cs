//-----------------------------------------------------------------------
// <copyright file="MyAppsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class MyAppsController.
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
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon;

    /// <summary>
    /// API to query my <c>Social Plus</c> apps
    /// </summary>
    [RoutePrefix("users/me/apps")]
    public class MyAppsController : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAppsController"/> class.
        /// </summary>
        /// <param name="log">log</param>
        public MyAppsController(ILog log)
        {
            this.log = log;
        }

        /// <summary>
        /// Get my list of <c>Social Plus</c> apps
        /// </summary>
        /// <returns>App feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(List<AppCompactView>))]
        public async Task<IHttpActionResult> GetApps()
        {
            string className = "MyAppsController";
            string methodName = "GetApps";
            this.LogControllerStart(this.log, className, methodName);

            await Task.Delay(100);
            this.LogControllerEnd(this.log, className, methodName);
            return this.NotImplemented(ResponseStrings.NotImplemented);
        }
    }
}
