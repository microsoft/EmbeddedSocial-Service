// <copyright file="QueueBase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Queue base class
    /// </summary>
    public class QueueBase : IQueueBase
    {
        /// <summary>
        /// Queue manager
        /// </summary>
        private readonly IQueueManager queueManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueBase"/> class
        /// </summary>
        /// <param name="queueManager">queue manager</param>
        public QueueBase(IQueueManager queueManager)
        {
            this.queueManager = queueManager;
        }

        /// <summary>
        /// Gets the private queue manager
        /// </summary>
        protected IQueueManager QueueManager
        {
            get
            {
                return this.queueManager;
            }
        }

        /// <summary>
        /// Gets or sets queue identifier associated with the queue class
        /// </summary>
        protected QueueIdentifier QueueIdentifier { get; set; }

        /// <summary>
        /// Receive message
        /// </summary>
        /// <returns>Queue message</returns>
        public async Task<IMessage> ReceiveMessage()
        {
            Queue queue = await this.queueManager.GetQueue(this.QueueIdentifier);
            var message = await queue.ReceiveAsync();
            return message as IMessage;
        }

        /// <summary>
        /// Receive messages as a batch
        /// </summary>
        /// <param name="count">Count of messages to receive</param>
        /// <returns>List of messages</returns>
        public async Task<IList<IMessage>> ReceiveMessages(int count)
        {
            Queue queue = await this.queueManager.GetQueue(this.QueueIdentifier);
            var messages = await queue.ReceiveBatchAsync(count);
            return messages.Cast<IMessage>().ToList();
        }

        /// <summary>
        /// Complete message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Complete message task</returns>
        public async Task CompleteMessage(IMessage message)
        {
            Queue queue = await this.queueManager.GetQueue(this.QueueIdentifier);
            await queue.CompleteAsync(message as QueueMessage);
        }

        /// <summary>
        /// Abandon message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Abandon message task</returns>
        public async Task AbandonMessage(IMessage message)
        {
            Queue queue = await this.queueManager.GetQueue(this.QueueIdentifier);
            await queue.AbandonAsync(message as QueueMessage);
        }
    }
}
