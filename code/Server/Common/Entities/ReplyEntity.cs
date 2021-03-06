﻿// <copyright file="ReplyEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// Reply entity class
    /// </summary>
    public class ReplyEntity : ObjectEntity, IReplyEntity
    {
        /// <summary>
        /// Gets or sets comment handle
        /// </summary>
        public string CommentHandle { get; set; }

        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        public string TopicHandle { get; set; }

        /// <summary>
        /// Gets or sets created time
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        public DateTime LastUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets owner user handle
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets comment text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets comment language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets review status
        /// </summary>
        public ReviewStatus ReviewStatus { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets request id associated with the create comment request
        /// </summary>
        public string RequestId { get; set; }
    }
}
