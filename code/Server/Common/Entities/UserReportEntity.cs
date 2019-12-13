// <copyright file="UserReportEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// User report entity
    /// </summary>
    public class UserReportEntity : ObjectEntity, IUserReportEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user being reported on
        /// </summary>
        public string ReportedUserHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user that is doing the reporting
        /// </summary>
        public string ReportingUserHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the app that both the users are in
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets the complaint that the reporting user has about the reported user
        /// </summary>
        public ReportReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the time at which the report was received from the reporting user
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }
}
