// <copyright file="IUserProfileEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using SocialPlus.Models;

    /// <summary>
    /// User profile interface
    /// </summary>
    public interface IUserProfileEntity
    {
        /// <summary>
        /// Gets or sets first name
        /// </summary>
        string FirstName { get; set; }

        /// <summary>
        /// Gets or sets last name
        /// </summary>
        string LastName { get; set; }

        /// <summary>
        /// Gets or sets user bio
        /// </summary>
        string Bio { get; set; }

        /// <summary>
        /// Gets or sets photo handle
        /// </summary>
        string PhotoHandle { get; set; }

        /// <summary>
        /// Gets or sets user visibility
        /// </summary>
        UserVisibilityStatus Visibility { get; set; }

        /// <summary>
        /// Gets or sets created time
        /// </summary>
        DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets last updated time
        /// </summary>
        DateTime LastUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets review status
        /// </summary>
        ReviewStatus ReviewStatus { get; set; }
    }
}
