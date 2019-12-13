// <copyright file="PostFollowingTopicRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to follow a topic
    /// </summary>
    public class PostFollowingTopicRequest
    {
        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        [Required]
        public string TopicHandle { get; set; }
    }
}
