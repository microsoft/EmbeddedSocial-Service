// <copyright file="RelationshipsQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Relationships queue class
    /// </summary>
    public class RelationshipsQueue : QueueBase, IRelationshipsQueue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipsQueue"/> class
        /// </summary>
        /// <param name="queueManager">queue manager</param>
        public RelationshipsQueue(IQueueManager queueManager)
            : base(queueManager)
        {
            this.QueueIdentifier = QueueIdentifier.Relationships;
        }

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
        public async Task SendRelationshipMessage(
            RelationshipOperation relationshipOperation,
            string relationshipHandle,
            string followerKeyUserHandle,
            string followingKeyUserHandle,
            string appHandle,
            DateTime lastUpdatedTime)
        {
            RelationshipMessage message = new RelationshipMessage()
            {
                RelationshipOperation = relationshipOperation,
                RelationshipHandle = relationshipHandle,
                FollowerKeyUserHandle = followerKeyUserHandle,
                FollowingKeyUserHandle = followingKeyUserHandle,
                AppHandle = appHandle,
                LastUpdatedTime = lastUpdatedTime
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }
    }
}
