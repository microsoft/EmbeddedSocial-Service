// <copyright file="RelationshipMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Relationship message class
    /// </summary>
    public class RelationshipMessage : QueueMessage, IRelationshipMessage
    {
        /// <summary>
        /// Gets or sets relationship operation
        /// </summary>
        public RelationshipOperation RelationshipOperation { get; set; }

        /// <summary>
        /// Gets or sets relationship handle
        /// </summary>
        public string RelationshipHandle { get; set; }

        /// <summary>
        /// Gets or sets follower key user handle (Key for the follower table)
        /// </summary>
        public string FollowerKeyUserHandle { get; set; }

        /// <summary>
        /// Gets or sets following key user handle (Key for the following table)
        /// </summary>
        public string FollowingKeyUserHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        public DateTime LastUpdatedTime { get; set; }
    }
}
