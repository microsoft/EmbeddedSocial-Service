// <copyright file="AzureStorageConfiguration.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Azure storage configuration
    /// </summary>
    public static class AzureStorageConfiguration
    {
        /// <summary>
        /// Request timeout for table
        /// </summary>
        private const int TableRequestTimeout = 5000;

        /// <summary>
        /// Retry attempts for table
        /// </summary>
        private const int TableRetryAttempts = 2;

        /// <summary>
        /// Retry back-off time for table
        /// </summary>
        private const int TableRetryBackoffTime = 3000;

        /// <summary>
        /// Maximum execution time for table
        /// </summary>
        private const int TableMaximumExecutionTime = 30000;

        /// <summary>
        /// Get table request options
        /// </summary>
        /// <returns>Table request options</returns>
        public static TableRequestOptions GetTableRequestOptions()
        {
            TableRequestOptions tableRequestOptions = new TableRequestOptions();
            tableRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromMilliseconds(TableRetryBackoffTime), TableRetryAttempts);
            tableRequestOptions.ServerTimeout = TimeSpan.FromMilliseconds(TableRequestTimeout);
            tableRequestOptions.MaximumExecutionTime = TimeSpan.FromMilliseconds(TableMaximumExecutionTime);
            return tableRequestOptions;
        }
    }
}
