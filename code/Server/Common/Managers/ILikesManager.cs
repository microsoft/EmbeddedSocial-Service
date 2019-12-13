// <copyright file="ILikesManager.cs" company="Microsoft">
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
    /// Likes manager interface
    /// </summary>
    public interface ILikesManager
    {
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
        Task UpdateLike(
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
            ILikeLookupEntity likeLookupEntity);

        /// <summary>
        /// Read like
        /// </summary>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Like lookup entity</returns>
        Task<ILikeLookupEntity> ReadLike(string contentHandle, string userHandle);

        /// <summary>
        /// Read likes for a content
        /// </summary>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of like feed entities</returns>
        Task<IList<ILikeFeedEntity>> ReadLikes(string contentHandle, string cursor, int limit);

        /// <summary>
        /// Read count of likes for a content
        /// </summary>
        /// <param name="contentHandle">Content handle</param>
        /// <returns>Likes count for a content</returns>
        Task<long?> ReadLikesCount(string contentHandle);

        /// <summary>
        /// Read user liked topics
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic feed entities</returns>
        Task<IList<ITopicFeedEntity>> ReadUserLikedTopics(string userHandle, string appHandle, string cursor, int limit);
    }
}
