// <copyright file="TopicNameEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// Topic name entity class
    /// </summary>
    public class TopicNameEntity : ObjectEntity, ITopicNameEntity
    {
        /// <summary>
        /// Gets or sets publisher type
        /// </summary>
        public PublisherType PublisherType { get; set; }

        /// <summary>
        /// Gets or sets topic name
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        public string TopicHandle { get; set; }
    }
}
