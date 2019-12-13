// <copyright file="ICommentEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// Comment entity interface
    /// </summary>
    public interface ICommentEntity
    {
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
        /// Gets or sets comment blob type
        /// </summary>
        BlobType BlobType { get; set; }

        /// <summary>
        /// Gets or sets comment blob handle
        /// </summary>
        string BlobHandle { get; set; }

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
