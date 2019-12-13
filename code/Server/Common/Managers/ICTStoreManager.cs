// <copyright file="ICTStoreManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// interface of cached table store manager
    /// </summary>
    public interface ICTStoreManager
    {
        /// <summary>
        /// Gets the key used as the partition key and object key for storing the social plus storage version number.
        /// </summary>
        string StoreVersionKey { get; }

        /// <summary>
        /// Gets the version string represents the version number of the data stored by the social plus service.
        /// </summary>
        string StoreVersionString { get; }

        /// <summary>
        /// Gets the default count key for tables to don't need a unique count key.
        /// </summary>
        string DefaultCountKey { get; }

        /// <summary>
        /// Gets the default feed key for tables to don't need a unique feed key.
        /// </summary>
        string DefaultFeedKey { get; }

        /// <summary>
        /// Gets the default object key for tables to don't need a unique object key.
        /// </summary>
        string DefaultObjectKey { get; }

        /// <summary>
        /// Initialization routine performs a version check.  Refuses to initialize if the version numbers don't match.
        /// </summary>
        /// <returns>true if the version check passes</returns>
        Task<bool> Initialize();

        /// <summary>
        /// Get store from container identifier
        /// </summary>
        /// <param name="containerIdentifier">Container identifier</param>
        /// <returns>CT store</returns>
        Task<CTStore> GetStore(ContainerIdentifier containerIdentifier);

        /// <summary>
        /// Get table from container identifier and table identifier
        /// </summary>
        /// <param name="containerIdentifier">Container identifier</param>
        /// <param name="tableIdentifier">Table identifier</param>
        /// <returns>Store table</returns>
        Table GetTable(ContainerIdentifier containerIdentifier, TableIdentifier tableIdentifier);
    }
}
