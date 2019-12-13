// <copyright file="AzureStorageConfiguration.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Blobs
{
    using System;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;

    /// <summary>
    /// Azure storage configuration
    /// </summary>
    public static class AzureStorageConfiguration
    {
        /// <summary>
        /// Request timeout for blobs in milliseconds
        /// </summary>
        private const int BlobRequestTimeout = 5000;

        /// <summary>
        /// Retry attempts for blobs
        /// </summary>
        private const int BlobRetryAttempts = 2;

        /// <summary>
        /// Retry back-off time for blobs in milliseconds
        /// </summary>
        private const int BlobRetryBackoffTime = 3000;

        /// <summary>
        /// Maximum execution time for blobs in milliseconds
        /// </summary>
        private const int BlobMaximumExecutionTime = 30000;

        /// <summary>
        /// Get blob request options
        /// </summary>
        /// <returns>Blob request options</returns>
        public static BlobRequestOptions GetBlobRequestOptions()
        {
            BlobRequestOptions blobRequestOptions = new BlobRequestOptions();
            blobRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromMilliseconds(BlobRetryBackoffTime), BlobRetryAttempts);
            blobRequestOptions.ServerTimeout = TimeSpan.FromMilliseconds(BlobRequestTimeout);
            blobRequestOptions.MaximumExecutionTime = TimeSpan.FromMilliseconds(BlobMaximumExecutionTime);
            return blobRequestOptions;
        }
    }
}
