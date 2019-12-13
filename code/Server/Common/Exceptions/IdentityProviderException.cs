// <copyright file="IdentityProviderException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Exceptions
{
    using System;

    /// <summary>
    /// Identity provider exception class
    /// </summary>
    public class IdentityProviderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityProviderException"/> class
        /// </summary>
        public IdentityProviderException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityProviderException"/> class
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="innerException">inner exception</param>
        public IdentityProviderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
