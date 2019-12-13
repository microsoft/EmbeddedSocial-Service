// <copyright file="IModerationMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;

    /// <summary>
    /// Interface declares the moderation message
    /// </summary>
    public interface IModerationMessage : IMessage
    {
        /// <summary>
        /// Gets or sets the app handle for the moderated content
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets the moderation handle
        /// </summary>
        string ModerationHandle { get; set; }

        /// <summary>
        /// Gets or sets the URI used to post the moderation results
        /// </summary>
        Uri CallbackUri { get; set; }
    }
}
