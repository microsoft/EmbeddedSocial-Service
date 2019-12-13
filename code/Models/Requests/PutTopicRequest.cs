// <copyright file="PutTopicRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to put (update) topic
    /// </summary>
    public class PutTopicRequest
    {
        /// <summary>
        /// Gets or sets topic title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets topic text
        /// </summary>
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets topic categories
        /// </summary>
        public string Categories { get; set; }
    }
}
