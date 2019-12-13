// <copyright file="IActivityLookupEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    /// <summary>
    /// Activity lookup entity interface
    /// </summary>
    public interface IActivityLookupEntity
    {
        /// <summary>
        /// Gets or sets activity handle
        /// </summary>
        string ActivityHandle { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        DateTime LastUpdatedTime { get; set; }
    }
}
