// <copyright file="UserVisibilityStatus.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    /// <summary>
    /// User visibility status
    /// </summary>
    public enum UserVisibilityStatus
    {
        /// <summary>
        /// Topics posted by public users are visible to everyone and are indexed by public feeds such as search and popular feed
        /// </summary>
        Public,

        /// <summary>
        /// Topics posted by private users are visible only to followers.
        /// While public users can be followed automatically, private users need to accept follower requests
        /// </summary>
        Private
    }
}
