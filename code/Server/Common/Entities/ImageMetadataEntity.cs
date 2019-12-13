// <copyright file="ImageMetadataEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// Image metadata entity class
    /// </summary>
    public class ImageMetadataEntity : ObjectEntity, IImageMetadataEntity
    {
        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets blob handle
        /// </summary>
        public string BlobHandle { get; set; }

        /// <summary>
        /// Gets or sets image length
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// Gets or sets content type
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets image type
        /// </summary>
        public ImageType ImageType { get; set; }

        /// <summary>
        /// Gets or sets moderation review status
        /// </summary>
        public ReviewStatus ReviewStatus { get; set; }
    }
}
