//-----------------------------------------------------------------------
// <copyright file="ObjectEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class ObjectEntity.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Object entity class
    /// </summary>
    public class ObjectEntity : Entity
    {
        /// <summary>
        /// Gets key for the entity
        /// </summary>
        public string ObjectKey { get; internal set; }

        /// <summary>
        /// Clone entity
        /// </summary>
        /// <returns>Cloned entity</returns>
        public new ObjectEntity Clone()
        {
            return base.Clone() as ObjectEntity;
        }
    }
}
