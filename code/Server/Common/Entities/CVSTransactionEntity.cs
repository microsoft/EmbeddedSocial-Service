// <copyright file="CVSTransactionEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// CVS transaction entity
    /// </summary>
    public class CVSTransactionEntity : ObjectEntity, ICVSTransactionEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the content moderation transaction
        /// </summary>
        public string ModerationHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the app that this transaction is for
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the moderation is for a user or content
        /// </summary>
        public ReportType ReportType { get; set; }

        /// <summary>
        /// Gets or sets the time at which the moderation request was posted to CVS
        /// </summary>
        public DateTime RequestTime { get; set; }

        /// <summary>
        /// Gets or sets the body of the request to CVS
        /// </summary>
        public string RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the job id for the request assigned by CVS
        /// </summary>
        public string JobId { get; set; }

        /// <summary>
        /// Gets or sets the url for the CVS callback
        /// </summary>
        public string CallbackUrl { get; set; }

        /// <summary>
        /// Gets or sets the time at which the callback response was received from CVS
        /// </summary>
        public DateTime CallbackTime { get; set; }

        /// <summary>
        /// Gets or sets the callback response body
        /// </summary>
        public string CallbackResponseBody { get; set; }
    }
}
