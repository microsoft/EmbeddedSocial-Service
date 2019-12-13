// <copyright file="RelationshipsWorker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Workers
{
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Queues;

    /// <summary>
    /// Relationships worker
    /// </summary>
    public class RelationshipsWorker : QueueWorker
    {
        /// <summary>
        /// Relationships manager
        /// </summary>
        private IRelationshipsManager relationshipsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipsWorker"/> class
        /// </summary>
        /// <param name="log">log</param>
        /// <param name="relationshipsQueue">Relationships queue</param>
        /// <param name="relationshipsManager">Relationships manager</param>
        public RelationshipsWorker(ILog log, IRelationshipsQueue relationshipsQueue, IRelationshipsManager relationshipsManager)
            : base(log)
        {
            this.Queue = relationshipsQueue;
            this.relationshipsManager = relationshipsManager;
        }

        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Process message task</returns>
        protected override async Task Process(IMessage message)
        {
            if (message is RelationshipMessage)
            {
                RelationshipMessage relationshipMessage = message as RelationshipMessage;
                ProcessType processType = ProcessType.Backend;
                if (relationshipMessage.DequeueCount > 1)
                {
                    processType = ProcessType.BackendRetry;
                }

                IUserRelationshipLookupEntity followerRelationshipLookupEntity = await this.relationshipsManager.ReadFollowerRelationship(
                    relationshipMessage.FollowerKeyUserHandle,
                    relationshipMessage.FollowingKeyUserHandle,
                    relationshipMessage.AppHandle);
                if (followerRelationshipLookupEntity != null && followerRelationshipLookupEntity.LastUpdatedTime > relationshipMessage.LastUpdatedTime)
                {
                    return;
                }

                IUserRelationshipLookupEntity followingRelationshipLookupEntity = await this.relationshipsManager.ReadFollowingRelationshipToUser(
                    relationshipMessage.FollowingKeyUserHandle,
                    relationshipMessage.FollowerKeyUserHandle,
                    relationshipMessage.AppHandle);
                if (followingRelationshipLookupEntity != null && followingRelationshipLookupEntity.LastUpdatedTime > relationshipMessage.LastUpdatedTime)
                {
                    return;
                }

                await this.relationshipsManager.UpdateRelationshipToUser(
                    processType,
                    relationshipMessage.RelationshipOperation,
                    relationshipMessage.RelationshipHandle,
                    relationshipMessage.FollowerKeyUserHandle,
                    relationshipMessage.FollowingKeyUserHandle,
                    relationshipMessage.AppHandle,
                    relationshipMessage.LastUpdatedTime,
                    followerRelationshipLookupEntity,
                    followingRelationshipLookupEntity);
            }
        }
    }
}
