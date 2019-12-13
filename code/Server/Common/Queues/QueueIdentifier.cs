// <copyright file="QueueIdentifier.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    /// <summary>
    /// Queue identifiers
    /// </summary>
    public enum QueueIdentifier
    {
        /// <summary>
        /// <c>Fanout</c> activities queue
        /// </summary>
        FanoutActivities,

        /// <summary>
        /// <c>Fanout</c> topics queue
        /// </summary>
        FanoutTopics,

        /// <summary>
        /// Following imports queue
        /// </summary>
        FollowingImports,

        /// <summary>
        /// Likes queue
        /// </summary>
        Likes,

        /// <summary>
        /// moderation queue
        /// </summary>
        Moderation,

        /// <summary>
        /// Relationships queue
        /// </summary>
        Relationships,

        /// <summary>
        /// Reports queue
        /// </summary>
        Reports,

        /// <summary>
        /// Resize images queue
        /// </summary>
        ResizeImages,

        /// <summary>
        /// Search queue
        /// </summary>
        Search
    }
}
