// <copyright file="IAVERTStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Interface to the store that contains AVERT transactions
    /// </summary>
    public interface IAVERTStore
    {
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
        Task InsertSubmission(
            StorageConsistencyMode storageConsistencyMode,
            string reportHandle,
            string appHandle,
            ReportType reportType,
            DateTime requestTime,
            string requestBody);

        /// <summary>
        /// Insert a response from AVERT into the store
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="reportHandle">uniquely identifies this report</param>
        /// <param name="responseTime">when the response was received from AVERT</param>
        /// <param name="responseBody">body of the response from AVERT</param>
        /// <param name="entity">AVERT transaction entity to update</param>
        /// <returns>a task that inserts the response into the store</returns>
        Task InsertResponse(
            StorageConsistencyMode storageConsistencyMode,
            string reportHandle,
            DateTime responseTime,
            string responseBody,
            IAVERTTransactionEntity entity);

        /// <summary>
        /// Look up a particular AVERT transaction
        /// </summary>
        /// <param name="reportHandle">uniquely identifies this report</param>
        /// <returns>a task that returns the AVERT transaction</returns>
        Task<IAVERTTransactionEntity> QueryTransaction(string reportHandle);
    }
}