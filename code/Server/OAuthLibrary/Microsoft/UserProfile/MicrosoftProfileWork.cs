// <copyright file="MicrosoftProfileWork.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// A work profile from a Microsoft user profile
    /// </summary>
    public class MicrosoftProfileWork
    {
        /// <summary>
        /// Gets the info about the user's employer
        /// </summary>
        [JsonProperty(PropertyName = "employer")]
        public MicrosoftProfileWorkEmployer Employer { get; internal set; }

        /// <summary>
        /// Gets the info about the user's work position.
        /// </summary>
        [JsonProperty(PropertyName = "position")]
        public MicrosoftProfileWorkPosition Position { get; internal set; }
    }
}
