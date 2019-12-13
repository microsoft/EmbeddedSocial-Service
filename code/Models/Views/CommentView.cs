// <copyright file="CommentView.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Comment view
    /// </summary>
    public class CommentView
    {
        /// <summary>
        /// Gets or sets comment handle
        /// </summary>
        [Required]
        public string CommentHandle { get; set; }

        /// <summary>
        /// Gets or sets parent topic handle
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
        /// Gets or sets owner of the comment
        /// </summary>
        [Required]
        public UserCompactView User { get; set; }

        /// <summary>
        /// Gets or sets comment text
        /// </summary>
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets comment blob type
        /// </summary>
        public BlobType BlobType { get; set; }

        /// <summary>
        /// Gets or sets comment blob handle
        /// </summary>
        public string BlobHandle { get; set; }

        /// <summary>
        /// Gets or sets comment blob url
        /// </summary>
        public string BlobUrl { get; set; }

        /// <summary>
        /// Gets or sets comment language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets total likes for the comment
        /// </summary>
        [Required]
        public long TotalLikes { get; set; }

        /// <summary>
        /// Gets or sets total replies for the comment
        /// </summary>
        [Required]
        public long TotalReplies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the querying user has liked the comment
        /// </summary>
        [Required]
        public bool Liked { get; set; }

        /// <summary>
        /// Gets or sets content status
        /// </summary>
        public ReviewStatus ContentStatus { get; set; }
    }
}
