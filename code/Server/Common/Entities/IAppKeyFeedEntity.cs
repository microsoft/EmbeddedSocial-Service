// <copyright file="IAppKeyFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    /// <summary>
    /// App key feed entity interface
    /// </summary>
    public interface IAppKeyFeedEntity
    {
        /// <summary>
        /// Gets or sets app key
        /// </summary>
        string AppKey { get; set; }

        /// <summary>
        /// Gets or sets created time
        /// </summary>
        DateTime CreatedTime { get; set; }
    }
}
