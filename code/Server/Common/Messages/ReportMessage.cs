// <copyright file="ReportMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;

    using SocialPlus.Models;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Report queue message
    /// </summary>
    public class ReportMessage : QueueMessage, IReportMessage
    {
        /// <summary>
        /// Gets or sets a value indicating whether the complaint is against a user or content
        /// </summary>
        public ReportType ReportType { get; set; }

        /// <summary>
        /// Gets or sets the app handle that the complaint was made in
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets the report handle
        /// </summary>
        public string ReportHandle { get; set; }

        /// <summary>
        /// Gets or sets the URI that can be used to post the result of a review
        /// </summary>
        public Uri CallbackUri { get; set; }
    }
}
