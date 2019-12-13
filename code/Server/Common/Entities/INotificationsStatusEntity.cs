// <copyright file="INotificationsStatusEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// Notification status entity interface
    /// </summary>
    public interface INotificationsStatusEntity
    {
        /// <summary>
        /// Gets or sets read activity handle
        /// </summary>
        string ReadActivityHandle { get; set; }
    }
}
