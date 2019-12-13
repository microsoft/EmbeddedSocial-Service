// <copyright file="TwitterAuthHeaderAttribute.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System;

    /// <summary>
    /// Attribute for deciding which properties in a Twitter authorization header
    /// should be signed.
    /// </summary>
    internal class TwitterAuthHeaderAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether a property should be signed.
        /// </summary>
        public bool Signed { get; set; }

        /// <summary>
        /// Gets or sets the name of the property in the authorization header.
        /// </summary>
        public string Name { get; set; }
    }
}
