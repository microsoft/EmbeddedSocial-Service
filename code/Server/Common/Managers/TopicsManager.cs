// <copyright file="TopicsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Topics manager class
    /// </summary>
    public class TopicsManager : ITopicsManager
    {
        /// <summary>
        /// Number of topics to import from new user following
        /// </summary>
        private const int FollowingImportTopicsCount = 10;

        /// <summary>
        /// Topics store
        /// </summary>
        private ITopicsStore topicsStore;

        /// <summary>
        /// User relationships store
        /// </summary>
        private IUserRelationshipsStore userRelationshipsStore;

        /// <summary>
        /// <c>Fanout</c> topics queue
        /// </summary>
        private IFanoutTopicsQueue fanoutTopicsQueue;

        /// <summary>
        /// Search queue
        /// </summary>
        private ISearchQueue searchQueue;

        /// <summary>
        /// Popular topics manager
        /// </summary>
        private IPopularTopicsManager popularTopicsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicsManager"/> class
        /// </summary>
        /// <param name="topicsStore">Topics store</param>
        /// <param name="userRelationshipsStore">User relationships store</param>
        /// <param name="fanoutTopicsQueue"><c>Fanout</c> topics queue</param>
        /// <param name="searchQueue">Search queue</param>
        /// <param name="popularTopicsManager">Popular topics manager</param>
        public TopicsManager(
            ITopicsStore topicsStore,
            IUserRelationshipsStore userRelationshipsStore,
            IFanoutTopicsQueue fanoutTopicsQueue,
            ISearchQueue searchQueue,
            IPopularTopicsManager popularTopicsManager)
        {
            this.topicsStore = topicsStore;
            this.userRelationshipsStore = userRelationshipsStore;
            this.fanoutTopicsQueue = fanoutTopicsQueue;
            this.searchQueue = searchQueue;
            this.popularTopicsManager = popularTopicsManager;
        }

        /// <summary>
        /// Create a topic
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="title">Topic title</param>
        /// <param name="text">Topic text</param>
        /// <param name="blobType">Blob type</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="categories">Topic categories</param>
        /// <param name="language">Topic language</param>
        /// <param name="group">Topic group</param>
        /// <param name="deepLink">Topic deep link</param>
        /// <param name="friendlyName">Topic friendly name</param>
        /// <param name="publisherType">Publisher type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="userVisibility">User visibility</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="requestId">Request id associated with the create topic request</param>
        /// <returns>Create topic task</returns>
        public async Task CreateTopic(
            ProcessType processType,
            string topicHandle,
            string title,
            string text,
            BlobType blobType,
            string blobHandle,
            string categories,
            string language,
            string group,
            string deepLink,
            string friendlyName,
            PublisherType publisherType,
            string userHandle,
            UserVisibilityStatus userVisibility,
            DateTime createdTime,
            ReviewStatus reviewStatus,
            string appHandle,
            string requestId)
        {
            await this.topicsStore.InsertTopic(
                StorageConsistencyMode.Strong,
                topicHandle,
                title,
                text,
                blobType,
                blobHandle,
                categories,
                language,
                group,
                deepLink,
                friendlyName,
                publisherType,
                userHandle,
                createdTime,
                reviewStatus,
                appHandle,
                requestId);

            if (publisherType == PublisherType.User)
            {
                await this.topicsStore.InsertUserTopic(StorageConsistencyMode.Strong, userHandle, appHandle, topicHandle);
                await this.topicsStore.InsertFollowingTopic(StorageConsistencyMode.Strong, userHandle, appHandle, topicHandle, userHandle);
                await this.popularTopicsManager.UpdatePopularUserTopic(processType, userHandle, appHandle, topicHandle, 0);
            }

            // TODO: should we add a popular app topic feed
            await this.topicsStore.InsertRecentTopic(StorageConsistencyMode.Strong, appHandle, topicHandle, userHandle);
            await this.popularTopicsManager.UpdatePopularTopic(processType, appHandle, topicHandle, userHandle, createdTime, 0);
            await this.searchQueue.SendSearchIndexTopicMessage(topicHandle, createdTime);
            if (publisherType == PublisherType.User)
            {
                await this.fanoutTopicsQueue.SendFanoutTopicMessage(userHandle, appHandle, topicHandle);
            }
        }

        /// <summary>
        /// Update a topic
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="title">Topic title</param>
        /// <param name="text">Topic text</param>
        /// <param name="blobType">Blob type</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="categories">Topic categories</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="topicEntity">Topic entity</param>
        /// <returns>Update topic task</returns>
        public async Task UpdateTopic(
            ProcessType processType,
            string topicHandle,
            string title,
            string text,
            BlobType blobType,
            string blobHandle,
            string categories,
            ReviewStatus reviewStatus,
            DateTime lastUpdatedTime,
            ITopicEntity topicEntity)
        {
            topicEntity.Title = title;
            topicEntity.Text = text;
            topicEntity.BlobType = blobType;
            topicEntity.BlobHandle = blobHandle;
            topicEntity.Categories = categories;
            topicEntity.ReviewStatus = reviewStatus;
            topicEntity.LastUpdatedTime = lastUpdatedTime;

            await this.topicsStore.UpdateTopic(StorageConsistencyMode.Strong, topicHandle, topicEntity);
            await this.searchQueue.SendSearchIndexTopicMessage(topicHandle, lastUpdatedTime);
        }

        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="publisherType">Publisher type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Remove topic task</returns>
        public async Task DeleteTopic(
            ProcessType processType,
            string topicHandle,
            PublisherType publisherType,
            string userHandle,
            string appHandle)
        {
            await this.topicsStore.DeleteTopic(StorageConsistencyMode.Strong, topicHandle);
            if (publisherType == PublisherType.User)
            {
                await this.topicsStore.DeleteUserTopic(StorageConsistencyMode.Strong, userHandle, appHandle, topicHandle);
                await this.popularTopicsManager.DeletePopularUserTopic(processType, userHandle, appHandle, topicHandle);
            }

            await this.topicsStore.DeleteRecentTopic(StorageConsistencyMode.Strong, appHandle, topicHandle);
            await this.popularTopicsManager.DeletePopularTopic(processType, appHandle, topicHandle, userHandle);
            await this.searchQueue.SendSearchRemoveTopicMessage(topicHandle);
        }

        /// <summary>
        /// Insert a topic into the user's combined following topics feed.
        /// This feed consists of topics that a user requests to follow, and topics authored by other users that the specified user is following.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Create following topic task</returns>
        public async Task CreateFollowingTopic(
            string userHandle,
            string appHandle,
            string topicHandle)
        {
            // lookup topic entity
            ITopicEntity topicEntity = await this.topicsStore.QueryTopic(topicHandle);

            // do the insert
            await this.topicsStore.InsertFollowingTopic(StorageConsistencyMode.Strong, userHandle, appHandle, topicHandle, topicEntity.UserHandle);
        }

        /// <summary>
        /// Remove a topic from the feed of topics authored by other users that the specified user is following
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Hide topic task</returns>
        public async Task DeleteFollowingTopic(
            string userHandle,
            string appHandle,
            string topicHandle)
        {
            await this.topicsStore.DeleteFollowingTopic(StorageConsistencyMode.Strong, userHandle, appHandle, topicHandle);
        }

        /// <summary>
        /// <c>Fanout</c> topic to followers
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns><c>Fanout</c> topic task</returns>
        public async Task FanoutTopic(
            string userHandle,
            string appHandle,
            string topicHandle)
        {
            var relationshipFeedEntities = await this.userRelationshipsStore.QueryFollowers(userHandle, appHandle, null, int.MaxValue);
            foreach (var relationshipFeedEntity in relationshipFeedEntities)
            {
                await this.topicsStore.InsertFollowingTopic(StorageConsistencyMode.Strong, relationshipFeedEntity.UserHandle, appHandle, topicHandle, userHandle);
            }
        }

        /// <summary>
        /// <c>Fanout</c> topic to followers between start and end follower index
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="followerStartIndex">Follower start index</param>
        /// <param name="followerEndIndex">Follower end index</param>
        /// <returns><c>Fanout</c> topic sub task</returns>
        public async Task FanoutTopicSub(
            string userHandle,
            string appHandle,
            string topicHandle,
            int followerStartIndex,
            int followerEndIndex)
        {
            await Task.Delay(0);
        }

        /// <summary>
        /// Import topics from a new following user
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="followingUserHandle">Following user handle</param>
        /// <returns>Import topics task</returns>
        public async Task ImportTopics(
            string userHandle,
            string appHandle,
            string followingUserHandle)
        {
            var topicFeedEntities = await this.ReadUserTopics(followingUserHandle, appHandle, null, 10);
            foreach (var topicFeedEntity in topicFeedEntities)
            {
                await this.topicsStore.InsertFollowingTopic(StorageConsistencyMode.Strong, userHandle, appHandle, topicFeedEntity.TopicHandle, topicFeedEntity.UserHandle);
            }
        }

        /// <summary>
        /// Read a topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Topic entity</returns>
        public async Task<ITopicEntity> ReadTopic(string topicHandle)
        {
            return await this.topicsStore.QueryTopic(topicHandle);
        }

        /// <summary>
        /// Read topics sorted by time
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> ReadTopics(string appHandle, string cursor, int limit)
        {
            return await this.topicsStore.QueryRecentTopics(appHandle, cursor, limit);
        }

        /// <summary>
        /// Read user topics
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> ReadUserTopics(string userHandle, string appHandle, string cursor, int limit)
        {
            return await this.topicsStore.QueryUserTopics(userHandle, appHandle, cursor, limit);
        }

        /// <summary>
        /// Read the count of user topics in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User topics count in an app</returns>
        public async Task<long?> ReadUserTopicsCount(string userHandle, string appHandle)
        {
            return await this.topicsStore.QueryUserTopicsCount(userHandle, appHandle);
        }

        /// <summary>
        /// Get the feed of topics authored by other users that the specified user is following
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> ReadFollowingTopics(string userHandle, string appHandle, string cursor, int limit)
        {
            return await this.topicsStore.QueryFollowingTopics(userHandle, appHandle, cursor, limit);
        }

        /// <summary>
        /// Read featured topics
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> ReadFeaturedTopics(string appHandle, string cursor, int limit)
        {
            return await this.topicsStore.QueryFeaturedTopics(appHandle, cursor, limit);
        }
    }
}
