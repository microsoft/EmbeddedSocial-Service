// <copyright file="IClientConfigEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// Client config entity interface
    /// </summary>
    public interface IClientConfigEntity
    {
        /// <summary>
        /// Gets or sets server side app key
        /// </summary>
        string ServerSideAppKey { get; set; }

        /// <summary>
        /// Gets or sets client configuration JSON
        /// </summary>
        string ClientConfigJson { get; set; }
    }
}
