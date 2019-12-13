// <copyright file="IMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;

    /// <summary>
    /// Base message interface
    /// </summary>
    public interface IMessage
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
