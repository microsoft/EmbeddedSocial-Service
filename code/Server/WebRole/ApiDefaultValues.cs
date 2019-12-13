// <copyright file="ApiDefaultValues.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server
{
    using SocialPlus.Server.WebRoleCommon;

    /// <summary>
    /// Default values
    /// </summary>
    public static class ApiDefaultValues
    {
        /// <summary>
        /// Default limit for get popular user feed
        /// </summary>
        public const int GetPopularUsersPageLimit = CommonApiDefaultValues.FeedPageLimit;

        /// <summary>
        /// Default limit for get topic feed
        /// </summary>
        public const int GetTopicsPageLimit = CommonApiDefaultValues.FeedPageLimit;

        /// <summary>
        /// Default limit for get comment feed
        /// </summary>
        public const int GetCommentsPageLimit = CommonApiDefaultValues.FeedPageLimit / 2;

        /// <summary>
        /// Default limit for get reply feed
        /// </summary>
        public const int GetRepliesPageLimit = CommonApiDefaultValues.FeedPageLimit / 2;

        /// <summary>
        /// Default limit for get like feed
        /// </summary>
        public const int GetLikesPageLimit = CommonApiDefaultValues.FeedPageLimit;

        /// <summary>
        /// Default limit for get pin feed
        /// </summary>
        public const int GetPinsPageLimit = CommonApiDefaultValues.FeedPageLimit;

        /// <summary>
        /// Default limit for get user feed
        /// </summary>
        public const int GetUsersPageLimit = CommonApiDefaultValues.FeedPageLimit;

        /// <summary>
        /// Default limit for get activity feed
        /// </summary>
        public const int GetActivitiesPageLimit = CommonApiDefaultValues.FeedPageLimit;

        /// <summary>
        /// Default limit for feed limits that have not been implemented.
        /// </summary>
        public const int NotImplemented = CommonApiDefaultValues.NotImplemented;
    }
}
