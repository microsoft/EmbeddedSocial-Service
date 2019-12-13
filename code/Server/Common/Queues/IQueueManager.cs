// <copyright file="IQueueManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System.Threading.Tasks;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// interface of queue manager
    /// </summary>
    public interface IQueueManager
    {
        /// <summary>
        /// Get queue from queue identifier
        /// </summary>
        /// <param name="queueIdentifier">Queue identifier</param>
        /// <returns>Messaging queue</returns>
        Task<Queue> GetQueue(QueueIdentifier queueIdentifier);
    }
}
