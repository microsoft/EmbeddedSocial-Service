// <copyright file="IPushNotificationsConfigurationEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// Push notifications configuration entity interface
    /// </summary>
    public interface IPushNotificationsConfigurationEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether push notifications is enabled
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets push notifications path
        /// Windows: Windows Package SID
        /// Android: Leave this empty
        /// iOS: iOS certificate path
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// Gets or sets push notifications key
        /// Windows: Windows secret key
        /// Android: Android API key
        /// iOS: iOS Certificate key
        /// </summary>
        string Key { get; set; }
    }
}
