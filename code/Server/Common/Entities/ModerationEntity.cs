// <copyright file="ModerationEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// The content moderation entity.
    /// </summary>
    public class ModerationEntity : ObjectEntity, IModerationEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the image being moderated
        /// </summary>
        public string ImageHandle { get; set; }

        /// <summary>
        /// Gets or sets the type of content being reported on
        /// </summary>
        public ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the content being reported on
        /// </summary>
        public string ContentHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user that created the content.
        /// This can be null if a user did not create the content.
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the app that the content came from
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets the type of image being reported
        /// </summary>
        public ImageType ImageType { get; set; }

        /// <summary>
        /// Gets or sets the time at which the moderation request was created
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the moderation status
        /// </summary>
        public ModerationStatus ModerationStatus { get; set; }
    }
}
