// <copyright file="ICdn.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.CBStore
{
    using System;

    /// <summary>
    /// CDN interface
    /// </summary>
    public interface ICdn
    {
        /// <summary>
        /// Query CDN url
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>CDN url</returns>
        Uri QueryCdnUrl(string containerName, string blobName);
    }
}
