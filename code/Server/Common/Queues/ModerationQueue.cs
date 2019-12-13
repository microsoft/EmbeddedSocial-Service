// <copyright file="ModerationQueue.cs" company="Microsoft">
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
    /// Moderation queue class will send a message to the worker to
    /// submit a moderation request with the provider
    /// </summary>
    public class ModerationQueue : QueueBase, IModerationQueue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModerationQueue"/> class
        /// </summary>
        /// <param name="queueManager">Queue manager</param>
        public ModerationQueue(IQueueManager queueManager)
            : base(queueManager)
        {
            this.QueueIdentifier = QueueIdentifier.Moderation;
        }

        /// <summary>
        /// Send moderation message for content
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="contentType">the type of content being reported</param>
        /// <param name="contentHandle">uniquely identifies the content</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Send message task</returns>
        public async Task SendContentModerationMessage(
            string appHandle,
            string moderationHandle,
            ContentType contentType,
            string contentHandle,
            Uri callbackUri)
        {
            ContentModerationMessage message = new ContentModerationMessage()
            {
                AppHandle = appHandle,
                ModerationHandle = moderationHandle,
                ContentType = contentType,
                ContentHandle = contentHandle,
                CallbackUri = callbackUri
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }

        /// <summary>
        /// Send moderation message for image
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="blobHandle">uniquely identifies the content</param>
        /// <param name="userHandle">User who owns the image</param>
        /// <param name="imageType">Image type</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Send message task</returns>
        public async Task SendImageModerationMessage(
            string appHandle,
            string moderationHandle,
            string blobHandle,
            string userHandle,
            ImageType imageType,
            Uri callbackUri)
        {
            ImageModerationMessage message = new ImageModerationMessage()
            {
                AppHandle = appHandle,
                ModerationHandle = moderationHandle,
                BlobHandle = blobHandle,
                UserHandle = userHandle,
                ImageType = imageType,
                CallbackUri = callbackUri
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }

        /// <summary>
        /// Send moderation message for user profile data
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="userHandle">Unique identitifier of the user being moderated</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Send message task</returns>
        public async Task SendUserModerationMessage(string appHandle, string moderationHandle, string userHandle, Uri callbackUri)
        {
            UserModerationMessage message = new UserModerationMessage()
            {
                AppHandle = appHandle,
                ModerationHandle = moderationHandle,
                UserHandle = userHandle,
                CallbackUri = callbackUri
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }
    }
}
