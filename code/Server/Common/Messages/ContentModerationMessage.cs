// <copyright file="ContentModerationMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using SocialPlus.Models;

    /// <summary>
    /// Content moderation message
    /// </summary>
    public class ContentModerationMessage : ModerationMessage, IContentModerationMessage
    {
        /// <summary>
        /// Gets or sets the content type
        /// </summary>
        public ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content handle
        /// </summary>
        public string ContentHandle { get; set; }
    }
}
