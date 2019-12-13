// <copyright file="DeleteColumn.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.DataFactory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Azure.Management.DataFactories.Models;
    using Microsoft.Azure.Management.DataFactories.Runtime;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Data factory activity deletes a column from an Azure table
    /// </summary>
    public class DeleteColumn : IDotNetActivity
    {
        /// <summary>
        /// Deletes a column from an Azure table.
        /// The table for the column is same as the input table from the dataset
        /// The column to be deleted is specified using the following extended properties
        /// Extended Properties
        ///     columnName - Name of the column to be deleted
        ///     rowKeyPrefix - Rowkey prefix of the row from which the column will be deleted. This is optional and will identify the subset of rows to do this operation.
        /// columnName is mandatory.
        /// Extended Properties Example
        ///     "columnName": "UseDefault",
        ///     "rowKeyPrefix": "IdentityCredentialsObject:"
        ///  Activity Operation
        ///     The activity iterates through all the rows from the input table with the matching rowKeyPrefix,
        ///     checks for the column, removes the column if found and runs a replace table operation to replace the contents of
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

            logger.Write("Logging extended properties if any...");
            foreach (KeyValuePair<string, string> entry in extendedProperties)
            {
                logger.Write("<key:{0}> <value:{1}>", entry.Key, entry.Value);
            }

            if (!extendedProperties.ContainsKey("columnName"))
            {
                throw new ArgumentException("Column name is required", "columnName");
            }

            string columnName = extendedProperties["columnName"];

            string rowKeyPrefix = string.Empty;
            if (extendedProperties.ContainsKey("rowKeyPrefix"))
            {
                rowKeyPrefix = extendedProperties["rowKeyPrefix"];
            }

            AzureStorageLinkedService inputLinkedService;
            AzureTableDataset sourceTable;

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
            CloudStorageAccount inputStorageAccount = CloudStorageAccount.Parse(inputConnectionString);
            CloudTableClient inputTableClient = inputStorageAccount.CreateCloudTableClient();
            CloudTable inputTable = inputTableClient.GetTableReference(sourceTable.TableName);

            long totalProcessedRecords = 0;
            long actualAffectedRecords = 0;
            TableContinuationToken tableContinuationToken = null;
            List<Task> tasks = new List<Task>();

            do
            {
                var resultSegment = inputTable.ExecuteQuerySegmented(new TableQuery(), tableContinuationToken);
                tableContinuationToken = resultSegment.ContinuationToken;

                var partitionGroups = (from s in resultSegment.Results
                                       where string.IsNullOrWhiteSpace(rowKeyPrefix) ? true : s.RowKey.StartsWith(rowKeyPrefix)
                                       select s).GroupBy(a => a.PartitionKey);

                foreach (IGrouping<string, DynamicTableEntity> g in partitionGroups)
                {
                    TableBatchOperation batch = new TableBatchOperation();
                    foreach (DynamicTableEntity e in g.AsEnumerable())
                    {
                        // If the columnName exist in the properties, then Remove it
                        if (e.Properties.ContainsKey(columnName))
                        {
                            e.Properties.Remove(columnName);
                            batch.Replace(e);
                            logger.Write("<partition key:{0}>, <row key:{1}> added to batch", e.PartitionKey, e.RowKey);
                        }
                    }

                    if (batch.Count > 0)
                    {
                        tasks.Add(inputTable.ExecuteBatchInChunkAsync(batch));
                        actualAffectedRecords += batch.Count;
                    }
                }

                totalProcessedRecords += resultSegment.Results.Count;
                logger.Write("Processed records count: {0}", totalProcessedRecords);
                logger.Write("Affected records count: {0}", actualAffectedRecords);
            }
            while (tableContinuationToken != null);

            // The batch operations complete when Task.WaitAll completes
            Task.WaitAll(tasks.ToArray());
            logger.Write("Deleted column from {0} records", actualAffectedRecords);

            return new Dictionary<string, string>();
        }
    }
}
