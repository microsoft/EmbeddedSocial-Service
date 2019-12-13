// <copyright file="PostPinRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to post (create) a pin
    /// </summary>
    public class PostPinRequest
    {
        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        [Required]
        public string TopicHandle { get; set; }
    }
}
