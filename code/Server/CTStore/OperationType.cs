//-----------------------------------------------------------------------
// <copyright file="OperationType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements enum OperationType.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Operation type
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// Insert operation
        /// </summary>
        Insert,

        /// <summary>
        /// Delete operation
        /// </summary>
        Delete,

        /// <summary>
        /// Delete if exists operation
        /// </summary>
        DeleteIfExists,

        /// <summary>
        /// Replace operation
        /// </summary>
        Replace,

        /// <summary>
        /// Insert or replace operation
        /// </summary>
        InsertOrReplace,

        /// <summary>
        /// Merge operation
        /// </summary>
        Merge,

        /// <summary>
        /// Insert or merge operation
        /// </summary>
        InsertOrMerge,

        /// <summary>
        /// Increment operation
        /// </summary>
        Increment,

        /// <summary>
        /// Insert or increment operation
        /// </summary>
        InsertOrIncrement,

        /// <summary>
        /// Insert or replace if not last operation
        /// Used only in feeds
        /// </summary>
        InsertOrReplaceIfNotLast,

        /// <summary>
        /// Insert if not first operation
        /// Used only in feeds
        /// </summary>
        InsertIfNotEmpty
    }
}
