// <copyright file="IReplyEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// Reply entity interface
    /// </summary>
    public interface IReplyEntity
    {
        /// <summary>
        /// Gets or sets comment handle
        /// </summary>
        string CommentHandle { get; set; }

        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        string TopicHandle { get; set; }

        /// <summary>
        /// Gets or sets created time
        /// </summary>
        DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        DateTime LastUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets owner user handle
        /// </summary>
        string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets comment text
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Gets or sets comment language
        /// </summary>
        string Language { get; set; }

        /// <summary>
        /// Gets or sets review status
        /// </summary>
        ReviewStatus ReviewStatus { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets request id associated with the create comment request
        /// </summary>
        string RequestId { get; set; }
    }
}
