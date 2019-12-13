// <copyright file="AzureCdn.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.CBStore
{
    using System;

    /// <summary>
    /// Azure CDN as CDN
    /// </summary>
    public class AzureCdn : ICdn
    {
        /// <summary>
        /// Azure CDN url
        /// </summary>
        private string url;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCdn"/> class
        /// </summary>
        /// <param name="url">Azure CDN url</param>
        public AzureCdn(string url)
        {
            this.url = url;
        }

        /// <summary>
        /// Query CDN url
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>CDN url</returns>
        public Uri QueryCdnUrl(string containerName, string blobName)
        {
            Uri baseUri = new Uri(this.url);
            return new Uri(baseUri, containerName + "/" + blobName);
        }
    }
}
