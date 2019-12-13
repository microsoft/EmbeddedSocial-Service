// <copyright file="AppsStats.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.SimpleStats
{
    using System;
    using System.Collections.Generic;

    using Microsoft.WindowsAzure.Storage.Table;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Simple stats about apps registered in a deployment
    /// </summary>
    public class AppsStats : GenericStoreStats
    {
        /// <summary>
        /// instance to this class
        /// </summary>
        private static AppsStats instance;

        /// <summary>
        /// object used for locking
        /// </summary>
        private static object syncRoot = new object();

        /// <summary>
        /// Creates an instance of a users class specific to an environment
        /// </summary>
        /// <param name="environmentName">name of environment</param>
        /// <returns>instance of class</returns>
        public static AppsStats Instance(string environmentName)
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    instance = new AppsStats();
                    instance.Init(environmentName).Wait();
                }
            }

            return instance;
        }

        /// <summary>
        /// Gets a collection of app handles (Container: AllApps, Table: Feed)
        /// </summary>
        /// <returns>collection of app handles</returns>
        public List<string> GetAllAppHandles()
        {
            string filterSelectAllAppHandles = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, "KeysFeed:f:");

            return this.GetPartitionKeysDistinct("Apps", filterSelectAllAppHandles);
        }

        /// <summary>
        /// Gets an application name
        /// </summary>
        /// <param name="appHandle">app handle</param>
        /// <returns>app name</returns>
        public string GetAppName(string appHandle)
        {
            string filterSelectAllAppProfiles = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "ProfilesObject:" + appHandle);

            var allAppNames = this.GetColumnsAsStrings("Apps", "Name", filterSelectAllAppProfiles);
            if (allAppNames.Count != 1)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("App {0} should have a single name. Instead we found {1} names.", appHandle, allAppNames.Count);
                Console.ResetColor();
            }

            return allAppNames[0];
        }
    }
}
