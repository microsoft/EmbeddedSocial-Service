// <copyright file="Extensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.DataFactory
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Class for Extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Execute batch operation on the cloudTable in multiples of 100
        /// </summary>
        /// <param name="cloudTable">Cloud table</param>
        /// <param name="batch">TableBatchOperation</param>
        /// <returns>Task</returns>
        public static async Task ExecuteBatchInChunkAsync(this CloudTable cloudTable, TableBatchOperation batch)
        {
            List<Task> tasks = new List<Task>();
            var batchOperation = new TableBatchOperation();
            foreach (var item in batch)
            {
                batchOperation.Add(item);
                if (batchOperation.Count == 100)
                {
                    tasks.Add(cloudTable.ExecuteBatchAsync(batchOperation));
                    batchOperation = new TableBatchOperation();
                }
            }

            if (batchOperation.Count > 0)
            {
                tasks.Add(cloudTable.ExecuteBatchAsync(batchOperation));
            }

            await Task.WhenAll(tasks.ToArray());
        }
    }
}