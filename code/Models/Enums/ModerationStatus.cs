// <copyright file="ModerationStatus.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    /// <summary>
    /// Content moderation status.
    /// Used to record the status of interactions between Social Plus and the Content Validation Service (CVS).
    /// </summary>
    public enum ModerationStatus
    {
        /// <summary>
        /// Moderation is pending
        /// </summary>
        Pending,

        /// <summary>
        /// Moderation is complete
        /// </summary>
        Completed,

        /// <summary>
        /// Moderation failed
        /// </summary>
        Failed
    }
}
