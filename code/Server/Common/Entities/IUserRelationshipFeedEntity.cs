// <copyright file="IUserRelationshipFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// User relationship feed entity interface
    /// </summary>
    public interface IUserRelationshipFeedEntity
    {
        /// <summary>
        /// Gets or sets relationship handle
        /// </summary>
        string RelationshipHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        string UserHandle { get; set; }
    }
}
