// <copyright file="ILikeMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;
    using SocialPlus.Models;

    /// <summary>
    /// Like message interface
    /// </summary>
    public interface ILikeMessage : IMessage
    {
        /// <summary>
        /// Gets or sets like handle
        /// </summary>
        string LikeHandle { get; set; }

        /// <summary>
        /// Gets or sets content type
        /// </summary>
        ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets content handle
        /// </summary>
        string ContentHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content is liked
        /// </summary>
        bool Liked { get; set; }

        /// <summary>
        /// Gets or sets content publisher type
        /// </summary>
        PublisherType ContentPublisherType { get; set; }

        /// <summary>
        /// Gets or sets content user handle
        /// </summary>
        string ContentUserHandle { get; set; }

        /// <summary>
        /// Gets or sets content created time
        /// </summary>
        DateTime ContentCreatedTime { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        DateTime LastUpdatedTime { get; set; }
    }
}
