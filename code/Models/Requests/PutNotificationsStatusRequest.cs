// <copyright file="PutNotificationsStatusRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    /// <summary>
    /// Request to put (update) notifications status
    /// </summary>
    public class PutNotificationsStatusRequest
    {
        /// <summary>
        /// Gets or sets last read activity handle
        /// </summary>
        public string ReadActivityHandle { get; set; }
    }
}
