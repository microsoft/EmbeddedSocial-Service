// <copyright file="FanoutTopicMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// <c>Fanout</c> topic message interface
    /// </summary>
    public class FanoutTopicMessage : QueueMessage, IFanoutTopicMessage
    {
        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        public string TopicHandle { get; set; }
    }
}
