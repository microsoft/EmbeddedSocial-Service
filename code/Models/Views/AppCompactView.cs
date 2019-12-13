// <copyright file="AppCompactView.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// App compact view
    /// </summary>
    public class AppCompactView
    {
        /// <summary>
        /// Gets or sets app name
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets app icon handle
        /// </summary>
        public string IconHandle { get; set; }

        /// <summary>
        /// Gets or sets app icon url
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Gets or sets platform type
        /// </summary>
        [Required]
        public PlatformType PlatformType { get; set; }

        /// <summary>
        /// Gets or sets app deep link
        /// </summary>
        public string DeepLink { get; set; }

        /// <summary>
        /// Gets or sets app store link
        /// </summary>
        public string StoreLink { get; set; }
    }
}
