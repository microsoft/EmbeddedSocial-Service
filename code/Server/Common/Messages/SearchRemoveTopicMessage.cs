// <copyright file="SearchRemoveTopicMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Search remove topic message
    /// </summary>
    public class SearchRemoveTopicMessage : QueueMessage, ISearchRemoveTopicMessage
    {
        /// <summary>
        /// Gets or sets TopicHandle of the topic to remove
        /// </summary>
        public string TopicHandle { get; set; }
    }
}
