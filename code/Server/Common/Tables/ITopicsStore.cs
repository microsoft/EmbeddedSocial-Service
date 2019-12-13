// <copyright file="ITopicsStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Topics store interface
    /// </summary>
    public interface ITopicsStore
    {
        /// <summary>
        /// Insert topic
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
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
        /// <param name="createdTime">Created time</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="requestId">Request id associated with the create topic request</param>
        /// <returns>Insert topic task</returns>
        Task InsertTopic(
            StorageConsistencyMode storageConsistencyMode,
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
            DateTime createdTime,
            ReviewStatus reviewStatus,
            string appHandle,
            string requestId);

        /// <summary>
        /// Update topic
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicEntity">Topic entity</param>
        /// <returns>Update topic task</returns>
        Task UpdateTopic(
            StorageConsistencyMode storageConsistencyMode,
            string topicHandle,
            ITopicEntity topicEntity);

        /// <summary>
        /// Delete topic
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Remove topic task</returns>
        Task DeleteTopic(
            StorageConsistencyMode storageConsistencyMode,
            string topicHandle);

        /// <summary>
        /// Query topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Topic entity</returns>
        Task<ITopicEntity> QueryTopic(string topicHandle);

        /// <summary>
        /// Insert recent topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">User handle</param>
        /// <returns>Insert recent topic task</returns>
        Task InsertRecentTopic(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicHandle,
            string topicUserHandle);

        /// <summary>
        /// Delete recent topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Delete recent topic task</returns>
        Task DeleteRecentTopic(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicHandle);

        /// <summary>
        /// Query recent topics
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> QueryRecentTopics(string appHandle, string cursor, int limit);

        /// <summary>
        /// Insert popular topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="timeRange">Time range</param>
        /// <param name="hostAppHandle">Hosting app handle: Master app or client app</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">User handle</param>
        /// <param name="score">Entity value</param>
        /// <param name="expiration">Topic expiration</param>
        /// <returns>Insert popular topic task</returns>
        Task InsertPopularTopic(
            StorageConsistencyMode storageConsistencyMode,
            TimeRange timeRange,
            string hostAppHandle,
            string appHandle,
            string topicHandle,
            string topicUserHandle,
            long score,
            DateTime expiration);

        /// <summary>
        /// Delete popular topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="timeRange">Time range</param>
        /// <param name="hostAppHandle">Hosting app handle: Master app or client app</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">User handle</param>
        /// <returns>Delete popular topic task</returns>
        Task DeletePopularTopic(
            StorageConsistencyMode storageConsistencyMode,
            TimeRange timeRange,
            string hostAppHandle,
            string appHandle,
            string topicHandle,
            string topicUserHandle);

        /// <summary>
        /// Query popular topics today
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <param name="hostAppHandle">Hosting app handle: Master app or client app</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> QueryPopularTopics(TimeRange timeRange, string hostAppHandle, int cursor, int limit);

        /// <summary>
        /// Query popular topics min score
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <param name="hostAppHandle">Hosting app handle: Master app or client app</param>
        /// <returns>Score of the last entity in popular topics feed</returns>
        Task<long?> QueryPopularTopicsMinScore(TimeRange timeRange, string hostAppHandle);

        /// <summary>
        /// Query popular topics expirations
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <param name="hostAppHandle">Host app handle</param>
        /// <param name="expiration">Expiration time</param>
        /// <returns>Topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> QueryPopularTopicsExpirations(TimeRange timeRange, string hostAppHandle, DateTime expiration);

        /// <summary>
        /// Query popular topics count
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <param name="hostAppHandle">Hosting app handle: Master app or client app</param>
        /// <returns>Popular topics feed length</returns>
        Task<long> QueryPopularTopicsCount(TimeRange timeRange, string hostAppHandle);

        /// <summary>
        /// /Query popular topics maximum count
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <returns>Maximum count of popular topics feeds</returns>
        Task<long> QueryPopularTopicsMaxCount(TimeRange timeRange);

        /// <summary>
        /// Insert user topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Insert user topic task</returns>
        Task InsertUserTopic(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string topicHandle);

        /// <summary>
        /// Delete user topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Delete user topic task</returns>
        Task DeleteUserTopic(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string topicHandle);

        /// <summary>
        /// Query user topics
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> QueryUserTopics(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Query user topics count
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User topics count</returns>
        Task<long?> QueryUserTopicsCount(string userHandle, string appHandle);

        /// <summary>
        /// Insert popular user topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="score">Entity value</param>
        /// <returns>Insert popular user topic task</returns>
        Task InsertPopularUserTopic(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string topicHandle,
            long score);

        /// <summary>
        /// Delete popular user topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Delete popular user topic task</returns>
        Task DeletePopularUserTopic(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string topicHandle);

        /// <summary>
        /// Query popular user topics
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> QueryPopularUserTopics(string userHandle, string appHandle, int cursor, int limit);

        /// <summary>
        /// Insert following topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">User handle of topic owner</param>
        /// <returns>Insert following topic task</returns>
        Task InsertFollowingTopic(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string topicHandle,
            string topicUserHandle);

        /// <summary>
        /// Delete following topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Delete following topic task</returns>
        Task DeleteFollowingTopic(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string topicHandle);

        /// <summary>
        /// Query following topics
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> QueryFollowingTopics(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Insert featured topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">User handle</param>
        /// <returns>Insert featured topic task</returns>
        Task InsertFeaturedTopic(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicHandle,
            string topicUserHandle);

        /// <summary>
        /// Delete featured topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Delete featured topic task</returns>
        Task DeleteFeaturedTopic(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicHandle);

        /// <summary>
        /// Query featured topics
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> QueryFeaturedTopics(string appHandle, string cursor, int limit);
    }
}
