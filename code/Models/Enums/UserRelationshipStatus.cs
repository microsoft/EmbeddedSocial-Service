// <copyright file="UserRelationshipStatus.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    /// <summary>
    /// User relationship status
    /// </summary>
    public enum UserRelationshipStatus
    {
        /// <summary>
        /// User has no relationship
        /// </summary>
        None,

        /// <summary>
        /// User has follow relationship
        /// </summary>
        Follow,

        /// <summary>
        /// User relationship is in pending status
        /// </summary>
        Pending,

        /// <summary>
        /// User has been blocked from a relationship
        /// </summary>
        Blocked
    }
}
