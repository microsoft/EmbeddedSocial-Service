// <copyright file="IImageModerationMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using SocialPlus.Models;

    /// <summary>
    /// Image moderation message interface
    /// </summary>
    public interface IImageModerationMessage : IModerationMessage
    {
        /// <summary>
        /// Gets or sets the content handle
        /// </summary>
        string BlobHandle { get; set; }

        /// <summary>
        /// Gets or sets the user handle
        /// </summary>
        string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets the type of image
        /// </summary>
        ImageType ImageType { get; set; }
    }
}
