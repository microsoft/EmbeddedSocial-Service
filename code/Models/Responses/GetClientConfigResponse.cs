// <copyright file="GetClientConfigResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    /// <summary>
    /// Response from get client config
    /// </summary>
    public class GetClientConfigResponse
    {
        /// <summary>
        /// Gets or sets the server-side component of a split app key
        /// </summary>
        public string ServerSideAppKey { get; set; }

        /// <summary>
        /// Gets or sets client configuration
        /// </summary>
        /// <remarks>
        /// Client configuration is expected to be in a JSON format
        /// </remarks>
        public string ClientConfigJson { get; set; }
    }
}
