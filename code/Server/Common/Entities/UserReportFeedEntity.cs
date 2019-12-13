// <copyright file="UserReportFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// User report feed entity interface
    /// </summary>
    public class UserReportFeedEntity : FeedEntity, IUserReportFeedEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user report
        /// </summary>
        public string ReportHandle { get; set; }

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
    }
}
