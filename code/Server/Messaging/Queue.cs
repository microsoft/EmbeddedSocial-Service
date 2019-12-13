// <copyright file="Queue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Messaging queue class
    /// </summary>
    public class Queue
    {
        /// <summary>
        /// Service bus queue
        /// </summary>
        private ServiceBusQueue serviceBusQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Queue"/> class
        /// </summary>
        /// <param name="serviceBusQueue">Service bus queue</param>
        public Queue(ServiceBusQueue serviceBusQueue)
        {
            this.serviceBusQueue = serviceBusQueue;
        }

        /// <summary>
        /// Send message async
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <param name="delay">Initial visibility delay</param>
        /// <returns>Send async task</returns>
        public async Task SendAsync(QueueMessage message, TimeSpan? delay = null)
        {
            await this.serviceBusQueue.SendAsync(message, delay);
        }

        /// <summary>
        /// Receive message async
        /// </summary>
        /// <returns>Queue message</returns>
        public async Task<IQueueMessage> ReceiveAsync()
        {
            return await this.serviceBusQueue.ReceiveAsync();
        }

        /// <summary>
        /// Receive messages in a batch
        /// </summary>
        /// <param name="count">Count of messages</param>
        /// <returns>Queue messages</returns>
        public async Task<IList<IQueueMessage>> ReceiveBatchAsync(int count)
        {
            return await this.serviceBusQueue.ReceiveBatchAsync(count);
        }

        /// <summary>
        /// Complete message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Complete async task</returns>
        public async Task CompleteAsync(IQueueMessage message)
        {
            await this.serviceBusQueue.CompleteAsync(message);
        }

        /// <summary>
        /// Abandon message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Abandon async task</returns>
        public async Task AbandonAsync(IQueueMessage message)
        {
            await this.serviceBusQueue.AbandonAsync(message);
        }
    }
}
