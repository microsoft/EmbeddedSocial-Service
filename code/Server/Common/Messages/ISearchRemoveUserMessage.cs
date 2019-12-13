// <copyright file="ISearchRemoveUserMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    /// <summary>
    /// Search remove user message interface
    /// </summary>
    public interface ISearchRemoveUserMessage : IMessage
    {
        /// <summary>
        /// Gets or sets UserHandle of user to remove
        /// </summary>
        string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets AppHandle of user to remove
        /// </summary>
        string AppHandle { get; set; }
    }
}
