// <copyright file="PinLookupEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;

    /// <summary>
    /// Pin lookup entity class
    /// </summary>
    public class PinLookupEntity : ObjectEntity, IPinLookupEntity
    {
        /// <summary>
        /// Gets or sets pin handle
        /// </summary>
        public string PinHandle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content is pinned by the user
        /// </summary>
        public bool Pinned { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        public DateTime LastUpdatedTime { get; set; }
    }
}
