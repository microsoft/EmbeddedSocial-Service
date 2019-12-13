//-----------------------------------------------------------------------
// <copyright file="Result.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class Result.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Operation result class
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Gets or sets ETag of the entity
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Gets or sets number of entities affected by operation
        /// </summary>
        public int EntitiesAffected { get; set; }

        /// <summary>
        /// Gets or sets return value of an operation
        /// </summary>
        public object Value { get; set; }
    }
}
