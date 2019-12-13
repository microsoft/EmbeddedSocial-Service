// <copyright file="IUserRelationshipLookupEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;
    using SocialPlus.Models;

    /// <summary>
    /// User relationship lookup entity interface
    /// </summary>
    public interface IUserRelationshipLookupEntity
    {
        /// <summary>
        /// Gets or sets relationship handle
        /// </summary>
        string RelationshipHandle { get; set; }

        /// <summary>
        /// Gets or sets user relationship status
        /// </summary>
        UserRelationshipStatus UserRelationshipStatus { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        DateTime LastUpdatedTime { get; set; }
    }
}
