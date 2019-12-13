// <copyright file="AVERTTransactionEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// AVERT transaction entity
    /// </summary>
    public class AVERTTransactionEntity : ObjectEntity, IAVERTTransactionEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the report that this transaction is for
        /// </summary>
        public string ReportHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the app that this transaction is for
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the complaint is against a user or content
        /// </summary>
        public ReportType ReportType { get; set; }

        /// <summary>
        /// Gets or sets the time at which the request was submitted to AVERT
        /// </summary>
        public DateTime RequestTime { get; set; }

        /// <summary>
        /// Gets or sets the body of the request to AVERT
        /// </summary>
        public string RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the time at which the response was received from AVERT
        /// </summary>
        public DateTime ResponseTime { get; set; }

        /// <summary>
        /// Gets or sets the body of the response from AVERT
        /// </summary>
        public string ResponseBody { get; set; }
    }
}