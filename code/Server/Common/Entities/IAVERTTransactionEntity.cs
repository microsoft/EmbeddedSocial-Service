// <copyright file="IAVERTTransactionEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// AVERT transaction entity interface
    /// </summary>
    public interface IAVERTTransactionEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the report that this transaction is for
        /// </summary>
        string ReportHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the app that this transaction is for
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the complaint is against a user or content
        /// </summary>
        ReportType ReportType { get; set; }

        /// <summary>
        /// Gets or sets the time at which the request was submitted to AVERT
        /// </summary>
        DateTime RequestTime { get; set; }

        /// <summary>
        /// Gets or sets the body of the request to AVERT
        /// </summary>
        string RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the time at which the response was received from AVERT
        /// </summary>
        DateTime ResponseTime { get; set; }

        /// <summary>
        /// Gets or sets the body of the response from AVERT
        /// </summary>
        string ResponseBody { get; set; }
    }
}