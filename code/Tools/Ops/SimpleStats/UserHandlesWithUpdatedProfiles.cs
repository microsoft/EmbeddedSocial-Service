// <copyright file="UserHandlesWithUpdatedProfiles.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.SimpleStats
{
    using System;
    using System.Collections.Generic;

    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Get a list of user handles where the user profile has been recently updated
    /// </summary>
    public class UserHandlesWithUpdatedProfiles : GenericStoreStats
    {
        /// <summary>
        /// instance to this class
        /// </summary>
        private static UserHandlesWithUpdatedProfiles instance;

        /// <summary>
        /// object used for locking
        /// </summary>
        private static object syncRoot = new object();

        /// <summary>
        /// Creates an instance of this class specific to an environment
        /// </summary>
        /// <param name="environmentName">name of environment</param>
        /// <returns>instance of class</returns>
        public static UserHandlesWithUpdatedProfiles Instance(string environmentName)
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    instance = new UserHandlesWithUpdatedProfiles();
                    instance.Init(environmentName).Wait();
                }
            }

            return instance;
        }

        /// <summary>
        /// Gets all user handles registerered with an app that have updated their profile after a certain DateTime
        /// </summary>
        /// <param name="appHandle">app handle</param>
        /// <param name="dateTime">DateTime after which the profile has been updated</param>
        /// <returns>collection of user handles</returns>
        public List<string> GetUpdatedUserHandles(string appHandle, DateTime dateTime)
        {
            string filterAppHandle = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "ProfilesObject:" + appHandle);
            string filterLastUpdatedTime = TableQuery.GenerateFilterConditionForDate("LastUpdatedTime", QueryComparisons.GreaterThan, new DateTimeOffset(dateTime));
            return this.GetPartitionKeysDistinct("Users", TableQuery.CombineFilters(filterAppHandle, TableOperators.And, filterLastUpdatedTime));
        }
    }
}
