// <copyright file="IUserFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// User feed entity interface
    /// </summary>
    public interface IUserFeedEntity
    {
        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        string AppHandle { get; set; }
    }
}
