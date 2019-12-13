// <copyright file="ISearchIndexTopicMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;

    /// <summary>
    /// Search index topic message interface
    /// </summary>
    public interface ISearchIndexTopicMessage : IMessage
    {
        /// <summary>
        /// Gets or sets TopicHandle of the topic to index
        /// </summary>
        string TopicHandle { get; set; }

        /// <summary>
        /// Gets or sets timestamp of the update corresponding to this message
        /// </summary>
        DateTime Timestamp { get; set; }
    }
}
