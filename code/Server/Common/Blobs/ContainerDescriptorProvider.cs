// <copyright file="ContainerDescriptorProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Blobs
{
    using System.Collections.Generic;

    /// <summary>
    /// Container descriptor provider -- All blob container descriptors are programmed here
    /// </summary>
    public static class ContainerDescriptorProvider
    {
        /// <summary>
        /// Blob container descriptors dictionary
        /// </summary>
        private static Dictionary<ContainerIdentifier, ContainerDescriptor> containers = new Dictionary<ContainerIdentifier, ContainerDescriptor>();

        /// <summary>
        /// Initializes static members of the <see cref="ContainerDescriptorProvider"/> class.
        /// </summary>
        static ContainerDescriptorProvider()
        {
            Initalize();
        }

        /// <summary>
        /// Gets blob container descriptors dictionary
        /// </summary>
        public static Dictionary<ContainerIdentifier, ContainerDescriptor> Containers
        {
            get
            {
                return containers;
            }
        }

        /// <summary>
        /// Initialize queue descriptors
        /// </summary>
        private static void Initalize()
        {
            ContainerDescriptor blobsContainer = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.Blobs.ToString().ToLower(),
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                AzureCdnInstanceType = AzureCdnInstanceType.Default
            };
            ContainerDescriptor imagesContainer = new ContainerDescriptor()
            {
                ContainerName = ContainerIdentifier.Images.ToString().ToLower(),
                AzureStorageInstanceType = AzureStorageInstanceType.Default,
                AzureCdnInstanceType = AzureCdnInstanceType.Default
            };

            Add(ContainerIdentifier.Blobs, blobsContainer);
            Add(ContainerIdentifier.Images, imagesContainer);
        }

        /// <summary>
        /// Add container descriptor for container identifier
        /// </summary>
        /// <param name="containerIdentifier">Container identifier</param>
        /// <param name="containerDescriptor">Container descriptor</param>
        private static void Add(ContainerIdentifier containerIdentifier, ContainerDescriptor containerDescriptor)
        {
            containers.Add(containerIdentifier, containerDescriptor);
        }
    }
}
