// <copyright file="GetServiceInfoResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    /// <summary>
    /// Response from get service info
    /// </summary>
    public class GetServiceInfoResponse
    {
        /// <summary>
        /// Gets or sets the service api version number
        /// </summary>
        public string ServiceApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the service api version numbers of all supported APIs
        /// </summary>
        public string ServiceApiAllVersions { get; set; }
    }
}
