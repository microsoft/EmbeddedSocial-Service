// <copyright file="IBlobMetadataEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using SocialPlus.Models;

    /// <summary>
    /// Blob metadata entity interface
    /// </summary>
    public interface IBlobMetadataEntity
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
        /// Gets or sets blob type
        /// </summary>
        BlobType BlobType { get; set; }
    }
}
