// <copyright file="IAppAdminEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// App admin entity
    /// </summary>
    public interface IAppAdminEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user is an app administrator
        /// </summary>
        bool IsAdmin { get; set; }
    }
}
