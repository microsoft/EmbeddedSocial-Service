// <copyright file="ReviewStatus.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    /// <summary>
    /// Review status for user profile or content
    /// </summary>
    /// <remarks>
    /// Used by AVERT and CVS
    /// </remarks>
    public enum ReviewStatus
    {
        /// <summary>
        /// User profile or content has not yet been classified as banned/mature/clean
        /// </summary>
        Active,

        /// <summary>
        /// User profile or content is banned
        /// </summary>
        Banned,

        /// <summary>
        /// User profile or content is for mature audiences
        /// </summary>
        Mature,

        /// <summary>
        /// User profile or content is appropriate for all ages
        /// </summary>
        Clean
    }
}
