// <copyright file="PostReplyResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Response from post (create) reply
    /// </summary>
    public class PostReplyResponse
    {
        /// <summary>
        /// Gets or sets reply handle of the reply
        /// </summary>
        [Required]
        public string ReplyHandle { get; set; }
    }
}
