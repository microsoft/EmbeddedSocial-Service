// <copyright file="TopicNamesStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// Default topic names table store implementation that talks to <c>CTStore</c>
    /// </summary>
    public class TopicNamesStore : ITopicNamesStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicNamesStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public TopicNamesStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Insert topic name
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Insert topic name task</returns>
        public async Task InsertTopicName(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicName,
            string topicHandle)
        {
            TopicNameEntity topicNameEntity = new TopicNameEntity()
            {
                PublisherType = PublisherType.App,
                TopicName = topicName,
                AppHandle = appHandle,
                TopicHandle = topicHandle,
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicNames);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.TopicNames, TableIdentifier.TopicNamesObject) as ObjectTable;
            string objectKey = this.GetTopicNameObjectKey(appHandle, topicName);
            Operation operation = Operation.Insert(table, objectKey, objectKey, topicNameEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Update topic name
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <param name="topicNameEntity">Topic name entity</param>
        /// <returns>Update topic name task</returns>
        public async Task UpdateTopicName(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicName,
            ITopicNameEntity topicNameEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicNames);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.TopicNames, TableIdentifier.TopicNamesObject) as ObjectTable;
            string objectKey = this.GetTopicNameObjectKey(appHandle, topicName);
            Operation operation = Operation.Replace(table, objectKey, objectKey, topicNameEntity as TopicNameEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete topic name
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <returns>Delete topic name task</returns>
        public async Task DeleteTopicName(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicName)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicNames);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.TopicNames, TableIdentifier.TopicNamesObject) as ObjectTable;
            string objectKey = this.GetTopicNameObjectKey(appHandle, topicName);
            Operation operation = Operation.Delete(table, objectKey, objectKey);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query topic name
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <returns>Query topic name task</returns>
        public async Task<ITopicNameEntity> QueryTopicName(string appHandle, string topicName)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicNames);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.TopicNames, TableIdentifier.TopicNamesObject) as ObjectTable;
            string objectKey = this.GetTopicNameObjectKey(appHandle, topicName);
            TopicNameEntity topicNameEntity = await store.QueryObjectAsync<TopicNameEntity>(table, objectKey, objectKey);
            return topicNameEntity;
        }

        /// <summary>
        /// Constructs the object key for a topic name
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <returns>Get topic name key task</returns>
        private string GetTopicNameObjectKey(string appHandle, string topicName)
        {
            return string.Join("+", appHandle, topicName);
        }
    }
}
