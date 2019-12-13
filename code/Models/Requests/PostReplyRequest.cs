// <copyright file="PostReplyRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to post (create) reply
    /// </summary>
    public class PostReplyRequest
    {
        /// <summary>
        /// Gets or sets reply text
        /// </summary>
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets reply language
        /// </summary>
        public string Language { get; set; }
    }
}
