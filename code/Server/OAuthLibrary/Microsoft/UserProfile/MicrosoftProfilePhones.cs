// <copyright file="MicrosoftProfilePhones.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// Phones in a Microsoft user profile
    /// </summary>
    public class MicrosoftProfilePhones
    {
        /// <summary>
        /// Gets the user's personal phone number, or null if one is not specified.
        /// </summary>
        [JsonProperty(PropertyName = "personal")]
        public string Personal { get; internal set; }

        /// <summary>
        /// Gets the user's business phone number, or null if one is not specified.
        /// </summary>
        [JsonProperty(PropertyName = "business")]
        public string Business { get; internal set; }

        /// <summary>
        /// Gets the user's mobile phone number, or null if one is not specified.
        /// </summary>
        [JsonProperty(PropertyName = "mobile")]
        public string Mobile { get; internal set; }
    }
}
