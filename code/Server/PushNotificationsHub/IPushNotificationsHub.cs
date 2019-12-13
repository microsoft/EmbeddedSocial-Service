// <copyright file="IPushNotificationsHub.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.PushNotificationsHub
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface to managing push notifications hub for each app
    /// </summary>
    public interface IPushNotificationsHub
    {
        /// <summary>
        /// Create or update push notification hub
        /// </summary>
        /// <returns>Create push notification hub task</returns>
        Task Create();

        /// <summary>
        /// Delete push notification hub
        /// </summary>
        /// <returns>Delete push notification hub task</returns>
        Task Delete();

        /// <summary>
        /// Create push notification registration in this hub
        /// </summary>
        /// <param name="clientRegistrationId">registration id from the mobile OS for the app instance being registered</param>
        /// <param name="tags">tags that describe the app and the user</param>
        /// <returns>Hub registration id</returns>
        Task<string> CreateRegistration(string clientRegistrationId, IEnumerable<string> tags);

        /// <summary>
        /// Delete a push notification registration in this hub
        /// </summary>
        /// <param name="hubRegistrationId">Hub registration id for the app instance being removed</param>
        /// <returns>Delete registration task</returns>
        Task DeleteRegistration(string hubRegistrationId);

        /// <summary>
        /// Send a push notification through this notification hub
        /// </summary>
        /// <param name="tags">tags that describe the app and user</param>
        /// <param name="message">Text message to send</param>
        /// <param name="activityHandle">Uniquely identifies the activity that the notification message is about</param>
        /// <returns>Send notification task. True if success.</returns>
        Task<bool> SendNotification(IEnumerable<string> tags, string message, string activityHandle);
    }
}
