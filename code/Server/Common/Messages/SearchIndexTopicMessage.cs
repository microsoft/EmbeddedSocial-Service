// <copyright file="SearchIndexTopicMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Search index topic message
    /// </summary>
    public class SearchIndexTopicMessage : QueueMessage, ISearchIndexTopicMessage
    {
        /// <summary>
        /// Gets or sets TopicHandle of the topic to index
        /// </summary>
        public string TopicHandle { get; set; }

        /// <summary>
        /// Gets or sets timestamp of the update corresponding to this message
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
