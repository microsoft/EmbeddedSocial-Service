// <copyright file="IPushRegistrationFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// Push registration feed entity interface
    /// </summary>
    public interface IPushRegistrationFeedEntity
    {
        /// <summary>
        /// Gets or sets platform type
        /// </summary>
        PlatformType PlatformType { get; set; }

        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets OS registration id
        /// </summary>
        string OSRegistrationId { get; set; }

        /// <summary>
        /// Gets or sets push notifications registration id from the Azure Notification Hub
        /// </summary>
        string HubRegistrationId { get; set; }

        /// <summary>
        /// Gets or sets language of the client
        /// </summary>
        string Language { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        DateTime LastUpdatedTime { get; set; }
    }
}
