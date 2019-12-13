// <copyright file="RelationshipOperation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    /// <summary>
    /// User relationship operation
    /// </summary>
    public enum RelationshipOperation
    {
        /// <summary>
        /// Follow user
        /// </summary>
        FollowUser,

        /// <summary>
        /// Add to pending requests
        /// </summary>
        PendingUser,

        /// <summary>
        /// Unfollow user
        /// </summary>
        UnfollowUser,

        /// <summary>
        /// Block user
        /// </summary>
        BlockUser,

        /// <summary>
        /// Unblock user
        /// </summary>
        UnblockUser,

        /// <summary>
        /// Accept user
        /// </summary>
        AcceptUser,

        /// <summary>
        /// Reject user
        /// </summary>
        RejectUser,

        /// <summary>
        /// Remove follower
        /// </summary>
        DeleteFollower,

        /// <summary>
        /// Follow a topic
        /// </summary>
        FollowTopic,

        /// <summary>
        /// Unfollow a topic
        /// </summary>
        UnfollowTopic
    }
}
