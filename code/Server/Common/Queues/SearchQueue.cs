// <copyright file="SearchQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Search queue class
    /// </summary>
    public class SearchQueue : QueueBase, ISearchQueue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchQueue"/> class.
        /// </summary>
        /// <param name="queueManager">Queue manager</param>
        public SearchQueue(IQueueManager queueManager)
            : base(queueManager)
        {
            this.QueueIdentifier = QueueIdentifier.Search;
        }

        /// <summary>
        /// Send search index topic message
        /// </summary>
        /// <param name="topicHandle">Topic handle of the topic to index</param>
        /// <param name="timestamp">Timestamp of the update corresponding to this index operation</param>
        /// <returns>Send message task</returns>
        public async Task SendSearchIndexTopicMessage(string topicHandle, DateTime timestamp)
        {
            SearchIndexTopicMessage message = new SearchIndexTopicMessage()
            {
                TopicHandle = topicHandle,
                Timestamp = timestamp
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }

        /// <summary>
        /// Send search remove topic message
        /// </summary>
        /// <param name="topicHandle">Topic handle of the topic to remove</param>
        /// <returns>Send message task</returns>
        public async Task SendSearchRemoveTopicMessage(string topicHandle)
        {
            SearchRemoveTopicMessage message = new SearchRemoveTopicMessage()
            {
                TopicHandle = topicHandle
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }

        /// <summary>
        /// Send search index user message
        /// </summary>
        /// <param name="userHandle">User handle of the user to index</param>
        /// <param name="appHandle">App handle of the user to index</param>
        /// <param name="timestamp">Timestamp of the update corresponding to this index operation</param>
        /// <returns>Send message task</returns>
        public async Task SendSearchIndexUserMessage(string userHandle, string appHandle, DateTime timestamp)
        {
            SearchIndexUserMessage message = new SearchIndexUserMessage()
            {
                UserHandle = userHandle,
                AppHandle = appHandle,
                Timestamp = timestamp
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }

        /// <summary>
        /// Send search remove user message
        /// </summary>
        /// <param name="userHandle">User handle of the user to remove</param>
        /// <param name="appHandle">App handle of the user to remove</param>
        /// <returns>Send message task</returns>
        public async Task SendSearchRemoveUserMessage(string userHandle, string appHandle)
        {
            SearchRemoveUserMessage message = new SearchRemoveUserMessage()
            {
                UserHandle = userHandle,
                AppHandle = appHandle
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }
    }
}
