// <copyright file="IModerationQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Models;

    /// <summary>
    /// The content moderation queue interface
    /// </summary>
    public interface IModerationQueue : IQueueBase
    {
        /// <summary>
        /// Send moderation message for content
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="contentType">the type of content being reported</param>
        /// <param name="contentHandle">uniquely identifies the content</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Send message task</returns>
        Task SendContentModerationMessage(
            string appHandle,
            string moderationHandle,
            ContentType contentType,
            string contentHandle,
            Uri callbackUri);

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
        Task SendImageModerationMessage(
            string appHandle,
            string moderationHandle,
            string blobHandle,
            string userHandle,
            ImageType imageType,
            Uri callbackUri);

        /// <summary>
        /// Send moderation message for user profile data
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="userHandle">Unique identitifier of the user being moderated</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Send message task</returns>
        Task SendUserModerationMessage(
            string appHandle,
            string moderationHandle,
            string userHandle,
            Uri callbackUri);
    }
}
