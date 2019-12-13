//-----------------------------------------------------------------------
// <copyright file="CountTable.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class CountTable.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// Count table class
    /// </summary>
    public class CountTable : Table
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CountTable"/> class
        /// </summary>
        public CountTable()
        {
            this.TableType = TableType.Count;
        }
    }
}
