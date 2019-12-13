// <copyright file="UserRelationshipLookupEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// User relationship lookup entity class
    /// </summary>
    public class UserRelationshipLookupEntity : ObjectEntity, IUserRelationshipLookupEntity
    {
        /// <summary>
        /// Gets or sets relationship handle
        /// </summary>
        public string RelationshipHandle { get; set; }

        /// <summary>
        /// Gets or sets relationship status
        /// </summary>
        public UserRelationshipStatus UserRelationshipStatus { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        public DateTime LastUpdatedTime { get; set; }
    }
}
