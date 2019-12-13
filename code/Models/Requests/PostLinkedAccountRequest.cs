// <copyright file="PostLinkedAccountRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to post (create) linked account
    /// </summary>
    public class PostLinkedAccountRequest
    {
        /// <summary>
        /// Gets or sets a session token.
        /// </summary>
        [Required]
        public string SessionToken { get; set; }
    }
}
