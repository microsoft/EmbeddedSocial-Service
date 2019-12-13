// <copyright file="BlobItem.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Blobs
{
    using System.IO;

    /// <summary>
    /// Blob item class
    /// </summary>
    public class BlobItem : IBlobItem
    {
        /// <summary>
        /// Gets or sets blob stream
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// Gets or sets content type
        /// </summary>
        public string ContentType { get; set; }
    }
}
