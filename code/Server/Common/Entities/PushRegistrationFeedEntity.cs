// <copyright file="PushRegistrationFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// Push registration feed entity class
    /// </summary>
    public class PushRegistrationFeedEntity : FeedEntity, IPushRegistrationFeedEntity
    {
        /// <summary>
        /// Gets or sets platform type
        /// </summary>
        public PlatformType PlatformType { get; set; }

        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets OS registration id
        /// </summary>
        public string OSRegistrationId { get; set; }

        /// <summary>
        /// Gets or sets push notifications registration id from the Azure Notification Hub
        /// </summary>
        public string HubRegistrationId { get; set; }

        /// <summary>
        /// Gets or sets language of the client
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        public DateTime LastUpdatedTime { get; set; }
    }
}
