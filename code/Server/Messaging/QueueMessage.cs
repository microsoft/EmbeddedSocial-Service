// <copyright file="QueueMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messaging
{
    using System;

    /// <summary>
    /// Queue message class
    /// </summary>
    public class QueueMessage : IQueueMessage
    {
        /// <summary>
        /// Gets dequeue count for the message
        /// </summary>
        public int DequeueCount { get; internal set; }

        /// <summary>
        /// Gets enqueued time
        /// </summary>
        public DateTime EnqueuedTime { get; internal set; }

        /// <summary>
        /// Gets sequence number
        /// </summary>
        public long SequenceNumber { get; internal set; }

        /// <summary>
        /// Gets or sets original system message
        /// </summary>
        internal object SystemMessage { get; set; }
    }
}
