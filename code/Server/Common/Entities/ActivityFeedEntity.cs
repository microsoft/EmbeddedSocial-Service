// <copyright file="ActivityFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// Activity feed entity class
    /// </summary>
    public class ActivityFeedEntity : FeedEntity, IActivityFeedEntity
    {
        /// <summary>
        /// Gets or sets activity handle
        /// </summary>
        public string ActivityHandle { get; set; }

        /// <summary>
        /// Gets or sets activity type
        /// </summary>
        public ActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets or sets created time
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets actor user handle
        /// </summary>
        public string ActorUserHandle { get; set; }

        /// <summary>
        /// Gets or sets acted on user handle
        /// </summary>
        public string ActedOnUserHandle { get; set; }

        /// <summary>
        /// Gets or sets acted on content type
        /// </summary>
        public ContentType ActedOnContentType { get; set; }

        /// <summary>
        /// Gets or sets acted on content handle
        /// </summary>
        public string ActedOnContentHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        public string AppHandle { get; set; }
    }
}
