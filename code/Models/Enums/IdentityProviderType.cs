// <copyright file="IdentityProviderType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    /// <summary>
    /// Identity provider type
    /// </summary>
    public enum IdentityProviderType
    {
        /// <summary>
        /// Facebook identity provider
        /// </summary>
        Facebook,

        /// <summary>
        /// Microsoft (MSA) identity provider
        /// </summary>
        Microsoft,

        /// <summary>
        /// Google identity provider
        /// </summary>
        Google,

        /// <summary>
        /// Twitter identity provider
        /// </summary>
        Twitter,

        /// <summary>
        /// AADS2S identity provider
        /// </summary>
        AADS2S,

        /// <summary>
        /// SocialPlus identity provider
        /// This identity provider is used only when client uses SocialPlus session token for auth
        /// </summary>
        SocialPlus,
    }
}
