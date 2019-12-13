// <copyright file="PostTopicRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to post (create) topic
    /// </summary>
    public class PostTopicRequest
    {
        /// <summary>
        /// Gets or sets publisher type
        /// </summary>
        [Required]
        public PublisherType PublisherType { get; set; }

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
        /// Gets or sets topic blob type
        /// </summary>
        public BlobType BlobType { get; set; }

        /// <summary>
        /// Gets or sets topic blob handle
        /// </summary>
        public string BlobHandle { get; set; }

        /// <summary>
        /// Gets or sets topic categories
        /// </summary>
        public string Categories { get; set; }

        /// <summary>
        /// Gets or sets topic language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets topic deep link
        /// </summary>
        public string DeepLink { get; set; }

        /// <summary>
        /// Gets or sets topic friendly name
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets topic group
        /// </summary>
        public string Group { get; set; }
    }
}
