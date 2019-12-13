// <copyright file="PostReportRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to post (create) a report for content
    /// </summary>
    public class PostReportRequest
    {
        /// <summary>
        /// Gets or sets report reason
        /// </summary>
        [Required]
        public ReportReason Reason { get; set; }
    }
}
