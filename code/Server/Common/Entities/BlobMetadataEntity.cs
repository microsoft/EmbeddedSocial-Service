// <copyright file="BlobMetadataEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// Blob metadata entity class
    /// </summary>
    public class BlobMetadataEntity : ObjectEntity, IBlobMetadataEntity
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
        /// Gets or sets blob type
        /// </summary>
        public BlobType BlobType { get; set; }
    }
}
