// <copyright file="IPinLookupEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    /// <summary>
    /// Pin lookup entity interface
    /// </summary>
    public interface IPinLookupEntity
    {
        /// <summary>
        /// Gets or sets pin handle
        /// </summary>
        string PinHandle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content is pinned by the user
        /// </summary>
        bool Pinned { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        DateTime LastUpdatedTime { get; set; }
    }
}
