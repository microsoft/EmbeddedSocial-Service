// <copyright file="GoogleOpenIDDiscovery.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// Fields encapsulating the Google OpenID discovery structure
    /// </summary>
    public class GoogleOpenIDDiscovery
    {
        /// <summary>
        /// Gets the URL using the https scheme with no query or fragment component that the OP asserts as its Issuer Identifier.
        /// If Issuer discovery is supported (see Section 2), this value MUST be identical to the issuer value returned by WebFinger.
        /// This also MUST be identical to the <c>iss</c> Claim value in ID Tokens issued from this Issuer.
        /// </summary>
        [JsonProperty(PropertyName = "issuer")]
        public string Issuer { get; internal set; }

        /// <summary>
        /// Gets the URL of the OP's <c>OAuth</c> 2.0 Authorization Endpoint
        /// </summary>
        [JsonProperty(PropertyName = "authorization_endpoint")]
        public string AuthorizationEndpoint { get; internal set; }

        /// <summary>
        /// Gets the URL of the OP's <c>OAuth</c> 2.0 Token Endpoint
        /// </summary>
        [JsonProperty(PropertyName = "token_endpoint")]
        public string TokenEndpoint { get; internal set; }

        /// <summary>
        /// Gets the URL of the OP's UserInfo Endpoint. This URL MUST use the https scheme and MAY contain port, path, and query parameter components.
        /// </summary>
        [JsonProperty(PropertyName = "userinfo_endpoint")]
        public string UserinfoEndpoing { get; internal set; }
    }
}