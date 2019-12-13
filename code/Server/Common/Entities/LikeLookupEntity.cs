// <copyright file="LikeLookupEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;

    /// <summary>
    /// Like lookup entity class
    /// </summary>
    public class LikeLookupEntity : ObjectEntity, ILikeLookupEntity
    {
        /// <summary>
        /// Gets or sets like handle
        /// </summary>
        public string LikeHandle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content is liked by the user
        /// </summary>
        public bool Liked { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        public DateTime LastUpdatedTime { get; set; }
    }
}
