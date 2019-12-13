// <copyright file="TopicRelationshipLookupEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// Topic relationship lookup entity class
    /// </summary>
    public class TopicRelationshipLookupEntity : ObjectEntity, ITopicRelationshipLookupEntity
    {
        /// <summary>
        /// Gets or sets relationship handle
        /// </summary>
        public string RelationshipHandle { get; set; }

        /// <summary>
        /// Gets or sets topic relationship status
        /// </summary>
        public TopicRelationshipStatus TopicRelationshipStatus { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        public DateTime LastUpdatedTime { get; set; }
    }
}
