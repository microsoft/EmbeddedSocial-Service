// <copyright file="PinFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// Pin feed entity class
    /// </summary>
    public class PinFeedEntity : FeedEntity, IPinFeedEntity
    {
        /// <summary>
        /// Gets or sets pin handle
        /// </summary>
        public string PinHandle { get; set; }

        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        public string TopicHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle of the owner of the topic
        /// </summary>
        public string TopicUserHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle of the topic
        /// </summary>
        public string AppHandle { get; set; }
    }
}
