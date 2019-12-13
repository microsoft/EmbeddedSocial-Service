// <copyright file="TransformColumn.cs" company="Microsoft">
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
    /// Data factory activity transforms column contents
    /// </summary>
    public class TransformColumn : IDotNetActivity
    {
        /// <summary>
        /// Transforms the values of a column in an Azure table. The column may be a normal column or the RowKey column, but cannot be the PartitionKey column.
        /// The column to be transformed is specified using the following extended properties
        /// Extended Properties
        ///     columnName - Name of the column to be transformed
        ///     columnType - Data type of the column. Only supported types right now are: int32, bool, and string
        ///     ifColumnValueMatches - The transformation is applied only if the contents of column value matches the specified value.
        ///     replaceColumnValueWith - Replace the contents of the matched column value with the specified value.
        ///     ifRowKeyContains - The transformation is applied only if the contents of row key contains the specified value.
        ///     replaceRowKeySubStrWith - Replace the contents of the matched row key with the specified value to generate a new row key.
        ///     rowKeyPrefixes - Rowkey prefixes of the rows in which the column transformation will be applied. This is optional and will identify the subset of rows to do this operation.
        /// You can specify columnName,columnType,ifColumnValueMatches,replaceColumnValueWith or ifRowKeyContains,replaceRowKeySubStrWith or both as they work on different column types
        /// Extended Properties Example
        ///   "columnName": "IdentityProviderType",
        ///   "columnType": "string",
        ///   "ifColumnValueMatches": "Beihai",
        ///   "replaceColumnValueWith": "AADS2S",
        ///   "ifRowKeyContains": "Beihai",
        ///   "replaceRowKeySubStrWith": "AADS2S"
        ///  Activity Operation
        ///     The activity iterates through all the rows from the input table with the matching rowKeyPrefixes,
        ///     checks for the column, apply the column transformation if the column value match is found
        ///     checks for the row key update, apply the row key transformation if the row key match is found
        ///     runs a replace table operation in case of column transformation only
        ///     runs a delete insert operation in case of row key transformation
        /// </summary>
        /// <param name="linkedServices">Linked services referenced by activity definition.</param>
        /// <param name="datasets">Datasets referenced by activity definition.</param>
        /// <param name="activity">Activity definition.</param>
        /// <param name="logger">Used to log messages during activity execution.</param>
        /// <returns>Activity state at the end of execution</returns>
        public IDictionary<string, string> Execute(
            IEnumerable<LinkedService> linkedServices,
            IEnumerable<Dataset> datasets,
            Microsoft.Azure.Management.DataFactories.Models.Activity activity,
            IActivityLogger logger)
        {
            DotNetActivity dotNetActivity = (DotNetActivity)activity.TypeProperties;
            IDictionary<string, string> extendedProperties = dotNetActivity.ExtendedProperties;

            logger.Write("Logging extended properties if any...");
            foreach (KeyValuePair<string, string> entry in extendedProperties)
            {
                logger.Write("<key:{0}> <value:{1}>", entry.Key, entry.Value);
            }

            string[] rowKeyPrefixes = null;
            if (extendedProperties.ContainsKey("rowKeyPrefixes"))
            {
                rowKeyPrefixes = extendedProperties["rowKeyPrefixes"].Split(',');
            }

            bool hasColumnUpdate = false;
            string columnName = string.Empty, columnType = string.Empty, ifColumnValueMatches = string.Empty, replaceColumnValueWith = string.Empty;
            if (extendedProperties.ContainsKey("columnName"))
            {
                columnName = extendedProperties["columnName"];
                columnType = extendedProperties["columnType"];
                ifColumnValueMatches = extendedProperties["ifColumnValueMatches"];
                replaceColumnValueWith = extendedProperties["replaceColumnValueWith"];
                hasColumnUpdate = true;
            }

            bool hasRowKeyUpdate = false;
            string ifRowKeyContains = string.Empty, replaceRowKeySubStrWith = string.Empty;
            if (extendedProperties.ContainsKey("ifRowKeyContains"))
            {
                ifRowKeyContains = extendedProperties["ifRowKeyContains"];
                replaceRowKeySubStrWith = extendedProperties["replaceRowKeySubStrWith"];
                hasRowKeyUpdate = true;
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
                                       where (rowKeyPrefixes == null || rowKeyPrefixes.Length <= 0) ? true : this.IsMatch(s.RowKey, rowKeyPrefixes)
                                       select s).GroupBy(a => a.PartitionKey);

                foreach (IGrouping<string, DynamicTableEntity> g in partitionGroups)
                {
                    TableBatchOperation batch = new TableBatchOperation();
                    foreach (DynamicTableEntity e in g.AsEnumerable())
                    {
                        string cachedRowkey = e.RowKey;
                        IDictionary<string, EntityProperty> cachedProperties = new Dictionary<string, EntityProperty>();
                        foreach (KeyValuePair<string, EntityProperty> p in e.Properties)
                        {
                            cachedProperties.Add(p);
                        }

                        bool recordUpdated = false, requiresDelete = false;
                        if (hasColumnUpdate)
                        {
                            recordUpdated = this.ReplaceIfMatch(e, columnName, columnType, ifColumnValueMatches, replaceColumnValueWith);
                        }

                        if (hasRowKeyUpdate && e.RowKey.Contains(ifRowKeyContains))
                        {
                            e.RowKey = e.RowKey.Replace(ifRowKeyContains, replaceRowKeySubStrWith);
                            recordUpdated = true;
                            requiresDelete = true;
                        }

                        if (recordUpdated)
                        {
                            if (!requiresDelete)
                            {
                                batch.Replace(e);
                            }
                            else
                            {
                                batch.Insert(e);
                                batch.Delete(new DynamicTableEntity(e.PartitionKey, cachedRowkey, "*", cachedProperties));
                            }

                            actualAffectedRecords++;
                            logger.Write("<partition key:{0}>, <row key:{1}> added to batch", e.PartitionKey, e.RowKey);
                        }
                    }

                    if (batch.Count > 0)
                    {
                        tasks.Add(inputTable.ExecuteBatchInChunkAsync(batch));
                    }

                    logger.Write("Updated partition: {0}", g.Key);
                }

                totalProcessedRecords += resultSegment.Results.Count;
                logger.Write("Processed records count: {0}", totalProcessedRecords);
                logger.Write("Affected records count: {0}", actualAffectedRecords);
            }
            while (tableContinuationToken != null);

            Task.WaitAll(tasks.ToArray());
            logger.Write("Updated {0} records", actualAffectedRecords);

            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Replaces column value with a newly provided value if there is an exact match
        /// </summary>
        /// <param name="e">Azure table row as <see cref="DynamicTableEntity"/></param>
        /// <param name="columnName">Column name</param>
        /// <param name="columnType">Column type</param>
        /// <param name="existingValue">Existing value</param>
        /// <param name="newValue">New value</param>
        /// <returns>
        /// Bool indicating if the column value was replaced.
        /// True if the value was replaced.
        /// False if the value was not replaced.
        /// </returns>
        private bool ReplaceIfMatch(
            DynamicTableEntity e,
            string columnName,
            string columnType,
            string existingValue,
            string newValue)
        {
            if (!e.Properties.ContainsKey(columnName))
            {
                return false;
            }

            switch (columnType.ToLower())
            {
                case "string":
                    if (e.Properties[columnName].StringValue == existingValue)
                    {
                        // Replace only if the value matches the specified existingValue
                        e.Properties[columnName].StringValue = newValue;
                        return true;
                    }

                    return false;
                case "int32":
                    if (e.Properties[columnName].Int32Value != null && e.Properties[columnName].Int32Value.Value.ToString() == existingValue)
                    {
                        // Replace only if the value matches the specified existingValue
                        e.Properties[columnName].Int32Value = Convert.ToInt32(newValue);
                        return true;
                    }

                    return false;
                case "bool":
                    if (e.Properties[columnName].BooleanValue != null && e.Properties[columnName].BooleanValue.Value.ToString() == existingValue)
                    {
                        // Replace only if the value matches the specified existingValue
                        e.Properties[columnName].BooleanValue = Convert.ToBoolean(newValue);
                        return true;
                    }

                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the rowkey starts with any of the input tables
        /// </summary>
        /// <param name="rowKey">The rowkey</param>
        /// <param name="rowKeyPrefixes">List of tables</param>
        /// <returns>
        /// bool : true if the row key is a match, false if none of the tables match
        /// </returns>
        private bool IsMatch(string rowKey, string[] rowKeyPrefixes)
        {
            foreach (string table in rowKeyPrefixes)
            {
                if (rowKey.StartsWith(table))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
