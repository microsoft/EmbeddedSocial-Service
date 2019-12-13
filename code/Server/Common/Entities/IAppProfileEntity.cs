// <copyright file="IAppProfileEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// App profile entity interface
    /// </summary>
    public interface IAppProfileEntity
    {
        /// <summary>
        /// Gets or sets developer id
        /// </summary>
        string DeveloperId { get; set; }

        /// <summary>
        /// Gets or sets app name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets app icon handle
        /// </summary>
        string IconHandle { get; set; }

        /// <summary>
        /// Gets or sets platform type
        /// </summary>
        PlatformType PlatformType { get; set; }

        /// <summary>
        /// Gets or sets app deep link
        /// </summary>
        string DeepLink { get; set; }

        /// <summary>
        /// Gets or sets app store link
        /// </summary>
        string StoreLink { get; set; }

        /// <summary>
        /// Gets or sets created time
        /// </summary>
        DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        DateTime LastUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets app status
        /// </summary>
        AppStatus AppStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether validating app-provided handles should be disabled
        /// </summary>
        bool DisableHandleValidation { get; set; }
    }
}
