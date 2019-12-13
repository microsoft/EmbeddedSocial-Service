// <copyright file="IUserReportFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// User report feed entity interface
    /// </summary>
    public interface IUserReportFeedEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user report
        /// </summary>
        string ReportHandle { get; set; }

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
    }
}