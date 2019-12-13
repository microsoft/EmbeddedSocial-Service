// <copyright file="ISearchIndexUserMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;

    /// <summary>
    /// Search index user message interface
    /// </summary>
    public interface ISearchIndexUserMessage : IMessage
    {
        /// <summary>
        /// Gets or sets UserHandle of the user to index
        /// </summary>
        string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets AppHandle of the user to index
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets timestamp of the update corresponding to this message
        /// </summary>
        DateTime Timestamp { get; set; }
    }
}
