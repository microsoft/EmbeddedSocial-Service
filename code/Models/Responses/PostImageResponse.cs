// <copyright file="PostImageResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Response from post (create) image
    /// </summary>
    public class PostImageResponse
    {
        /// <summary>
        /// Gets or sets blob handle
        /// </summary>
        [Required]
        public string BlobHandle { get; set; }
    }
}
