// <copyright file="MyAppFollowingController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.WebRole.Versioning;

    /// <summary>
    /// API to find users the current user is following in other apps but not in the current app
    /// </summary>
    [RoutePrefix("users/me/apps/{appHandle}/following")]
    public class MyAppFollowingController : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAppFollowingController"/> class.
        /// </summary>
        /// <param name="log">log</param>
        public MyAppFollowingController(ILog log)
        {
            this.log = log;
        }

        /// <summary>
        /// Find users the current user is following in another app but not in the current app
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Current read cursor</param>
        /// <returns>User feed. The number of items returned can vary and is best effort.</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("difference")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<UserCompactView>))]
        public async Task<IHttpActionResult> GetUsers(string appHandle, string cursor = null)
        {
            string className = "MyAppFollowingController";
            string methodName = "GetUsers";
            this.LogControllerStart(this.log, className, methodName);

            await Task.Delay(100);
            this.LogControllerEnd(this.log, className, methodName);
            return this.Ok();
        }
    }
}
