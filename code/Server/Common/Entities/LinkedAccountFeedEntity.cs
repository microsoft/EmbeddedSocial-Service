// <copyright file="LinkedAccountFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// User linked account feed entity class
    /// </summary>
    public class LinkedAccountFeedEntity : FeedEntity, ILinkedAccountFeedEntity
    {
        /// <summary>
        /// Gets or sets identity provider type
        /// </summary>
        public IdentityProviderType IdentityProviderType { get; set; }

        /// <summary>
        /// Gets or sets account id
        /// </summary>
        public string AccountId { get; set; }
    }
}
