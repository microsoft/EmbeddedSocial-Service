//-----------------------------------------------------------------------
// <copyright file="RankFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class RankFeedEntity.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Rank feed entity class
    /// </summary>
    public class RankFeedEntity : Entity
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
        /// Gets score for the entity
        /// </summary>
        public double Score { get; internal set; }

        /// <summary>
        /// Gets cursor for the feed entity
        /// Cursor is used for continuations
        /// Use cursor of a particular feed item to retrieve from the next feed item
        /// </summary>
        public string Cursor { get; internal set; }
    }
}
