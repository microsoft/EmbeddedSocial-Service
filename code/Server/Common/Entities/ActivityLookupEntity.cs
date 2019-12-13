// <copyright file="ActivityLookupEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;

    /// <summary>
    /// Activity lookup entity class
    /// </summary>
    public class ActivityLookupEntity : ObjectEntity, IActivityLookupEntity
    {
        /// <summary>
        /// Gets or sets activity handle
        /// </summary>
        public string ActivityHandle { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        public DateTime LastUpdatedTime { get; set; }
    }
}
