// <copyright file="ICVSTransactionStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Provides Api to manage Content Validation Service (CVS) transactions
    /// </summary>
    public interface ICVSTransactionStore
    {
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
        /// <param name="callbackUrl">url for CVS callback</param>
        /// <returns>a task that inserts the submission into the store</returns>
        Task InsertTransaction(
            StorageConsistencyMode storageConsistencyMode,
            string moderationHandle,
            string appHandle,
            ReportType reportType,
            DateTime requestTime,
            string requestBody,
            string jobId,
            string callbackUrl);

        /// <summary>
        /// Update a CVS transaction with a response from CVS
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="moderationHandle">uniquely identifies moderation request</param>
        /// <param name="responseTime">when the response was received from CVS</param>
        /// <param name="responseBody">body of the response from CVS</param>
        /// <param name="entity">CVS transaction entity to update</param>
        /// <returns>a task that inserts the response into the store</returns>
        Task UpdateTransactionWithResponse(
            StorageConsistencyMode storageConsistencyMode,
            string moderationHandle,
            DateTime responseTime,
            string responseBody,
            ICVSTransactionEntity entity);

        /// <summary>
        /// Delete a CVS transaction
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="moderationHandle">moderation handle</param>
        /// <returns>a task that deletes the CVS transaction</returns>
        Task DeleteTransaction(StorageConsistencyMode storageConsistencyMode, string moderationHandle);

        /// <summary>
        /// Look up a particular CVS transaction
        /// </summary>
        /// <param name="moderationHandle">uniquely identifies moderation request</param>
        /// <returns>a task that returns the CVS transaction</returns>
        Task<ICVSTransactionEntity> QueryTransaction(string moderationHandle);
    }
}
