// <copyright file="ActivityType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    /// <summary>
    /// Activity type
    /// </summary>
    public enum ActivityType
    {
        /// <summary>
        /// Like activity
        /// </summary>
        Like,

        /// <summary>
        /// Comment activity
        /// </summary>
        Comment,

        /// <summary>
        /// Reply activity
        /// </summary>
        Reply,

        /// <summary>
        /// Comment peer activity
        /// </summary>
        CommentPeer,

        /// <summary>
        /// Reply peer activity
        /// </summary>
        ReplyPeer,

        /// <summary>
        /// Following activity
        /// </summary>
        Following,

        /// <summary>
        /// Follow request activity
        /// </summary>
        FollowRequest,

        /// <summary>
        /// Follow accept activity
        /// </summary>
        FollowAccept,
    }
}
