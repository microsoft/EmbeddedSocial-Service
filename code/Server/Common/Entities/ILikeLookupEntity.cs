// <copyright file="ILikeLookupEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    /// <summary>
    /// Like lookup entity interface
    /// </summary>
    public interface ILikeLookupEntity
    {
        /// <summary>
        /// Gets or sets like handle
        /// </summary>
        string LikeHandle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content is liked by the user
        /// </summary>
        bool Liked { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        DateTime LastUpdatedTime { get; set; }
    }
}
