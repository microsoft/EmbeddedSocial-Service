//-----------------------------------------------------------------------
// <copyright file="SpecialFieldNames.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class SpecialFieldNames.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;

    /// <summary>
    /// Defines all the special field names
    /// </summary>
    public static class SpecialFieldNames
    {
        /// <summary>
        /// Partition key field
        /// </summary>
        public const string PartitionKey = "PartitionKey";

        /// <summary>
        /// ETag field
        /// </summary>
        public const string ETag = "ETag";

        /// <summary>
        /// Object key field
        /// </summary>
        public const string ObjectKey = "ObjectKey";

        /// <summary>
        /// Count key field
        /// </summary>
        public const string CountKey = "CountKey";

        /// <summary>
        /// Count field
        /// </summary>
        public const string Count = "Count";

        /// <summary>
        /// Feed key field
        /// </summary>
        public const string FeedKey = "FeedKey";

        /// <summary>
        /// Item key field
        /// </summary>
        public const string ItemKey = "ItemKey";

        /// <summary>
        /// Cursor field
        /// </summary>
        public const string Cursor = "Cursor";

        /// <summary>
        /// Score field
        /// </summary>
        public const string Score = "Score";

        /// <summary>
        /// Cache flags field
        /// </summary>
        public const string CacheFlags = "CacheFlags";

        /// <summary>
        /// Cache expiry field
        /// </summary>
        public const string CacheExpiry = "CacheExpiry";

        /// <summary>
        /// The method returns whether a field name is a special field name
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="entityType">Entity type</param>
        /// <returns>A value indicating whether the property name is a special field name</returns>
        public static bool IsSpecialFieldName(string fieldName, Type entityType)
        {
            if (fieldName.Equals(SpecialFieldNames.PartitionKey)
                || fieldName.Equals(SpecialFieldNames.ETag)
                || fieldName.Equals(SpecialFieldNames.CacheFlags)
                || fieldName.Equals(SpecialFieldNames.CacheExpiry))
            {
                return true;
            }

            if (typeof(ObjectEntity).IsAssignableFrom(entityType))
            {
                if (fieldName.Equals(SpecialFieldNames.ObjectKey))
                {
                    return true;
                }
            }
            else if (typeof(CountEntity).IsAssignableFrom(entityType))
            {
                if (fieldName.Equals(SpecialFieldNames.CountKey))
                {
                    return true;
                }
            }
            else if (typeof(FeedEntity).IsAssignableFrom(entityType))
            {
                if (fieldName.Equals(SpecialFieldNames.FeedKey) || fieldName.Equals(SpecialFieldNames.ItemKey) || fieldName.Equals(SpecialFieldNames.Cursor))
                {
                    return true;
                }
            }
            else if (typeof(RankFeedEntity).IsAssignableFrom(entityType))
            {
                if (fieldName.Equals(SpecialFieldNames.FeedKey) || fieldName.Equals(SpecialFieldNames.Score))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
