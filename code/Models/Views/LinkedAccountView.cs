// <copyright file="LinkedAccountView.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Linked account view
    /// </summary>
    public class LinkedAccountView
    {
        /// <summary>
        /// Gets or sets identity provider type
        /// </summary>
        [Required]
        public IdentityProviderType IdentityProvider { get; set; }

        /// <summary>
        /// Gets or sets third party account id -- Unique user id provided by the third-party identity provider
        /// </summary>
        [Required]
        public string AccountId { get; set; }
    }
}
