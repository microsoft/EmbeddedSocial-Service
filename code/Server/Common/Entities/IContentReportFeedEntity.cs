// <copyright file="IContentReportFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// Content report feed entity interface
    /// </summary>
    public interface IContentReportFeedEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the content report
        /// </summary>
        string ReportHandle { get; set; }

        /// <summary>
        /// Gets or sets the type of content being reported on
        /// </summary>
        ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the content being reported on
        /// </summary>
        string ContentHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user that created the content
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
    }
}