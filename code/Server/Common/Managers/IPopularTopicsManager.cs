// <copyright file="IPopularTopicsManager.cs" company="Microsoft">
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
    /// Popular topics manager interface
    /// </summary>
    public interface IPopularTopicsManager
    {
        /// <summary>
        /// Insert popular topic
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">User handle</param>
        /// <param name="createdTime">Topic creation time</param>
        /// <param name="likesCount">Likes count</param>
        /// <returns>Insert popular topic task</returns>
        Task UpdatePopularTopic(
            ProcessType processType,
            string appHandle,
            string topicHandle,
            string topicUserHandle,
            DateTime createdTime,
            long likesCount);

        /// <summary>
        /// Delete popular topic
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">Topic user handle</param>
        /// <returns>Delete popular topic task</returns>
        Task DeletePopularTopic(
            ProcessType processType,
            string appHandle,
            string topicHandle,
            string topicUserHandle);

        /// <summary>
        /// Expire topics
        /// </summary>
        /// <returns>Expire topics task</returns>
        Task ExpireTopics();

        /// <summary>
        /// Expire topics for app
        /// </summary>
        /// <param name="hostAppHandle">Host app handle</param>
        /// <returns>Expire topics task</returns>
        Task ExpireTopics(string hostAppHandle);

        /// <summary>
        /// Insert popular user topic
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="likesCount">Likes count</param>
        /// <returns>Insert popular user topic task</returns>
        Task UpdatePopularUserTopic(
            ProcessType processType,
            string userHandle,
            string appHandle,
            string topicHandle,
            long likesCount);

        /// <summary>
        /// Insert popular user topic
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Insert popular user topic task</returns>
        Task DeletePopularUserTopic(
            ProcessType processType,
            string userHandle,
            string appHandle,
            string topicHandle);

        /// <summary>
        /// Read popular topics
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> ReadPopularTopics(TimeRange timeRange, string appHandle, int cursor, int limit);

        /// <summary>
        /// Read popular user topics
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> ReadPopularUserTopics(string userHandle, string appHandle, int cursor, int limit);
    }
}
