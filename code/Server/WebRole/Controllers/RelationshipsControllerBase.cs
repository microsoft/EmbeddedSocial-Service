// <copyright file="RelationshipsControllerBase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Microsoft.CTStore;
    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRoleCommon;
    using SocialPlus.Utils;

    /// <summary>
    /// Relationships base controller class
    /// </summary>
    public class RelationshipsControllerBase : BaseController
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
        /// Users manager
        /// </summary>
        private readonly IUsersManager usersManager;

        /// <summary>
        /// Views manager
        /// </summary>
        private readonly IViewsManager viewsManager;

        /// <summary>
        /// Handle generator
        /// </summary>
        private readonly IHandleGenerator handleGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipsControllerBase"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="relationshipsManager">Relationships manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="handleGenerator">Handle generator</param>
        public RelationshipsControllerBase(
            ILog log,
            IRelationshipsManager relationshipsManager,
            ITopicsManager topicsManager,
            IUsersManager usersManager,
            IViewsManager viewsManager,
            IHandleGenerator handleGenerator)
        {
            this.log = log;
            this.relationshipsManager = relationshipsManager;
            this.topicsManager = topicsManager;
            this.usersManager = usersManager;
            this.viewsManager = viewsManager;
            this.handleGenerator = handleGenerator;
        }

        /// <summary>
        /// Update a relationship with another user
        /// </summary>
        /// <param name="callerClassName">name of the controller class of the caller</param>
        /// <param name="callerMethodName">name of method insider controller class of the caller (should correspond to an HTTP action)</param>
        /// <param name="relationshipOperation">Relationship operation</param>
        /// <param name="actedOnUserHandle">Acted on user handle</param>
        /// <returns>No content on success</returns>
        protected async Task<IHttpActionResult> UpdateRelationshipToUser(
            string callerClassName,
            string callerMethodName,
            RelationshipOperation relationshipOperation,
            string actedOnUserHandle)
        {
            string actorUserHandle = this.UserHandle;
            DateTime currentTime = DateTime.UtcNow;

            if (actorUserHandle == actedOnUserHandle)
            {
                this.BadRequest(ResponseStrings.NotAllowed);
            }

            IUserProfileEntity userProfileEntity = await this.usersManager.ReadUserProfile(actedOnUserHandle, this.AppHandle);
            if (userProfileEntity == null)
            {
                this.NotFound(ResponseStrings.UserNotFound);
            }

            if (relationshipOperation == RelationshipOperation.FollowUser
                && userProfileEntity.Visibility == UserVisibilityStatus.Private)
            {
                relationshipOperation = RelationshipOperation.PendingUser;
            }

            string followerKeyUserHandle = actorUserHandle;
            string followingKeyUserHandle = actedOnUserHandle;

            if (relationshipOperation == RelationshipOperation.FollowUser
                || relationshipOperation == RelationshipOperation.PendingUser
                || relationshipOperation == RelationshipOperation.UnfollowUser)
            {
                followerKeyUserHandle = actedOnUserHandle;
                followingKeyUserHandle = actorUserHandle;
            }

            IUserRelationshipLookupEntity followerRelationshipLookupEntity
                = await this.relationshipsManager.ReadFollowerRelationship(followerKeyUserHandle, followingKeyUserHandle, this.AppHandle);
            if (followerRelationshipLookupEntity != null && followerRelationshipLookupEntity.LastUpdatedTime > currentTime)
            {
                return this.Conflict(ResponseStrings.NewerItemExists);
            }

            IUserRelationshipLookupEntity followingRelationshipLookupEntity
                = await this.relationshipsManager.ReadFollowingRelationshipToUser(followingKeyUserHandle, followerKeyUserHandle, this.AppHandle);
            if (followingRelationshipLookupEntity != null && followingRelationshipLookupEntity.LastUpdatedTime > currentTime)
            {
                return this.Conflict(ResponseStrings.NewerItemExists);
            }

            if (relationshipOperation == RelationshipOperation.AcceptUser)
            {
                if (followerRelationshipLookupEntity == null
                    || followerRelationshipLookupEntity.UserRelationshipStatus != UserRelationshipStatus.Pending)
                {
                    return this.Forbidden(ResponseStrings.NotAllowed);
                }
            }

            string relationshipHandle = null;
            if (relationshipOperation == RelationshipOperation.AcceptUser
                || relationshipOperation == RelationshipOperation.BlockUser
                || relationshipOperation == RelationshipOperation.FollowUser
                || relationshipOperation == RelationshipOperation.PendingUser)
            {
                relationshipHandle = this.handleGenerator.GenerateShortHandle();
            }

            await this.relationshipsManager.UpdateRelationshipToUser(
                ProcessType.Frontend,
                relationshipOperation,
                relationshipHandle,
                followerKeyUserHandle,
                followingKeyUserHandle,
                this.AppHandle,
                currentTime,
                followerRelationshipLookupEntity,
                followingRelationshipLookupEntity);

            string logEntry = $"ActedOnUserHandle = {actedOnUserHandle}, RelationshipOperation = {relationshipOperation.ToString()}, RelationshipHandle = {relationshipHandle}";
            logEntry += $", OldRelationshipStatus = {followingRelationshipLookupEntity?.UserRelationshipStatus.ToString()}, NewRelationshipStatus = {relationshipOperation.ToString()}";
            this.LogControllerEnd(this.log, callerClassName, callerMethodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Update a relationship with a topic
        /// </summary>
        /// <param name="callerClassName">name of the controller class of the caller</param>
        /// <param name="callerMethodName">name of method insider controller class of the caller (should correspond to an HTTP action)</param>
        /// <param name="relationshipOperation">Relationship operation</param>
        /// <param name="actedOnTopicHandle">Acted on topic handle</param>
        /// <returns>No content on success</returns>
        protected async Task<IHttpActionResult> UpdateRelationshipToTopic(
            string callerClassName,
            string callerMethodName,
            RelationshipOperation relationshipOperation,
            string actedOnTopicHandle)
        {
            string actorUserHandle = this.UserHandle;
            DateTime currentTime = DateTime.UtcNow;

            if (relationshipOperation != RelationshipOperation.FollowTopic && relationshipOperation != RelationshipOperation.UnfollowTopic)
            {
                // the caller should never specify a operation other than FollowTopic or UnfollowTopic
                return this.InternalServerError();
            }

            // get the topic being followed
            TopicView topicView = await this.viewsManager.GetTopicView(actedOnTopicHandle, actorUserHandle);
            if (topicView == null)
            {
                // This could mean one of three situations:
                // (1) the topic has been banned and hence is no longer accessible to anyone,
                // (2) the topic has been deleted,
                // (3) the topic is from a private user that the actor user is not following; this should
                //     not happen because the actor user should not be able to get a topicHandle for a topic
                //     posted by a private user that they are not following
                return this.NotFound(ResponseStrings.TopicNotFound);
            }

            // lookup the existing relationships
            ITopicRelationshipLookupEntity followerRelationshipLookupEntity
                = await this.relationshipsManager.ReadTopicFollowerRelationship(actedOnTopicHandle, actorUserHandle, this.AppHandle);
            if (followerRelationshipLookupEntity != null && followerRelationshipLookupEntity.LastUpdatedTime > currentTime)
            {
                // this relationship has been updated more recently than this request
                return this.Conflict(ResponseStrings.NewerItemExists);
            }

            ITopicRelationshipLookupEntity followingRelationshipLookupEntity =
                await this.relationshipsManager.ReadFollowingRelationshipToTopic(actorUserHandle, actedOnTopicHandle, this.AppHandle);
            if (followingRelationshipLookupEntity != null && followingRelationshipLookupEntity.LastUpdatedTime > currentTime)
            {
                // this relationship has been updated more recently than this request
                return this.Conflict(ResponseStrings.NewerItemExists);
            }

            // if following a topic
            string relationshipHandle = null;
            if (relationshipOperation == RelationshipOperation.FollowTopic)
            {
                // create a relationship handle
                relationshipHandle = this.handleGenerator.GenerateShortHandle();

                // insert the topic into the user's following topic feed
                await this.topicsManager.CreateFollowingTopic(actorUserHandle, this.AppHandle, actedOnTopicHandle);
            }

            if (relationshipOperation == RelationshipOperation.UnfollowTopic)
            {
                try
                {
                    // remove the topic from the user's following topic feed
                    await this.topicsManager.DeleteFollowingTopic(actorUserHandle, this.AppHandle, actedOnTopicHandle);
                }
                catch (NotFoundException)
                {
                    return this.NotFound(ResponseStrings.NotFollowingTopic);
                }
            }

            // submit the request to the relationship manager
            await this.relationshipsManager.UpdateRelationshipToTopic(
                ProcessType.Frontend,
                relationshipOperation,
                relationshipHandle,
                actorUserHandle,
                actedOnTopicHandle,
                this.AppHandle,
                currentTime,
                followerRelationshipLookupEntity,
                followingRelationshipLookupEntity);

            string logEntry = $"TopicHandle = {topicView?.TopicHandle}, RelationshipHandle = {relationshipHandle}, RelationshipOperation = {relationshipOperation.ToString()}";
            this.LogControllerEnd(this.log, callerClassName, callerMethodName, logEntry);

            return this.NoContent();
        }

        /// <summary>
        /// Get relationship users
        /// </summary>
        /// <param name="callerClassName">name of the controller class of the caller</param>
        /// <param name="callerMethodName">name of method insider controller class of the caller (should correspond to an HTTP action)</param>
        /// <param name="relationshipType">Relationship type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>User feed on success</returns>
        protected async Task<IHttpActionResult> GetRelationshipUsers(
            string callerClassName,
            string callerMethodName,
            RelationshipType relationshipType,
            string userHandle,
            string appHandle,
            string cursor,
            int limit)
        {
            IList<IUserRelationshipFeedEntity> userRelationshipFeedEntities = null;
            switch (relationshipType)
            {
                case RelationshipType.Follower:
                    userRelationshipFeedEntities = await this.relationshipsManager.ReadFollowers(userHandle, appHandle, cursor, limit + 1);
                    break;
                case RelationshipType.Following:
                    userRelationshipFeedEntities = await this.relationshipsManager.ReadFollowing(userHandle, appHandle, cursor, limit + 1);
                    break;
                case RelationshipType.PendingUser:
                    userRelationshipFeedEntities = await this.relationshipsManager.ReadPendingUsers(userHandle, appHandle, cursor, limit + 1);
                    break;
                case RelationshipType.BlockedUser:
                    userRelationshipFeedEntities = await this.relationshipsManager.ReadBlockedUsers(userHandle, appHandle, cursor, limit + 1);
                    break;
            }

            FeedResponse<UserCompactView> response = new FeedResponse<UserCompactView>();
            if (userRelationshipFeedEntities.Count == limit + 1)
            {
                userRelationshipFeedEntities.Remove(userRelationshipFeedEntities.Last());
                response.Cursor = userRelationshipFeedEntities.Last().RelationshipHandle;
            }

            response.Data = await this.viewsManager.GetUserCompactViews(userRelationshipFeedEntities, appHandle, userHandle);

            // Concatenate all handles of the users in response data into long strings, delimited by ','
            string userHandles = null;
            if (response.Data != null)
            {
                userHandles = string.Join(",", response.Data.Select(v => v.UserHandle).ToArray());
            }

            string logEntry = $"RelationshipUserHandle = {userHandle}, RelationshipType = {relationshipType}";
            logEntry += $", CountRelationshipUserHandles = {response.Data?.Count}, RelationshipUserHandlesList = [{userHandles}]";
            this.LogControllerEnd(this.log, callerClassName, callerMethodName, logEntry);
            return this.Ok(response);
        }
    }
}
