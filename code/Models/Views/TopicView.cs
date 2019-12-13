// <copyright file="TopicView.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Topic view
    /// </summary>
    public class TopicView
    {
        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        [Required]
        public string TopicHandle { get; set; }

        /// <summary>
        /// Gets or sets created time
        /// </summary>
        [Required]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        [Required]
        public DateTime LastUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets publisher type
        /// </summary>
        [Required]
        public PublisherType PublisherType { get; set; }

        /// <summary>
        /// Gets or sets owner of the topic
        /// </summary>
        public UserCompactView User { get; set; }

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
        /// Gets or sets topic blob url
        /// </summary>
        public string BlobUrl { get; set; }

        /// <summary>
        /// Gets or sets topic categories
        /// </summary>
        public string Categories { get; set; }

        /// <summary>
        /// Gets or sets topic language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets topic group
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Gets or sets topic deep link
        /// </summary>
        public string DeepLink { get; set; }

        /// <summary>
        /// Gets or sets topic friendly name
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets total likes for the topic
        /// </summary>
        [Required]
        public long TotalLikes { get; set; }

        /// <summary>
        /// Gets or sets total comments for the topic
        /// </summary>
        [Required]
        public long TotalComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the querying user has liked the topic
        /// </summary>
        [Required]
        public bool Liked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the querying user has pinned the topic
        /// </summary>
        public bool Pinned { get; set; }

        /// <summary>
        /// Gets or sets content status
        /// </summary>
        public ReviewStatus ContentStatus { get; set; }

        /// <summary>
        /// Gets or sets the containing app
        /// </summary>
        public AppCompactView App { get; set; }
    }
}
