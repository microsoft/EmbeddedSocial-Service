// <copyright file="PostTopicNameRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Post topic name request
    /// </summary>
    public class PostTopicNameRequest
    {
        /// <summary>
        /// Gets or sets publisher type
        /// </summary>
        [Required]
        public PublisherType PublisherType { get; set; }

        /// <summary>
        /// Gets or sets topic name
        /// </summary>
        [Required]
        public string TopicName { get; set; }

        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        [Required]
        public string TopicHandle { get; set; }
    }
}
