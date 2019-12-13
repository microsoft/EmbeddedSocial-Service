// <copyright file="CountResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Count response
    /// </summary>
    public class CountResponse
    {
        /// <summary>
        /// Gets or sets count
        /// </summary>
        [Required]
        public long Count { get; set; }
    }
}
