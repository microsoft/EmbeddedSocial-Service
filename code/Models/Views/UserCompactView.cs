// <copyright file="UserCompactView.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// User compact view
    /// </summary>
    public class UserCompactView
    {
        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        [Required]
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets first name of the user
        /// </summary>
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets last name of the user
        /// </summary>
        [Required]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets photo handle of the user
        /// </summary>
        public string PhotoHandle { get; set; }

        /// <summary>
        /// Gets or sets photo url of the user
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets visibility of the user
        /// </summary>
        [Required]
        public UserVisibilityStatus Visibility { get; set; }

        /// <summary>
        /// Gets or sets follower relationship status of the querying user
        /// </summary>
        [Required]
        public UserRelationshipStatus FollowerStatus { get; set; }
    }
}
