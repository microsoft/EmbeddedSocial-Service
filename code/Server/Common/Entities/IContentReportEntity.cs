// <copyright file="IContentReportEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// Content report entity interface
    /// </summary>
    public interface IContentReportEntity
    {
        /// <summary>
        /// Gets or sets the type of content being reported on
        /// </summary>
        ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the content being reported on
        /// </summary>
        string ContentHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user that created the content.
        /// This can be null if a user did not create the content.
        /// </summary>
        string ContentUserHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user that is doing the reporting
        /// </summary>
        string ReportingUserHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the app that the content came from
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets the complaint that the user has about this content
        /// </summary>
        ReportReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the time at which the report was received from the user
        /// </summary>
        DateTime CreatedTime { get; set; }
    }
}
