// <copyright file="ResizeImageMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using SocialPlus.Models;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Resize image message class
    /// </summary>
    public class ResizeImageMessage : QueueMessage, IResizeImageMessage
    {
        /// <summary>
        /// Gets or sets blob handle to the image
        /// </summary>
        public string BlobHandle { get; set; }

        /// <summary>
        /// Gets or sets image type
        /// </summary>
        public ImageType ImageType { get; set; }
    }
}
