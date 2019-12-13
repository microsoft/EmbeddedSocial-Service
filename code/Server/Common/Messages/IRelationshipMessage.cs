// <copyright file="IRelationshipMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// Relationship message interface
    /// </summary>
    public interface IRelationshipMessage : IMessage
    {
        /// <summary>
        /// Gets or sets relationship operation
        /// </summary>
        RelationshipOperation RelationshipOperation { get; set; }

        /// <summary>
        /// Gets or sets relationship handle
        /// </summary>
        string RelationshipHandle { get; set; }

        /// <summary>
        /// Gets or sets follower key user handle (Key for the follower table)
        /// </summary>
        string FollowerKeyUserHandle { get; set; }

        /// <summary>
        /// Gets or sets following key user handle (Key for the following table)
        /// </summary>
        string FollowingKeyUserHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        DateTime LastUpdatedTime { get; set; }
    }
}
