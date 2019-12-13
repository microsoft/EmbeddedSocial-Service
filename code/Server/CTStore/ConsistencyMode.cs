//-----------------------------------------------------------------------
// <copyright file="ConsistencyMode.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements enum ConsistencyMode.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Consistency mode
    /// </summary>
    public enum ConsistencyMode
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
