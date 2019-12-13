//-----------------------------------------------------------------------
// <copyright file="UnexpectedException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class UnexpectedException.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;

    /// <summary>
    /// Unexpected exception class
    /// </summary>
    [Serializable]
    public class UnexpectedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedException"/> class
        /// </summary>
        public UnexpectedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedException"/> class
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="innerException">inner exception</param>
        public UnexpectedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
