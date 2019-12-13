// <copyright file="PutUserInfoRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to put (update) user info
    /// </summary>
    public class PutUserInfoRequest
    {
        /// <summary>
        /// Gets or sets first name of the user
        /// </summary>
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets last name of the user
        /// </summary>
        [Required]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets short bio of the user
        /// </summary>
        public string Bio { get; set; }
    }
}
