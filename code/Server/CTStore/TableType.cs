//-----------------------------------------------------------------------
// <copyright file="TableType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements enum TableType.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Table type
    /// </summary>
    public enum TableType
    {
        /// <summary>
        /// Object table
        /// </summary>
        Object,

        /// <summary>
        /// Fixed object table
        /// </summary>
        FixedObject,

        /// <summary>
        /// Count table
        /// </summary>
        Count,

        /// <summary>
        /// Feed table
        /// </summary>
        Feed,

        /// <summary>
        /// Mutable feed table
        /// </summary>
        MutableFeed,

        /// <summary>
        /// Rank feed table
        /// </summary>
        RankFeed
    }
}
