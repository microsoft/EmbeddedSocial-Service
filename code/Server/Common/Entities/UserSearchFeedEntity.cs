// <copyright file="UserSearchFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// User search feed entity class
    /// </summary>
    public class UserSearchFeedEntity : IUserFeedEntity
    {
        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        public string AppHandle { get; set; }
    }
}
