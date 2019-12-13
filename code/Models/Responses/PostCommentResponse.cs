// <copyright file="PostCommentResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Response from post (create) comment
    /// </summary>
    public class PostCommentResponse
    {
        /// <summary>
        /// Gets or sets comment handle of the comment
        /// </summary>
        [Required]
        public string CommentHandle { get; set; }
    }
}
