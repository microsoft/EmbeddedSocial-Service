// <copyright file="NotificationsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Notifications manager class
    /// </summary>
    public class NotificationsManager : INotificationsManager
    {
        /// <summary>
        /// Notifications store
        /// </summary>
        private INotificationsStore notificationsStore;

        /// <summary>
        /// Push notifications manager
        /// </summary>
        private IPushNotificationsManager pushNotificationsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsManager"/> class
        /// </summary>
        /// <param name="notificationsStore">Notifications store</param>
        /// <param name="pushNotificationsManager">Push notifications manager</param>
        public NotificationsManager(INotificationsStore notificationsStore, IPushNotificationsManager pushNotificationsManager)
        {
            this.notificationsStore = notificationsStore;
            this.pushNotificationsManager = pushNotificationsManager;
        }

        /// <summary>
        /// Create a notification
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
        public async Task CreateNotification(
            ProcessType processType,
            string userHandle,
            string appHandle,
            string activityHandle,
            ActivityType activityType,
            string actorUserHandle,
            string actedOnUserHandle,
            ContentType actedOnContentType,
            string actedOnContentHandle,
            DateTime createdTime)
        {
            await this.notificationsStore.InsertNotification(
                StorageConsistencyMode.Strong,
                userHandle,
                appHandle,
                activityHandle,
                activityType,
                actorUserHandle,
                actedOnUserHandle,
                actedOnContentType,
                actedOnContentHandle,
                createdTime);

            // check if the user has already seen this activity in the app
            INotificationsStatusEntity notificationsStatusEntity = await this.ReadNotificationsStatus(userHandle, appHandle);

            // Send a push notification if the user hasn't already seen this notification in the app UI.
            // This is a similar comparison to what is done in GetActivityView
            if (notificationsStatusEntity == null ||
                string.CompareOrdinal(activityHandle, notificationsStatusEntity.ReadActivityHandle) <= 0)
            {
                // send a push notification
                await this.pushNotificationsManager.SendNotification(
                    userHandle,
                    appHandle,
                    activityHandle,
                    activityType,
                    actorUserHandle,
                    actedOnUserHandle,
                    actedOnContentType,
                    actedOnContentHandle,
                    createdTime);
            }
        }

        /// <summary>
        /// Update notifications status.
        /// This records the most recent notification that the user has read (or seen).
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="readActivityHandle">Read activity handle</param>
        /// <param name="notificationsStatusEntity">Notifications status entity</param>
        /// <returns>Update notifications status task</returns>
        public async Task UpdateNotificationsStatus(
            string userHandle,
            string appHandle,
            string readActivityHandle,
            INotificationsStatusEntity notificationsStatusEntity)
        {
            await this.notificationsStore.UpdateNotificationsStatus(
                StorageConsistencyMode.Strong,
                userHandle,
                appHandle,
                readActivityHandle,
                notificationsStatusEntity);
        }

        /// <summary>
        /// Read notifications for a user in an app.
        /// This gets a feed of activities.
        /// This feed is time ordered, with the most recent activity first.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of activity feed entities</returns>
        public async Task<IList<IActivityFeedEntity>> ReadNotifications(string userHandle, string appHandle, string cursor, int limit)
        {
            return await this.notificationsStore.QueryNotifications(userHandle, appHandle, cursor, limit);
        }

        /// <summary>
        /// Read a notification
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="activityHandle">Activity handle</param>
        /// <returns>Activity feed entity</returns>
        public async Task<IActivityFeedEntity> ReadNotification(string userHandle, string appHandle, string activityHandle)
        {
            return await this.notificationsStore.QueryNotification(userHandle, appHandle, activityHandle);
        }

        /// <summary>
        /// Get a count of activities in my notification feed that have an unread status of true.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Outstanding notifications count</returns>
        public async Task<long?> ReadNotificationsCount(string userHandle, string appHandle)
        {
            return await this.notificationsStore.QueryNotificationsCount(userHandle, appHandle);
        }

        /// <summary>
        /// Read notifications status
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Notifications status entity</returns>
        public async Task<INotificationsStatusEntity> ReadNotificationsStatus(string userHandle, string appHandle)
        {
            return await this.notificationsStore.QueryNotificationsStatus(userHandle, appHandle);
        }
    }
}
