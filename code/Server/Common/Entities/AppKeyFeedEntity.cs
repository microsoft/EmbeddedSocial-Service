// <copyright file="AppKeyFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;

    /// <summary>
    /// App key feed entity class
    /// </summary>
    public class AppKeyFeedEntity : FeedEntity, IAppKeyFeedEntity
    {
        /// <summary>
        /// Gets or sets app key
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// Gets or sets created time
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }
}
