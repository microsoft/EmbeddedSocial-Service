// <copyright file="PushRegistrationsStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// Push Registrations store class
    /// </summary>
    public class PushRegistrationsStore : IPushRegistrationsStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PushRegistrationsStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public PushRegistrationsStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Insert push registrations
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="platformType">Platform type</param>
        /// <param name="registrationId">OS registration id</param>
        /// <param name="hubRegistrationId">Hub registration id</param>
        /// <param name="language">Client language</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <returns>Insert push registration task</returns>
        public async Task InsertPushRegistration(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            PlatformType platformType,
            string registrationId,
            string hubRegistrationId,
            string language,
            DateTime lastUpdatedTime)
        {
            PushRegistrationFeedEntity entity = new PushRegistrationFeedEntity()
            {
                UserHandle = userHandle,
                AppHandle = appHandle,
                PlatformType = platformType,
                OSRegistrationId = registrationId,
                HubRegistrationId = hubRegistrationId,
                Language = language,
                LastUpdatedTime = lastUpdatedTime
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PushRegistrations);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PushRegistrations, TableIdentifier.PushRegistrationsFeed) as FeedTable;
            Operation operation = Operation.InsertOrReplace(table, userHandle, appHandle, registrationId, entity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete push registrations
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="registrationId">OS registration id</param>
        /// <returns>Delete push registration task</returns>
        public async Task DeletePushRegistration(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string registrationId)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PushRegistrations);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PushRegistrations, TableIdentifier.PushRegistrationsFeed) as FeedTable;
            Operation operation = Operation.Delete(table, userHandle, appHandle, registrationId);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query push registrations
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Push registration entities</returns>
        public async Task<IList<IPushRegistrationFeedEntity>> QueryPushRegistrations(
            string userHandle,
            string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PushRegistrations);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PushRegistrations, TableIdentifier.PushRegistrationsFeed) as FeedTable;
            var result = await store.QueryFeedAsync<PushRegistrationFeedEntity>(table, userHandle, appHandle, null, int.MaxValue);
            return result.ToList<IPushRegistrationFeedEntity>();
        }

        /// <summary>
        /// Query push registration
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="registrationId">OS registration id</param>
        /// <returns>Push registration entities</returns>
        public async Task<IPushRegistrationFeedEntity> QueryPushRegistration(
            string userHandle,
            string appHandle,
            string registrationId)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PushRegistrations);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PushRegistrations, TableIdentifier.PushRegistrationsFeed) as FeedTable;
            return await store.QueryFeedItemAsync<PushRegistrationFeedEntity>(table, userHandle, appHandle, registrationId);
        }
    }
}
