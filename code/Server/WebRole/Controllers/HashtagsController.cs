// <copyright file="HashtagsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon;

    /// <summary>
    /// APIs to get trending <c>hashtags</c> and autocomplete <c>hashtags</c>
    /// </summary>
    [RoutePrefix("hashtags")]
    public class HashtagsController : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Search manager
        /// </summary>
        private readonly ISearchManager searchManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashtagsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="searchManager">Search manager</param>
        public HashtagsController(ILog log, ISearchManager searchManager)
        {
            this.log = log;
            this.searchManager = searchManager;
        }

        /// <summary>
        /// Get trending <c>hashtags</c>
        /// </summary>
        /// <returns>List of <c>hashtags</c></returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app or user is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("trending")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(List<string>))]
        public async Task<IHttpActionResult> GetTrendingHashtags()
        {
            string className = "HashtagController";
            string methodName = "GetTrendingHashtags";
            this.LogControllerStart(this.log, className, methodName);

            IList<string> hashtags = await this.searchManager.GetTrendingHashtags(this.AppHandle);
            string logEntry = $"CountHashtags = {hashtags?.Count}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(hashtags.ToList());
        }

        /// <summary>
        /// Get autocompleted <c>hashtags</c>
        /// </summary>
        /// <remarks>
        /// The query string must be at least 3 characters long, and no more than 25 characters long.
        /// </remarks>
        /// <param name="query">Search query</param>
        /// <returns>List of <c>hashtags</c></returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app or user is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("autocomplete")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(List<string>))]
        public async Task<IHttpActionResult> GetAutocompletedHashtags(string query)
        {
            string className = "HashtagController";
            string methodName = "GetAutocompletedHashtags";
            this.LogControllerStart(this.log, className, methodName);

            // test the input query
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
            {
                return this.BadRequest(ResponseStrings.QueryTooSmall);
            }

            if (query.Length > 25)
            {
                return this.BadRequest(ResponseStrings.QueryTooBig);
            }

            IList<string> hashtags = await this.searchManager.GetAutocompletedHashtags(query, this.AppHandle);
            string logEntry = $"CountHashtags = {hashtags?.Count}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(hashtags.ToList());
        }
    }
}
