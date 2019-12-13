// <copyright file="AppFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// App feed entity class
    /// </summary>
    public class AppFeedEntity : FeedEntity, IAppFeedEntity
    {
        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        public string AppHandle { get; set; }
    }
}
