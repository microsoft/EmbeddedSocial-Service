// <copyright file="Blobs.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageServerState
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Server.Blobs;
    using SocialPlus.Server.CBStore;

    /// <summary>
    /// portion of Program class that deals with creating and deleting blobs
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// Provision azure blob storage with containers
        /// </summary>
        /// <param name="blobStoreManager">cached blob store manager</param>
        /// <param name="azureBlobConnectionString">Azure connection string</param>
        /// <returns>provision task</returns>
        private static async Task ProvisionAzureStorageBlobs(ICBStoreManager blobStoreManager, string azureBlobConnectionString)
        {
            Console.WriteLine("Creating all blob containers in Azure Blob Store...");
            AzureBlobStorage azureBlobStorage = new AzureBlobStorage(azureBlobConnectionString);
            CBStore store = new CBStore(azureBlobStorage, null);

            foreach (ContainerIdentifier containerIdentifier in Enum.GetValues(typeof(ContainerIdentifier)))
            {
                if (!ContainerDescriptorProvider.Containers.ContainsKey(containerIdentifier))
                {
                    Console.WriteLine("  " + containerIdentifier.ToString() + " - Descriptor not found");
                    continue;
                }

                string containerName = blobStoreManager.GetContainerName(containerIdentifier);
                await store.CreateContainerAsync(containerName);
                Console.WriteLine("  " + containerIdentifier.ToString() + " - Container Provisioned");
            }
        }

        /// <summary>
        /// deletes the blob container (and therefore all the blobs)
        /// </summary>
        /// <param name="blobStoreManager">cached blob store manager</param>
        /// <param name="azureBlobStorageConnectionString">connection string to azure blob storage</param>
        /// <returns>delete task</returns>
        private static async Task DeleteAzureBlobs(ICBStoreManager blobStoreManager, string azureBlobStorageConnectionString)
        {
            Console.WriteLine("Deleting all blobs from Azure Blob Store...");
            AzureBlobStorage azureBlobStorage = new AzureBlobStorage(azureBlobStorageConnectionString);
            CBStore store = new CBStore(azureBlobStorage, null);

            foreach (ContainerIdentifier containerIdentifier in Enum.GetValues(typeof(ContainerIdentifier)))
            {
                if (!ContainerDescriptorProvider.Containers.ContainsKey(containerIdentifier))
                {
                    Console.WriteLine("  " + containerIdentifier.ToString() + " - Descriptor not found");
                    continue;
                }

                string containerName = blobStoreManager.GetContainerName(containerIdentifier);
                await store.DeleteContainerAsync(containerName);
                Console.WriteLine("  " + containerIdentifier.ToString() + " - Container Deleted");
            }
        }
    }
}
