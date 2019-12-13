// <copyright file="IQueueMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messaging
{
    using System;

    /// <summary>
    /// IQueueMessage interface
    /// </summary>
    /// <remarks>
    /// This interface should match the interface in Common\Messages\IMessage.cs.
    /// However, to avoid circular dependencies, we cannot have an explicit dependency on IMessage.
    /// </remarks>
    public interface IQueueMessage
    {
        /// <summary>
        /// Gets dequeue count for the message
        /// </summary>
        int DequeueCount { get; }

        /// <summary>
        /// Gets enqueued time
        /// </summary>
        DateTime EnqueuedTime { get; }

        /// <summary>
        /// Gets sequence number
        /// </summary>
        long SequenceNumber { get; }
    }
}
