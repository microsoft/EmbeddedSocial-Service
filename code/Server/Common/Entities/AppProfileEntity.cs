// <copyright file="AppProfileEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// App profile entity class
    /// </summary>
    public class AppProfileEntity : ObjectEntity, IAppProfileEntity
    {
        /// <summary>
        /// Gets or sets developer id
        /// </summary>
        public string DeveloperId { get; set; }

        /// <summary>
        /// Gets or sets app name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets app icon handle
        /// </summary>
        public string IconHandle { get; set; }

        /// <summary>
        /// Gets or sets platform type
        /// </summary>
        public PlatformType PlatformType { get; set; }

        /// <summary>
        /// Gets or sets app deep link
        /// </summary>
        public string DeepLink { get; set; }

        /// <summary>
        /// Gets or sets app store link
        /// </summary>
        public string StoreLink { get; set; }

        /// <summary>
        /// Gets or sets created time
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        public DateTime LastUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets app status
        /// </summary>
        public AppStatus AppStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether validating app-provided handles should be disabled
        /// </summary>
        public bool DisableHandleValidation { get; set; }
    }
}
