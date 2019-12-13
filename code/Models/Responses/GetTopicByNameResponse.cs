// <copyright file="GetTopicByNameResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Response from get topic by name
    /// </summary>
    public class GetTopicByNameResponse
    {
        /// <summary>
        /// Gets or sets topic handle of the response
        /// </summary>
        [Required]
        public string TopicHandle { get; set; }
    }
}
