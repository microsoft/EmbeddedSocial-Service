//-----------------------------------------------------------------------
// <copyright file="Operation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class Operation.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Operation class
    /// </summary>
    public class Operation
    {
        /// <summary>
        /// Gets or sets table operated on
        /// </summary>
        public Table Table { get; set; }

        /// <summary>
        /// Gets or sets operation type
        /// </summary>
        public OperationType OperationType { get; set; }

        /// <summary>
        /// Gets or sets entity for operation
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// Gets or sets partition key for entity
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets key for the object, count or feed
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets item key for entity in a feed
        /// </summary>
        public string ItemKey { get; set; }

        /// <summary>
        /// Gets or sets value or score for counter based operations
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets start value or score for range operations
        /// </summary>
        public long RangeStart { get; set; }

        /// <summary>
        /// Gets or sets end value or score for range operations
        /// </summary>
        public long RangeEnd { get; set; }

        /// <summary>
        /// Insert operation on object table
        /// </summary>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <param name="entity">Object entity</param>
        /// <returns>Table operation</returns>
        public static Operation Insert(ObjectTable table, string partitionKey, string objectKey, ObjectEntity entity)
        {
            ValidateObjectTableParameters(table, partitionKey, objectKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Insert,
                PartitionKey = partitionKey,
                Key = objectKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Delete operation on object table
        /// </summary>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <param name="entity">Object entity</param>
        /// <returns>Table operation</returns>
        public static Operation Delete(ObjectTable table, string partitionKey, string objectKey, ObjectEntity entity = null)
        {
            ValidateObjectTableParameters(table, partitionKey, objectKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Delete,
                PartitionKey = partitionKey,
                Key = objectKey,
                Entity = entity != null ? entity.Clone() : null
            };
        }

        /// <summary>
        /// Delete if exists operation on object table
        /// </summary>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <returns>Table operation</returns>
        public static Operation DeleteIfExists(ObjectTable table, string partitionKey, string objectKey)
        {
            ValidateObjectTableParameters(table, partitionKey, objectKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.DeleteIfExists,
                PartitionKey = partitionKey,
                Key = objectKey
            };
        }

        /// <summary>
        /// Replace operation on object table
        /// </summary>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <param name="entity">Object entity</param>
        /// <returns>Table operation</returns>
        public static Operation Replace(ObjectTable table, string partitionKey, string objectKey, ObjectEntity entity)
        {
            ValidateObjectTableParameters(table, partitionKey, objectKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Replace,
                PartitionKey = partitionKey,
                Key = objectKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Insert or replace operation on object table
        /// </summary>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <param name="entity">Object entity</param>
        /// <returns>Table operation</returns>
        public static Operation InsertOrReplace(ObjectTable table, string partitionKey, string objectKey, ObjectEntity entity)
        {
            ValidateObjectTableParameters(table, partitionKey, objectKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.InsertOrReplace,
                PartitionKey = partitionKey,
                Key = objectKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Merge operation on object table
        /// </summary>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <param name="entity">Object entity</param>
        /// <returns>Table operation</returns>
        public static Operation Merge(ObjectTable table, string partitionKey, string objectKey, ObjectEntity entity)
        {
            ValidateObjectTableParameters(table, partitionKey, objectKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Merge,
                PartitionKey = partitionKey,
                Key = objectKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Insert or merge operation on object table
        /// </summary>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <param name="entity">Object entity</param>
        /// <returns>Table operation</returns>
        public static Operation InsertOrMerge(ObjectTable table, string partitionKey, string objectKey, ObjectEntity entity)
        {
            ValidateObjectTableParameters(table, partitionKey, objectKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.InsertOrMerge,
                PartitionKey = partitionKey,
                Key = objectKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Insert operation on fixed object table
        /// </summary>
        /// <param name="table">Fixed object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <param name="entity">Object entity</param>
        /// <returns>Table operation</returns>
        public static Operation Insert(FixedObjectTable table, string partitionKey, string objectKey, ObjectEntity entity)
        {
            ValidateObjectTableParameters(table, partitionKey, objectKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Insert,
                PartitionKey = partitionKey,
                Key = objectKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Delete operation on fixed object table
        /// </summary>
        /// <param name="table">Fixed object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <param name="entity">Object entity</param>
        /// <returns>Table operation</returns>
        public static Operation Delete(FixedObjectTable table, string partitionKey, string objectKey, ObjectEntity entity = null)
        {
            ValidateObjectTableParameters(table, partitionKey, objectKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Delete,
                PartitionKey = partitionKey,
                Key = objectKey,
                Entity = entity != null ? entity.Clone() : null
            };
        }

        /// <summary>
        /// Delete if exists operation on fixed object table
        /// </summary>
        /// <param name="table">Fixed object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <returns>Table operation</returns>
        public static Operation DeleteIfExists(FixedObjectTable table, string partitionKey, string objectKey)
        {
            ValidateObjectTableParameters(table, partitionKey, objectKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.DeleteIfExists,
                PartitionKey = partitionKey,
                Key = objectKey
            };
        }

        /// <summary>
        /// Replace operation on fixed object table
        /// </summary>
        /// <param name="table">Fixed object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <param name="entity">Object entity</param>
        /// <returns>Table operation</returns>
        public static Operation Replace(FixedObjectTable table, string partitionKey, string objectKey, ObjectEntity entity)
        {
            ValidateObjectTableParameters(table, partitionKey, objectKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Replace,
                PartitionKey = partitionKey,
                Key = objectKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Insert or replace operation on fixed object table
        /// </summary>
        /// <param name="table">Fixed object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <param name="entity">Object entity</param>
        /// <returns>Table operation</returns>
        public static Operation InsertOrReplace(FixedObjectTable table, string partitionKey, string objectKey, ObjectEntity entity)
        {
            ValidateObjectTableParameters(table, partitionKey, objectKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.InsertOrReplace,
                PartitionKey = partitionKey,
                Key = objectKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Insert operation on feed table
        /// </summary>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key for feed entity</param>
        /// <param name="entity">Feed entity</param>
        /// <returns>Table operation</returns>
        public static Operation Insert(FeedTable table, string partitionKey, string feedKey, string itemKey, FeedEntity entity)
        {
            ValidateFeedTableParameters(table, partitionKey, feedKey, itemKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Insert,
                PartitionKey = partitionKey,
                Key = feedKey,
                ItemKey = itemKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Delete operation on feed table
        /// </summary>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key for feed entity</param>
        /// <param name="entity">Feed entity</param>
        /// <returns>Table operation</returns>
        public static Operation Delete(FeedTable table, string partitionKey, string feedKey, string itemKey, FeedEntity entity = null)
        {
            ValidateFeedTableParameters(table, partitionKey, feedKey, itemKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Delete,
                PartitionKey = partitionKey,
                Key = feedKey,
                ItemKey = itemKey,
                Entity = entity != null ? entity.Clone() : null
            };
        }

        /// <summary>
        /// Delete if exists operation on feed table
        /// </summary>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key for feed entity</param>
        /// <returns>Table operation</returns>
        public static Operation DeleteIfExists(FeedTable table, string partitionKey, string feedKey, string itemKey)
        {
            ValidateFeedTableParameters(table, partitionKey, feedKey, itemKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.DeleteIfExists,
                PartitionKey = partitionKey,
                Key = feedKey,
                ItemKey = itemKey
            };
        }

        /// <summary>
        /// Replace operation on feed table
        /// </summary>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key for feed entity</param>
        /// <param name="entity">Feed entity</param>
        /// <returns>Table operation</returns>
        public static Operation Replace(FeedTable table, string partitionKey, string feedKey, string itemKey, FeedEntity entity)
        {
            ValidateFeedTableParameters(table, partitionKey, feedKey, itemKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Replace,
                PartitionKey = partitionKey,
                Key = feedKey,
                ItemKey = itemKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Insert or replace operation on feed table
        /// </summary>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key for feed entity</param>
        /// <param name="entity">Feed entity</param>
        /// <returns>Table operation</returns>
        public static Operation InsertOrReplace(FeedTable table, string partitionKey, string feedKey, string itemKey, FeedEntity entity)
        {
            ValidateFeedTableParameters(table, partitionKey, feedKey, itemKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.InsertOrReplace,
                PartitionKey = partitionKey,
                Key = feedKey,
                ItemKey = itemKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Insert operation on count table
        /// </summary>
        /// <param name="table">Count table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="countKey">Key for entity</param>
        /// <param name="value">Initial value</param>
        /// <returns>Table operation</returns>
        public static Operation Insert(CountTable table, string partitionKey, string countKey, double value)
        {
            ValidateCountTableParameters(table, partitionKey, countKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Insert,
                PartitionKey = partitionKey,
                Key = countKey,
                Score = value,
                Entity = new CountEntity()
                {
                    Count = value
                }
            };
        }

        /// <summary>
        /// Insert or replace operation on count table
        /// </summary>
        /// <param name="table">Count table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="countKey">Key for entity</param>
        /// <param name="value">Initial value</param>
        /// <returns>Table operation</returns>
        public static Operation InsertOrReplace(CountTable table, string partitionKey, string countKey, double value)
        {
            ValidateCountTableParameters(table, partitionKey, countKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.InsertOrReplace,
                PartitionKey = partitionKey,
                Key = countKey,
                Score = value,
                Entity = new CountEntity()
                {
                    Count = value
                }
            };
        }

        /// <summary>
        /// Delete operation on count table
        /// </summary>
        /// <param name="table">Count table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="countKey">Key for entity</param>
        /// <param name="entity">Count entity</param>
        /// <returns>Table operation</returns>
        public static Operation Delete(CountTable table, string partitionKey, string countKey, CountEntity entity = null)
        {
            ValidateCountTableParameters(table, partitionKey, countKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Delete,
                PartitionKey = partitionKey,
                Key = countKey,
                Entity = entity != null ? entity.Clone() : null
            };
        }

        /// <summary>
        /// Delete operation on count table
        /// </summary>
        /// <param name="table">Count table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="countKey">Key for entity</param>
        /// <returns>Table operation</returns>
        public static Operation DeleteIfExists(CountTable table, string partitionKey, string countKey)
        {
            ValidateCountTableParameters(table, partitionKey, countKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.DeleteIfExists,
                PartitionKey = partitionKey,
                Key = countKey
            };
        }

        /// <summary>
        /// Increment operation on count table
        /// </summary>
        /// <param name="table">Count table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="countKey">Key for entity</param>
        /// <param name="value">Increment value</param>
        /// <returns>Table operation</returns>
        public static Operation Increment(CountTable table, string partitionKey, string countKey, double value = 1.0)
        {
            ValidateCountTableParameters(table, partitionKey, countKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Increment,
                PartitionKey = partitionKey,
                Key = countKey,
                Score = value
            };
        }

        /// <summary>
        /// Increment operation on count table
        /// </summary>
        /// <param name="table">Count table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="countKey">Key for entity</param>
        /// <param name="value">Increment value</param>
        /// <returns>Table operation</returns>
        public static Operation InsertOrIncrement(CountTable table, string partitionKey, string countKey, double value = 1.0)
        {
            ValidateCountTableParameters(table, partitionKey, countKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.InsertOrIncrement,
                PartitionKey = partitionKey,
                Key = countKey,
                Score = value,
                Entity = new CountEntity()
                {
                    Count = value
                }
            };
        }

        /// <summary>
        /// Replace operation on count table
        /// </summary>
        /// <param name="table">Count table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="countKey">Key for entity</param>
        /// <param name="entity">Count entity</param>
        /// <returns>Table operation</returns>
        public static Operation Replace(CountTable table, string partitionKey, string countKey, CountEntity entity)
        {
            ValidateCountTableParameters(table, partitionKey, countKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Replace,
                PartitionKey = partitionKey,
                Key = countKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Insert operation on rank feed table
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key</param>
        /// <param name="score">Initial score</param>
        /// <returns>Table operation</returns>
        public static Operation Insert(RankFeedTable table, string partitionKey, string feedKey, string itemKey, double score)
        {
            ValidateRankFeedTableParameters(table, partitionKey, feedKey, itemKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Insert,
                PartitionKey = partitionKey,
                Key = feedKey,
                ItemKey = itemKey,
                Score = score
            };
        }

        /// <summary>
        /// Insert or replace operation on rank feed table
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key</param>
        /// <param name="score">Initial score</param>
        /// <returns>Table operation</returns>
        public static Operation InsertOrReplace(RankFeedTable table, string partitionKey, string feedKey, string itemKey, double score)
        {
            ValidateRankFeedTableParameters(table, partitionKey, feedKey, itemKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.InsertOrReplace,
                PartitionKey = partitionKey,
                Key = feedKey,
                ItemKey = itemKey,
                Score = score
            };
        }

        /// <summary>
        /// Delete operation on rank feed table
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key</param>
        /// <returns>Table operation</returns>
        public static Operation Delete(RankFeedTable table, string partitionKey, string feedKey, string itemKey)
        {
            ValidateRankFeedTableParameters(table, partitionKey, feedKey, itemKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Delete,
                PartitionKey = partitionKey,
                Key = feedKey,
                ItemKey = itemKey
            };
        }

        /// <summary>
        /// Delete if exists operation on rank feed table
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key</param>
        /// <returns>Table operation</returns>
        public static Operation DeleteIfExists(RankFeedTable table, string partitionKey, string feedKey, string itemKey)
        {
            ValidateRankFeedTableParameters(table, partitionKey, feedKey, itemKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.DeleteIfExists,
                PartitionKey = partitionKey,
                Key = feedKey,
                ItemKey = itemKey
            };
        }

        /// <summary>
        /// Increment operation on rank feed table
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Key for entity</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key</param>
        /// <param name="score">Increment value</param>
        /// <returns>Table operation</returns>
        public static Operation Increment(RankFeedTable table, string partitionKey, string feedKey, string itemKey, double score = 1)
        {
            ValidateRankFeedTableParameters(table, partitionKey, feedKey, itemKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.Increment,
                PartitionKey = partitionKey,
                Key = feedKey,
                ItemKey = itemKey,
                Score = score
            };
        }

        /// <summary>
        /// Insert or Increment operation on rank feed table
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Key for entity</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key</param>
        /// <param name="score">Increment value</param>
        /// <returns>Table operation</returns>
        public static Operation InsertOrIncrement(RankFeedTable table, string partitionKey, string feedKey, string itemKey, double score = 1)
        {
            ValidateRankFeedTableParameters(table, partitionKey, feedKey, itemKey);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.InsertOrIncrement,
                PartitionKey = partitionKey,
                Key = feedKey,
                ItemKey = itemKey,
                Score = score
            };
        }

        /// <summary>
        /// Insert or replace if not last operation on feed table
        /// </summary>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key for feed entity</param>
        /// <param name="entity">Feed entity</param>
        /// <returns>Table operation</returns>
        internal static Operation InsertOrReplaceIfNotLast(FeedTable table, string partitionKey, string feedKey, string itemKey, FeedEntity entity)
        {
            ValidateFeedTableParameters(table, partitionKey, feedKey, itemKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.InsertOrReplaceIfNotLast,
                PartitionKey = partitionKey,
                Key = feedKey,
                ItemKey = itemKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Insert if not empty operation on feed table
        /// </summary>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key for feed entity</param>
        /// <param name="entity">Feed entity</param>
        /// <returns>Table operation</returns>
        internal static Operation InsertIfNotEmpty(FeedTable table, string partitionKey, string feedKey, string itemKey, FeedEntity entity)
        {
            ValidateFeedTableParameters(table, partitionKey, feedKey, itemKey, entity);
            return new Operation()
            {
                Table = table,
                OperationType = OperationType.InsertIfNotEmpty,
                PartitionKey = partitionKey,
                Key = feedKey,
                ItemKey = itemKey,
                Entity = entity.Clone()
            };
        }

        /// <summary>
        /// Validate object table parameters and throw exceptions
        /// </summary>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="objectKey">Object key</param>
        /// <param name="entity">Object entity</param>
        private static void ValidateObjectTableParameters(Table table, string partitionKey, string objectKey, ObjectEntity entity)
        {
            ValidateObjectTableParameters(table, partitionKey, objectKey);
            if (entity == null)
            {
                throw new ArgumentNullException("Entity cannot be null");
            }
        }

        /// <summary>
        /// Validate object table parameters and throw exceptions
        /// </summary>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="objectKey">Object key</param>
        private static void ValidateObjectTableParameters(Table table, string partitionKey, string objectKey)
        {
            if (table == null)
            {
                throw new ArgumentNullException("Table cannot be null");
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentNullException("Partition key cannot be null or empty");
            }

            if (string.IsNullOrEmpty(objectKey))
            {
                throw new ArgumentNullException("Object key cannot be null or empty");
            }
        }

        /// <summary>
        /// Validate feed table parameters and throw exceptions
        /// </summary>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="feedKey">Feed key</param>
        /// <param name="itemKey">Item key</param>
        /// <param name="entity">Object entity</param>
        private static void ValidateFeedTableParameters(Table table, string partitionKey, string feedKey, string itemKey, FeedEntity entity)
        {
            ValidateFeedTableParameters(table, partitionKey, feedKey, itemKey);
            if (entity == null)
            {
                throw new ArgumentNullException("Entity cannot be null");
            }
        }

        /// <summary>
        /// Validate feed table parameters and throw exceptions
        /// </summary>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="feedKey">Feed key</param>
        /// <param name="itemKey">Item key</param>
        private static void ValidateFeedTableParameters(Table table, string partitionKey, string feedKey, string itemKey)
        {
            if (table == null)
            {
                throw new ArgumentNullException("Table cannot be null");
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentNullException("Partition key cannot be null or empty");
            }

            if (string.IsNullOrEmpty(feedKey))
            {
                throw new ArgumentNullException("Feed key cannot be null or empty");
            }

            if (string.IsNullOrEmpty(itemKey))
            {
                throw new ArgumentNullException("Item key cannot be null or empty");
            }
        }

        /// <summary>
        /// Validate count table parameters and throw exceptions
        /// </summary>
        /// <param name="table">Count table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="countKey">Count key</param>
        /// <param name="entity">Count entity</param>
        private static void ValidateCountTableParameters(Table table, string partitionKey, string countKey, CountEntity entity)
        {
            ValidateCountTableParameters(table, partitionKey, countKey);
            if (entity == null)
            {
                throw new ArgumentNullException("Entity cannot be null");
            }
        }

        /// <summary>
        /// Validate count table parameters and throw exceptions
        /// </summary>
        /// <param name="table">Count table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="countKey">Count key</param>
        private static void ValidateCountTableParameters(Table table, string partitionKey, string countKey)
        {
            if (table == null)
            {
                throw new ArgumentNullException("Table cannot be null");
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentNullException("Partition key cannot be null or empty");
            }

            if (string.IsNullOrEmpty(countKey))
            {
                throw new ArgumentNullException("Count key cannot be null or empty");
            }
        }

        /// <summary>
        /// Validate rank feed table parameters and throw exceptions
        /// </summary>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="feedKey">Feed key</param>
        /// <param name="itemKey">Item key</param>
        private static void ValidateRankFeedTableParameters(Table table, string partitionKey, string feedKey, string itemKey)
        {
            if (table == null)
            {
                throw new ArgumentNullException("Table cannot be null");
            }

            if (table.StorageMode != StorageMode.CacheOnly)
            {
                throw new NotSupportedException("Rank feed tables are supported only in cache-only mode");
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentNullException("Partition key cannot be null or empty");
            }

            if (string.IsNullOrEmpty(feedKey))
            {
                throw new ArgumentNullException("Feed key cannot be null or empty");
            }

            if (string.IsNullOrEmpty(itemKey))
            {
                throw new ArgumentNullException("Item key cannot be null or empty");
            }
        }
    }
}
