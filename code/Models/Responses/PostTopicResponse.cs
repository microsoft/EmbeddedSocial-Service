// <copyright file="PostTopicResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Response from post (create) topic
    /// </summary>
    public class PostTopicResponse
    {
        /// <summary>
        /// Gets or sets topic handle of the topic
        /// </summary>
        [Required]
        public string TopicHandle { get; set; }
    }
}
