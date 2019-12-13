// <copyright file="MicrosoftProfileWorkEmployer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// An employer of a work profile from a Microsoft user profile
    /// </summary>
    public class MicrosoftProfileWorkEmployer
    {
        /// <summary>
        /// Gets the name of the user's employer, or null if the employer's name is not specified
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; internal set; }
    }
}
