// <copyright file="TransformPartitionKey.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.DataFactory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Microsoft.Azure.Management.DataFactories.Models;
    using Microsoft.Azure.Management.DataFactories.Runtime;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Data factory activity transforms partition key
    /// </summary>
    public class TransformPartitionKey : IDotNetActivity
    {
        /// <summary>
        /// If Azure Tables times out, then retry an operation these many times
        /// </summary>
        private static int numRetriesOnTimeout = 5;

        /// <summary>
        /// If Azure Tables times out, then retry an operation after waiting these many milliseconds
        /// </summary>
        private static int numMsDelayOnTimeout = 5000;

        /// <summary>
        /// Transforms Azure table partition key
        /// The partition key to be transformed is specified using the following extended properties
        /// Extended Properties
        ///     ifPartitionKeyContains - The transformation is applied only if the contents of partition key contains the specified value.
        ///     replacePartitionKeySubStrWith - Replace the contents of the matched partition key with the specified value to generate a new partition key.
        ///     rowKeyPrefixes - Rowkey prefixes of the rows in which the partition key transformation will be applied. This is optional and will identify the subset of rows to do this operation.
        /// ifPartitionKeyContains,replacePartitionKeySubStrWith are mandatory
        /// Extended Properties Example
        ///   "ifPartitionKeyContains": "Beihai",
        ///   "replacePartitionKeySubStrWith": "AADS2S"
        ///  Activity Operation
        ///     The activity iterates through all the rows from the input table with the matching rowKeyPrefixes,
        ///     checks for the partition key update, apply the partition key transformation if the partition key match is found
        ///     runs an insert operation for entities with new partition key and delete operation on existing entities with matching partition keys
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

            if (!extendedProperties.ContainsKey("ifPartitionKeyContains"))
            {
                throw new ArgumentException("Partition key match criteria is required", "ifPartitionKeyContains");
            }

            if (!extendedProperties.ContainsKey("replacePartitionKeySubStrWith"))
            {
                throw new ArgumentException("Partition key substring replacement value is required", "replacePartitionKeySubStrWith");
            }

            string ifPartitionKeyContains = extendedProperties["ifPartitionKeyContains"];
            string replacePartitionKeySubStrWith = extendedProperties["replacePartitionKeySubStrWith"];

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
                    TableBatchOperation deleteBatch = new TableBatchOperation();
                    TableBatchOperation insertBatch = new TableBatchOperation();
                    foreach (DynamicTableEntity e in g.AsEnumerable())
                    {
                        if (!e.PartitionKey.Contains(ifPartitionKeyContains))
                        {
                            continue;
                        }

                        DynamicTableEntity newEntity = new DynamicTableEntity(
                            e.PartitionKey.Replace(ifPartitionKeyContains, replacePartitionKeySubStrWith),
                            e.RowKey);
                        foreach (KeyValuePair<string, EntityProperty> property in e.Properties)
                        {
                            newEntity.Properties.Add(property);
                        }

                        insertBatch.InsertOrReplace(newEntity);
                        deleteBatch.Delete(e);
                        actualAffectedRecords++;
                    }

                    if (insertBatch.Count > 0)
                    {
                        tasks.Add(this.RetryOnStorageTimeout(inputTable.ExecuteBatchInChunkAsync(insertBatch), numRetriesOnTimeout, numMsDelayOnTimeout, logger));
                    }

                    if (deleteBatch.Count > 0)
                    {
                        tasks.Add(this.RetryOnStorageTimeout(inputTable.ExecuteBatchInChunkAsync(deleteBatch), numRetriesOnTimeout, numMsDelayOnTimeout, logger));
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

        /// <summary>
        /// Execute the given task and if it gets a timeout exception from Azure Table Storage, retry it
        /// </summary>
        /// <param name="inputTask">task to execute</param>
        /// <param name="retries">number of retries to attempt</param>
        /// <param name="msDelay">number of milliseconds to wait before trying again</param>
        /// <param name="logger">Used to log messages during task execution.</param>
        /// <returns>task that will run the input task at least once</returns>
        private async Task RetryOnStorageTimeout(Task inputTask, int retries, int msDelay, IActivityLogger logger)
        {
            int attempts = 0;

            for (; ;)
            {
                try
                {
                    // execute the input task
                    attempts++;
                    await inputTask;

                    // break if successful
                    break;
                }
                catch (StorageException e)
                {
                    // is it a timeout exception?
                    if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.RequestTimeout)
                    {
                        if (retries <= 0)
                        {
                            // if we are all out of retries, then log, throw exception and quit
                            logger.Write("****** Failing after {0} attempts", attempts);
                            throw e;
                        }

                        // otherwise, then go ahead and retry again after a short delay
                        await Task.Delay(msDelay);
                        retries--;
                    }
                    else
                    {
                        // not a timeout; throw the exception
                        throw e;
                    }
                }
            }
        }
    }
}
