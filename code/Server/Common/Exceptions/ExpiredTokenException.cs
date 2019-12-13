// <copyright file="ExpiredTokenException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Exceptions
{
    using System;

    /// <summary>
    /// Expired token exception class
    /// </summary>
    public class ExpiredTokenException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpiredTokenException"/> class
        /// </summary>
        public ExpiredTokenException()
        {
        }
    }
}
