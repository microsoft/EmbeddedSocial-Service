// <copyright file="IFanoutTopicMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    /// <summary>
    /// <c>Fanout</c> topic message interface
    /// </summary>
    public interface IFanoutTopicMessage : IMessage
    {
        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        string TopicHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        string AppHandle { get; set; }
    }
}
