// <copyright file="GenericUserProfile.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System.Collections.Generic;

    /// <summary>
    /// Simple user profile common across all identity providers.
    /// Implements a user's profile. Each identity provider has its own profile format. The Auth library maps
    /// a provider's profile format to this simpler format. If the caller requires access to profile
    /// properties specific to a provider, the Auth library has calls specific to each provider that returns
    /// profiles specific to the provider.
    /// </summary>
    public class GenericUserProfile
    {
        /// <summary>
        /// Gets or sets third party account id
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets first name from identity provider
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets last name from identity provider
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets emails from identity provider
        /// </summary>
        public List<string> Emails { get; set; }
    }
}
