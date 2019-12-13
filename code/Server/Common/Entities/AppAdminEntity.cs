// <copyright file="AppAdminEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// App admin entity
    /// </summary>
    public class AppAdminEntity : ObjectEntity, IAppAdminEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user is an app administrator
        /// </summary>
        public bool IsAdmin { get; set; }
    }
}
