// <copyright file="GenericStoreStats.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.SimpleStats
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using SocialPlus.Logging;
    using SocialPlus.Server;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Utils;

    /// <summary>
    /// Simple statistics for a generic store
    /// </summary>
    public class GenericStoreStats
    {
        /// <summary>
        /// String used to name the setting of the Azure AD client
        /// </summary>
        private static readonly string EmbeddedSocialClientIdSetting = "AADEmbeddedSocialClientId";

        /// <summary>
        /// String used to name the setting of the SocialPlus's cert thumbprint
        /// </summary>
        private static readonly string SocialPlusCertThumbprint = "SocialPlusCertThumbprint";

        /// <summary>
        /// String used to name the setting of the URL to access keyvault
        /// </summary>
        private static readonly string SocialPlusVaultUrlSetting = "KeyVaultUri";

        /// <summary>
        /// cloud table client
        /// </summary>
        private CloudTableClient tableClient;

        /// <summary>
        /// Gets all distinct partition keys that satisfy the filter
        /// </summary>
        /// <param name="tableName">name of table</param>
        /// <param name="filter">filter</param>
        /// <returns>partition keys</returns>
        protected List<string> GetPartitionKeysDistinct(string tableName, string filter)
        {
            return this.GetPartitionKeysIncludingDups(tableName, filter).Distinct().ToList();
        }

        /// <summary>
        /// Gets all partition keys that satisfy the filter. The set return might include duplicates
        /// </summary>
        /// <param name="tableName">name of table</param>
        /// <param name="filter">filter</param>
        /// <returns>partition keys</returns>
        protected List<string> GetPartitionKeysIncludingDups(string tableName, string filter)
        {
            var rows = this.GetRows(tableName, filter);

            List<string> partitionKeys = new List<string>();
            foreach (var r in rows)
            {
                partitionKeys.Add(r.PartitionKey);
            }

            return partitionKeys;
        }

        /// <summary>
        /// Gets the values from a column a string
        /// </summary>
        /// <param name="tableName">name of table</param>
        /// <param name="colName">name of column</param>
        /// <param name="filter">filter</param>
        /// <returns>list of values</returns>
        protected List<string> GetColumnsAsStrings(string tableName, string colName, string filter)
        {
            var rows = this.GetRows(tableName, filter);

            List<string> partitionKeys = new List<string>();
            foreach (var r in rows)
            {
                if (r.Properties.ContainsKey(colName))
                {
                    partitionKeys.Add(r[colName].StringValue);
                }
                else
                {
                    partitionKeys.Add("N/A");
                }
            }

            return partitionKeys;
        }

        /// <summary>
        /// Gets all rows that satisfy the filter.
        /// </summary>
        /// <param name="tableName">name of table</param>
        /// <param name="filter">filter</param>
        /// <returns>rows</returns>
        protected List<DynamicTableEntity> GetRows(string tableName, string filter)
        {
            CloudTable table = this.tableClient.GetTableReference(tableName);
            TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where(filter);
            return table.ExecuteQuery(query).ToList();
        }

        /// <summary>
        /// Initializes the store instance for a specific environment name
        /// </summary>
        /// <param name="environmentName">name of environment</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task Init(string environmentName)
        {
            var sr = new FileSettingsReader(ConfigurationManager.AppSettings["ConfigRelativePath"] + Path.DirectorySeparatorChar + environmentName + ".config");
            var certThumbprint = sr.ReadValue(SocialPlusCertThumbprint);
            var clientID = sr.ReadValue(EmbeddedSocialClientIdSetting);
            var vaultUrl = sr.ReadValue(SocialPlusVaultUrlSetting);
            var storeLocation = StoreLocation.CurrentUser;
            ICertificateHelper cert = new CertificateHelper(certThumbprint, clientID, storeLocation);
            IKeyVaultClient client = new AzureKeyVaultClient(cert);

            var log = new Log(LogDestination.Console, Log.DefaultCategoryName);
            IKV kv = new KV(log, clientID, vaultUrl, certThumbprint, storeLocation, client);
            var kvReader = new KVSettingsReader(sr, kv);

            ConnectionStringProvider csp = new ConnectionStringProvider(kvReader);
            string azureTableStorageConnectionString = await kvReader.ReadValueAsync("AzureStorageConnectionString");

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(azureTableStorageConnectionString);

            // Create the table client.
            this.tableClient = storageAccount.CreateCloudTableClient();
        }
    }
}
