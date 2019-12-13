// <copyright file="PushNotificationsConfigurationEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// Push notifications configuration entity class
    /// </summary>
    public class PushNotificationsConfigurationEntity : ObjectEntity, IPushNotificationsConfigurationEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether push notifications is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets push notifications path
        /// Windows: Windows Package SID
        /// Android: Leave this empty
        /// iOS: iOS certificate path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets push notifications key
        /// Windows: Windows secret key
        /// Android: Android API key
        /// iOS: iOS Certificate key
        /// </summary>
        public string Key { get; set; }
    }
}
