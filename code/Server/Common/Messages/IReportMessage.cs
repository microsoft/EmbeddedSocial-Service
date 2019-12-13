// <copyright file="IReportMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// Report message interface
    /// </summary>
    public interface IReportMessage : IMessage
    {
        /// <summary>
        /// Gets or sets a value indicating whether the complaint is against a user or content
        /// </summary>
        ReportType ReportType { get; set; }

        /// <summary>
        /// Gets or sets the app handle that the complaint was made in
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets the report handle
        /// </summary>
        string ReportHandle { get; set; }

        /// <summary>
        /// Gets or sets the URI that can be used to post the result of a review
        /// </summary>
        Uri CallbackUri { get; set; }
    }
}