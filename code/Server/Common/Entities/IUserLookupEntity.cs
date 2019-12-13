// <copyright file="IUserLookupEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// User lookup entity interface
    /// </summary>
    public interface IUserLookupEntity
    {
        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        string UserHandle { get; set; }
    }
}
