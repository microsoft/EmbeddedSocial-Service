// <copyright file="IImageMetadataEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using SocialPlus.Models;

    /// <summary>
    /// Image metadata entity interface
    /// </summary>
    public interface IImageMetadataEntity
    {
        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets blob handle
        /// </summary>
        string BlobHandle { get; set; }

        /// <summary>
        /// Gets or sets image length
        /// </summary>
        long Length { get; set; }

        /// <summary>
        /// Gets or sets content type
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Gets or sets image type
        /// </summary>
        ImageType ImageType { get; set; }

        /// <summary>
        /// Gets or sets the moderation review status
        /// </summary>
        ReviewStatus ReviewStatus { get; set; }
    }
}
