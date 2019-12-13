// <copyright file="IModerationManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;
    using SocialPlus.Models;

    /// <summary>
    /// Moderation manager interface
    /// </summary>
    public interface IModerationManager
    {
        /// <summary>
        /// Creates a content moderation request
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="contentType">Content type</param>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Create content moderation request task</returns>
        Task CreateContentModerationRequest(
            ProcessType processType,
            string appHandle,
            string moderationHandle,
            ContentType contentType,
            string contentHandle,
            Uri callbackUri);

        /// <summary>
        /// Creates a image moderation request
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="userHandle">User who uploaded the image</param>
        /// <param name="imageType">Image type</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Create content moderation request task</returns>
        Task CreateImageModerationRequest(
            ProcessType processType,
            string appHandle,
            string moderationHandle,
            string blobHandle,
            string userHandle,
            ImageType imageType,
            Uri callbackUri);

        /// <summary>
        /// Creates a user moderation request
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="userHandle">Unique identitifier of the user being moderated</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Create user moderation request task</returns>
        Task CreateUserModerationRequest(
            ProcessType processType,
            string appHandle,
            string moderationHandle,
            string userHandle,
            Uri callbackUri);

        /// <summary>
        /// Submit a moderation request for the content to the moderation provider
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="contentType">Content type</param>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Submit content for moderation task</returns>
        Task SubmitContentForModeration(
            ProcessType processType,
            string appHandle,
            string moderationHandle,
            ContentType contentType,
            string contentHandle,
            Uri callbackUri);

        /// <summary>
        /// Submit a moderation request for the content to the moderation provider
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="userHandle">User who owns the imnage</param>
        /// <param name="imageType">Image type</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Submit content for moderation task</returns>
        Task SubmitImageForModeration(
            ProcessType processType,
            string appHandle,
            string moderationHandle,
            string blobHandle,
            string userHandle,
            ImageType imageType,
            Uri callbackUri);

        /// <summary>
        /// Submit a moderation request for the user profile data to the moderation provider
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="userHandle">Unique identitifier of the user being moderated</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Submit user for moderation task</returns>
        Task SubmitUserForModeration(
            ProcessType processType,
            string appHandle,
            string moderationHandle,
            string userHandle,
            Uri callbackUri);

        /// <summary>
        /// Process the results from the moderation provider
        /// </summary>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="results">Results of moderation review</param>
        /// <returns>Task that updates the moderation record</returns>
        Task ProcessModerationResults(string moderationHandle, JToken results);
    }
}
