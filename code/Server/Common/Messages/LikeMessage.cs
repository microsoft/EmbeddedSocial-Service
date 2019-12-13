// <copyright file="LikeMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;

    using SocialPlus.Models;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Like message class
    /// </summary>
    public class LikeMessage : QueueMessage, ILikeMessage
    {
        /// <summary>
        /// Gets or sets like handle
        /// </summary>
        public string LikeHandle { get; set; }

        /// <summary>
        /// Gets or sets content type
        /// </summary>
        public ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets content handle
        /// </summary>
        public string ContentHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content is liked
        /// </summary>
        public bool Liked { get; set; }

        /// <summary>
        /// Gets or sets content publisher type
        /// </summary>
        public PublisherType ContentPublisherType { get; set; }

        /// <summary>
        /// Gets or sets content user handle
        /// </summary>
        public string ContentUserHandle { get; set; }

        /// <summary>
        /// Gets or sets content created time
        /// </summary>
        public DateTime ContentCreatedTime { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        public DateTime LastUpdatedTime { get; set; }
    }
}
