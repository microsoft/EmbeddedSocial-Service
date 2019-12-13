// <copyright file="GetRequestTokenResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Response from get request token response
    /// </summary>
    public class GetRequestTokenResponse
    {
        /// <summary>
        /// Gets or sets request token from identity provider
        /// </summary>
        [Required]
        public string RequestToken { get; set; }
    }
}
