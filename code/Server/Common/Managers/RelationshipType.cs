// <copyright file="RelationshipType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    /// <summary>
    /// Relationship type
    /// </summary>
    public enum RelationshipType
    {
        /// <summary>
        /// Follower user : someone who follows me
        /// </summary>
        Follower,

        /// <summary>
        /// Following user : someone who i am following
        /// </summary>
        Following,

        /// <summary>
        /// Pending user
        /// </summary>
        PendingUser,

        /// <summary>
        /// Blocked user
        /// </summary>
        BlockedUser
    }
}
