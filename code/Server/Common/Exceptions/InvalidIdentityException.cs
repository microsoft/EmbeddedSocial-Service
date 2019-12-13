// <copyright file="InvalidIdentityException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Exceptions
{
    using System;

    /// <summary>
    /// Invalid identity exception class
    /// </summary>
    public class InvalidIdentityException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidIdentityException"/> class
        /// </summary>
        public InvalidIdentityException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidIdentityException"/> class
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="innerException">inner exception</param>
        public InvalidIdentityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
