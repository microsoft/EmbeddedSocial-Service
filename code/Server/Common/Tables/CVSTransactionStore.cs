// <copyright file="CVSTransactionStore.cs" company="Microsoft">
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
    using SocialPlus.Utils;

    /// <summary>
    /// API to manage the store hosting CVS transaction data
    /// </summary>
    public class CVSTransactionStore : ICVSTransactionStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CVSTransactionStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public CVSTransactionStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Insert a new CVS transaction into the store
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="moderationHandle">uniquely identifies moderation request</param>
        /// <param name="appHandle">uniquely identifies the app that the user is in</param>
        /// <param name="reportType">is the complaint against a user or content?</param>
        /// <param name="requestTime">when the request was submitted to CVS</param>
        /// <param name="requestBody">body of the request to CVS</param>
        /// <param name="jobId">job id assigned by CVS</param>
        /// <param name="callbackUrl">callback url for CVS callback</param>
        /// <returns>a task that inserts a CVS transaction into the store</returns>
        public async Task InsertTransaction(
            StorageConsistencyMode storageConsistencyMode,
            string moderationHandle,
            string appHandle,
            ReportType reportType,
            DateTime requestTime,
            string requestBody,
            string jobId,
            string callbackUrl)
        {
            // check input
            if (string.IsNullOrWhiteSpace(appHandle))
            {
                throw new ArgumentNullException("appHandle");
            }

            if (string.IsNullOrWhiteSpace(moderationHandle))
            {
                throw new ArgumentNullException("moderationHandle");
            }

            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.CVS);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.CVS, TableIdentifier.CVSLookup) as ObjectTable;

            // create the entity that will be inserted into the table
            CVSTransactionEntity cvsTransactionEntity = new CVSTransactionEntity()
            {
                ModerationHandle = moderationHandle,
                AppHandle = appHandle,
                ReportType = reportType,
                RequestTime = requestTime,
                RequestBody = requestBody,
                JobId = jobId,
                CallbackUrl = callbackUrl,
                CallbackTime = TimeUtils.BeginningOfUnixTime,
                CallbackResponseBody = string.Empty
            };

            var operation = Operation.Insert(lookupTable, moderationHandle, moderationHandle, cvsTransactionEntity);

            // execute the insert
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Update a CVS transaction with a response from CVS
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="moderationHandle">uniquely identifies moderation request</param>
        /// <param name="responseTime">when the response was received from AVERT</param>
        /// <param name="responseBody">body of the response from AVERT</param>
        /// <param name="entity">CVS transaction entity to update</param>
        /// <returns>a task that updates the CVS transaction with the response</returns>
        public async Task UpdateTransactionWithResponse(
            StorageConsistencyMode storageConsistencyMode,
            string moderationHandle,
            DateTime responseTime,
            string responseBody,
            ICVSTransactionEntity entity)
        {
            // check input
            if (string.IsNullOrWhiteSpace(moderationHandle))
            {
                throw new ArgumentNullException("moderationHandle");
            }

            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.CVS);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.CVS, TableIdentifier.CVSLookup) as ObjectTable;

            // update the entity
            entity.CallbackTime = responseTime;
            entity.CallbackResponseBody = responseBody;

            var operation = Operation.Replace(
                    lookupTable,
                    moderationHandle,
                    moderationHandle,
                    entity as CVSTransactionEntity);

            // update the transaction in the store
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete a CVS transaction
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="moderationHandle">moderation handle</param>
        /// <returns>a task that deletes the CVS transaction</returns>
        public async Task DeleteTransaction(StorageConsistencyMode storageConsistencyMode, string moderationHandle)
        {
            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.CVS);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.CVS, TableIdentifier.CVSLookup) as ObjectTable;

            var operation = Operation.Delete(lookupTable, moderationHandle, moderationHandle);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Look up a particular CVS transaction
        /// </summary>
        /// <param name="moderationHandle">uniquely identifies moderation request</param>
        /// <returns>a task that returns the CVS transaction</returns>
        public async Task<ICVSTransactionEntity> QueryTransaction(string moderationHandle)
        {
            // check input
            if (string.IsNullOrWhiteSpace(moderationHandle))
            {
                throw new ArgumentNullException("moderationHandle");
            }

            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.CVS);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.CVS, TableIdentifier.CVSLookup) as ObjectTable;

            // do the lookup & return it
            return await store.QueryObjectAsync<CVSTransactionEntity>(lookupTable, moderationHandle, moderationHandle);
        }
    }
}
