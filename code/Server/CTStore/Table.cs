//-----------------------------------------------------------------------
// <copyright file="Table.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class Table.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;

    /// <summary>
    /// Table class
    /// </summary>
    public class Table
    {
        /// <summary>
        /// Gets table container name
        /// </summary>
        public string ContainerName { get; internal set; }

        /// <summary>
        /// Gets table container initial
        /// </summary>
        public string ContainerInitial { get; internal set; }

        /// <summary>
        /// Gets table name
        /// </summary>
        public string TableName { get; internal set; }

        /// <summary>
        /// Gets table initial
        /// </summary>
        public string TableInitial { get; internal set; }

        /// <summary>
        /// Gets storage mode
        /// </summary>
        public StorageMode StorageMode { get; internal set; }

        /// <summary>
        /// Gets table type
        /// </summary>
        public TableType TableType { get; internal set; }

        /// <summary>
        /// Get object table
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="containerInitial">Container initial</param>
        /// <param name="tableName">Table name</param>
        /// <param name="tableInitial">Table initial</param>
        /// <param name="storageMode">Storage mode</param>
        /// <returns>Object table</returns>
        public static ObjectTable GetObjectTable(
            string containerName,
            string containerInitial,
            string tableName,
            string tableInitial,
            StorageMode storageMode = StorageMode.Default)
        {
            ValidateTableParameters(containerName, containerInitial, tableName, tableInitial);
            return new ObjectTable()
            {
                ContainerName = containerName,
                ContainerInitial = containerInitial,
                TableName = tableName,
                TableInitial = tableInitial,
                StorageMode = storageMode
            };
        }

        /// <summary>
        /// Get fixed object table
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="containerInitial">Container initial</param>
        /// <param name="tableName">Table name</param>
        /// <param name="tableInitial">Table initial</param>
        /// <param name="storageMode">Storage mode</param>
        /// <returns>Fixed object table</returns>
        public static FixedObjectTable GetFixedObjectTable(
            string containerName,
            string containerInitial,
            string tableName,
            string tableInitial,
            StorageMode storageMode = StorageMode.Default)
        {
            ValidateTableParameters(containerName, containerInitial, tableName, tableInitial);
            return new FixedObjectTable()
            {
                ContainerName = containerName,
                ContainerInitial = containerInitial,
                TableName = tableName,
                TableInitial = tableInitial,
                StorageMode = storageMode
            };
        }

        /// <summary>
        /// Get count table
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="containerInitial">Container initial</param>
        /// <param name="tableName">Table name</param>
        /// <param name="tableInitial">Table initial</param>
        /// <param name="storageMode">Storage mode</param>
        /// <returns>Count table</returns>
        public static CountTable GetCountTable(
            string containerName,
            string containerInitial,
            string tableName,
            string tableInitial,
            StorageMode storageMode = StorageMode.Default)
        {
            ValidateTableParameters(containerName, containerInitial, tableName, tableInitial);
            return new CountTable()
            {
                ContainerName = containerName,
                ContainerInitial = containerInitial,
                TableName = tableName,
                TableInitial = tableInitial,
                StorageMode = storageMode
            };
        }

        /// <summary>
        /// Get feed table
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="containerInitial">Container initial</param>
        /// <param name="tableName">Table name</param>
        /// <param name="tableInitial">Table initial</param>
        /// <param name="storageMode">Storage mode</param>
        /// <param name="maxFeedSizeInCache">Max feed size in cache</param>
        /// <returns>Feed table</returns>
        public static FeedTable GetFeedTable(
            string containerName,
            string containerInitial,
            string tableName,
            string tableInitial,
            StorageMode storageMode = StorageMode.Default,
            int maxFeedSizeInCache = int.MaxValue)
        {
            ValidateTableParameters(containerName, containerInitial, tableName, tableInitial);
            return new FeedTable()
            {
                ContainerName = containerName,
                ContainerInitial = containerInitial,
                TableName = tableName,
                TableInitial = tableInitial,
                StorageMode = storageMode,
                MaxFeedSizeInCache = maxFeedSizeInCache
            };
        }

        /// <summary>
        /// Get rank feed table
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="containerInitial">Container initial</param>
        /// <param name="tableName">Table name</param>
        /// <param name="tableInitial">Table initial</param>
        /// <param name="storageMode">Storage mode</param>
        /// <param name="maxFeedSizeInCache">Max feed size in cache</param>
        /// <param name="order">Order of items</param>
        /// <returns>Rank feed table</returns>
        public static RankFeedTable GetRankFeedTable(
            string containerName,
            string containerInitial,
            string tableName,
            string tableInitial,
            StorageMode storageMode = StorageMode.Default,
            int maxFeedSizeInCache = int.MaxValue,
            FeedOrder order = FeedOrder.Ascending)
        {
            ValidateTableParameters(containerName, containerInitial, tableName, tableInitial);
            if (storageMode != StorageMode.CacheOnly)
            {
                throw new NotSupportedException("Rank feed tables are supported only in cache-only mode");
            }

            return new RankFeedTable()
            {
                ContainerName = containerName,
                ContainerInitial = containerInitial,
                TableName = tableName,
                TableInitial = tableInitial,
                StorageMode = storageMode,
                MaxFeedSizeInCache = maxFeedSizeInCache,
                Order = order
            };
        }

        /// <summary>
        /// Validate table parameters
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="containerInitial">Container initial</param>
        /// <param name="tableName">Table name</param>
        /// <param name="tableInitial">Table initial</param>
        private static void ValidateTableParameters(
            string containerName,
            string containerInitial,
            string tableName,
            string tableInitial)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentNullException("Container name cannot be null or empty");
            }

            if (string.IsNullOrEmpty(containerInitial))
            {
                throw new ArgumentNullException("Container initial cannot be null or empty");
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("Table name cannot be null or empty");
            }

            if (string.IsNullOrEmpty(tableInitial))
            {
                throw new ArgumentNullException("Table initial cannot be null or empty");
            }
        }
    }
}
