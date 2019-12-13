// <copyright file="PostFollowerRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to post (accept) a follower
    /// </summary>
    public class PostFollowerRequest
    {
        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        [Required]
        public string UserHandle { get; set; }
    }
}
