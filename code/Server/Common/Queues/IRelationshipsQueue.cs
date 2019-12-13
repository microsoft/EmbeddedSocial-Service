// <copyright file="IRelationshipsQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System;
    using System.Threading.Tasks;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// Relationships queue interface
    /// </summary>
    public interface IRelationshipsQueue : IQueueBase
    {
        /// <summary>
        /// Send relationship message
        /// </summary>
        /// <param name="relationshipOperation">User relationship operation</param>
        /// <param name="relationshipHandle">Relationship handle</param>
        /// <param name="followerKeyUserHandle">Follower key user handle</param>
        /// <param name="followingKeyUserHandle">Following key user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <returns>Send message task</returns>
        Task SendRelationshipMessage(
            RelationshipOperation relationshipOperation,
            string relationshipHandle,
            string followerKeyUserHandle,
            string followingKeyUserHandle,
            string appHandle,
            DateTime lastUpdatedTime);
    }
}
