//-----------------------------------------------------------------------
// <copyright file="StorageMode.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements enum StorageMode.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Storage mode
    /// </summary>
    public enum StorageMode
    {
        /// <summary>
        /// Both persistent store and cache
        /// </summary>
        Default,

        /// <summary>
        /// Cache only
        /// </summary>
        CacheOnly,

        /// <summary>
        /// Persistent store only
        /// </summary>
        PersistentOnly
    }
}
