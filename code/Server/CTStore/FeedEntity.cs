//-----------------------------------------------------------------------
// <copyright file="FeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class FeedEntity.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Feed entity class
    /// </summary>
    public class FeedEntity : Entity
    {
        /// <summary>
        /// Gets key for the feed
        /// </summary>
        public string FeedKey { get; internal set; }

        /// <summary>
        /// Gets item key for the feed entity
        /// </summary>
        public string ItemKey { get; internal set; }

        /// <summary>
        /// Gets cursor for the feed entity
        /// Cursor is used for continuations
        /// Use cursor of a particular feed item to retrieve from the next feed item
        /// </summary>
        public string Cursor { get; internal set; }
    }
}
