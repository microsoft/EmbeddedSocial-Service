// <copyright file="AVERTStore.cs" company="Microsoft">
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
    /// Store that contains AVERT transactions
    /// </summary>
    public class AVERTStore : IAVERTStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVERTStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public AVERTStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Insert a new submission to AVERT into the store
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="reportHandle">uniquely identifies this report</param>
        /// <param name="appHandle">uniquely identifies the app that the user is in</param>
        /// <param name="reportType">is the complaint against a user or content?</param>
        /// <param name="requestTime">when the request was submitted to AVERT</param>
        /// <param name="requestBody">body of the request to AVERT</param>
        /// <returns>a task that inserts the submission into the store</returns>
        public async Task InsertSubmission(
            StorageConsistencyMode storageConsistencyMode,
            string reportHandle,
            string appHandle,
            ReportType reportType,
            DateTime requestTime,
            string requestBody)
        {
            // check input
            if (string.IsNullOrWhiteSpace(reportHandle))
            {
                throw new ArgumentNullException("reportHandle");
            }

            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AVERT);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.AVERT, TableIdentifier.AVERTLookup) as ObjectTable;

            // create the entity that will be inserted into the table
            AVERTTransactionEntity avertTransactionEntity = new AVERTTransactionEntity()
            {
                ReportHandle = reportHandle,
                AppHandle = appHandle,
                ReportType = reportType,
                RequestTime = requestTime,
                RequestBody = requestBody,
                ResponseTime = TimeUtils.BeginningOfUnixTime,
                ResponseBody = null
            };

            // insert it
            await store.ExecuteOperationAsync(Operation.Insert(lookupTable, reportHandle, reportHandle, avertTransactionEntity), storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Insert a response from AVERT into the store
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="reportHandle">uniquely identifies this report</param>
        /// <param name="responseTime">when the response was received from AVERT</param>
        /// <param name="responseBody">body of the response from AVERT</param>
        /// <param name="entity">AVERT transaction entity to update</param>
        /// <returns>a task that inserts the response into the store</returns>
        public async Task InsertResponse(
            StorageConsistencyMode storageConsistencyMode,
            string reportHandle,
            DateTime responseTime,
            string responseBody,
            IAVERTTransactionEntity entity)
        {
            // check inputs
            if (string.IsNullOrWhiteSpace(reportHandle))
            {
                throw new ArgumentNullException("reportHandle");
            }

            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AVERT);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.AVERT, TableIdentifier.AVERTLookup) as ObjectTable;

            // update the entity
            entity.ResponseTime = responseTime;
            entity.ResponseBody = responseBody;

            // update it in the store
            await store.ExecuteOperationAsync(Operation.Replace(lookupTable, reportHandle, reportHandle, entity as AVERTTransactionEntity), storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Look up a particular AVERT transaction
        /// </summary>
        /// <param name="reportHandle">uniquely identifies this report</param>
        /// <returns>a task that returns the AVERT transaction</returns>
        public async Task<IAVERTTransactionEntity> QueryTransaction(string reportHandle)
        {
            // check input
            if (string.IsNullOrWhiteSpace(reportHandle))
            {
                throw new ArgumentNullException("reportHandle");
            }

            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AVERT);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.AVERT, TableIdentifier.AVERTLookup) as ObjectTable;

            // do the lookup & return it
            return await store.QueryObjectAsync<AVERTTransactionEntity>(lookupTable, reportHandle, reportHandle);
        }
    }
}
