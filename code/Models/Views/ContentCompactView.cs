// <copyright file="ContentCompactView.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Content compact view
    /// </summary>
    public class ContentCompactView
    {
        /// <summary>
        /// Gets or sets content type
        /// </summary>
        [Required]
        public ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets content handle
        /// </summary>
        [Required]
        public string ContentHandle { get; set; }

        /// <summary>
        /// Gets or sets parent handle
        /// </summary>
        public string ParentHandle { get; set; }

        /// <summary>
        /// Gets or sets root handle
        /// </summary>
        public string RootHandle { get; set; }

        /// <summary>
        /// Gets or sets content text
        /// </summary>
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets content blob type
        /// </summary>
        public BlobType BlobType { get; set; }

        /// <summary>
        /// Gets or sets content blob handle
        /// </summary>
        public string BlobHandle { get; set; }

        /// <summary>
        /// Gets or sets content blob url
        /// </summary>
        public string BlobUrl { get; set; }
    }
}
