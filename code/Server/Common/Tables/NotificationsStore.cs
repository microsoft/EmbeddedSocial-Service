// <copyright file="NotificationsStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using Microsoft.WindowsAzure.Storage;
    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// Default notifications table store implementation that talks to <c>CTStore</c>
    /// </summary>
    public class NotificationsStore : INotificationsStore
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsStore"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="tableStoreManager">cached table store manager</param>
        public NotificationsStore(ILog log, ICTStoreManager tableStoreManager)
        {
            this.log = log;
            this.tableStoreManager = tableStoreManager;
        }

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
        /// <returns>Insert notification task</returns>
        public async Task InsertNotification(
            StorageConsistencyMode storageConsistencyMode,
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
            ActivityFeedEntity activityFeedEntity = new ActivityFeedEntity()
            {
                ActivityHandle = activityHandle,
                AppHandle = appHandle,
                ActivityType = activityType,
                ActorUserHandle = actorUserHandle,
                ActedOnUserHandle = actedOnUserHandle,
                ActedOnContentType = actedOnContentType,
                ActedOnContentHandle = actedOnContentHandle,
                CreatedTime = createdTime
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Notifications);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.Notifications, TableIdentifier.NotificationsFeed) as FeedTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.Notifications, TableIdentifier.NotificationsCount) as CountTable;

            // do an insert & increment in a transaction.
            // if a queue message for inserting a notification gets processed twice, then this transaction will generate
            // a storage exception on the second attempt (because the insert will fail with a conflict (409) http status)
            try
            {
                Transaction transaction = new Transaction();
                transaction.Add(Operation.Insert(feedTable, userHandle, appHandle, activityHandle, activityFeedEntity));
                transaction.Add(Operation.InsertOrIncrement(countTable, userHandle, appHandle));
                await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
            }
            catch (StorageException e)
            {
                // ignore this exception only if item exists (error code 409 conflict)
                if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
                {
                    this.log.LogInformation("NotificationsStore.InsertNotification received a conflict on insert" + e.Message);
                }
                else
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// Update notifications status
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="readActivityHandle">Read activity handle</param>
        /// <param name="readNotificationsStatusEntity">Read notifications status entity</param>
        /// <returns>Update notifications status task</returns>
        public async Task UpdateNotificationsStatus(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string readActivityHandle,
            INotificationsStatusEntity readNotificationsStatusEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Notifications);
            ObjectTable statusTable = this.tableStoreManager.GetTable(ContainerIdentifier.Notifications, TableIdentifier.NotificationsStatus) as ObjectTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.Notifications, TableIdentifier.NotificationsCount) as CountTable;

            Transaction transaction = new Transaction();
            if (readNotificationsStatusEntity == null)
            {
                NotificationsStatusEntity notificationsStatusEntity = new NotificationsStatusEntity()
                {
                    ReadActivityHandle = readActivityHandle
                };

                transaction.Add(Operation.Insert(statusTable, userHandle, appHandle, notificationsStatusEntity));
            }
            else
            {
                readNotificationsStatusEntity.ReadActivityHandle = readActivityHandle;
                transaction.Add(Operation.Replace(statusTable, userHandle, appHandle, readNotificationsStatusEntity as NotificationsStatusEntity));
            }

            transaction.Add(Operation.InsertOrReplace(countTable, userHandle, appHandle, 0));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query notifications
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Activity feed entities</returns>
        public async Task<IList<IActivityFeedEntity>> QueryNotifications(string userHandle, string appHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Notifications);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Notifications, TableIdentifier.NotificationsFeed) as FeedTable;
            var result = await store.QueryFeedAsync<ActivityFeedEntity>(table, userHandle, appHandle, cursor, limit);
            return result.ToList<IActivityFeedEntity>();
        }

        /// <summary>
        /// Query notification
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="activityHandle">Activity handle</param>
        /// <returns>Activity feed entity</returns>
        public async Task<IActivityFeedEntity> QueryNotification(string userHandle, string appHandle, string activityHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Notifications);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Notifications, TableIdentifier.NotificationsFeed) as FeedTable;
            return await store.QueryFeedItemAsync<ActivityFeedEntity>(table, userHandle, appHandle, activityHandle);
        }

        /// <summary>
        /// Query notifications count
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Notifications count</returns>
        public async Task<long?> QueryNotificationsCount(string userHandle, string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Notifications);
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.Notifications, TableIdentifier.NotificationsCount) as CountTable;
            var result = await store.QueryCountAsync(countTable, userHandle, appHandle);
            if (result == null)
            {
                return null;
            }

            return (long)result.Count;
        }

        /// <summary>
        /// Query notifications status
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Notifications status entity</returns>
        public async Task<INotificationsStatusEntity> QueryNotificationsStatus(string userHandle, string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Notifications);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Notifications, TableIdentifier.NotificationsStatus) as ObjectTable;
            NotificationsStatusEntity notificationsStatusEntity = await store.QueryObjectAsync<NotificationsStatusEntity>(table, userHandle, appHandle);
            return notificationsStatusEntity;
        }
    }
}
