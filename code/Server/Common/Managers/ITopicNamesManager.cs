// <copyright file="ITopicNamesManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System.Threading.Tasks;

    using SocialPlus.Server.Entities;

    /// <summary>
    /// Topic names manager interface
    /// </summary>
    public interface ITopicNamesManager
    {
        /// <summary>
        /// Insert a topic name
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Insert topic name task</returns>
        Task InsertTopicName(
            ProcessType processType,
            string appHandle,
            string topicName,
            string topicHandle);

        /// <summary>
        /// Update a topic name
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <param name="topicNameEntity">Topic name entity</param>
        /// <returns>Update topic name task</returns>
        Task UpdateTopicName(
            ProcessType processType,
            string appHandle,
            string topicName,
            ITopicNameEntity topicNameEntity);

        /// <summary>
        /// Delete a topic name
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <returns>Delete topic name task</returns>
        Task DeleteTopicName(
            ProcessType processType,
            string appHandle,
            string topicName);

        /// <summary>
        /// Read topic name
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicName">Topic name</param>
        /// <returns>Topic name entity</returns>
        Task<ITopicNameEntity> ReadTopicName(
            string appHandle,
            string topicName);
    }
}
