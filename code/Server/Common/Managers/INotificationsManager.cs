// <copyright file="INotificationsManager.cs" company="Microsoft">
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
    /// Notifications manager interface
    /// </summary>
    public interface INotificationsManager
    {
        /// <summary>
        /// Create notification
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="activityHandle">Activity handle</param>
        /// <param name="activityType">Activity type</param>
        /// <param name="actorUserHandle">Actor user handle</param>
        /// <param name="actedOnUserHandle">Acted on user handle</param>
        /// <param name="actedOnContentType">Acted on content type</param>
        /// <param name="actedOnContentHandle">Acted on content handle</param>
        /// <param name="createdTime">Created time</param>
        /// <returns>Create notification task</returns>
        Task CreateNotification(
            ProcessType processType,
            string userHandle,
            string appHandle,
            string activityHandle,
            ActivityType activityType,
            string actorUserHandle,
            string actedOnUserHandle,
            ContentType actedOnContentType,
            string actedOnContentHandle,
            DateTime createdTime);

        /// <summary>
        /// Update notifications status
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="readActivityHandle">Read activity handle</param>
        /// <param name="notificationsStatusEntity">Notifications status entity</param>
        /// <returns>Update notifications status task</returns>
        Task UpdateNotificationsStatus(
            string userHandle,
            string appHandle,
            string readActivityHandle,
            INotificationsStatusEntity notificationsStatusEntity);

        /// <summary>
        /// Read notifications for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of activity feed entities</returns>
        Task<IList<IActivityFeedEntity>> ReadNotifications(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Read notification
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="activityHandle">Activity handle</param>
        /// <returns>Activity feed entity</returns>
        Task<IActivityFeedEntity> ReadNotification(string userHandle, string appHandle, string activityHandle);

        /// <summary>
        /// Read count of outstanding notifications for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Outstanding notifications count</returns>
        Task<long?> ReadNotificationsCount(string userHandle, string appHandle);

        /// <summary>
        /// Read notifications status
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Notifications status entity</returns>
        Task<INotificationsStatusEntity> ReadNotificationsStatus(string userHandle, string appHandle);
    }
}
