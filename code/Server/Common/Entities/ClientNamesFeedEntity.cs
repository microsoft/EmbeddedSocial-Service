// <copyright file="ClientNamesFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// Client names feed entity class
    /// </summary>
    public class ClientNamesFeedEntity : FeedEntity, IClientNamesFeedEntity
    {
        /// <summary>
        /// Gets or sets client name
        /// </summary>
        public string ClientName { get; set; }
    }
}
