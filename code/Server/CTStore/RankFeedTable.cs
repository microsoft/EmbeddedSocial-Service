//-----------------------------------------------------------------------
// <copyright file="RankFeedTable.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class RankFeedTable.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Rank feed table class
    /// </summary>
    public class RankFeedTable : Table
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RankFeedTable"/> class
        /// </summary>
        public RankFeedTable()
        {
            this.TableType = TableType.RankFeed;
        }

        /// <summary>
        /// Gets maximum size for feed table in cache
        /// </summary>
        public int MaxFeedSizeInCache { get; internal set; }

        /// <summary>
        /// Gets order of items in rank feed
        /// </summary>
        public FeedOrder Order { get; internal set; }
    }
}
