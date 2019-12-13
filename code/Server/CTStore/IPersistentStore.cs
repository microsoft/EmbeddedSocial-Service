//-----------------------------------------------------------------------
// <copyright file="IPersistentStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements interface IPersistentStore.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Persistent Store interface
    /// </summary>
    public interface IPersistentStore
    {
        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True of success</returns>
        Task<bool> CreateContainer(string containerName);

        /// <summary>
        /// Delete container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>True of success</returns>
        Task<bool> DeleteContainer(string containerName);

        /// <summary>
        /// Execute operation async
        /// </summary>
        /// <param name="operation">Table operation</param>
        /// <returns>Operation result</returns>
        Task<Result> ExecuteOperationAsync(Operation operation);

        /// <summary>
        /// Execute transaction async
        /// </summary>
        /// <param name="transaction">Table transaction</param>
        /// <returns>List of operation results</returns>
        Task<IList<Result>> ExecuteTransactionAsync(Transaction transaction);

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
        /// <returns>Count entity for key</returns>
        Task<CountEntity> QueryCountAsync(CountTable table, string partitionKey, string countKey);

        /// <summary>
        /// Query feed item async
        /// </summary>
        /// <typeparam name="T">Feed entity</typeparam>
        /// <param name="table">Feed table</param>
        /// <param name="partitionKey">Partition key for feed</param>
        /// <param name="feedKey">Key for feed</param>
        /// <param name="itemKey">Item key for feed entity</param>
        /// <returns>Feed entity for key</returns>
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
    }
}
