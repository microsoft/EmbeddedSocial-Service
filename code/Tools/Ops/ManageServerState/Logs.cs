// <copyright file="Logs.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageServerState
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using SocialPlus.Server.Blobs;

    /// <summary>
    /// portion of Program class that deals with creating and deleting logs
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// names of WAD* tables
        /// </summary>
        private static readonly string[] WADTableNames = new string[6]
        {
            "WADCrashDump",
            "WADDiagnosticsInfrastructureLogsTable",
            "WADDirectoriesTable",
            "WADLogsTable",
            "WADPerformanceCountersTable",
            "WADWindowsEventLogsTable"
        };

        /// <summary>
        /// deletes WAD* tables: WADCrashDump, WADDiagnosticsInfrastructureLogsTable, WADDirectoriesTable, WADLogsTable,
        /// WADPerformanceCountersTable, WADWindowsEventLogsTable
        /// </summary>
        /// <param name="azureBlobStorageConnectionString">connection string to azure blob storage</param>
        /// <returns>delete task</returns>
        private static async Task DeleteAzureLogs(string azureBlobStorageConnectionString)
        {
            AzureTableStorage azureLogsStorage = new AzureTableStorage(azureBlobStorageConnectionString);
            CTStore store = new CTStore(azureLogsStorage, null);

            // Delete tables one at a time (could be parallelized)
            foreach (var containerName in WADTableNames)
            {
                Console.WriteLine("Deleting " + containerName + " tables from Azure Blob Store...");
                await store.DeleteContainerAsync(containerName);
                Console.WriteLine("  " + containerName + " - Container Deleted");
            }
        }
    }
}
