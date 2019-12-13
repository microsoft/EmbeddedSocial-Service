// <copyright file="ReplyView.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Reply view
    /// </summary>
    public class ReplyView
    {
        /// <summary>
        /// Gets or sets reply handle
        /// </summary>
        [Required]
        public string ReplyHandle { get; set; }

        /// <summary>
        /// Gets or sets parent comment handle
        /// </summary>
        [Required]
        public string CommentHandle { get; set; }

        /// <summary>
        /// Gets or sets root topic handle
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
        /// Gets or sets owner of the reply
        /// </summary>
        [Required]
        public UserCompactView User { get; set; }

        /// <summary>
        /// Gets or sets reply text
        /// </summary>
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets reply language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets total likes for the reply
        /// </summary>
        [Required]
        public long TotalLikes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the querying user has liked the reply
        /// </summary>
        [Required]
        public bool Liked { get; set; }

        /// <summary>
        /// Gets or sets content status
        /// </summary>
        public ReviewStatus ContentStatus { get; set; }
    }
}
