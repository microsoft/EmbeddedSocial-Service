// <copyright file="ITopicRelationshipLookupEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;
    using SocialPlus.Models;

    /// <summary>
    /// Topic relationship lookup entity interface
    /// </summary>
    public interface ITopicRelationshipLookupEntity
    {
        /// <summary>
        /// Gets or sets relationship handle
        /// </summary>
        string RelationshipHandle { get; set; }

        /// <summary>
        /// Gets or sets topic relationship status
        /// </summary>
        TopicRelationshipStatus TopicRelationshipStatus { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        DateTime LastUpdatedTime { get; set; }
    }
}
