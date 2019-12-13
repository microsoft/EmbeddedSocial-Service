// <copyright file="INotificationsStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Notifications store interface
    /// </summary>
    public interface INotificationsStore
    {
        /// <summary>
        /// Insert notification
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
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
        Task InsertNotification(
            StorageConsistencyMode storageConsistencyMode,
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
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="readActivityHandle">Read activity handle</param>
        /// <param name="readNotificationsStatusEntity">Read notifications status entity</param>
        /// <returns>Update notifications status task</returns>
        Task UpdateNotificationsStatus(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string readActivityHandle,
            INotificationsStatusEntity readNotificationsStatusEntity);

        /// <summary>
        /// Query notifications
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Activity feed entities</returns>
        Task<IList<IActivityFeedEntity>> QueryNotifications(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Query notification
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="activityHandle">Activity handle</param>
        /// <returns>Activity feed entity</returns>
        Task<IActivityFeedEntity> QueryNotification(string userHandle, string appHandle, string activityHandle);

        /// <summary>
        /// Query notifications count
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Notifications count</returns>
        Task<long?> QueryNotificationsCount(string userHandle, string appHandle);

        /// <summary>
        /// Query notifications status
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Notifications status entity</returns>
        Task<INotificationsStatusEntity> QueryNotificationsStatus(string userHandle, string appHandle);
    }
}
