// <copyright file="LikesManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Likes manager class
    /// </summary>
    public class LikesManager : ILikesManager
    {
        /// <summary>
        /// Update popular topics score every PopularTopicsUpdateLikesCount
        /// </summary>
        private const int PopularTopicsUpdateLikesCount = 1;

        /// <summary>
        /// Likes store
        /// </summary>
        private ILikesStore likesStore;

        /// <summary>
        /// Topics store
        /// </summary>
        private ITopicsStore topicsStore;

        /// <summary>
        /// Users store
        /// </summary>
        private IUsersStore usersStore;

        /// <summary>
        /// Likes queue
        /// </summary>
        private ILikesQueue likesQueue;

        /// <summary>
        /// <c>Fanout</c> activities queue
        /// </summary>
        private IFanoutActivitiesQueue fanoutActivitiesQueue;

        /// <summary>
        /// Popular topics manager
        /// </summary>
        private IPopularTopicsManager popularTopicsManager;

        /// <summary>
        /// Notifications manager
        /// </summary>
        private INotificationsManager notificationsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LikesManager"/> class
        /// </summary>
        /// <param name="likesStore">Likes store</param>
        /// <param name="topicsStore">Topics store</param>
        /// <param name="usersStore">Users store</param>
        /// <param name="likesQueue">Likes queue</param>
        /// <param name="fanoutActivitiesQueue"><c>Fanout</c> activities queue</param>
        /// <param name="popularTopicsManager">Popular topics manager</param>
        /// <param name="notificationsManager">Notifications manager</param>
        public LikesManager(
            ILikesStore likesStore,
            ITopicsStore topicsStore,
            IUsersStore usersStore,
            ILikesQueue likesQueue,
            IFanoutActivitiesQueue fanoutActivitiesQueue,
            IPopularTopicsManager popularTopicsManager,
            INotificationsManager notificationsManager)
        {
            this.likesStore = likesStore;
            this.topicsStore = topicsStore;
            this.usersStore = usersStore;
            this.likesQueue = likesQueue;
            this.fanoutActivitiesQueue = fanoutActivitiesQueue;
            this.popularTopicsManager = popularTopicsManager;
            this.notificationsManager = notificationsManager;
        }

        /// <summary>
        /// Update like
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="likeHandle">Like handle</param>
        /// <param name="contentType">Content type</param>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="liked">Like status</param>
        /// <param name="contentPublisherType">Content publisher type</param>
        /// <param name="contentUserHandle">User handle of the content publisher</param>
        /// <param name="contentCreatedTime">Content createdTime</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="likeLookupEntity">Like lookup entity</param>
        /// <returns>Update like task</returns>
        public async Task UpdateLike(
            ProcessType processType,
            string likeHandle,
            ContentType contentType,
            string contentHandle,
            string userHandle,
            bool liked,
            PublisherType contentPublisherType,
            string contentUserHandle,
            DateTime contentCreatedTime,
            string appHandle,
            DateTime lastUpdatedTime,
            ILikeLookupEntity likeLookupEntity)
        {
            if (processType == ProcessType.Frontend)
            {
                await this.likesStore.UpdateLike(
                    StorageConsistencyMode.Strong,
                    likeHandle,
                    contentHandle,
                    userHandle,
                    liked,
                    lastUpdatedTime,
                    likeLookupEntity);

                await this.likesQueue.SendLikeMessage(
                    likeHandle,
                    contentType,
                    contentHandle,
                    userHandle,
                    liked,
                    contentPublisherType,
                    contentUserHandle,
                    contentCreatedTime,
                    appHandle,
                    lastUpdatedTime);
            }
            else if (processType == ProcessType.Backend || processType == ProcessType.BackendRetry)
            {
                if (liked & contentPublisherType == PublisherType.User && userHandle != contentUserHandle)
                {
                    await this.notificationsManager.CreateNotification(
                        processType,
                        contentUserHandle,
                        appHandle,
                        likeHandle,
                        ActivityType.Like,
                        userHandle,
                        contentUserHandle,
                        contentType,
                        contentHandle,
                        lastUpdatedTime);
                }

                if (liked && contentType == ContentType.Topic)
                {
                    await this.fanoutActivitiesQueue.SendFanoutActivityMessage(
                        userHandle,
                        appHandle,
                        likeHandle,
                        ActivityType.Like,
                        userHandle,
                        contentUserHandle,
                        contentType,
                        contentHandle,
                        lastUpdatedTime);

                    // TODO: check what happens if the topic is AppPublished?
                    await this.fanoutActivitiesQueue.SendFanoutTopicActivityMessage(
                        contentHandle,
                        appHandle,
                        likeHandle,
                        ActivityType.Like,
                        userHandle,
                        contentUserHandle,
                        contentType,
                        contentHandle,
                        lastUpdatedTime);
                }

                long? likesCount = await this.likesStore.QueryLikesCount(contentHandle);
                long likesCountValue = likesCount.HasValue ? likesCount.Value : 0;
                if (likesCountValue % PopularTopicsUpdateLikesCount == 0)
                {
                    if (contentType == ContentType.Topic)
                    {
                        await this.popularTopicsManager.UpdatePopularTopic(processType, appHandle, contentHandle, contentUserHandle, contentCreatedTime, likesCountValue);
                    }

                    if (contentType == ContentType.Topic && contentPublisherType == PublisherType.User)
                    {
                        await this.popularTopicsManager.UpdatePopularUserTopic(processType, contentUserHandle, appHandle, contentHandle, likesCountValue);
                    }
                }
            }
        }

        /// <summary>
        /// Read like
        /// </summary>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Like lookup entity</returns>
        public async Task<ILikeLookupEntity> ReadLike(string contentHandle, string userHandle)
        {
            return await this.likesStore.QueryLike(contentHandle, userHandle);
        }

        /// <summary>
        /// Read likes for a content
        /// </summary>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of like feed entities</returns>
        public async Task<IList<ILikeFeedEntity>> ReadLikes(string contentHandle, string cursor, int limit)
        {
            return await this.likesStore.QueryLikes(contentHandle, cursor, limit);
        }

        /// <summary>
        /// Read count of likes for a content
        /// </summary>
        /// <param name="contentHandle">Content handle</param>
        /// <returns>Likes count for a content</returns>
        public async Task<long?> ReadLikesCount(string contentHandle)
        {
            return await this.likesStore.QueryLikesCount(contentHandle);
        }

        /// <summary>
        /// Read topics liked by a user
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> ReadUserLikedTopics(string userHandle, string appHandle, string cursor, int limit)
        {
            // TODO: Not implemented yet
            await Task.Delay(0);
            return null;
        }
    }
}
