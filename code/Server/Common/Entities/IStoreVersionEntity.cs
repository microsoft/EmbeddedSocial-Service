// <copyright file="IStoreVersionEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// Entity to store internal version number for social plus tables
    /// </summary>
    public interface IStoreVersionEntity
    {
        /// <summary>
        /// Gets or sets the version number of the Social Plus tables
        /// </summary>
        string Version { get; set; }
    }
}
