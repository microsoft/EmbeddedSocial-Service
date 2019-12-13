// <copyright file="UserRankFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// User rank feed entity class
    /// </summary>
    public class UserRankFeedEntity : RankFeedEntity, IUserFeedEntity
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
