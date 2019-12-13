// <copyright file="TopicRelationshipFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// Topic relationship feed entity class
    /// </summary>
    public class TopicRelationshipFeedEntity : FeedEntity, ITopicRelationshipFeedEntity
    {
        /// <summary>
        /// Gets or sets relationship handle
        /// </summary>
        public string RelationshipHandle { get; set; }

        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        public string TopicHandle { get; set; }
    }
}
