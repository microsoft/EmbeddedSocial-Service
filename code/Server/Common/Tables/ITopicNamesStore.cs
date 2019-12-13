// <copyright file="ITopicNamesStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Topic names store interface
    /// </summary>
    public interface ITopicNamesStore
    {
        /// <summary>
        /// Insert topic name
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Insert topic name task</returns>
        Task InsertTopicName(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicName,
            string topicHandle);

        /// <summary>
        /// Update topic name
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <param name="topicNameEntity">Topic name entity</param>
        /// <returns>Update topic name task</returns>
        Task UpdateTopicName(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicName,
            ITopicNameEntity topicNameEntity);

        /// <summary>
        /// Delete topic name
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <returns>Delete topic name task</returns>
        Task DeleteTopicName(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicName);

        /// <summary>
        /// Query topic name
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <returns>Query topic name task</returns>
        Task<ITopicNameEntity> QueryTopicName(string appHandle, string topicName);
    }
}
