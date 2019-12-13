// <copyright file="TopicNamesManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System.Threading.Tasks;

    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Topic names manager
    /// </summary>
    public class TopicNamesManager : ITopicNamesManager
    {
        /// <summary>
        /// Topics store
        /// </summary>
        private ITopicNamesStore topicNamesStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicNamesManager"/> class
        /// </summary>
        /// <param name="topicNamesStore">Topic names store</param>
        public TopicNamesManager(ITopicNamesStore topicNamesStore)
        {
            this.topicNamesStore = topicNamesStore;
        }

        /// <summary>
        /// Insert a topic name
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Insert topic name task</returns>
        public async Task InsertTopicName(
             ProcessType processType,
             string appHandle,
             string topicName,
             string topicHandle)
        {
            await this.topicNamesStore.InsertTopicName(
                StorageConsistencyMode.Strong,
                appHandle,
                topicName,
                topicHandle);
        }

        /// <summary>
        /// Update a topic name
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <param name="topicNameEntity">Topic name entity</param>
        /// <returns>Update topic name task</returns>
        public async Task UpdateTopicName(
            ProcessType processType,
            string appHandle,
            string topicName,
            ITopicNameEntity topicNameEntity)
        {
            await this.topicNamesStore.UpdateTopicName(
                StorageConsistencyMode.Strong,
                appHandle,
                topicName,
                topicNameEntity);
        }

        /// <summary>
        /// Delete a topic name
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <returns>Delete topic name task</returns>
        public async Task DeleteTopicName(
            ProcessType processType,
            string appHandle,
            string topicName)
        {
            await this.topicNamesStore.DeleteTopicName(
                StorageConsistencyMode.Strong,
                appHandle,
                topicName);
        }

        /// <summary>
        /// Read topic name
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <returns>Topic name entity</returns>
        public async Task<ITopicNameEntity> ReadTopicName(
            string appHandle,
            string topicName)
        {
            return await this.topicNamesStore.QueryTopicName(appHandle, topicName);
        }
    }
}
