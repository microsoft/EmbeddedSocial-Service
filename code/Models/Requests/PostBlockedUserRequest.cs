// <copyright file="PostBlockedUserRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to post a blocked user (block user)
    /// </summary>
    public class PostBlockedUserRequest
    {
        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        [Required]
        public string UserHandle { get; set; }
    }
}
