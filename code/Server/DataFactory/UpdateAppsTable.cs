// <copyright file="UpdateAppsTable.cs" company="Microsoft">
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
    /// Data factory activity updates Apps table contents.
    /// The activity iterates through all the rows from the Apps table with the matching rowKeyPrefixes, checks if the column is present,
    /// updates the column value if the partition key belongs to the app handles associated with the owner specified in partitionKeyOwnerValueRule
    /// </summary>
    public class UpdateAppsTable : IDotNetActivity
    {
        /// <summary>
        /// List of Owners and their AppHandles
        /// </summary>
        private static Dictionary<string, HashSet<string>> ownerAppHandles = new Dictionary<string, HashSet<string>>()
        {
            { "SMU", new HashSet<string>() { "4761N0kVtnG", "4AP_t8asGhE", "48-60hx0huQ", "48-6Egq_peI", "4ANDnB1PU_C" } },
            { "Academic", new HashSet<string>() { "4AN5TM_0a6P", "466jCM1nEGs", "466jLR_2gE7", "466jYXMrPwJ" } },
            { "EndToEndTests", new HashSet<string>() { "4ARFEjGm19x", "4ANEuLaaAvU", "4AOsFL2NMLi", "49RkiWyq8AP" } },
            {
                "Beihai", new HashSet<string>()
            {
                "476WO3Cc8NE", "476WOjOYXTC", "476WPMigV5q", "476WQ0WFnhh", "476WQgE8FZk",
                "476WRSXY5H3", "476WSFjGxwD", "476WSvIFc8u", "476WTZBISU4", "476WUEE-rko", "476WUwuW1JI", "476WVcSizcg", "476WWGRqZUi",
                "476WWwyjMOG", "476WciwWOQG", "49N9OqC8zyG", "49nzCIEN3Lh", "49nzD6wdgw0", "49nzEsTyomK", "4AO51em6Jjt",
                "476MLeTWAwp", "476MNIRma78", "476MTJvYI8H", "476MUQTo2uN", "476MVSC784T", "476MWUnz6h_", "476MXTm_UIb", "476MYQq0Q8u",
                "476MZT-5Rq1", "476M_dIRNPS", "49NDry_9fKp", "49NE-Bjlvas", "49NEFVauaO3"
            }
            }
        };

        /// <summary>
        /// Updates a column value in Apps Azure table.
        /// The column to be transformed is specified using the following extended properties
        /// Extended Properties
        ///     columnName - Name of the column to be added
        ///     columnType - Data type of the column. Only supported types right now are: int32, bool, and string
        ///     rowKeyPrefixes - Rowkey prefixes of the rows in which the column update will be applied. This is optional and will identify the subset of rows to do this operation.
        ///     partitionKeyOwnerValueRule - The updates are specified using the partition key owner and the value for it in the ; separated key-value format.
        /// Extended Properties Example
        ///   "columnName": "DisableHandleValidation",
        ///   "columnType": "bool",
        ///   "rowKeyPrefix": "ProfilesObject:"
        ///   "partitionKeyOwnerValueRule": "Beihai=true;EndToEndTests=true"
        ///  Activity Operation
        ///     The activity iterates through all the rows from the input table with the matching rowKeyPrefixes,
        ///     checks if the column is present, updates the column value if the partition key belongs to the app handles
        ///     associated with the owner specified in partitionKeyOwnerValueRule
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

            if (!extendedProperties.ContainsKey("columnType"))
            {
                throw new ArgumentException("Column Type information is required", "columnType");
            }

            string columnType = extendedProperties["columnType"];

            // Note that partitionKeyOwnerValueRule is required as the rules for updating value comes from it
            // We do not update column value with default value if the matching rule is not found. The record is ignored. All rules need to be explicitly specified
            if (!extendedProperties.ContainsKey("partitionKeyOwnerValueRule"))
            {
                throw new ArgumentException("PartitionKeyOwnerValueRule information is required", "partitionKeyOwnerValueRule");
            }

            string partitionKeyOwnerValueRule = extendedProperties["partitionKeyOwnerValueRule"];

            string[] rowKeyPrefixes = null;
            if (extendedProperties.ContainsKey("rowKeyPrefixes"))
            {
                rowKeyPrefixes = extendedProperties["rowKeyPrefixes"].Split(',');
            }

            var partitionKeyOwnerValueRuleDict = partitionKeyOwnerValueRule.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(part => part.Split('='))
               .ToDictionary(split => split[0], split => split[1]);

            var appHandles = ownerAppHandles.Where(item => partitionKeyOwnerValueRuleDict.ContainsKey(item.Key)).SelectMany(item => item.Value).ToList();

            logger.Write("Matching appHandles:{0}", string.Join(",", appHandles));

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
                        // If appHandles do not contain the partition key, Continue
                        if (!appHandles.Contains(e.PartitionKey))
                        {
                            continue;
                        }
                        else
                        {
                            // Pick the value to be used for specified AppHandle
                            // This is done by getting the owber key first from e.PartitionKey
                            var ownerKey = ownerAppHandles.FirstOrDefault(x => x.Value.Contains(e.PartitionKey)).Key;

                            // The owner key is used to pick the value for the column
                            string newColumnValue = partitionKeyOwnerValueRuleDict[ownerKey];
                            if (this.ReplaceColumnValue(e, columnName, columnType, newColumnValue))
                            {
                                batch.Merge(e);
                                logger.Write("<partition key:{0}>, <row key:{1}>", e.PartitionKey, e.RowKey);
                            }
                        }
                    }

                    if (batch.Count > 0)
                    {
                        tasks.Add(inputTable.ExecuteBatchInChunkAsync(batch));
                        actualAffectedRecords += batch.Count;
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
        /// Replaces column value with a newly provided value
        /// </summary>
        /// <param name="e">Azure table row as <see cref="DynamicTableEntity"/></param>
        /// <param name="columnName">Column name</param>
        /// <param name="columnType">Column type</param>
        /// <param name="newValue">New value</param>
        /// <returns>
        /// Bool indicating if the column value was replaced.
        /// True if the value was replaced.
        /// False if the value was not replaced.
        /// </returns>
        private bool ReplaceColumnValue(
            DynamicTableEntity e,
            string columnName,
            string columnType,
            string newValue)
        {
            if (!e.Properties.ContainsKey(columnName))
            {
                return false;
            }

            switch (columnType.ToLower())
            {
                case "string":
                    e.Properties[columnName].StringValue = newValue;
                    return true;
                case "int32":
                    e.Properties[columnName].Int32Value = Convert.ToInt32(newValue);
                    return true;
                case "bool":
                    e.Properties[columnName].BooleanValue = Convert.ToBoolean(newValue);
                    return true;
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
