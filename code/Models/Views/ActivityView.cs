// <copyright file="ActivityView.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Activity view
    /// </summary>
    public class ActivityView
    {
        /// <summary>
        /// Gets or sets activity handle
        /// </summary>
        [Required]
        public string ActivityHandle { get; set; }

        /// <summary>
        /// Gets or sets created time
        /// </summary>
        [Required]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets activity type
        /// </summary>
        [Required]
        public ActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets or sets actor users
        /// </summary>
        [Required]
        public List<UserCompactView> ActorUsers { get; set; }

        /// <summary>
        /// Gets or sets acted on user
        /// </summary>
        public UserCompactView ActedOnUser { get; set; }

        /// <summary>
        /// Gets or sets acted on content
        /// </summary>
        public ContentCompactView ActedOnContent { get; set; }

        /// <summary>
        /// Gets or sets total actions
        /// </summary>
        [Required]
        public int TotalActions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the activity was read
        /// </summary>
        [Required]
        public bool Unread { get; set; }

        /// <summary>
        /// Gets or sets the containing app
        /// </summary>
        public AppCompactView App { get; set; }
    }
}
