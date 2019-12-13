// <copyright file="ICVSTransactionEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// Represents the transaction state of content moderation
    /// </summary>
    public interface ICVSTransactionEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the content moderation transaction
        /// </summary>
        string ModerationHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the app that this transaction is for
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the moderation is for a user, content, or image
        /// </summary>
        ReportType ReportType { get; set; }

        /// <summary>
        /// Gets or sets the time at which the moderation request was posted to CVS
        /// </summary>
        DateTime RequestTime { get; set; }

        /// <summary>
        /// Gets or sets the body of the request to CVS
        /// </summary>
        string RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the job id for the request assigned by CVS
        /// </summary>
        string JobId { get; set; }

        /// <summary>
        /// Gets or sets the url for the CVS callback
        /// </summary>
        string CallbackUrl { get; set; }

        /// <summary>
        /// Gets or sets the time at which the callback response was received from CVS
        /// </summary>
        DateTime CallbackTime { get; set; }

        /// <summary>
        /// Gets or sets the callback response body
        /// </summary>
        string CallbackResponseBody { get; set; }
    }
}
