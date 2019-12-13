// <copyright file="UserRelationshipFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// User relationship feed entity class
    /// </summary>
    public class UserRelationshipFeedEntity : FeedEntity, IUserRelationshipFeedEntity
    {
        /// <summary>
        /// Gets or sets relationship handle
        /// </summary>
        public string RelationshipHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        public string UserHandle { get; set; }
    }
}
