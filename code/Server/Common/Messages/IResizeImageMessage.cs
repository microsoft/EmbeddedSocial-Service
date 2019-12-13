// <copyright file="IResizeImageMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using SocialPlus.Models;

    /// <summary>
    /// Resize image message interface
    /// </summary>
    public interface IResizeImageMessage : IMessage
    {
        /// <summary>
        /// Gets or sets blob handle to the image
        /// </summary>
        string BlobHandle { get; set; }

        /// <summary>
        /// Gets or sets image type
        /// </summary>
        ImageType ImageType { get; set; }
    }
}
