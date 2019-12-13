// <copyright file="Extensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using Microsoft.CTStore;

    /// <summary>
    /// Extensions for storage types
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert storage consistency mode to CT store consistency mode
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <returns>Consistency mode</returns>
        public static ConsistencyMode ToConsistencyMode(this StorageConsistencyMode storageConsistencyMode)
        {
            if (storageConsistencyMode == StorageConsistencyMode.Express)
            {
                return ConsistencyMode.Express;
            }
            else if (storageConsistencyMode == StorageConsistencyMode.Eventual)
            {
                return ConsistencyMode.Eventual;
            }

            return ConsistencyMode.Strong;
        }
    }
}
