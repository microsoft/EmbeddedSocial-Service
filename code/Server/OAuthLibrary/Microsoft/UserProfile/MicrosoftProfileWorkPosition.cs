// <copyright file="MicrosoftProfileWorkPosition.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// A position of a work profile from a Microsoft user profile
    /// </summary>
    public class MicrosoftProfileWorkPosition
    {
        /// <summary>
        /// Gets the name of the user's work position, or null if the name of the work position is not specified.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; internal set; }
    }
}
