// <copyright file="ContentReportFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// Content report feed entity interface
    /// </summary>
    public class ContentReportFeedEntity : FeedEntity, IContentReportFeedEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the content report
        /// </summary>
        public string ReportHandle { get; set; }

        /// <summary>
        /// Gets or sets the type of content being reported on
        /// </summary>
        public ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the content being reported on
        /// </summary>
        public string ContentHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user that created the content
        /// </summary>
        public string ContentUserHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user that is doing the reporting
        /// </summary>
        public string ReportingUserHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the app that the content came from
        /// </summary>
        public string AppHandle { get; set; }
    }
}
