//-----------------------------------------------------------------------
// <copyright file="CacheFlags.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements enum CacheFlags.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;

    /// <summary>
    /// Cache flags
    /// </summary>
    [Flags]
    public enum CacheFlags
    {
        /// <summary>
        /// No cache flags are set
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Cache entity is invalid
        /// </summary>
        Invalid = 0x01,

        /// <summary>
        /// Entity has no ETag
        /// </summary>
        NoETag = 0x02
    }
}
