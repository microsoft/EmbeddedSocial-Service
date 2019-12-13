// <copyright file="SearchController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRole.Versioning;

    /// <summary>
    /// APIs to search topics and users
    /// </summary>
    [RoutePrefix("search")]
    public class SearchController : BaseController
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
        /// Views manager
        /// </summary>
        private readonly IViewsManager viewsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="searchManager">Search manager</param>
        /// <param name="viewsManager">Views manager</param>
        public SearchController(ILog log, ISearchManager searchManager, IViewsManager viewsManager)
        {
            this.log = log;
            this.searchManager = searchManager;
            this.viewsManager = viewsManager;
        }

        /// <summary>
        /// Search topics with a query
        /// </summary>
        /// <remarks>
        /// The query string will be searched across hashtags, topic titles, and topic texts,
        /// and matching results will be returned.
        ///
        /// If the query string contains only hashtags, e.g. #foo #bar, then only the hashtags
        /// in topics will be searched.
        ///
        /// Query string supports the following operators:
        /// - suffix: "foo*"
        /// - and: "foo+bar"
        /// - or: "foo|bar"
        /// - not: "-foo"
        /// - phrase: ""foo bar""
        /// - precedence: "foo+(bar|baz)"
        /// You need to escape * if it is at the end of a word, and - if it is at the start of a word.
        /// Default behavior is to use and, so if you use whitespace to separate words,
        /// such as "foo bar", that is equivalent to "foo+bar".
        /// </remarks>
        /// <param name="query">Search query</param>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app or user is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("topics")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<TopicView>))]
        public async Task<IHttpActionResult> GetTopics(
            string query,
            int cursor = 0,
            int limit = ApiDefaultValues.GetTopicsPageLimit)
        {
            string className = "SearchController";
            string methodName = "GetTopics";
            this.LogControllerStart(this.log, className, methodName);

            var topicFeedEntities = await this.searchManager.GetTopics(query, this.AppHandle, cursor, limit + 1);
            var response = new FeedResponse<TopicView>();
            if (topicFeedEntities.Count == limit + 1)
            {
                topicFeedEntities.Remove(topicFeedEntities.Last());
                response.Cursor = (cursor + limit).ToString();
            }

            response.Data = await this.viewsManager.GetTopicViews(topicFeedEntities, this.UserHandle);

            // Concatenate all handles of the topics in response data into long strings, delimited by ','
            string topicHandles = null;
            if (response.Data != null)
            {
                topicHandles = string.Join(",", response.Data.Select(v => v.TopicHandle).ToArray());
            }

            string logEntry = $"CountTopicHandles = {response.Data?.Count}, TopicHandlesList = [{topicHandles}]";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(response);
        }

        /// <summary>
         /// Search users with a query
         /// </summary>
         /// <remarks>
         /// The query string will be searched across the full name of users
         /// and matching results will be returned.
         ///
         /// Query string supports the following operators:
         /// - suffix: "foo*"
         /// - and: "foo+bar"
         /// - or: "foo|bar"
         /// - not: "-foo"
         /// - phrase: ""foo bar""
         /// - precedence: "foo+(bar|baz)"
         /// You need to escape * if it is at the end of a word, and - if it is at the start of a word.
         /// Default behavior is to use and, so if you use whitespace to separate words,
         /// such as "foo bar", that is equivalent to "foo+bar".
         /// </remarks>
         /// <param name="query">Search query</param>
         /// <param name="cursor">Current read cursor</param>
         /// <param name="limit">Number of items to return</param>
         /// <returns>User feed</returns>
         /// <response code="200">OK. The request was successful.</response>
         /// <response code="400">Bad request. The request is invalid.</response>
         /// <response code="401">Unauthorized. The app or user is not authorized.</response>
         /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("users")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<UserCompactView>))]
        public async Task<IHttpActionResult> GetUsers(string query, int cursor = 0, int limit = ApiDefaultValues.GetUsersPageLimit)
        {
            string className = "SearchController";
            string methodName = "GetUsers";
            this.LogControllerStart(this.log, className, methodName);

            var userFeedEntities = await this.searchManager.GetUsers(query, this.AppHandle, cursor, limit);
            var response = new FeedResponse<UserCompactView>();
            if (userFeedEntities.Count == limit + 1)
            {
                userFeedEntities.Remove(userFeedEntities.Last());
                response.Cursor = null;
            }

            response.Data = await this.viewsManager.GetUserCompactViews(userFeedEntities, this.AppHandle, this.UserHandle);

            // Concatenate all handles of the users in response data into long strings, delimited by ','
            string userHandles = null;
            if (response.Data != null)
            {
                userHandles = string.Join(",", response.Data.Select(v => v.UserHandle).ToArray());
            }

            string logEntry = $"CountUserHandles = {response.Data?.Count}, UserHandlesList = [{userHandles}]";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(response);
        }
    }
}
