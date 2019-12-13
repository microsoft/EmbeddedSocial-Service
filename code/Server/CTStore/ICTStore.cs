//-----------------------------------------------------------------------
// <copyright file="ICTStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements interface ICTStore.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// CT Store interface
    /// </summary>
    public interface ICTStore
    {
        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True of success</returns>
        Task<bool> CreateContainerAsync(string containerName);

        /// <summary>
        /// Delete container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True of success</returns>
        Task<bool> DeleteContainerAsync(string containerName);

        /// <summary>
        /// Execute operation async
        /// </summary>
        /// <param name="operation">Table operation</param>
        /// <param name="consistencyMode">Consistency mode</param>
        /// <returns>Operation result</returns>
        Task<Result> ExecuteOperationAsync(Operation operation, ConsistencyMode consistencyMode);

        /// <summary>
        /// Execute transaction async
        /// </summary>
        /// <param name="transaction">Table transaction</param>
        /// <param name="consistencyMode">Consistency mode</param>
        /// <returns>List of operation results</returns>
        Task<IList<Result>> ExecuteTransactionAsync(Transaction transaction, ConsistencyMode consistencyMode);

        /// <summary>
        /// Query object async
        /// </summary>
        /// <typeparam name="T">Object entity</typeparam>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <returns>Object entity result. Returns null if entity does not exist.</returns>
        Task<T> QueryObjectAsync<T>(ObjectTable table, string partitionKey, string objectKey)
            where T : ObjectEntity, new();

        /// <summary>
        /// Query partial object async
        /// </summary>
        /// <typeparam name="T">Object entity</typeparam>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <returns>Object entity result. Returns null if entity does not exist.</returns>
        Task<T> QueryPartialObjectAsync<T>(ObjectTable table, string partitionKey, string objectKey)
            where T : ObjectEntity, new();

        /// <summary>
        /// Query partial object async
        /// </summary>
        /// <typeparam name="T">Object entity</typeparam>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <param name="fields">Fields to query</param>
        /// <returns>Object entity result. Returns null if entity does not exist.</returns>
        Task<T> QueryPartialObjectAsync<T>(ObjectTable table, string partitionKey, string objectKey, List<string> fields)
            where T : ObjectEntity, new();

        /// <summary>
        /// Query fixed object async
        /// </summary>
        /// <typeparam name="T">Object entity</typeparam>
        /// <param name="table">Object table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="objectKey">Key for entity</param>
        /// <returns>Object entity result. Returns null if entity does not exist.</returns>
        Task<T> QueryObjectAsync<T>(FixedObjectTable table, string partitionKey, string objectKey)
            where T : ObjectEntity, new();

        /// <summary>
        /// Query count async
        /// </summary>
        /// <param name="table">Count table</param>
        /// <param name="partitionKey">Partition key for entity</param>
        /// <param name="countKey">Key for entity</param>
        /// <returns>Count entity</returns>
        Task<CountEntity> QueryCountAsync(CountTable table, string partitionKey, string countKey);

        /// <summary>
        /// Query feed item async
        /// </summary>
        /// <typeparam name="T">Feed entity</typeparam>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key for feed item</param>
        /// <returns>Feed entity in feed</returns>
        Task<T> QueryFeedItemAsync<T>(FeedTable table, string partitionKey, string feedKey, string itemKey)
            where T : FeedEntity, new();

        /// <summary>
        /// Query feed async
        /// </summary>
        /// <typeparam name="T">Feed entity</typeparam>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="cursor">Feed cursor</param>
        /// <param name="limit">Feed count limit</param>
        /// <returns>List of feed entities</returns>
        Task<IList<T>> QueryFeedAsync<T>(
            FeedTable table,
            string partitionKey,
            string feedKey,
            string cursor,
            int limit)
            where T : FeedEntity, new();

        /// <summary>
        /// Query rank feed item async
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key</param>
        /// <returns>List of rank feed entities</returns>
        Task<RankFeedEntity> QueryRankFeedItemAsync(
            RankFeedTable table,
            string partitionKey,
            string feedKey,
            string itemKey);

        /// <summary>
        /// Query rank feed async
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="cursor">Feed cursor</param>
        /// <param name="limit">Feed count limit</param>
        /// <returns>List of rank feed entities</returns>
        Task<IList<RankFeedEntity>> QueryRankFeedAsync(
            RankFeedTable table,
            string partitionKey,
            string feedKey,
            string cursor,
            int limit);

        /// <summary>
        /// Query rank feed in reverse async
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="cursor">Feed cursor</param>
        /// <param name="limit">Feed count limit</param>
        /// <returns>List of rank feed entities</returns>
        Task<IList<RankFeedEntity>> QueryRankFeedReverseAsync(
            RankFeedTable table,
            string partitionKey,
            string feedKey,
            string cursor,
            int limit);

        /// <summary>
        /// Query rank feed by score async
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="startScore">Start score</param>
        /// <param name="endScore">End score</param>
        /// <returns>List of rank feed entities</returns>
        Task<IList<RankFeedEntity>> QueryRankFeedByScoreAsync(
            RankFeedTable table,
            string partitionKey,
            string feedKey,
            double startScore,
            double endScore);

        /// <summary>
        /// Query rank feed length
        /// </summary>
        /// <param name="table">Rank feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <returns>Rank feed length</returns>
        Task<long> QueryRankFeedLengthAsync(
            RankFeedTable table,
            string partitionKey,
            string feedKey);
    }
}
