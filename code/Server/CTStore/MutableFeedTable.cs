//-----------------------------------------------------------------------
// <copyright file="MutableFeedTable.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class MutableFeedTable.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Mutable feed table class
    /// </summary>
    public class MutableFeedTable : FeedTable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MutableFeedTable"/> class
        /// </summary>
        public MutableFeedTable()
        {
            this.TableType = TableType.MutableFeed;
        }
    }
}
