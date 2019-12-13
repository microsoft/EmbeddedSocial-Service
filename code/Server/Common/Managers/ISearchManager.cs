// <copyright file="ISearchManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Server.Entities;

    /// <summary>
    /// Search manager interface
    /// </summary>
    public interface ISearchManager
    {
        /// <summary>
        /// Index topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="title">Topic title</param>
        /// <param name="text">Topic text</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="lastModifiedTime">When this topic was created or modified</param>
        /// <returns>Index topic task</returns>
        Task IndexTopic(string topicHandle, string title, string text, string userHandle, string appHandle, DateTime lastModifiedTime);

        /// <summary>
        /// Remove topic from index
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Remove topic from index task</returns>
        Task RemoveTopic(string topicHandle);

        /// <summary>
        /// Index user
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Index user task</returns>
        Task IndexUser(string userHandle, string firstName, string lastName, string appHandle);

        /// <summary>
        /// Remove user from index
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Remove user from index task</returns>
        Task RemoveUser(string userHandle, string appHandle);

        /// <summary>
        /// Search topics
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> GetTopics(string query, string appHandle, int cursor, int limit);

        /// <summary>
        /// Search users
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>User feed entities</returns>
        Task<IList<IUserFeedEntity>> GetUsers(string query, string appHandle, int cursor, int limit);

        /// <summary>
        /// Get trending <c>hashtags</c>
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>List of <c>hashtags</c></returns>
        Task<IList<string>> GetTrendingHashtags(string appHandle);

        /// <summary>
        /// Get autocompleted <c>hashtags</c>
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>List of <c>hashtags</c></returns>
        Task<IList<string>> GetAutocompletedHashtags(string query, string appHandle);
    }
}
