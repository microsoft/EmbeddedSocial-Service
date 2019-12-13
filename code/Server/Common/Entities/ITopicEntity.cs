// <copyright file="ITopicEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// Topic entity interface
    /// </summary>
    public interface ITopicEntity
    {
        /// <summary>
        /// Gets or sets created time
        /// </summary>
        DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        DateTime LastUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets publisher type
        /// </summary>
        PublisherType PublisherType { get; set; }

        /// <summary>
        /// Gets or sets owner user handle
        /// </summary>
        string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets topic title
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets topic text
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Gets or sets topic blob type
        /// </summary>
        BlobType BlobType { get; set; }

        /// <summary>
        /// Gets or sets topic blob handle
        /// </summary>
        string BlobHandle { get; set; }

        /// <summary>
        /// Gets or sets topic categories
        /// </summary>
        string Categories { get; set; }

        /// <summary>
        /// Gets or sets topic language
        /// </summary>
        string Language { get; set; }

        /// <summary>
        /// Gets or sets topic group
        /// </summary>
        string Group { get; set; }

        /// <summary>
        /// Gets or sets topic deep link
        /// </summary>
        string DeepLink { get; set; }

        /// <summary>
        /// Gets or sets topic friendly name
        /// </summary>
        string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets review status
        /// </summary>
        ReviewStatus ReviewStatus { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets request id associated with the create topic request
        /// </summary>
        string RequestId { get; set; }
    }
}
