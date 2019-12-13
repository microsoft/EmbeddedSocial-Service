// <copyright file="DeleteTopicNameRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to delete a topic name
    /// </summary>
    public class DeleteTopicNameRequest
    {
        /// <summary>
        /// Gets or sets publisher type
        /// </summary>
        [Required]
        public PublisherType PublisherType { get; set; }
    }
}
