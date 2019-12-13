// <copyright file="ContainerDescriptor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Blobs
{
    /// <summary>
    /// Container descriptor class
    /// </summary>
    public class ContainerDescriptor
    {
        /// <summary>
        /// Gets or sets container name
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets Azure storage instance type
        /// </summary>
        public AzureStorageInstanceType AzureStorageInstanceType { get; set; }

        /// <summary>
        /// Gets or sets Azure CDN instance type
        /// </summary>
        public AzureCdnInstanceType AzureCdnInstanceType { get; set; }
    }
}
