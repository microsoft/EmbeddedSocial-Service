// <copyright file="ImageModerationMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using SocialPlus.Models;

    /// <summary>
    /// Image moderation message class
    /// </summary>
    public class ImageModerationMessage : ModerationMessage, IImageModerationMessage
    {
        /// <summary>
        /// Gets or sets the blob handle
        /// </summary>
        public string BlobHandle { get; set; }

        /// <summary>
        /// Gets or sets the user handle
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets the type of image
        /// </summary>
        public ImageType ImageType { get; set; }
    }
}
