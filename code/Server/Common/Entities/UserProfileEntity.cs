// <copyright file="UserProfileEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// User profile class
    /// </summary>
    public class UserProfileEntity : ObjectEntity, IUserProfileEntity
    {
        /// <summary>
        /// Gets or sets first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets user bio
        /// </summary>
        public string Bio { get; set; }

        /// <summary>
        /// Gets or sets photo handle
        /// </summary>
        public string PhotoHandle { get; set; }

        /// <summary>
        /// Gets or sets user visibility
        /// </summary>
        public UserVisibilityStatus Visibility { get; set; }

        /// <summary>
        /// Gets or sets created time
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        public DateTime LastUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets review status
        /// </summary>
        public ReviewStatus ReviewStatus { get; set; }

        /// <summary>
        /// Gets or sets request id associated with the create topic request
        /// </summary>
        public string RequestId { get; set; }
    }
}
