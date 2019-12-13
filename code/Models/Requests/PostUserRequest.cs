// <copyright file="PostUserRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to post (create) user
    /// </summary>
    public class PostUserRequest
    {
        /// <summary>
        /// Gets or sets instance id -- Unique installation id of the app
        /// </summary>
        [Required]
        public string InstanceId { get; set; }

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
        /// Gets or sets short bio of the user
        /// </summary>
        public string Bio { get; set; }

        /// <summary>
        /// Gets or sets photo handle of the user
        /// </summary>
        public string PhotoHandle { get; set; }
    }
}
