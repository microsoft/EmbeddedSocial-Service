// <copyright file="TopicEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// Topic entity class
    /// </summary>
    public class TopicEntity : ObjectEntity, ITopicEntity
    {
        /// <summary>
        /// Gets or sets created time
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        public DateTime LastUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets publisher type
        /// </summary>
        public PublisherType PublisherType { get; set; }

        /// <summary>
        /// Gets or sets owner user handle
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets topic title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets topic text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets topic blob type
        /// </summary>
        public BlobType BlobType { get; set; }

        /// <summary>
        /// Gets or sets topic blob handle
        /// </summary>
        public string BlobHandle { get; set; }

        /// <summary>
        /// Gets or sets topic categories
        /// </summary>
        public string Categories { get; set; }

        /// <summary>
        /// Gets or sets topic language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets topic group
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Gets or sets topic deep link
        /// </summary>
        public string DeepLink { get; set; }

        /// <summary>
        /// Gets or sets topic friendly name
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets review status
        /// </summary>
        public ReviewStatus ReviewStatus { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets request id associated with the create topic request
        /// </summary>
        public string RequestId { get; set; }
    }
}
