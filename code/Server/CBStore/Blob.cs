// <copyright file="Blob.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.CBStore
{
    using System.IO;

    /// <summary>
    /// Blob class
    /// </summary>
    public class Blob
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
