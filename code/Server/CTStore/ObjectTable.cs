//-----------------------------------------------------------------------
// <copyright file="ObjectTable.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class ObjectTable.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Object table class
    /// </summary>
    public class ObjectTable : Table
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTable"/> class
        /// </summary>
        public ObjectTable()
        {
            this.TableType = TableType.Object;
        }
    }
}
