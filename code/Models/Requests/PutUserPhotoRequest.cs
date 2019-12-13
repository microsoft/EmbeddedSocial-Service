// <copyright file="PutUserPhotoRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    /// <summary>
    /// Request to put (update) user photo
    /// </summary>
    public class PutUserPhotoRequest
    {
        /// <summary>
        /// Gets or sets photo handle of the user
        /// </summary>
        public string PhotoHandle { get; set; }
    }
}
