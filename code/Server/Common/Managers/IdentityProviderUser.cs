// <copyright file="IdentityProviderUser.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System.Collections.Generic;

    /// <summary>
    /// User of third party identity provider
    /// </summary>
    public class IdentityProviderUser
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
