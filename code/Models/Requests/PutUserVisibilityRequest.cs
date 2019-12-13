// <copyright file="PutUserVisibilityRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to put (update) user visibility
    /// </summary>
    public class PutUserVisibilityRequest
    {
        /// <summary>
        /// Gets or sets visibility of the user
        /// </summary>
        [Required]
        public UserVisibilityStatus Visibility { get; set; }
    }
}
