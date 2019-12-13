// <copyright file="ModerationMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;

    using SocialPlus.Server.Messaging;

    /// <summary>
    /// The moderation queue message
    /// </summary>
    public abstract class ModerationMessage : QueueMessage, IModerationMessage
    {
        /// <summary>
        /// Gets or sets the app handle for the moderated content
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets the moderation handle
        /// </summary>
        public string ModerationHandle { get; set; }

        /// <summary>
        /// Gets or sets the URI used to post the moderation results
        /// </summary>
        public Uri CallbackUri { get; set; }
    }
}
