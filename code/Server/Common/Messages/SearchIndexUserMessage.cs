// <copyright file="SearchIndexUserMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Search index user message
    /// </summary>
    public class SearchIndexUserMessage : QueueMessage, ISearchIndexUserMessage
    {
        /// <summary>
        /// Gets or sets UserHandle of the user to index
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets AppHandle of the user to index
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets timestamp of the update corresponding to this message
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
