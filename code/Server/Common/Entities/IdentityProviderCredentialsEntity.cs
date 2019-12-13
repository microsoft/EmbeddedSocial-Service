// <copyright file="IdentityProviderCredentialsEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// Identity provider credentials entity class
    /// When an app developers registers an app with SocialPlus, part of the registration is adding
    /// one or more identity provider credentials. These credentials let SocialPlus validate tokens
    /// sent by the application against an identity provider. Currently, SocialPlus supports five
    /// identity providers: Facebook, Microsoft, Twitter, Google, and Azure Active Directory (AAD)
    /// for server-to-server authentication. Each identity provider requires a set of credentials
    /// typically called clientId (aka tenantId in the case of AAD), clientSecret (not such thing for AAD)
    /// and clientRedirectUri (aka audience in case of AAD)
    /// </summary>
    public class IdentityProviderCredentialsEntity : ObjectEntity, IIdentityProviderCredentialsEntity
    {
        /// <summary>
        /// Gets or sets client id
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets client secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the callback which is a URL-encoded version of the URL
        /// </summary>
        public string ClientRedirectUri { get; set; }
    }
}
