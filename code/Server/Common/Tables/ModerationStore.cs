// <copyright file="ModerationStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// The content moderation store
    /// </summary>
    public class ModerationStore : IModerationStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModerationStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public ModerationStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Insert a new moderation request to store.
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="moderationHandle">uniquely identifies this moderation request</param>
        /// <param name="imageHandle">image handle</param>
        /// <param name="contentType">the type of content being reported</param>
        /// <param name="contentHandle">content handle</param>
        /// <param name="userHandle">uniquely identifies the content owner</param>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="imageType">image type</param>
        /// <param name="moderationStatus">Moderation status</param>
        /// <param name="createdTime">When the moderation request was received</param>
        /// <returns>a task that inserts the moderation request into the store</returns>
        public async Task InsertModeration(
            StorageConsistencyMode storageConsistencyMode,
            string moderationHandle,
            string imageHandle,
            ContentType contentType,
            string contentHandle,
            string userHandle,
            string appHandle,
            ImageType imageType,
            ModerationStatus moderationStatus,
            DateTime createdTime)
        {
            // get content moderation table
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Moderation);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(
                ContainerIdentifier.Moderation,
                TableIdentifier.ModerationObject) as ObjectTable;

            // create content moderation entity
            ModerationEntity contentModerationEntity = new ModerationEntity()
            {
                AppHandle = appHandle,
                ContentHandle = contentHandle,
                ContentType = contentType,
                ImageHandle = imageHandle,
                ImageType = imageType,
                UserHandle = userHandle,
                ModerationStatus = moderationStatus,
                CreatedTime = createdTime
            };

            var operation = Operation.Insert(lookupTable, appHandle, moderationHandle, contentModerationEntity);

            // execute insert operation
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Look up a particular moderation entity
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="moderationHandle">uniquely identifies this moderation request</param>
        /// <returns>a task that returns the content report</returns>
        public async Task<IModerationEntity> QueryModeration(string appHandle, string moderationHandle)
        {
            if (string.IsNullOrWhiteSpace(appHandle))
            {
                throw new ArgumentNullException("appHandle");
            }

            if (string.IsNullOrWhiteSpace(moderationHandle))
            {
                throw new ArgumentNullException("moderationHandle");
            }

            // get content moderation table
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Moderation);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(
                ContainerIdentifier.Moderation,
                TableIdentifier.ModerationObject) as ObjectTable;

            // do the lookup & return it
            return await store.QueryObjectAsync<ModerationEntity>(lookupTable, appHandle, moderationHandle);
        }

        /// <summary>
        /// Updates content moderation entity
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="moderationHandle">uniquely identifies this moderation request</param>
        /// <param name="moderationStatus">Moderation status</param>
        /// <param name="entity"><see cref="IModerationEntity"/> to update</param>
        /// <returns>Content moderation entity update task</returns>
        public async Task UpdateModerationStatus(
            StorageConsistencyMode storageConsistencyMode,
            string moderationHandle,
            ModerationStatus moderationStatus,
            IModerationEntity entity)
        {
            if (string.IsNullOrWhiteSpace(moderationHandle))
            {
                throw new ArgumentNullException("moderationHandle");
            }

            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            // get content moderation table
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Moderation);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(
                ContainerIdentifier.Moderation,
                TableIdentifier.ModerationObject) as ObjectTable;

            // update the entity
            entity.ModerationStatus = moderationStatus;

            // update it in the store
            var operation = Operation.Replace(lookupTable, entity.AppHandle, moderationHandle, entity as ModerationEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete content moderation entity
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="moderationHandle">moderation handle</param>
        /// <param name="appHandle">app handle</param>
        /// <returns>A task that deletes the content moderation entity</returns>
        public async Task DeleteModeration(
            StorageConsistencyMode storageConsistencyMode,
            string moderationHandle,
            string appHandle)
        {
            // get content moderation table
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Moderation);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(
                ContainerIdentifier.Moderation,
                TableIdentifier.ModerationObject) as ObjectTable;

            // delete moderation entity from the store
            var operation = Operation.Delete(lookupTable, appHandle, moderationHandle);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }
    }
}
