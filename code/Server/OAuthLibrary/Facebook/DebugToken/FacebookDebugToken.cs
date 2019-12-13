// <copyright file="FacebookDebugToken.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// Fields representing a Facebook debug token.
    /// A Facebook access token can be exchanged for a Facebook debug token.
    /// </summary>
    public class FacebookDebugToken
    {
        /// <summary>
        /// Gets the ID of the application this access token is for.
        /// </summary>
        [JsonProperty(PropertyName = "app_id")]
        public string AppId { get; internal set; }

        /// <summary>
        /// Gets the name of the application this access token is for.
        /// </summary>
        [JsonProperty(PropertyName = "application")]
        public string Application { get; internal set; }

        /// <summary>
        /// Gets any error that a request to the graph API would return due to the access token.
        /// </summary>
        [JsonProperty(PropertyName = "error")]
        public FacebookDebugTokenError Error { get; internal set; }

        /// <summary>
        /// Gets the timestamp when this access token expires.
        /// </summary>
        [JsonProperty(PropertyName = "expires_at")]
        public double ExpiresAt { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the access token is still valid or not.
        /// </summary>
        [JsonProperty(PropertyName = "is_valid")]
        public bool IsValid { get; internal set; }

        /// <summary>
        /// Gets the timestamp when this access token was issued.
        /// </summary>
        [JsonProperty(PropertyName = "issued_at")]
        public double IssuedAt { get; internal set; }

        /// <summary>
        /// Gets the general metadata associated with the access token. Can contain data like <c>'sso'</c>, <c>'auth_type'</c>, <c>'auth_nonce'</c>
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public object Metadata { get; internal set; }

        /// <summary>
        /// Gets for impersonated access tokens, the ID of the page this token contains.
        /// </summary>
        [JsonProperty(PropertyName = "profile_id")]
        public string ProfileId { get; internal set; }

        /// <summary>
        /// Gets the list of permissions that the user has granted for the app in this access token.
        /// </summary>
        [JsonProperty(PropertyName = "scopes")]
        public List<string> Scopes { get; internal set; }

        /// <summary>
        /// Gets the ID of the user this access token is for.
        /// </summary>
        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; internal set; }
    }
}
