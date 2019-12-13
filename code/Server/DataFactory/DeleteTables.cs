// <copyright file="DeleteTables.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.DataFactory
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Azure.Management.DataFactories.Models;
    using Microsoft.Azure.Management.DataFactories.Runtime;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// This activity deletes multiple specified tables
    /// </summary>
    public class DeleteTables : IDotNetActivity
    {
        /// <summary>
        /// Method deletes multiple specified tables
        /// The tables to be deleted are specified using the following extended properties
        /// Extended Properties
        ///     tablesToDelete - Name of the tables (comma separated) to be deleted.
        /// At least one table needs to be specified.
        /// Extended Properties Example
        ///     "tablesToDelete": "Following",
        ///  Activity Operation
        ///     The activity iterates through all the tables from the tablesToDelete extended property,
        ///     checks for the table and deletes it if found.
        /// </summary>
        /// <param name="linkedServices">Linked services referenced by activity definition.</param>
        /// <param name="datasets">Datasets referenced by activity definition.</param>
        /// <param name="activity">Activity definition.</param>
        /// <param name="logger">Used to log messages during activity execution.</param>
        /// <returns>Activity state at the end of execution</returns>
        public IDictionary<string, string> Execute(
            IEnumerable<LinkedService> linkedServices,
            IEnumerable<Dataset> datasets,
            Activity activity,
            IActivityLogger logger)
        {
            DotNetActivity dotNetActivity = (DotNetActivity)activity.TypeProperties;
            IDictionary<string, string> extendedProperties = dotNetActivity.ExtendedProperties;

            logger.Write("Logging extended properties if any...");
            foreach (KeyValuePair<string, string> entry in extendedProperties)
            {
                logger.Write("<key:{0}> <value:{1}>", entry.Key, entry.Value);
            }

            string[] tablesToDelete = null;
            if (extendedProperties.ContainsKey("tablesToDelete"))
            {
                tablesToDelete = extendedProperties["tablesToDelete"].Split(',');
            }

            if (tablesToDelete == null || tablesToDelete.Length <= 0)
            {
                logger.Write("No tables to delete");
                return new Dictionary<string, string>();
            }

            AzureStorageLinkedService inputLinkedService;
            AzureTableDataset sourceTable;

            // Use the input dataset to get the storage connection string
            // For activities working on a single dataset, the first entry is the input dataset.
            // The activity.Inputs can have multiple datasets for building pipeline workflow dependencies. We can ignore the rest of the datasets
            Dataset inputDataset = datasets.Single(dataset => dataset.Name == activity.Inputs.First().Name);
            sourceTable = inputDataset.Properties.TypeProperties as AzureTableDataset;

            logger.Write("input table:{0}", sourceTable.TableName);

            inputLinkedService = linkedServices.First(
                ls =>
                ls.Name ==
                inputDataset.Properties.LinkedServiceName).Properties.TypeProperties
                as AzureStorageLinkedService;
            string inputConnectionString = inputLinkedService.ConnectionString;

            // create storage client for input. Pass the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(inputConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            foreach (string tableName in tablesToDelete)
            {
                CloudTable table = tableClient.GetTableReference(tableName);
                if (!table.Exists())
                {
                    logger.Write("Table {0} does not exist.", tableName);
                }
                else
                {
                    table.Delete();
                    logger.Write("Table {0} deleted.", tableName);
                }
            }

            return new Dictionary<string, string>();
        }
    }
}
