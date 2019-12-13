//-----------------------------------------------------------------------
// <copyright file="Configuration.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class Configuration.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;

    /// <summary>
    /// CTStore configuration
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Default cache invalidation timeout in seconds
        /// </summary>
        private const int DefaultCacheInvalidationTimeoutInSeconds = 120;

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class
        /// </summary>
        public Configuration()
        {
            this.GetTimeMethod = new Func<DateTime>(() => { return DateTime.UtcNow; });
            this.CacheExpiryInSeconds = DefaultCacheInvalidationTimeoutInSeconds;
        }

        /// <summary>
        /// Gets or sets get time method delegate
        /// </summary>
        public Func<DateTime> GetTimeMethod { get; set; }

        /// <summary>
        /// Gets or sets cache expiry timeout in seconds
        /// </summary>
        public int CacheExpiryInSeconds { get; set; }
    }
}
