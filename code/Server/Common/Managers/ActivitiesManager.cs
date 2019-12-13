// <copyright file="ActivitiesManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Activity Manager manages the "Following activity feed" for an user.
    /// A following activity feed is a feed of activities done by the people a user is following.
    /// It exposes methods to fanout an user activity to all his followers and read the following activity feed.
    /// Currently, we support the activities below in the "following activity feed"
    /// 1. User likes a topic (we do not fanout comment and reply likes)
    /// 2. User comments on a topic
    /// 3. User replies to a comment
    /// 4. User follows another user
    /// In the future, if we decide to support a feed for "My activities", the methods to manage it should be implemented in this file.
    /// </summary>
    public class ActivitiesManager : IActivitiesManager
    {
        /// <summary>
        /// Activities store
        /// This manager primarily operates on the activities store
        /// </summary>
        private IActivitiesStore activitiesStore;

        /// <summary>
        /// User relationships store
        /// This manager requires the user relationships store to query for followers for <c>fanout</c>
        /// </summary>
        private IUserRelationshipsStore userRelationshipsStore;

        /// <summary>
        /// Topic relationships store
        /// This manager requires the topic relationships store to query for followers for <c>fanout</c>
        /// </summary>
        private ITopicRelationshipsStore topicRelationshipsStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivitiesManager"/> class
        /// </summary>
        /// <param name="activitiesStore">Activities store</param>
        /// <param name="userRelationshipsStore">User relationships store</param>
        /// <param name="topicRelationshipsStore">Topic relationships store</param>
        public ActivitiesManager(
            IActivitiesStore activitiesStore,
            IUserRelationshipsStore userRelationshipsStore,
            ITopicRelationshipsStore topicRelationshipsStore)
        {
            this.activitiesStore = activitiesStore;
            this.userRelationshipsStore = userRelationshipsStore;
            this.topicRelationshipsStore = topicRelationshipsStore;
        }

        /// <summary>
        /// <c>Fanout</c> activity to followers
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="activityHandle">Activity handle</param>
        /// <param name="activityType">Activity type</param>
        /// <param name="actorUserHandle">Actor user handle</param>
        /// <param name="actedOnUserHandle">Acted on user handle</param>
        /// <param name="actedOnContentType">Acted on content type</param>
        /// <param name="actedOnContentHandle">Acted on content handle</param>
        /// <param name="createdTime">Created time</param>
        /// <returns><c>Fanout</c> activity task</returns>
        public async Task FanoutActivity(
            string userHandle,
            string appHandle,
            string activityHandle,
            ActivityType activityType,
            string actorUserHandle,
            string actedOnUserHandle,
            ContentType actedOnContentType,
            string actedOnContentHandle,
            DateTime createdTime)
        {
            // Called by fanout activities worker
            // Implements:
            // Query *all* followers of the user who did the activity (specified by user handle). Loop through them and insert activity into their activity feeds.

            // TODO: In the future, we want to split this task into sub tasks.
            // For example, we want each sub task (FanoutActivitySub -- not yet implemented below) to fanout for at most 1000 users.
            // To split this task into sub task, we can insert one queue message for every 1000 followers.
            // The message will include the start index and end index for querying followers.
            // NOTE: We don't have a way to query followers by index range yet (e.g. we can't query 1000 to 2000 followers for a user).
            // This restriction stems from the capabilities of Azure table storage. In Redis, we can easily do this query.
            // To implement the sub task design, one option is to keep the social graph (follower, following relationships) entirely in persistent Redis.
            var relationshipFeedEntities = await this.userRelationshipsStore.QueryFollowers(userHandle, appHandle, null, int.MaxValue);
            foreach (var relationshipFeedEntity in relationshipFeedEntities)
            {
                await this.activitiesStore.InsertFollowingActivity(
                    StorageConsistencyMode.Strong,
                    relationshipFeedEntity.UserHandle,
                    appHandle,
                    activityHandle,
                    activityType,
                    actorUserHandle,
                    actedOnUserHandle,
                    actedOnContentType,
                    actedOnContentHandle,
                    createdTime);
            }
        }

        /// <summary>
        /// <c>Fanout</c> activity to followers between start and end follower index
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="activityHandle">Activity handle</param>
        /// <param name="activityType">Activity type</param>
        /// <param name="actorUserHandle">Actor user handle</param>
        /// <param name="actedOnUserHandle">Acted on user handle</param>
        /// <param name="actedOnContentType">Acted on content type</param>
        /// <param name="actedOnContentHandle">Acted on content handle</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="followerStartIndex">Follower start index</param>
        /// <param name="followerEndIndex">Follower end index</param>
        /// <returns><c>Fanout</c> activity sub task</returns>
        public async Task FanoutActivitySub(
            string userHandle,
            string appHandle,
            string activityHandle,
            ActivityType activityType,
            string actorUserHandle,
            string actedOnUserHandle,
            ContentType actedOnContentType,
            string actedOnContentHandle,
            DateTime createdTime,
            int followerStartIndex,
            int followerEndIndex)
        {
            // Not implemented yet. Please see by comment in the Fanout activity method
            // This method might be implemented in the future
            await Task.Delay(0);
        }

        /// <summary>
        /// <c>Fanout</c> activity to followers of a topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="activityHandle">Activity handle</param>
        /// <param name="activityType">Activity type</param>
        /// <param name="actorUserHandle">Actor user handle</param>
        /// <param name="actedOnUserHandle">Acted on user handle</param>
        /// <param name="actedOnContentType">Acted on content type</param>
        /// <param name="actedOnContentHandle">Acted on content handle</param>
        /// <param name="createdTime">Created time</param>
        /// <returns><c>Fanout</c> activity task</returns>
        public async Task FanoutTopicActivity(
            string topicHandle,
            string appHandle,
            string activityHandle,
            ActivityType activityType,
            string actorUserHandle,
            string actedOnUserHandle,
            ContentType actedOnContentType,
            string actedOnContentHandle,
            DateTime createdTime)
        {
            // Called by fanout activities worker
            // Implements:
            // Query *all* followers of the topic where the activity occured (specified by topic handle). Loop through them and insert activity into their activity feeds.

            // TODO: In the future, we want to split this task into sub tasks.
            // For example, we want each sub task (FanoutActivitySub -- not yet implemented below) to fanout for at most 1000 users.
            // To split this task into sub task, we can insert one queue message for every 1000 followers.
            // The message will include the start index and end index for querying followers.
            // NOTE: We don't have a way to query followers by index range yet (e.g. we can't query 1000 to 2000 followers for a user).
            // This restriction stems from the capabilities of Azure table storage. In Redis, we can easily do this query.
            // To implement the sub task design, one option is to keep the social graph (follower, following relationships) entirely in persistent Redis.
            var relationshipFeedEntities = await this.topicRelationshipsStore.QueryTopicFollowers(topicHandle, appHandle, null, int.MaxValue);
            foreach (var relationshipFeedEntity in relationshipFeedEntities)
            {
                await this.activitiesStore.InsertFollowingActivity(
                    StorageConsistencyMode.Strong,
                    relationshipFeedEntity.UserHandle,
                    appHandle,
                    activityHandle,
                    activityType,
                    actorUserHandle,
                    actedOnUserHandle,
                    actedOnContentType,
                    actedOnContentHandle,
                    createdTime);
            }
        }

        /// <summary>
        /// Read following activities for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of activity feed entities</returns>
        public async Task<IList<IActivityFeedEntity>> ReadFollowingActivities(string userHandle, string appHandle, string cursor, int limit)
        {
            // Directly query the activities store to read the following activity feed
            return await this.activitiesStore.QueryFollowingActivities(userHandle, appHandle, cursor, limit);
        }
    }
}
