// <copyright file="ITopicFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// Topic feed entity interface
    /// </summary>
    public interface ITopicFeedEntity
    {
        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        string TopicHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle of the owner of the topic
        /// </summary>
        string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle of app the topic belongs to
        /// </summary>
        string AppHandle { get; set; }
    }
}
