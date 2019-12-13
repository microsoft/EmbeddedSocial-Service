// <copyright file="ITopicRelationshipFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// Topic relationship feed entity interface
    /// </summary>
    public interface ITopicRelationshipFeedEntity
    {
        /// <summary>
        /// Gets or sets relationship handle
        /// </summary>
        string RelationshipHandle { get; set; }

        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        string TopicHandle { get; set; }
    }
}
