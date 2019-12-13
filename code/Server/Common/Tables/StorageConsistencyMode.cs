// <copyright file="StorageConsistencyMode.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    /// <summary>
    /// Storage consistency mode
    /// </summary>
    public enum StorageConsistencyMode
    {
        /// <summary>
        /// Strong consistency mode keeps the persistent store and cache consistent. It is not optimized for performance.
        /// </summary>
        Strong,

        /// <summary>
        /// Eventual consistency mode updates the cache first. It assumes that operations are retried after a problem.
        /// </summary>
        Eventual,

        /// <summary>
        /// Express mode is optimized for performance. The system has to run the operations in eventual mode at a later point to make the persistent store consistent with the cache.
        /// </summary>
        Express
    }
}
