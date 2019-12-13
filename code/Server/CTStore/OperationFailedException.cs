//-----------------------------------------------------------------------
// <copyright file="OperationFailedException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class OperationFailedException.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Operation failed exception class
    /// </summary>
    [Serializable]
    public class OperationFailedException : Exception
    {
        /// <summary>
        /// Operation index
        /// </summary>
        private readonly int operationIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationFailedException"/> class
        /// </summary>
        public OperationFailedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationFailedException"/> class
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="operationIndex">Operation index</param>
        /// <param name="innerException">Inner exception</param>
        public OperationFailedException(string message, int operationIndex, Exception innerException)
            : base(message, innerException)
        {
            this.operationIndex = operationIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationFailedException"/> class
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streamong context</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected OperationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.operationIndex = info.GetInt32("OperationIndex");
        }

        /// <summary>
        /// Gets Operation index
        /// </summary>
        public int OperationIndex
        {
            get
            {
                return this.operationIndex;
            }
        }

        /// <summary>
        /// Get object data
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("OperationIndex", this.operationIndex);
            base.GetObjectData(info, context);
        }
    }
}
