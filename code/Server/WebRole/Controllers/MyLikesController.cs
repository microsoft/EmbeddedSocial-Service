// <copyright file="MyLikesController.cs" company="Microsoft">
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
    using SocialPlus.Server.WebRoleCommon;

    /// <summary>
    /// APIs to query user topics
    /// </summary>
    [RoutePrefix("users/me/likes")]
    public class MyLikesController : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyLikesController"/> class.
        /// </summary>
        /// <param name="log">log</param>
        public MyLikesController(ILog log)
        {
            this.log = log;
        }

        /// <summary>
        /// Get my liked topics.
        /// </summary>
        /// <remarks>
        /// Not yet implemented.
        /// </remarks>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("topics")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<TopicView>))]
        public async Task<IHttpActionResult> GetLikedTopics(string cursor = null, int limit = ApiDefaultValues.GetTopicsPageLimit)
        {
            string className = "MyLikesController";
            string methodName = "GetLikedTopics";
            this.LogControllerStart(this.log, className, methodName);

            await Task.Delay(100);
            this.LogControllerEnd(this.log, className, methodName);
            return this.NotImplemented(ResponseStrings.NotImplemented);
        }
    }
}
