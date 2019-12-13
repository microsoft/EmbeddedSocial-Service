// <copyright file="MyFollowingController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon;
    using SocialPlus.Utils;

    /// <summary>
    /// API calls to follow, unfollow, and query my following users, my following topics, and my activities
    /// </summary>
    [RoutePrefix("users/me/following")]
    public class MyFollowingController : RelationshipsControllerBase
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Relationships manager
        /// </summary>
        private readonly IRelationshipsManager relationshipsManager;

        /// <summary>
        /// Topics manager
        /// </summary>
        private readonly ITopicsManager topicsManager;

        /// <summary>
        /// Activities manager
        /// </summary>
        private readonly IActivitiesManager activitiesManager;

        /// <summary>
        /// Views manager
        /// </summary>
        private readonly IViewsManager viewsManager;

        /// <summary>
        /// Auth manager (used for finding friends on Facebook)
        /// </summary>
        private readonly ICompositeAuthManager authManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyFollowingController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="relationshipsManager">Relationships manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="activitiesManager">Activities manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="authManager">Auth manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public MyFollowingController(
            ILog log,
            IRelationshipsManager relationshipsManager,
            IUsersManager usersManager,
            ITopicsManager topicsManager,
            IActivitiesManager activitiesManager,
            IViewsManager viewsManager,
            ICompositeAuthManager authManager,
            IHandleGenerator handleGenerator)
            : base(log, relationshipsManager, topicsManager, usersManager, viewsManager, handleGenerator)
        {
            this.log = log;
            this.relationshipsManager = relationshipsManager;
            this.topicsManager = topicsManager;
            this.activitiesManager = activitiesManager;
            this.viewsManager = viewsManager;
            this.authManager = authManager;
        }

        /// <summary>
        /// Follow a user
        /// </summary>
        /// <remarks>
        /// When I follow a user, that user will appear on my following feed. That feed is
        /// visible to all users, unless my profile is set to private, in which case only those
        /// users that request to follow me and I approve will see that feed. If I try to follow a
        /// user with a private profile, then that private user controls whether I am allowed to
        /// follow them or not.
        /// That user's topics will appear in my following topics feed and actions
        /// performed by that user will also appear in my following activities feed.
        /// </remarks>
        /// <param name="request">Post following user request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The user is not found.</response>
        /// <response code="409">Conflict. Newer item exists.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("users")]
        [HttpPost]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PostFollowingUser([FromBody]PostFollowingUserRequest request)
        {
            string className = "MyFollowingController";
            string methodName = "PostFollowingUser";
            string logEntry = $"FollowingUserHandle = {request?.UserHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // Call the RelationshipControllerBase's method for updating the relationship. This method of the base class takes care of tracing also.
            return await this.UpdateRelationshipToUser(className, methodName, RelationshipOperation.FollowUser, request.UserHandle);
        }

        /// <summary>
        /// Follow a topic
        /// </summary>
        /// <remarks>
        /// When I follow a topic, that topic will appear on my following topics feed. When users
        /// perform actions on the topic (such as posting comments or replies), those actions will
        /// appear on my following activites feed.
        /// </remarks>
        /// <param name="request">Post following topic request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic is not found.</response>
        /// <response code="409">Conflict. Newer item exists.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("topics")]
        [HttpPost]
        [VersionRange("All")]
        public async Task<IHttpActionResult> PostFollowingTopic([FromBody]PostFollowingTopicRequest request)
        {
            string className = "MyFollowingController";
            string methodName = "PostFollowingTopic";
            string logEntry = $"FollowingTopicHandle = {request?.TopicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // Call the RelationshipControllerBase's method for updating the relationship. This method of the base class takes care of tracing also.
            return await this.UpdateRelationshipToTopic(className, methodName, RelationshipOperation.FollowTopic, request.TopicHandle);
        }

        /// <summary>
        /// Unfollow a user
        /// </summary>
        /// <remarks>
        /// After I unfollow a user, that user will no longer appear on my following feed.
        /// All of that user's previous topics will be removed from my following topics feed and
        /// none of their future topics will be added to that feed.
        /// Their past and future activities will no longer appear in my following activities feed.
        /// </remarks>
        /// <param name="userHandle">Handle of following user</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The user is not found.</response>
        /// <response code="409">Conflict. Newer item exists.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("users/{userHandle}")]
        [HttpDelete]
        [VersionRange("All")]
        public async Task<IHttpActionResult> DeleteFollowingUser(string userHandle)
        {
            string className = "MyFollowingController";
            string methodName = "DeleteFollowingUser";
            string logEntry = $"FollowingUserHandle = {userHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // Call the RelationshipControllerBase's method for updating the relationship. This method of the base class takes care of tracing also.
            return await this.UpdateRelationshipToUser(className, methodName, RelationshipOperation.UnfollowUser, userHandle);
        }

        /// <summary>
        /// Unfollow a topic
        /// </summary>
        /// <remarks>
        /// After I unfollow a topic, that topic will no longer appear on my following topics feed.
        /// The past and future activities on that topic will no longer appear in my following activities feed.
        /// </remarks>
        /// <param name="topicHandle">Handle of following topic</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic is not found or the user was not following the topic.</response>
        /// <response code="409">Conflict. Newer item exists.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("topics/{topicHandle}")]
        [HttpDelete]
        [VersionRange("All")]
        public async Task<IHttpActionResult> DeleteFollowingTopic(string topicHandle)
        {
            string className = "MyFollowingController";
            string methodName = "DeleteFollowingTopic";
            string logEntry = $"FollowingTopicHandle = {topicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // Call the RelationshipControllerBase's method for updating the relationship. This method of the base class takes care of tracing also.
            return await this.UpdateRelationshipToTopic(className, methodName, RelationshipOperation.UnfollowTopic, topicHandle);
        }

        /// <summary>
        /// Get the feed of users that I am following
        /// </summary>
        /// <remarks>
        /// These are the users whose topics appear on my following topics feed, and whose activities
        /// appear on my following activities feed.
        /// </remarks>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>User feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("users")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<UserCompactView>))]
        public async Task<IHttpActionResult> GetFollowingUsers(string cursor = null, int limit = ApiDefaultValues.GetUsersPageLimit)
        {
            string className = "MyFollowingController";
            string methodName = "GetFollowingUsers";
            this.LogControllerStart(this.log, className, methodName);

            // Call the RelationshipControllerBase's method for getting the users. This method of the base class takes care of tracing also.
            return await this.GetRelationshipUsers(className, methodName, RelationshipType.Following, this.UserHandle, this.AppHandle, cursor, limit);
        }

        /// <summary>
        /// Get the feed of topics that I am following
        /// </summary>
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
        public async Task<IHttpActionResult> GetFollowingTopics(string cursor = null, int limit = ApiDefaultValues.GetUsersPageLimit)
        {
            string className = "MyFollowingController";
            string methodName = "GetFollowingTopics";
            this.LogControllerStart(this.log, className, methodName);

            IList<ITopicRelationshipFeedEntity> topicRelationshipFeedEntities = null;
            topicRelationshipFeedEntities = await this.relationshipsManager.ReadTopicFollowing(this.UserHandle, this.AppHandle, cursor, limit + 1);

            FeedResponse<TopicView> response = new FeedResponse<TopicView>();
            if (topicRelationshipFeedEntities.Count == limit + 1)
            {
                topicRelationshipFeedEntities.Remove(topicRelationshipFeedEntities.Last());
                response.Cursor = topicRelationshipFeedEntities.Last().RelationshipHandle;
            }

            response.Data = await this.viewsManager.GetTopicViews(topicRelationshipFeedEntities, this.UserHandle);

            // Concatenate all handles of the topics in response data into long strings, delimited by ','
            string topicHandles = null;
            if (response.Data != null)
            {
                topicHandles = string.Join(",", response.Data.Select(v => v.TopicHandle).ToArray());
            }

            string logEntry = $"CountFollowingTopicHandles = {response.Data?.Count}, TopicHandlesList = [{topicHandles}]";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(response);
        }

        /// <summary>
        /// Remove a topic from my combined following topics feed.
        /// </summary>
        /// <remarks>
        /// My combined following topics feed is a feed of topics I am explicitly following, combined with topics created by all users
        /// that I am following.  This call will remove the specified topic from that feed.
        /// </remarks>
        /// <param name="topicHandle">Handle of following topic</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The topic is not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("combined/{topicHandle}")]
        [HttpDelete]
        [VersionRange("All")]
        public async Task<IHttpActionResult> DeleteTopicFromCombinedFollowingFeed(string topicHandle)
        {
            string className = "MyFollowingController";
            string methodName = "DeleteTopicFromCombinedFollowingFeed";
            string logEntry = $"FollowingTopicHandle = {topicHandle}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            await this.topicsManager.DeleteFollowingTopic(this.UserHandle, this.AppHandle, topicHandle);
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Get my combined following topics feed.
        /// </summary>
        /// <remarks>
        /// My combined following topics feed includes:
        /// (1) topics that I'm explicitly following and
        /// (2) topics authored by users that I'm following
        ///
        /// This feed is time ordered, with the most recent topic first.
        /// This feed will not include topics that I have explicitly deleted from this feed.
        /// When I follow a user, a limited set of their past topics will be added to this feed,
        /// and all their future topics will be added to this feed when they are created.
        /// When I unfollow a user, all of their previous topics will be removed from the feed and
        /// none of their future topics will be added to this feed.
        /// When I follow a topic, it will appear in this feed.
        /// When I unfollow a topic, it will no longer appear in this feed.
        /// </remarks>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("combined")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<TopicView>))]
        public async Task<IHttpActionResult> GetTopics(string cursor = null, int limit = ApiDefaultValues.GetTopicsPageLimit)
        {
            string className = "MyFollowingController";
            string methodName = "GetTopics";
            this.LogControllerStart(this.log, className, methodName);

            var topicFeedEntities = await this.topicsManager.ReadFollowingTopics(this.UserHandle, this.AppHandle, cursor, limit + 1);
            var response = new FeedResponse<TopicView>();
            if (topicFeedEntities.Count == limit + 1)
            {
                topicFeedEntities.Remove(topicFeedEntities.Last());
                response.Cursor = topicFeedEntities.Last().TopicHandle;
            }

            response.Data = await this.viewsManager.GetTopicViews(topicFeedEntities, this.UserHandle, true);

            // Concatenate all handles of the topics in response data into long strings, delimited by ','
            string topicHandles = null;
            if (response.Data != null)
            {
                topicHandles = string.Join(",", response.Data.Select(v => v.TopicHandle).ToArray());
            }

            string logEntry = $"CountFollowingTopicHandles = {response.Data?.Count}, TopicHandlesList = [{topicHandles}]";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(response);
        }

        /// <summary>
        /// Get the feed of activities by users that I'm following or on topics that I'm following.
        /// </summary>
        /// <remarks>
        /// My following activity feed is a list of activities that are either
        /// (1) performed by users that I am following, or
        /// (2) performed on topics that I am following.
        /// This feed is time ordered, with the most recent activity first.
        /// An activity is added to this feed when a user I am following does one of the following 4 actions:
        /// (a) create a comment; (b) create a reply; (c) like a topic; (d) follow a user.
        /// If a user that I am following is deleted, then their past activities will no longer appear in this feed.
        /// If an activity is performed on content that is then deleted, that activity will no longer appear in this feed.
        /// If a user has un-done an activity (e.g. unlike a previous like), then that activity will no longer appear in this feed.
        /// Similarly, an activity is added to this feed when a user does one of the following 3 actions on a topic that I am following:
        /// (a) create a comment; (b) create a reply; (c) like the topic.
        /// If a topic that I am following is deleted, then past activities on that topic will no longer appear in this feed.
        /// If an activity that is performed is then deleted, that activity will no longer appear in this feed.
        /// Ignore the unread status of each activity - it will always be true.
        /// </remarks>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Activity feed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="401">Unauthorized. The app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("activities")]
        [HttpGet]
        [VersionRange("All")]
        [ResponseType(typeof(FeedResponse<ActivityView>))]
        public async Task<IHttpActionResult> GetActivities(string cursor = null, int limit = ApiDefaultValues.GetActivitiesPageLimit)
        {
            string className = "MyFollowingController";
            string methodName = "GetActivities";
            this.LogControllerStart(this.log, className, methodName);

            // GET users/me/following/activities?cursor={cursor}&limit={limit}
            // Access: Authenticated
            // Validation: None
            // Core logic:
            // 1. Read following activities feed through activities manager
            // 2. Construct view for the feed using Views manager
            var activityFeedEntities = await this.activitiesManager.ReadFollowingActivities(this.UserHandle, this.AppHandle, cursor, limit + 1);
            FeedResponse<ActivityView> response = new FeedResponse<ActivityView>();
            if (activityFeedEntities.Count == limit + 1)
            {
                activityFeedEntities.Remove(activityFeedEntities.Last());
                response.Cursor = activityFeedEntities.Last().ActivityHandle;
            }

            response.Data = await this.viewsManager.GetActivityViews(activityFeedEntities, this.AppHandle, this.UserHandle);

            // Concatenate all handles of the activities in response data into long strings, delimited by ','
            string activityHandles = null;
            if (response.Data != null)
            {
                activityHandles = string.Join(",", response.Data.Select(v => v.ActivityHandle).ToArray());
            }

            string logEntry = $"CountFollowingActivitiesHandles = {response.Data?.Count}, ActivityHandlesList = [{activityHandles}]";
            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.Ok(response);
        }

        /// <summary>
        /// Get my suggestions of users to follow.
        /// </summary>
        /// <remarks>
        /// This call uses the token from the Authorization header to determine the type of suggestions to provide.
        /// In particular, the token determines which third-party to contact to obtain a list of suggested users,
        /// such as friends (for Facebook), following users (for Twitter), and contacts (for Google and Microsoft).
        /// We check each retrieved user to see whether they are registered with Embedded Social (this is done by checking
        /// whether the user appears as a linked account in any Embedded Social profile).
        /// Note that passing a token without the appropiate scopes will prevent Embedded Social from obtaining a list
        /// of suggested users.
        /// Support for input parameters 'cursor' and 'limit' is not implemented in the current API release.
        /// </remarks>
        /// <param name="cursor">Current read cursor</param>
        /// <param name="limit">Number of users compact views to return</param>
        /// <returns>Profiles of users suggested to be followed</returns>
        /// <response code="200">OK. The request was successful.</response>
        /// <response code="400">Bad request. The request is invalid.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        /// <response code="501">Not implemented. </response>
        [Route("suggestions/users")]
        [HttpGet]
        [VersionRange("v0.7-Cur")]
        [ResponseType(typeof(List<UserCompactView>))]
        public async Task<IHttpActionResult> GetSuggestionsUsers(string cursor = null, int limit = ApiDefaultValues.NotImplemented)
        {
            string className = "MyFollowingController";
            string methodName = "GetSuggestionsUsers";
            this.LogControllerStart(this.log, className, methodName);

            // We do not offer support for cursor and limit input parameters. We added them to the API because we might offer
            // support in the future.
            if (cursor != null || limit != ApiDefaultValues.NotImplemented)
            {
                return this.NotImplemented("Support for input parameters not implemented in current API release. Please retry the call without input parameters.");
            }

            List<IPrincipal> friendList = null;
            switch (this.UserPrincipal.IdentityProvider)
            {
                case IdentityProviderType.Facebook:
                    try
                    {
                        friendList = await this.authManager.GetFriends(this.AuthorizationHeader, "Facebook");
                    }
                    catch (Exception ex)
                    {
                        // An exception can be raised due to several reasons, such as access token with the wrong scope (or some other problem), or Facebook being down.
                        // At this point
                        this.log.LogError(string.Format("Exception thrown when getting friends from Facebook. Auth header: {0}", this.AuthorizationHeader), ex);
                        return this.BadRequest("Can't retrieve list of Facebook friends using access token from the Authorization header.");
                    }

                    break;

                default:
                    return this.NotImplemented("Support for this identity provider not offered in current API release.");
            }

            var response = await this.viewsManager.GetUserCompactViews(friendList, this.AppHandle, this.UserHandle);

            // Concatenate all handles of the users in response data into long strings, delimited by ','
            string userHandles = null;
            if (response != null)
            {
                userHandles = string.Join(",", response.Select(v => v.UserHandle).ToArray());
            }

            string logEntry = $"CountSuggestionsUsers = {response?.Count}, UserHandlesList = [{userHandles}]";
            this.LogControllerEnd(this.log, className, methodName, logEntry);

            return this.Ok(response);
        }
    }
}
