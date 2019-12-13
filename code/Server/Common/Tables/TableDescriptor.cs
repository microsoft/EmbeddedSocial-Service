// <copyright file="TableDescriptor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using Microsoft.CTStore;

    /// <summary>
    /// Table descriptor class
    /// </summary>
    public class TableDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableDescriptor" /> class
        /// </summary>
        public TableDescriptor()
        {
            this.MaxFeedSizeInCache = int.MaxValue;
            this.QueryFeedInPersistentStore = true;
        }

        /// <summary>
        /// Gets or sets table type
        /// </summary>
        public TableType TableType { get; set; }

        /// <summary>
        /// Gets or sets table name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets table initial
        /// </summary>
        public string TableInitial { get; set; }

        /// <summary>
        /// Gets or sets storage mode
        /// </summary>
        public StorageMode StorageMode { get; set; }

        /// <summary>
        /// Gets or sets maximum size for feed table in cache
        /// </summary>
        public int MaxFeedSizeInCache { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the feed query should continue to persistent store
        /// </summary>
        public bool QueryFeedInPersistentStore { get; set; }
    }
}
