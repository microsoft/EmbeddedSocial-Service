// <copyright file="FeedResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Feed response
    /// </summary>
    /// <typeparam name="T">The generic type parameter.</typeparam>
    public class FeedResponse<T>
    {
        /// <summary>
        /// Gets or sets feed data
        /// </summary>
        [Required]
        public List<T> Data { get; set; }

        /// <summary>
        /// Gets or sets feed cursor
        /// </summary>
        [Required]
        public string Cursor { get; set; }
    }
}
