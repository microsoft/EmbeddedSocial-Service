// <copyright file="IContentModerationMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using SocialPlus.Models;

    /// <summary>
    /// Content moderation message
    /// </summary>
    public interface IContentModerationMessage : IModerationMessage
    {
        /// <summary>
        /// Gets or sets the content type
        /// </summary>
        ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content handle
        /// </summary>
        string ContentHandle { get; set; }
    }
}
