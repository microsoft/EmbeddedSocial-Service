// <copyright file="GetTopicNameResponseV06.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Response from get topic name
    /// </summary>
    /// <remarks>This class is obsolete in v0.7.  It is replaced by GetTopicByNameResponse. </remarks>
    public class GetTopicNameResponseV06
    {
        /// <summary>
        /// Gets or sets topic handle of the response
        /// </summary>
        [Required]
        public string TopicHandle { get; set; }
    }
}
