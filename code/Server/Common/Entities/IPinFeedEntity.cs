// <copyright file="IPinFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// Pin feed entity interface
    /// </summary>
    public interface IPinFeedEntity
    {
        /// <summary>
        /// Gets or sets pin handle
        /// </summary>
        string PinHandle { get; set; }

        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        string TopicHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle of the owner of the topic
        /// </summary>
        string TopicUserHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle of the topic
        /// </summary>
        string AppHandle { get; set; }
    }
}
