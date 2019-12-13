// <copyright file="ReportType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    /// <summary>
    /// Report type
    /// </summary>
    /// <remarks>
    /// Used by AVERT and CVS
    /// </remarks>
    public enum ReportType
    {
        /// <summary>
        /// Report on a user
        /// </summary>
        User,

        /// <summary>
        /// Report on content
        /// </summary>
        Content,

        /// <summary>
        /// Report on an image
        /// </summary>
        Image
    }
}
