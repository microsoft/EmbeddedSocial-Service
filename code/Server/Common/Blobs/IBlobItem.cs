// <copyright file="IBlobItem.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Blobs
{
    using System.IO;

    /// <summary>
    /// Blob item interface
    /// </summary>
    public interface IBlobItem
    {
        /// <summary>
        /// Gets or sets blob stream
        /// </summary>
        Stream Stream { get; set; }

        /// <summary>
        /// Gets or sets content type
        /// </summary>
        string ContentType { get; set; }
    }
}
