// <copyright file="ClientConfigEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// Client config entity class
    /// </summary>
    public class ClientConfigEntity : ObjectEntity, IClientConfigEntity
    {
        /// <summary>
        /// Gets or sets server side app key
        /// </summary>
        public string ServerSideAppKey { get; set; }

        /// <summary>
        /// Gets or sets client configuration JSON
        /// </summary>
        public string ClientConfigJson { get; set; }
    }
}
