// <copyright file="ITopicsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Topics manager interface
    /// </summary>
    public interface ITopicsManager
    {
        /// <summary>
        /// Create topic
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
        Task CreateTopic(
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
            string requestId);

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
        Task UpdateTopic(
            ProcessType processType,
            string topicHandle,
            string title,
            string text,
            BlobType blobType,
            string blobHandle,
            string categories,
            ReviewStatus reviewStatus,
            DateTime lastUpdatedTime,
            ITopicEntity topicEntity);

        /// <summary>
        /// Remove topic
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="publisherType">Publisher type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Remove topic task</returns>
        Task DeleteTopic(
            ProcessType processType,
            string topicHandle,
            PublisherType publisherType,
            string userHandle,
            string appHandle);

        /// <summary>
        /// Insert a topic into the user's combined following topics feed.
        /// This feed consists of topics that a user requests to follow, and topics authored by other users that the specified user is following.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Create following topic task</returns>
        Task CreateFollowingTopic(
            string userHandle,
            string appHandle,
            string topicHandle);

        /// <summary>
        /// Delete topic from combined following topics feed
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Hide topic task</returns>
        Task DeleteFollowingTopic(
            string userHandle,
            string appHandle,
            string topicHandle);

        /// <summary>
        /// <c>Fanout</c> topic to followers
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns><c>Fanout</c> topic task</returns>
        Task FanoutTopic(
            string userHandle,
            string appHandle,
            string topicHandle);

        /// <summary>
        /// <c>Fanout</c> topic to followers between start and end follower index
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="followerStartIndex">Follower start index</param>
        /// <param name="followerEndIndex">Follower end index</param>
        /// <returns><c>Fanout</c> topic sub task</returns>
        Task FanoutTopicSub(
            string userHandle,
            string appHandle,
            string topicHandle,
            int followerStartIndex,
            int followerEndIndex);

        /// <summary>
        /// Import topics from new following user
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="followingUserHandle">Following user handle</param>
        /// <returns>Import topics task</returns>
        Task ImportTopics(
            string userHandle,
            string appHandle,
            string followingUserHandle);

        /// <summary>
        /// Read topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Topic entity</returns>
        Task<ITopicEntity> ReadTopic(string topicHandle);

        /// <summary>
        /// Read topics sorted by time
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> ReadTopics(string appHandle, string cursor, int limit);

        /// <summary>
        /// Read user topics
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> ReadUserTopics(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Read count of user topics in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User topics count in an app</returns>
        Task<long?> ReadUserTopicsCount(string userHandle, string appHandle);

        /// <summary>
        /// Read following topics
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> ReadFollowingTopics(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Read featured topics
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> ReadFeaturedTopics(string appHandle, string cursor, int limit);
    }
}
