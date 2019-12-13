// <copyright file="UserProfileView.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// User profile view
    /// </summary>
    public class UserProfileView
    {
        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets first name of the user
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets last name of the user
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets short bio of the user
        /// </summary>
        [Required(AllowEmptyStrings = true)]
        public string Bio { get; set; }

        /// <summary>
        /// Gets or sets photo handle of the user
        /// </summary>
        [Required(AllowEmptyStrings = true)]
        public string PhotoHandle { get; set; }

        /// <summary>
        /// Gets or sets photo url of the user
        /// </summary>
        [Required(AllowEmptyStrings = true)]
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets visibility of the user
        /// </summary>
        [Required]
        public UserVisibilityStatus Visibility { get; set; }

        /// <summary>
        /// Gets or sets total topics posted by user
        /// </summary>
        [Required]
        public long TotalTopics { get; set; }

        /// <summary>
        /// Gets or sets total followers for the user
        /// </summary>
        [Required]
        public long TotalFollowers { get; set; }

        /// <summary>
        /// Gets or sets total following users
        /// </summary>
        [Required]
        public long TotalFollowing { get; set; }

        /// <summary>
        /// Gets or sets follower relationship status of the querying user
        /// </summary>
        [Required]
        public UserRelationshipStatus FollowerStatus { get; set; }

        /// <summary>
        /// Gets or sets following relationship status of the querying user
        /// </summary>
        [Required]
        public UserRelationshipStatus FollowingStatus { get; set; }

        /// <summary>
        /// Gets or sets user profile status
        /// </summary>
        [Required]
        public ReviewStatus ProfileStatus { get; set; }
    }
}
