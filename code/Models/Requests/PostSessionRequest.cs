// <copyright file="PostSessionRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Request to post (create) session
    /// </summary>
    public class PostSessionRequest
    {
        /// <summary>
        /// Gets or sets instance id -- Unique installation id of the app
        /// </summary>
        [Required]
        public string InstanceId { get; set; }

        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        [Required]
        public string UserHandle { get; set; }
    }
}
