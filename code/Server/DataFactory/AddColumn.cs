// <copyright file="AddColumn.cs" company="Microsoft">
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
    /// Data factory activity adds a column to an Azure table.
    /// </summary>
    public class AddColumn : IDotNetActivity
    {
        /// <summary>
        /// Adds a column to an Azure table.
        /// The column to be added is specified using the following extended properties
        /// Extended Properties
        ///     columnName - Name of the column to be added
        ///     type - Data type of the column. Only supported types right now are: int32, bool, and string
        ///     defaultValue - Default value of the column. This is optional and will default to type's default value.
        ///     rowKeyPrefix - Rowkey prefix of the row in which the column will be added. This is optional and will identify the subset of rows to do this operation.
        /// columnName and type are mandatory.
        /// Extended Properties Example
        ///     "columnName": "DisableHandleValidation",
        ///     "type": "bool",
        ///     "defaultValue": "False",
        ///     "rowKeyPrefix": "ProfilesObject:"
        ///  Activity Operation
        ///     The activity iterates through all the rows from the input table with the matching rowKeyPrefix,
        ///     checks for the column, adds it if the column is not found and runs a merge table operation to merge the contents of
        ///     modified row/entity with an existing row/entity in the table.
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

            if (!extendedProperties.ContainsKey("type"))
            {
                throw new ArgumentException("Type information is required", "type");
            }

            string type = extendedProperties["type"];

            string defaultValueStr = null;
            if (extendedProperties.ContainsKey("defaultValue"))
            {
                defaultValueStr = extendedProperties["defaultValue"];
            }

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

            EntityProperty columnValue = this.GetEntityProperty(type, defaultValueStr);
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
                    // Create a new batch for every partition group within the resultSegment
                    TableBatchOperation batch = new TableBatchOperation();
                    foreach (DynamicTableEntity e in g.AsEnumerable())
                    {
                        // If the columnName does not exist in the properties, then only Add it
                        if (!e.Properties.ContainsKey(columnName))
                        {
                            e.Properties.Add(columnName, columnValue);
                            batch.Merge(e);
                            logger.Write("<partition key:{0}>, <row key:{1}>", e.PartitionKey, e.RowKey);
                        }
                    }

                    if (batch.Count > 0)
                    {
                        // ExecuteBatchInChunkAsync is an extension method to chunk and process 100 operations in a batch
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
            // TODO : Add ContinueWith on ExecuteBatchAsync for tracing of each batch operation as it completes
            Task.WaitAll(tasks.ToArray());
            logger.Write("Added new column to {0} records", actualAffectedRecords);
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the entity property for a provided type
        /// </summary>
        /// <param name="type">the type of the value</param>
        /// <param name="value">the value</param>
        /// <returns>entity property for the value</returns>
        private EntityProperty GetEntityProperty(string type, string value)
        {
            switch (type.ToLower())
            {
                case "string":
                    return new EntityProperty(value);
                case "int32":
                    int intValue = 0;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        intValue = Convert.ToInt32(value);
                    }

                    return new EntityProperty(intValue);
                case "bool":
                    bool boolValue = false;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        boolValue = Convert.ToBoolean(value);
                    }

                    return new EntityProperty(boolValue);
                default:
                    return null;
            }
        }
    }
}
