// <copyright file="MicrosoftProfileEmails.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// Emails in a Microsoft user profile
    /// </summary>
    public class MicrosoftProfileEmails
    {
        /// <summary>
        /// Gets the user's preferred email address, or null if one is not specified.
        /// </summary>
        [JsonProperty(PropertyName = "preferred")]
        public string Preferred { get; internal set; }

        /// <summary>
        /// Gets the email address that is associated with the account.
        /// </summary>
        [JsonProperty(PropertyName = "account")]
        public string Account { get; internal set; }

        /// <summary>
        /// Gets the user's personal email address, or null if one is not specified.
        /// </summary>
        [JsonProperty(PropertyName = "personal")]
        public string Personal { get; internal set; }

        /// <summary>
        /// Gets the user's business email address, or null if one is not specified.
        /// </summary>
        [JsonProperty(PropertyName = "business")]
        public string Business { get; internal set; }

        /// <summary>
        /// Gets the user's "alternate" email address, or null if one is not specified
        /// </summary>
        [JsonProperty(PropertyName = "other")]
        public string Other { get; internal set; }
    }
}
