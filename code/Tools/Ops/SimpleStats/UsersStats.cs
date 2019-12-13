// <copyright file="UsersStats.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.SimpleStats
{
    using System.Collections.Generic;

    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Simple stats about users
    /// </summary>
    public class UsersStats : GenericStoreStats
    {
        /// <summary>
        /// instance to this class
        /// </summary>
        private static UsersStats instance;

        /// <summary>
        /// object used for locking
        /// </summary>
        private static object syncRoot = new object();

        /// <summary>
        /// Creates an instance of a users class specific to an environment
        /// </summary>
        /// <param name="environmentName">name of environment</param>
        /// <returns>instance of class</returns>
        public static UsersStats Instance(string environmentName)
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    instance = new UsersStats();
                    instance.Init(environmentName).Wait();
                }
            }

            return instance;
        }

        /// <summary>
        /// Gets all users profiles registerered with an app
        /// </summary>
        /// <param name="appHandle">app handle</param>
        /// <returns>collection of user handles</returns>
        public List<string> GetAllUserProfiles(string appHandle)
        {
            string filterSelectAppHandle = TableQuery.GenerateFilterCondition("AppHandle", QueryComparisons.Equal, appHandle);
            string filterSelectAllUserHandles = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, "AppsFeed:f:");

            return this.GetPartitionKeysDistinct("Users", TableQuery.CombineFilters(filterSelectAppHandle, TableOperators.And, filterSelectAllUserHandles));
        }
    }
}
