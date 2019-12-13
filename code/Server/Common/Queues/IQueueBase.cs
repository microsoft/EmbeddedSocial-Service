// <copyright file="IQueueBase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Server.Messages;

    /// <summary>
    /// Queue base interface
    /// </summary>
    public interface IQueueBase
    {
        /// <summary>
        /// Receive message
        /// </summary>
        /// <returns>Queue message</returns>
        Task<IMessage> ReceiveMessage();

        /// <summary>
        /// Receive messages as a batch
        /// </summary>
        /// <param name="count">Count of messages to receive</param>
        /// <returns>List of messages</returns>
        Task<IList<IMessage>> ReceiveMessages(int count);

        /// <summary>
        /// Complete message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Complete message task</returns>
        Task CompleteMessage(IMessage message);

        /// <summary>
        /// Abandon message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Abandon message task</returns>
        Task AbandonMessage(IMessage message);
    }
}
