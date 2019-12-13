//-----------------------------------------------------------------------
// <copyright file="ConflictException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class ConflictException.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Conflict exception class
    /// </summary>
    [Serializable]
    public class ConflictException : OperationFailedException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class
        /// </summary>
        public ConflictException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="operationIndex">Operation index</param>
        /// <param name="innerException">Inner exception</param>
        public ConflictException(string message, int operationIndex, Exception innerException)
            : base(message, operationIndex, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streamong context</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected ConflictException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Get object data
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
