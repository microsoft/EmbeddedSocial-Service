// <copyright file="RedisConfiguration.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    /// <summary>
    /// <c>Redis</c> configuration
    /// </summary>
    public static class RedisConfiguration
    {
        /// <summary>
        /// Retry attempts for requests
        /// </summary>
        private const int RetryAttempts = 2;

        /// <summary>
        /// Get retry attempts
        /// </summary>
        /// <returns>Retry attempts</returns>
        public static int GetRetryAttempts()
        {
            return RetryAttempts;
        }
    }
}
