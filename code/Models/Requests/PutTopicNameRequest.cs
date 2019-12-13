// <copyright file="PutTopicNameRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to put (update) a topic name
    /// </summary>
    public class PutTopicNameRequest
    {
        /// <summary>
        /// Gets or sets publisher type
        /// </summary>
        [Required]
        public PublisherType PublisherType { get; set; }

        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        [Required]
        public string TopicHandle { get; set; }
    }
}
