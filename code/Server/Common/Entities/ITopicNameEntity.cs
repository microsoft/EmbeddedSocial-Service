// <copyright file="ITopicNameEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;
    using SocialPlus.Models;

    /// <summary>
    /// Topic entity interface
    /// </summary>
    public interface ITopicNameEntity
    {
        /// <summary>
        /// Gets or sets publisher type
        /// </summary>
        PublisherType PublisherType { get; set; }

        /// <summary>
        /// Gets or sets topic name
        /// </summary>
        string TopicName { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        string TopicHandle { get; set; }
    }
}
