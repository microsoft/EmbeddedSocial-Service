//-----------------------------------------------------------------------
// <copyright file="FixedObjectTable.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class FixedObjectTable.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Fixed object table class
    /// </summary>
    public class FixedObjectTable : Table
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FixedObjectTable"/> class
        /// </summary>
        public FixedObjectTable()
        {
            this.TableType = TableType.Object;
        }
    }
}
