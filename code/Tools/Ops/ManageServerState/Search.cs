// <copyright file="Search.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageServerState
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Server.Search;

    /// <summary>
    /// portion of Program class that deals with creating and deleting search indices
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// Create the two search indices
        /// </summary>
        /// <param name="searchServiceName">name of the search service</param>
        /// <param name="searchServiceAdminKey">admin access key</param>
        /// <returns>provision search task</returns>
        private static async Task ProvisionSearch(string searchServiceName, string searchServiceAdminKey)
        {
            Console.WriteLine("Creating search indices...");

            // instantiate the two search interfaces
            SearchUsers searchUsers = new SearchUsers(log, searchServiceName, searchServiceAdminKey);
            SearchTopics searchTopics = new SearchTopics(log, searchServiceName, searchServiceAdminKey);

            // create the two indices
            await searchUsers.CreateIndex();
            Console.WriteLine("  User search index - Provisioned");
            await searchTopics.CreateIndex();
            Console.WriteLine("  Topic search index - Provisioned");
        }

        /// <summary>
        /// Delete the search indices
        /// </summary>
        /// <param name="searchServiceName">name of the search service</param>
        /// <param name="searchServiceAdminKey">admin access key</param>
        /// <returns>delete search task</returns>
        private static async Task DeleteSearch(string searchServiceName, string searchServiceAdminKey)
        {
            Console.WriteLine("Deleting search indices...");

            // instantiate the two search interfaces
            SearchUsers searchUsers = new SearchUsers(log, searchServiceName, searchServiceAdminKey);
            SearchTopics searchTopics = new SearchTopics(log, searchServiceName, searchServiceAdminKey);
            await searchTopics.DeleteIndex();
            Console.WriteLine("  Topic search index - Deleted");
            await searchUsers.DeleteIndex();
            Console.WriteLine("  User search index - Deleted");
        }
    }
}
