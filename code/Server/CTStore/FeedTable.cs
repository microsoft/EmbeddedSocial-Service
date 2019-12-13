//-----------------------------------------------------------------------
// <copyright file="FeedTable.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class FeedTable.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Feed table class
    /// </summary>
    public class FeedTable : Table
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedTable"/> class
        /// </summary>
        public FeedTable()
        {
            this.TableType = TableType.Feed;
        }

        /// <summary>
        /// Gets maximum size for feed table in cache
        /// </summary>
        public int MaxFeedSizeInCache { get; internal set; }
    }
}
