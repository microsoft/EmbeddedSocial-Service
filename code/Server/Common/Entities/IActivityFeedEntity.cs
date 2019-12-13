// <copyright file="IActivityFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// Activity feed entity interface
    /// </summary>
    public interface IActivityFeedEntity
    {
        /// <summary>
        /// Gets or sets activity handle
        /// </summary>
        string ActivityHandle { get; set; }

        /// <summary>
        /// Gets or sets activity type
        /// </summary>
        ActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets or sets created time
        /// </summary>
        DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets actor user handle
        /// </summary>
        string ActorUserHandle { get; set; }

        /// <summary>
        /// Gets or sets acted on user handle
        /// </summary>
        string ActedOnUserHandle { get; set; }

        /// <summary>
        /// Gets or sets acted on content type
        /// </summary>
        ContentType ActedOnContentType { get; set; }

        /// <summary>
        /// Gets or sets acted on content handle
        /// </summary>
        string ActedOnContentHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        string AppHandle { get; set; }
    }
}
