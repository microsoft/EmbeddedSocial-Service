// <copyright file="SearchRemoveUserMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Search remove user message
    /// </summary>
    public class SearchRemoveUserMessage : QueueMessage, ISearchRemoveUserMessage
    {
        /// <summary>
        /// Gets or sets UserHandle of user to remove
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets AppHandle of user to remove
        /// </summary>
        public string AppHandle { get; set; }
    }
}
