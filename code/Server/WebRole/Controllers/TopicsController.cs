//-----------------------------------------------------------------------
// <copyright file="TopicsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class TopicsController.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon;
    using SocialPlus.Utils;

    /// <summary>
    /// APIs to create, delete, update and query topics
    /// </summary>
    [RoutePrefix("topics")]
    public class TopicsController : TopicsControllerBase
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Apps manager
        /// </summary>
        private readonly IAppsManager appsManager;

        /// <summary>
        /// Users manager
        /// </summary>
        private readonly IUsersManager usersManager;

        /// <summary>
        /// Topics manager
        /// </summary>
        private readonly ITopicsManager topicsManager;

        /// <summary>
        /// Popular topics manager
        /// </summary>
        private readonly IPopularTopicsManager popularTopicsManager;

        /// <summary>
        /// Views manager
        /// </summary>
        private readonly IViewsManager viewsManager;

        /// <summary>
        /// Topic names manager
        /// </summary>
        private readonly ITopicNamesManager topicNamesManager;

        /// <summary>
        /// Handle generator
        /// </summary>
        private readonly IHandleGenerator handleGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="appsManager">Apps manager</param>
        /// <param name="popularTopicsManager">Popular topics manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="topicNamesManager">Topic names manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public TopicsController(
            ILog log,
            IUsersManager usersManager,
            ITopicsManager topicsManager,
            IAppsManager appsManager,
            IPopularTopicsManager popularTopicsManager,
            IViewsManager viewsManager,
            ITopicNamesManager topicNamesManager,
            IHandleGenerator handleGenerator)
            : base(log, viewsManager)
        {
            this.log = log;
            this.usersManager = usersManager;
            this.topicsManager = topicsManager;
            this.appsManager = appsManager;
            this.popularTopicsManager = popularTopicsManager;
            this.viewsManager = viewsManager;
            this.topicNamesManager = topicNamesManager;
            this.handleGenerator = handleGenerator;
        }

        /// <summary>
        /// Create a new topic
        /// </summary>
        /// <param name="request">Post topic request</param>
        /// <returns>Post topic response</returns>
        /// <response code="201">Created. The response contains the topic handle.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        /// <response code="501">Not implemented. The server is yet to implement this feature.</response>
        [Route("")]
        [HttpPost]
        [VersionRange("All")]
        [ResponseType(typeof(PostTopicResponse))]
        public async Task<IHttpActionResult> PostTopic([FromBody]PostTopicRequest request)
        {
            string className = "TopicsController";
            string methodName = "PostTopic";
            string logEntry = $"PublisherType = {request?.PublisherType}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var userProfileEntity = await this.usersManager.ReadUserProfile(this.UserHandle, this.AppHandle);
            if (userProfileEntity == null)
            {
                return this.Unauthorized(ResponseStrings.UserNotFound);
            }

            UserVisibilityStatus visibility = userProfileEntity.Visibility;

            // app published topics can only be created by an administrator of that application
            if (request.PublisherType == PublisherType.App)
            {
                bool isAdmin = await this.appsManager.IsAdminUser(this.AppHandle, this.UserHandle);
                if (!isAdmin)
                {
                    return this.Unauthorized(ResponseStrings.UserUnauthorized);
                }
            }

            // Define a locally-scoped userHandle. If App is PublisherType, we will set it to null
            string userHandle = this.UserHandle;
            if (request.PublisherType == PublisherType.App)
            {
                // for app published topics, we don't record the userHandle as the topic owner
                // this is because if the user who created the topic is later deleted, we do not want the topic to disappear
                userHandle = null;

                // app published topics are always public
                visibility = UserVisibilityStatus.Public;
            }

            string topicHandle = this.handleGenerator.GenerateShortHandle();
            await this.topicsManager.CreateTopic(
                ProcessType.Frontend,
                topicHandle,
                request.Title,
                request.Text,
                request.BlobType,
                request.BlobHandle,
                request.Categories,
                request.Language,
                request.Group,
                request.DeepLink,
                request.FriendlyName,
                request.PublisherType,
                userHandle,
                visibility,
                DateTime.UtcNow,
                ReviewStatus.Active,
                this.AppHandle,
                null);

            var response = new PostTopicResponse()
            {
                TopicHandle = topicHandle
            };

            logEntry += $", TopicHandle = {topicHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Created<PostTopicResponse>(new Uri(this.RequestAbsoluteUri + "/" + topicHandle), response);
        }

        /// <summary>
        /// Delete topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{topicHandle}")]
        [HttpDelete]
        [VersionRange("All")]
        public async Task<IHttpActionResult> DeleteTopic(string topicHandle)
        {
            string className = "TopicsController";
            string methodName = "DeleteTopic";
            string logEntry = $"TopicHandle = {topicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var topicEntity = await this.topicsManager.ReadTopic(topicHandle);
            if (topicEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNotFound);
            }

            if (topicEntity.AppHandle != this.AppHandle)
            {
                return this.Unauthorized(ResponseStrings.AppUnauthorized);
            }

            if (topicEntity.PublisherType == PublisherType.User && topicEntity.UserHandle != this.UserHandle)
            {
                return this.Unauthorized(ResponseStrings.UserUnauthorized);
            }

            // Define a locally-scoped userHandle. If App is PublisherType, we will set it to null
            string userHandle = this.UserHandle;

            // app published topics can only be deleted by an administrator of that application
            if (topicEntity.PublisherType == PublisherType.App)
            {
                bool isAdmin = await this.appsManager.IsAdminUser(this.AppHandle, this.UserHandle);
                if (!isAdmin)
                {
                    return this.Unauthorized(ResponseStrings.UserUnauthorized);
                }

                // for app published topics, we don't record the userHandle as the topic owner
                userHandle = null;
            }

            await this.topicsManager.DeleteTopic(
                ProcessType.Frontend,
                topicHandle,
                topicEntity.PublisherType,
                userHandle,
                topicEntity.AppHandle);

            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Update topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="request">Put topic request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic is not found.</response>
        /// <response code="409">Conflict. Newer item exists.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{topicHandle}")]
        [HttpPut]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PutTopic(string topicHandle, [FromBody]PutTopicRequest request)
        {
            string className = "TopicsController";
            string methodName = "PutTopic";
            string logEntry = $"TopicHandle = {topicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var topicEntity = await this.topicsManager.ReadTopic(topicHandle);
            if (topicEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNotFound);
            }

            if (topicEntity.AppHandle != this.AppHandle)
            {
                return this.Unauthorized(ResponseStrings.AppUnauthorized);
            }

            if (topicEntity.PublisherType == PublisherType.User && topicEntity.UserHandle != this.UserHandle)
            {
                return this.Unauthorized(ResponseStrings.UserUnauthorized);
            }

            // app published topics can only be updated by an administrator of that application
            if (topicEntity.PublisherType == PublisherType.App)
            {
                bool isAdmin = await this.appsManager.IsAdminUser(this.AppHandle, this.UserHandle);
                if (!isAdmin)
                {
                    return this.Unauthorized(ResponseStrings.UserUnauthorized);
                }
            }

            DateTime currentTime = DateTime.UtcNow;
            if (topicEntity.LastUpdatedTime > currentTime)
            {
                return this.Conflict(ResponseStrings.NewerItemExists);
            }

            await this.topicsManager.UpdateTopic(
                ProcessType.Frontend,
                topicHandle,
                request.Title,
                request.Text,
                topicEntity.BlobType,
                topicEntity.BlobHandle,
                request.Categories,
                topicEntity.ReviewStatus,
                currentTime,
                topicEntity);

            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Get topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Topic data</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{topicHandle}")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(TopicView))]
        public async Task<IHttpActionResult> GetTopic(string topicHandle)
        {
            string className = "TopicsController";
            string methodName = "GetTopic";
            string logEntry = $"QueriedTopicHandle = {topicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var topicEntity = await this.topicsManager.ReadTopic(topicHandle);
            if (topicEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNotFound);
            }

            var topicView = await this.viewsManager.GetTopicView(topicHandle, topicEntity, this.UserHandle);

            // check for null as the topic may have been filtered by the views manager
            if (topicView == null)
            {
                return this.NotFound(ResponseStrings.TopicNotFound);
            }

            logEntry = $"RequestedTopicHandle = {topicView.TopicHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(topicView);
        }

        /// <summary>
        /// Get recent topics
        /// </summary>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<TopicView>))]
        public async Task<IHttpActionResult> GetTopics(string cursor = null, int limit = ApiDefaultValues.GetTopicsPageLimit)
        {
            string className = "TopicsController";
            string methodName = "GetTopics";
            this.LogControllerStart(this.log, className, methodName);

            var topicFeedEntities = await this.topicsManager.ReadTopics(this.AppHandle, cursor, limit + 1);

            // Call the TopicsControllerBase's method for getting topics. This method of the base class takes care of tracing also.
            return await this.GetResponse(className, methodName, topicFeedEntities, limit);
        }

        /// <summary>
        /// Get popular topics for a time range
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("popular/{timeRange}")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<TopicView>))]
        public async Task<IHttpActionResult> GetPopularTopics(TimeRange timeRange, int cursor = 0, int limit = ApiDefaultValues.GetTopicsPageLimit)
        {
            string className = "TopicsController";
            string methodName = "GetPopularTopics";
            string logEntry = $"TimeRange = {timeRange}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var topicFeedEntities = await this.popularTopicsManager.ReadPopularTopics(timeRange, this.AppHandle, cursor, limit + 1);

            // Call the TopicsControllerBase's method for getting topics. This method of the base class takes care of tracing also.
            return await this.GetResponse(className, methodName, topicFeedEntities, cursor, limit);
        }

        /// <summary>
         /// Get featured topics
         /// </summary>
         /// <param name="cursor">Current read cursor</param>
         /// <param name="limit">Number of items to return</param>
         /// <returns>Topic feed</returns>
         /// <response code="200">OK. The request was successful.</response>
         /// <response code="400">Bad request. The request is invalid.</response>
         /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
         /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("featured")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<TopicView>))]
        public async Task<IHttpActionResult> GetFeaturedTopics(string cursor = null, int limit = ApiDefaultValues.GetTopicsPageLimit)
        {
            string className = "MyTopicsController";
            string methodName = "GetFeaturedTopics";
            this.LogControllerStart(this.log, className, methodName);

            var topicFeedEntities = await this.topicsManager.ReadFeaturedTopics(this.AppHandle, cursor, limit + 1);

            // Call the TopicsControllerBase's method for getting topics. This method of the base class takes care of tracing also.
            return await this.GetResponse(className, methodName, topicFeedEntities, limit);
        }

        /// <summary>
        /// Create a topic name
        /// </summary>
        /// <param name="request">Post topic name request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No content. The request was successful.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic is not found.</response>
        /// <response code="409">Conflict. Item already exists.</response>
        /// <response code="501">Not implemented. This feature is not implemented.</response>
        [Route("names")]
        [HttpPost]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PostTopicName([FromBody]PostTopicNameRequest request)
        {
            string className = "TopicsController";
            string methodName = "PostTopicName";
            string logEntry = $"PublisherType = {request?.PublisherType}, TopicHandle = {request?.TopicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var userProfileEntity = await this.usersManager.ReadUserProfile(this.UserHandle, this.AppHandle);
            if (userProfileEntity == null)
            {
                return this.Unauthorized(ResponseStrings.UserNotFound);
            }

            // app published topic names can only be created by an administrator of that application
            if (request.PublisherType == PublisherType.App)
            {
                bool isAdmin = await this.appsManager.IsAdminUser(this.AppHandle, this.UserHandle);
                if (!isAdmin)
                {
                    return this.Unauthorized(ResponseStrings.UserUnauthorized);
                }
            }

            // currently, we only support app published topic names
            if (request.PublisherType != PublisherType.App)
            {
                return this.NotImplemented(ResponseStrings.NotImplemented);
            }

            // if the topic handle does not exist, return an error that indicates the topic was not found
            var topicEntity = await this.topicsManager.ReadTopic(request.TopicHandle);
            if (topicEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNotFound);
            }

            // check if the requested topic name is already in use for this app
            var topicNameEntity = await this.topicNamesManager.ReadTopicName(this.AppHandle, request.TopicName);
            if (topicNameEntity != null)
            {
                return this.Conflict(ResponseStrings.ItemExists);
            }

            await this.topicNamesManager.InsertTopicName(
                ProcessType.Frontend,
                this.AppHandle,
                request.TopicName,
                request.TopicHandle);

            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Update a topic name
        /// </summary>
        /// <param name="topicName">Topic name</param>
        /// <param name="request">Update topic name request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No content. The request was successful.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic or topic name is not found.</response>
        /// <response code="501">Not implemented. This feature is not implemented.</response>
        [Route("names/{topicName}")]
        [HttpPut]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PutTopicName(string topicName, [FromBody]PutTopicNameRequest request)
        {
            string className = "TopicsController";
            string methodName = "PutTopicName";
            string logEntry = $"PublisherType = {request?.PublisherType}, TopicHandle = {request?.TopicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var userProfileEntity = await this.usersManager.ReadUserProfile(this.UserHandle, this.AppHandle);
            if (userProfileEntity == null)
            {
                return this.Unauthorized(ResponseStrings.UserNotFound);
            }

            // app published topic names can only be updated by an administrator of that application
            if (request.PublisherType == PublisherType.App)
            {
                bool isAdmin = await this.appsManager.IsAdminUser(this.AppHandle, this.UserHandle);
                if (!isAdmin)
                {
                    return this.Unauthorized(ResponseStrings.UserUnauthorized);
                }
            }

            // currently, we only support app published topic names
            if (request.PublisherType != PublisherType.App)
            {
                return this.NotImplemented(ResponseStrings.NotImplemented);
            }

            // if the topic handle does not exist, return an error that indicates the topic was not found
            var topicEntity = await this.topicsManager.ReadTopic(request.TopicHandle);
            if (topicEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNotFound);
            }

            // find the topic name to update
            var topicNameEntity = await this.topicNamesManager.ReadTopicName(this.AppHandle, topicName);
            if (topicNameEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNameNotFound);
            }

            // the following condition cannot happen right now because we only support App published
            // topic names.  But if we add User published topic names, then this check is needed.
            if (request.PublisherType != topicNameEntity.PublisherType)
            {
                return this.BadRequest();
            }

            topicNameEntity.TopicHandle = request.TopicHandle;

            await this.topicNamesManager.UpdateTopicName(
                ProcessType.Frontend,
                this.AppHandle,
                topicName,
                topicNameEntity);

            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Delete a topic name
        /// </summary>
        /// <param name="topicName">Topic name</param>
        /// <param name="request">Delete topic request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No content. The request was successful.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic name is not found.</response>
        /// <response code="501">Not implemented. This feature is not implemented.</response>
        [Route("names/{topicName}")]
        [HttpDelete]
        [VersionRange("All")]
        public async Task<IHttpActionResult> DeleteTopicName(string topicName, [FromBody]DeleteTopicNameRequest request)
        {
            string className = "TopicsController";
            string methodName = "DeleteTopicName";
            string logEntry = $"PublisherType = {request?.PublisherType}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            var userProfileEntity = await this.usersManager.ReadUserProfile(this.UserHandle, this.AppHandle);
            if (userProfileEntity == null)
            {
                return this.Unauthorized(ResponseStrings.UserNotFound);
            }

            // app published topic names can only be deleted by an administrator of that application
            if (request.PublisherType == PublisherType.App)
            {
                bool isAdmin = await this.appsManager.IsAdminUser(this.AppHandle, this.UserHandle);
                if (!isAdmin)
                {
                    return this.Unauthorized(ResponseStrings.UserUnauthorized);
                }
            }

            // currently, we only support app published topic names
            if (request.PublisherType != PublisherType.App)
            {
                return this.NotImplemented(ResponseStrings.NotImplemented);
            }

            // find the topic name to delete
            var topicNameEntity = await this.topicNamesManager.ReadTopicName(this.AppHandle, topicName);
            if (topicNameEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNameNotFound);
            }

            await this.topicNamesManager.DeleteTopicName(
                ProcessType.Frontend,
                this.AppHandle,
                topicName);

            logEntry += $", TopicHandle = {topicNameEntity?.TopicHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Get a topic by topic name
        /// </summary>
        /// <param name="topicName">Topic name</param>
        /// <param name="publisherType">Publisher type</param>
        /// <returns>Get topic response</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic name is not found.</response>
        /// <response code="501">Not implemented. This feature is not implemented.</response>
        [Route("names/{topicName}")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("v0.7-Cur")]
        [ResponseType(typeof(GetTopicByNameResponse))]
        public async Task<IHttpActionResult> GetTopicByName(string topicName, PublisherType publisherType)
        {
            string className = "TopicsController";
            string methodName = "GetTopicByName";
            string logEntry = $"PublisherType = {publisherType}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // currently, we only support app published topic names
            if (publisherType != PublisherType.App)
            {
                return this.NotImplemented(ResponseStrings.NotImplemented);
            }

            // lookup the topic name using the name and the appHandle
            var topicNameEntity = await this.topicNamesManager.ReadTopicName(this.AppHandle, topicName);
            if (topicNameEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNameNotFound);
            }

            var response = new GetTopicByNameResponse()
            {
                TopicHandle = topicNameEntity.TopicHandle
            };

            logEntry += $", TopicHandle = {response.TopicHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok<GetTopicByNameResponse>(response);
        }

        /// <summary>
        /// Get a topic name
        /// </summary>
        /// <param name="topicName">Topic name</param>
        /// <param name="publisherType">Publisher type</param>
        /// <returns>Get topic response</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic name is not found.</response>
        /// <response code="501">Not implemented. This feature is not implemented.</response>
        [Route("names/{topicName}")]
        [HttpGet]
        [AllowAnonymous]
        [VersionRange("v0.6")]
        [ResponseType(typeof(GetTopicNameResponseV06))]
        public async Task<IHttpActionResult> GetTopicNameV06(string topicName, PublisherType publisherType)
        {
            string className = "TopicsController";
            string methodName = "GetTopicNameV06";
            string logEntry = $"PublisherType = {publisherType}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // currently, we only support app published topic names
            if (publisherType != PublisherType.App)
            {
                return this.NotImplemented(ResponseStrings.NotImplemented);
            }

            // lookup the topic name using the name and the appHandle
            var topicNameEntity = await this.topicNamesManager.ReadTopicName(this.AppHandle, topicName);
            if (topicNameEntity == null)
            {
                return this.NotFound(ResponseStrings.TopicNameNotFound);
            }

            var response = new GetTopicNameResponseV06()
            {
                TopicHandle = topicNameEntity.TopicHandle
            };

            logEntry += $", TopicHandle = {response.TopicHandle}";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok<GetTopicNameResponseV06>(response);
        }
    }
}
