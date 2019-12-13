// <copyright file="StoreVersionEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// Entity to store internal version number for social plus tables
    /// </summary>
    public class StoreVersionEntity : ObjectEntity, IStoreVersionEntity
    {
        /// <summary>
        /// Gets or sets the version number of the Social Plus tables
        /// </summary>
        public string Version { get; set; }
    }
}
