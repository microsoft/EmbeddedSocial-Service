// <copyright file="ICBStoreManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Blobs
{
    using System.Threading.Tasks;

    using SocialPlus.Server.CBStore;

    /// <summary>
    /// interface of cached blob storage manager
    /// </summary>
    public interface ICBStoreManager
    {
        /// <summary>
        /// Get store from container identifier
        /// </summary>
        /// <param name="containerIdentifier">Container identifier</param>
        /// <returns>CB store</returns>
        Task<CBStore> GetStore(ContainerIdentifier containerIdentifier);

        /// <summary>
        /// Get container name for container identifier
        /// </summary>
        /// <param name="containerIdentifier">Container identifier</param>
        /// <returns>Container name</returns>
        string GetContainerName(ContainerIdentifier containerIdentifier);
    }
}
