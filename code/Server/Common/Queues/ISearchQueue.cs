// <copyright file="ISearchQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Search queue interface
    /// </summary>
    public interface ISearchQueue : IQueueBase
    {
        /// <summary>
        /// Send search index topic message
        /// </summary>
        /// <param name="topicHandle">Topic handle of the topic to index</param>
        /// <param name="timestamp">Timestamp of the update corresponding to this index operation</param>
        /// <returns>Send message task</returns>
        Task SendSearchIndexTopicMessage(string topicHandle, DateTime timestamp);

        /// <summary>
        /// Send search remove topic message
        /// </summary>
        /// <param name="topicHandle">Topic handle of the topic to remove</param>
        /// <returns>Send message task</returns>
        Task SendSearchRemoveTopicMessage(string topicHandle);

        /// <summary>
        /// Send search index user message
        /// </summary>
        /// <param name="userHandle">User handle of the user to index</param>
        /// <param name="appHandle">App handle of the user to index</param>
        /// <param name="timestamp">Timestamp of the update corresponding to this index operation</param>
        /// <returns>Send message task</returns>
        Task SendSearchIndexUserMessage(string userHandle, string appHandle, DateTime timestamp);

        /// <summary>
        /// Send search remove user message
        /// </summary>
        /// <param name="userHandle">User handle of the user to remove</param>
        /// <param name="appHandle">App handle of the user to remove</param>
        /// <returns>Send message task</returns>
        Task SendSearchRemoveUserMessage(string userHandle, string appHandle);
    }
}
