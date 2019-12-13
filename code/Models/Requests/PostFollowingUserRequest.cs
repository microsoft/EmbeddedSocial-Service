// <copyright file="PostFollowingUserRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to follow a user
    /// </summary>
    public class PostFollowingUserRequest
    {
        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        [Required]
        public string UserHandle { get; set; }
    }
}
