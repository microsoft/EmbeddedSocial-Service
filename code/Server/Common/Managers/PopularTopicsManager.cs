// <copyright file="PopularTopicsManager.cs" company="Microsoft">
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
    /// Popular topics manager class
    /// </summary>
    public class PopularTopicsManager : IPopularTopicsManager
    {
        /// <summary>
        /// Topics store
        /// </summary>
        private ITopicsStore topicsStore;

        /// <summary>
        /// Users store
        /// </summary>
        private IUsersStore usersStore;

        /// <summary>
        /// Apps store
        /// </summary>
        private IAppsStore appsStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="PopularTopicsManager"/> class
        /// </summary>
        /// <param name="topicsStore">Topics store</param>
        /// <param name="usersStore">Users store</param>
        /// <param name="appsStore">Apps store</param>
        public PopularTopicsManager(
            ITopicsStore topicsStore,
            IUsersStore usersStore,
            IAppsStore appsStore)
        {
            this.topicsStore = topicsStore;
            this.usersStore = usersStore;
            this.appsStore = appsStore;
        }

        /// <summary>
        /// Insert popular topic
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">User handle</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="likesCount">Likes count</param>
        /// <returns>Insert popular topic task</returns>
        public async Task UpdatePopularTopic(
            ProcessType processType,
            string appHandle,
            string topicHandle,
            string topicUserHandle,
            DateTime createdTime,
            long likesCount)
        {
            List<string> appHandles = new List<string>() { appHandle, MasterApp.AppHandle };
            foreach (TimeRange timeRange in Enum.GetValues(typeof(TimeRange)))
            {
                foreach (string hostAppHandle in appHandles)
                {
                    long topicsCount = await this.topicsStore.QueryPopularTopicsCount(timeRange, hostAppHandle);
                    long topicsMaxCount = await this.topicsStore.QueryPopularTopicsMaxCount(timeRange);
                    bool insertPopularTopic = false;
                    if (topicsCount < topicsMaxCount)
                    {
                        insertPopularTopic = true;
                    }
                    else
                    {
                        long? minScore = await this.topicsStore.QueryPopularTopicsMinScore(timeRange, hostAppHandle);
                        if (minScore.HasValue && likesCount >= minScore.Value)
                        {
                            insertPopularTopic = true;
                        }
                    }

                    if (insertPopularTopic)
                    {
                        await this.topicsStore.InsertPopularTopic(StorageConsistencyMode.Strong, timeRange, hostAppHandle, appHandle, topicHandle, topicUserHandle, likesCount, this.GetExpirationTime(createdTime, timeRange));
                    }
                }
            }
        }

        /// <summary>
        /// Expire topics
        /// </summary>
        /// <returns>Expire topics task</returns>
        public async Task ExpireTopics()
        {
            var appFeedEntities = await this.appsStore.QueryAllApps();
            foreach (var appFeedEntity in appFeedEntities)
            {
                await this.ExpireTopics(appFeedEntity.AppHandle);
            }

            await this.ExpireTopics(MasterApp.AppHandle);
        }

        /// <summary>
        /// Expire topics for app
        /// </summary>
        /// <param name="hostAppHandle">Host app handle</param>
        /// <returns>Expire topics task</returns>
        public async Task ExpireTopics(string hostAppHandle)
        {
            foreach (TimeRange timeRange in Enum.GetValues(typeof(TimeRange)))
            {
                if (timeRange == TimeRange.AllTime)
                {
                    continue;
                }

                DateTime currentTime = DateTime.UtcNow;
                var topicFeedEntities = await this.topicsStore.QueryPopularTopicsExpirations(timeRange, hostAppHandle, currentTime);
                foreach (var topicFeedEntity in topicFeedEntities)
                {
                    await this.topicsStore.DeletePopularTopic(
                        StorageConsistencyMode.Strong,
                        timeRange,
                        hostAppHandle,
                        topicFeedEntity.AppHandle,
                        topicFeedEntity.TopicHandle,
                        topicFeedEntity.UserHandle);
                }
            }
        }

        /// <summary>
        /// Delete popular topic
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">Topic user handle</param>
        /// <returns>Delete popular topic task</returns>
        public async Task DeletePopularTopic(
            ProcessType processType,
            string appHandle,
            string topicHandle,
            string topicUserHandle)
        {
            List<string> appHandles = new List<string>() { appHandle, MasterApp.AppHandle };
            foreach (TimeRange timeRange in Enum.GetValues(typeof(TimeRange)))
            {
                foreach (string hostAppHandle in appHandles)
                {
                    await this.topicsStore.DeletePopularTopic(StorageConsistencyMode.Strong, timeRange, hostAppHandle, appHandle, topicHandle, topicUserHandle);
                }
            }
        }

        /// <summary>
        /// Insert popular user topic
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="likesCount">Likes count</param>
        /// <returns>Insert popular user topic task</returns>
        public async Task UpdatePopularUserTopic(
            ProcessType processType,
            string userHandle,
            string appHandle,
            string topicHandle,
            long likesCount)
        {
            await this.topicsStore.InsertPopularUserTopic(StorageConsistencyMode.Strong, userHandle, appHandle, topicHandle, likesCount);
        }

        /// <summary>
        /// Insert popular user topic
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Insert popular user topic task</returns>
        public async Task DeletePopularUserTopic(
            ProcessType processType,
            string userHandle,
            string appHandle,
            string topicHandle)
        {
            await this.topicsStore.DeletePopularUserTopic(StorageConsistencyMode.Strong, userHandle, appHandle, topicHandle);
        }

        /// <summary>
        /// Read popular topics
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> ReadPopularTopics(TimeRange timeRange, string appHandle, int cursor, int limit)
        {
            return await this.topicsStore.QueryPopularTopics(timeRange, appHandle, cursor, limit);
        }

        /// <summary>
        /// Read popular user topics
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> ReadPopularUserTopics(string userHandle, string appHandle, int cursor, int limit)
        {
            return await this.topicsStore.QueryPopularUserTopics(userHandle, appHandle, cursor, limit);
        }

        /// <summary>
        /// Get expiration time
        /// </summary>
        /// <param name="createdTime">Content created time</param>
        /// <param name="timeRange">Time range</param>
        /// <returns>Expiration time</returns>
        private DateTime GetExpirationTime(DateTime createdTime, TimeRange timeRange)
        {
            switch (timeRange)
            {
                case TimeRange.Today:
                    return createdTime.AddDays(1);
                case TimeRange.ThisWeek:
                    return createdTime.AddDays(7);
                case TimeRange.ThisMonth:
                    return createdTime.AddDays(30);
            }

            return DateTime.MaxValue;
        }
    }
}
