// <copyright file="GoogleToken.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// Fields describing a Google token.
    /// A Google token is received when a POST request is made to Google's token endpoint.
    /// </summary>
    public class GoogleToken
    {
        /// <summary>
        /// Gets the token that can be sent to a Google API.
        /// </summary>
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; internal set; }

        /// <summary>
        /// Gets the JWT that contains identity information about the user that is digitally signed by Google.
        /// </summary>
        [JsonProperty(PropertyName = "id_token")]
        public string IdToken { get; internal set; }

        /// <summary>
        /// Gets the remaining lifetime of the access token.
        /// </summary>
        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; internal set; }

        /// <summary>
        /// Gets the type of token returned. At this time, this field always has the value Bearer.
        /// </summary>
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; internal set; }

        /// <summary>
        /// Gets the refresh token. This field is only present if access_type=offline is included in the authentication request.
        /// </summary>
        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; internal set; }
    }
}
