//-----------------------------------------------------------------------
// <copyright file="CountEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class CountEntity.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Count entity class
    /// </summary>
    public class CountEntity : Entity
    {
        /// <summary>
        /// Gets key for the entity
        /// </summary>
        public string CountKey { get; internal set; }

        /// <summary>
        /// Gets or sets count for the entity
        /// </summary>
        public double Count { get; set; }
    }
}
