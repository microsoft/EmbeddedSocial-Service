// <copyright file="IUserReportEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// User report entity interface
    /// </summary>
    public interface IUserReportEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user being reported on
        /// </summary>
        string ReportedUserHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user that is doing the reporting
        /// </summary>
        string ReportingUserHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the app that both the users are in
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets the complaint that the reporting user has about the reported user
        /// </summary>
        ReportReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the time at which the report was received from the reporting user
        /// </summary>
        DateTime CreatedTime { get; set; }
    }
}