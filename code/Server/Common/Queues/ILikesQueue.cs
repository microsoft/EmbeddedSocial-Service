// <copyright file="ILikesQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System;
    using System.Threading.Tasks;
    using SocialPlus.Models;

    /// <summary>
    /// Likes queue interface
    /// </summary>
    public interface ILikesQueue : IQueueBase
    {
        /// <summary>
        /// Send like message
        /// </summary>
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
        /// <returns>Send message task</returns>
        Task SendLikeMessage(
            string likeHandle,
            ContentType contentType,
            string contentHandle,
            string userHandle,
            bool liked,
            PublisherType contentPublisherType,
            string contentUserHandle,
            DateTime contentCreatedTime,
            string appHandle,
            DateTime lastUpdatedTime);
    }
}
