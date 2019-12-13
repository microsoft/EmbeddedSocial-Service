// <copyright file="NotificationsStatusEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// Notification status entity class
    /// </summary>
    public class NotificationsStatusEntity : ObjectEntity, INotificationsStatusEntity
    {
        /// <summary>
        /// Gets or sets read activity handle
        /// </summary>
        public string ReadActivityHandle { get; set; }
    }
}
