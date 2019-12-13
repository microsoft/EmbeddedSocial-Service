// <copyright file="IModerationEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// The content moderation entity interface
    /// </summary>
    public interface IModerationEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the image being moderated
        /// </summary>
        string ImageHandle { get; set; }

        /// <summary>
        /// Gets or sets the type of content (topic, comment, or reply) being moderated
        /// </summary>
        ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the content (topic, comment, or reply) being moderated
        /// </summary>
        string ContentHandle { get; set; }

        /// <summary>
        /// Gets or sets the handle for the user that owns the content, or the user who is being reviewed.
        /// This may be null, but only for content created by an app administrator.
        /// </summary>
        string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the app that the content came from
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets the type of image being reported
        /// </summary>
        ImageType ImageType { get; set; }

        /// <summary>
        /// Gets or sets the time at which the report was received from the user
        /// </summary>
        DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the moderation status
        /// </summary>
        ModerationStatus ModerationStatus { get; set; }
    }
}
