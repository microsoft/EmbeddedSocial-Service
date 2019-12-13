// <copyright file="FanoutTopicsQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System.Threading.Tasks;

    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// <c>Fanout</c> topics queue class
    /// </summary>
    public class FanoutTopicsQueue : QueueBase, IFanoutTopicsQueue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FanoutTopicsQueue"/> class
        /// </summary>
        /// <param name="queueManager">queue manager</param>
        public FanoutTopicsQueue(IQueueManager queueManager)
            : base(queueManager)
        {
            this.QueueIdentifier = QueueIdentifier.FanoutTopics;
        }

        /// <summary>
        /// Send <c>fanout</c> topic message
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Send message task</returns>
        public async Task SendFanoutTopicMessage(string userHandle, string appHandle, string topicHandle)
        {
            FanoutTopicMessage message = new FanoutTopicMessage()
            {
                UserHandle = userHandle,
                AppHandle = appHandle,
                TopicHandle = topicHandle
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }
    }
}
