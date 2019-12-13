// <copyright file="LikesQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Likes queue class
    /// </summary>
    public class LikesQueue : QueueBase, ILikesQueue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LikesQueue"/> class
        /// </summary>
        /// <param name="queueManager">queue manager</param>
        public LikesQueue(IQueueManager queueManager)
            : base(queueManager)
        {
            this.QueueIdentifier = QueueIdentifier.Likes;
        }

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
        public async Task SendLikeMessage(
            string likeHandle,
            ContentType contentType,
            string contentHandle,
            string userHandle,
            bool liked,
            PublisherType contentPublisherType,
            string contentUserHandle,
            DateTime contentCreatedTime,
            string appHandle,
            DateTime lastUpdatedTime)
        {
            LikeMessage message = new LikeMessage()
            {
                LikeHandle = likeHandle,
                ContentType = contentType,
                ContentHandle = contentHandle,
                UserHandle = userHandle,
                Liked = liked,
                ContentPublisherType = contentPublisherType,
                ContentUserHandle = contentUserHandle,
                ContentCreatedTime = contentCreatedTime,
                AppHandle = appHandle,
                LastUpdatedTime = lastUpdatedTime
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }
    }
}
