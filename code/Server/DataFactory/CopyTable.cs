// <copyright file="CopyTable.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.DataFactory
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Azure.Management.DataFactories.Models;
    using Microsoft.Azure.Management.DataFactories.Runtime;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// This activity copies an Azure table to another table.
    /// </summary>
    public class CopyTable : IDotNetActivity
    {
        /// <summary>
        /// Method copies an Azure table
        /// The table to be copied is same as the input table from the dataset
        /// Extended Properties
        ///     ignore - Name of the columns (comma separated) to be ignored as part of copy operation. This is an optional paramater.
        ///  Activity Operation
        ///     The activity iterates through all the rows from the input table,
        ///     checks for the column to be ignored, remove the ignored columns is found,
        ///     runs an InsertOrReplace table operation to insert or replace the contents of
        ///     row/entity in the table.
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

            string[] columnsToIgnore = null;

            // extendedProperties are optional for this activity
            if (extendedProperties != null)
            {
                logger.Write("Logging extended properties if any...");
                foreach (KeyValuePair<string, string> entry in extendedProperties)
                {
                    logger.Write("<key:{0}> <value:{1}>", entry.Key, entry.Value);
                }

                if (extendedProperties.ContainsKey("ignore"))
                {
                    columnsToIgnore = extendedProperties["ignore"].Split(',');
                }
            }

            AzureStorageLinkedService inputLinkedService, outputLinkedService;
            AzureTableDataset sourceTable, destinationTable;

            // For activities working on a single dataset, the first entry is the input dataset.
            // The activity.Inputs can have multiple datasets for building pipeline workflow dependencies. We can ignore the rest of the datasets
            Dataset inputDataset = datasets.Single(dataset => dataset.Name == activity.Inputs.First().Name);
            sourceTable = inputDataset.Properties.TypeProperties as AzureTableDataset;

            logger.Write("input table:{0}", sourceTable.TableName);

            Dataset outputDataset = datasets.Single(dataset => dataset.Name == activity.Outputs.Single().Name);
            destinationTable = outputDataset.Properties.TypeProperties as AzureTableDataset;

            logger.Write("output table:{0}", destinationTable.TableName);

            inputLinkedService = linkedServices.First(
                ls =>
                ls.Name ==
                inputDataset.Properties.LinkedServiceName).Properties.TypeProperties
                as AzureStorageLinkedService;
            string inputConnectionString = inputLinkedService.ConnectionString;

            outputLinkedService = linkedServices.First(
                ls =>
                ls.Name ==
                outputDataset.Properties.LinkedServiceName).Properties.TypeProperties
                as AzureStorageLinkedService;
            string outputConnectionString = outputLinkedService.ConnectionString;

            // create storage client for input. Pass the connection string.
            CloudStorageAccount inputStorageAccount = CloudStorageAccount.Parse(inputConnectionString);
            CloudTableClient inputTableClient = inputStorageAccount.CreateCloudTableClient();
            CloudTable inputTable = inputTableClient.GetTableReference(sourceTable.TableName);

            CloudStorageAccount outputStorageAccount = CloudStorageAccount.Parse(outputConnectionString);
            CloudTableClient outputTableClient = outputStorageAccount.CreateCloudTableClient();
            CloudTable outputTable = outputTableClient.GetTableReference(destinationTable.TableName);

            if (!outputTable.Exists())
            {
                outputTable.Create();
            }

            long totalProcessedRecords = 0;
            TableContinuationToken tableContinuationToken = null;
            List<Task> tasks = new List<Task>();

            do
            {
                var resultSegment = inputTable.ExecuteQuerySegmented(new TableQuery(), tableContinuationToken);
                tableContinuationToken = resultSegment.ContinuationToken;

                var partitionGroups = (from s in resultSegment.Results
                                       select s).GroupBy(a => a.PartitionKey);

                foreach (IGrouping<string, DynamicTableEntity> g in partitionGroups)
                {
                    TableBatchOperation batch = new TableBatchOperation();
                    foreach (DynamicTableEntity e in g.AsEnumerable())
                    {
                        if (columnsToIgnore != null && columnsToIgnore.Length > 0)
                        {
                            foreach (string column in columnsToIgnore)
                            {
                                if (e.Properties.ContainsKey(column))
                                {
                                    e.Properties.Remove(column);
                                }
                            }
                        }

                        batch.InsertOrReplace(e);
                        logger.Write("<partition key:{0}>, <row key:{1}>", e.PartitionKey, e.RowKey);
                    }

                    if (batch.Count > 0)
                    {
                        tasks.Add(outputTable.ExecuteBatchInChunkAsync(batch));
                    }

                    logger.Write("Copied data for partition: {0}", g.Key);
                }

                // In case of Copy, number of processed and affected records is the same
                totalProcessedRecords += resultSegment.Results.Count;
                logger.Write("Processed records count: {0}", totalProcessedRecords);
            }
            while (tableContinuationToken != null);

            // The batch operations complete when Task.WaitAll completes
            Task.WaitAll(tasks.ToArray());
            logger.Write("Copied {0} records from {1} to {2}", totalProcessedRecords, sourceTable.TableName, destinationTable.TableName);

            return new Dictionary<string, string>();
        }
    }
}
