// <copyright file="PutPushRegistrationRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to put push registration (register or update)
    /// </summary>
    public class PutPushRegistrationRequest
    {
        /// <summary>
        /// Gets or sets last updated time from the OS in ISO 8601 format.
        /// This is used to expire out registrations that have not been updated every 30 days.
        /// </summary>
        [Required]
        public string LastUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets language of the user
        /// </summary>
        [Required]
        public string Language { get; set; }
    }
}
