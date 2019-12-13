// <copyright file="IModerationStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Moderation store
    /// </summary>
    public interface IModerationStore
    {
        /// <summary>
        /// Insert a new moderation request to store.
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="moderationHandle">uniquely identifies this moderation request</param>
        /// <param name="imageHandle">image handle</param>
        /// <param name="contentType">the type of content being reported</param>
        /// <param name="contentHandle">content handle</param>
        /// <param name="userHandle">uniquely identifies the user who authored the content</param>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="imageType">image type</param>
        /// <param name="moderationStatus">Moderation status</param>
        /// <param name="createdTime">When the moderation request was recieved</param>
        /// <returns>a task that inserts the moderation request into the store</returns>
        Task InsertModeration(
            StorageConsistencyMode storageConsistencyMode,
            string moderationHandle,
            string imageHandle,
            ContentType contentType,
            string contentHandle,
            string userHandle,
            string appHandle,
            ImageType imageType,
            ModerationStatus moderationStatus,
            DateTime createdTime);

        /// <summary>
        /// Look up a particular moderation entity
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="moderationHandle">uniquely identifies this moderation request</param>
        /// <returns>a task that returns the content moderation</returns>
        Task<IModerationEntity> QueryModeration(string appHandle, string moderationHandle);

        /// <summary>
        /// Updates a moderation entity
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="moderationHandle">uniquely identifies this moderation request</param>
        /// <param name="moderationStatus">Moderation status</param>
        /// <param name="entity"><see cref="IModerationEntity"/> to update</param>
        /// <returns>Content moderation entity update task</returns>
        Task UpdateModerationStatus(
            StorageConsistencyMode storageConsistencyMode,
            string moderationHandle,
            ModerationStatus moderationStatus,
            IModerationEntity entity);

        /// <summary>
        /// Delete content moderation entity
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="moderationHandle">moderation handle</param>
        /// <param name="appHandle">app handle</param>
        /// <returns>A task that deletes the content moderation entity</returns>
        Task DeleteModeration(
            StorageConsistencyMode storageConsistencyMode,
            string moderationHandle,
            string appHandle);
    }
}
