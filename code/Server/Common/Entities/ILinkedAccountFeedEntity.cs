// <copyright file="ILinkedAccountFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using SocialPlus.Models;

    /// <summary>
    /// User linked account feed entity interface
    /// </summary>
    public interface ILinkedAccountFeedEntity
    {
        /// <summary>
        /// Gets or sets identity provider type
        /// </summary>
        IdentityProviderType IdentityProviderType { get; set; }

        /// <summary>
        /// Gets or sets account id
        /// </summary>
        string AccountId { get; set; }
    }
}
