// <copyright file="Tables.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageServerState
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// portion of Program class that deals with creating and deleting tables
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// Provision azure table storage with tables
        /// </summary>
        /// <param name="tableStoreManager">table store manager</param>
        /// <param name="azureConnectionString">Azure connection string</param>
        /// <returns>provision task</returns>
        private static async Task ProvisionAzureStorageTables(CTStoreManager tableStoreManager, string azureConnectionString)
        {
            // Creates Social Plus containers/tables defined in ContainerIdentifier.cs/TableIdentifier.cs
            // Containers map to tables in Azure table storage (container names map to table names)
            // We create them (if not exists) through the CTStore interface
            Console.WriteLine("Creating all tables in Azure Table Store...");

            // Get azure table storage with the give connection string
            AzureTableStorage azureTableStorage = new AzureTableStorage(azureConnectionString);

            // Get CTStore using the azure table storage
            CTStore store = new CTStore(azureTableStorage, null);

            // Enumerate all the containers defined
            foreach (ContainerIdentifier containerIdentifier in Enum.GetValues(typeof(ContainerIdentifier)))
            {
                if (!ContainerTableDescriptorProvider.Containers.ContainsKey(containerIdentifier))
                {
                    Console.WriteLine("  " + containerIdentifier.ToString() + " - Descriptor not found");
                    continue;
                }

                // in Azure, table deletion can potentially take a long time.
                // this may lead to conflict exceptions if you delete a table and then attempt to
                // recreate it.  Below, we retry once every 30 seconds for up to 5 minutes if needed.

                // wait up to 5 minutes before giving up on table creation
                int attempts = 0;
                int maxAttempts = 10;
                while (true)
                {
                    try
                    {
                        attempts++;

                        // create the container
                        await store.CreateContainerAsync(containerIdentifier.ToString());

                        // if we reach here, the create was successful
                        break;
                    }
                    catch (ConflictException e)
                    {
                        if (attempts < maxAttempts)
                        {
                            // sleep for 30 seconds before trying
                            await Task.Delay(30 * 1000);
                        }
                        else
                        {
                            // give up after reaching maxAttempts
                            throw e;
                        }
                    }
                }

                Console.WriteLine("  " + containerIdentifier.ToString() + " - Table Container Provisioned");
            }

            // insert the store version number into table storage
            var versionEntity = new StoreVersionEntity { Version = tableStoreManager.StoreVersionString };

            // the StoreVersion container has no descriptor, so we need to create it
            await store.CreateContainerAsync(ContainerIdentifier.ServiceConfig.ToString());
            Console.WriteLine("  " + ContainerIdentifier.ServiceConfig.ToString() + " - Table Container Provisioned");
            ObjectTable versionTable = Table.GetObjectTable(
                ContainerIdentifier.ServiceConfig.ToString(),
                tableStoreManager.ServiceConfigContainerInitials,
                tableStoreManager.StoreVersionTableName,
                tableStoreManager.StoreVersionTableInitials,
                StorageMode.PersistentOnly);
            var operation = Operation.Insert(versionTable, tableStoreManager.StoreVersionKey, tableStoreManager.StoreVersionKey, versionEntity);

            // perform the insert on Azure table storage
            await store.ExecuteOperationAsync(operation, ConsistencyMode.Strong);
            Console.WriteLine("  SocialPlusStoreVersion number provisioned");
        }

        /// <summary>
        /// Deletes all the Azure tables
        /// </summary>
        /// <param name="azureTableStorageConnectionString">connection string to azure table storage</param>
        /// <returns>delete task</returns>
        private static async Task DeleteAzureTables(string azureTableStorageConnectionString)
        {
            Console.WriteLine("Deleting all tables from Azure Table Store...");

            // Get azure table storage with the give connection string
            AzureTableStorage azureTableStorage = new AzureTableStorage(azureTableStorageConnectionString);

            // Get CTStore using only azure table storage
            CTStore store = new CTStore(azureTableStorage, null);

            // Enumerate all the containers defined
            foreach (ContainerIdentifier containerIdentifier in Enum.GetValues(typeof(ContainerIdentifier)))
            {
                if (!ContainerTableDescriptorProvider.Containers.ContainsKey(containerIdentifier))
                {
                    Console.WriteLine("  " + containerIdentifier.ToString() + " - Descriptor not found");
                    continue;
                }

                // delete each table container
                await store.DeleteContainerAsync(containerIdentifier.ToString());

                Console.WriteLine("  " + containerIdentifier.ToString() + " - Table Container Deleted");
            }

            // clean up the StoreVersion container because it has no descriptor
            await store.DeleteContainerAsync(ContainerIdentifier.ServiceConfig.ToString());
            Console.WriteLine("  " + ContainerIdentifier.ServiceConfig.ToString() + " - Table Container Deleted");
        }

        /// <summary>
        /// Upgrade the store version number in azure table storage
        /// </summary>
        /// <param name="tableStoreManager">table store manager</param>
        /// <param name="azureConnectionString">azure connection string</param>
        /// <returns>true if upgrade is successful</returns>
        private static async Task<bool> UpgradeStoreVersionAzureTables(CTStoreManager tableStoreManager, string azureConnectionString)
        {
            // Get azure table storage with the give connection string
            AzureTableStorage azureTableStorage = new AzureTableStorage(azureConnectionString);

            // Get CTStore using the azure table storage
            CTStore store = new CTStore(azureTableStorage, null);

            ObjectTable versionTable = Table.GetObjectTable(
                ContainerIdentifier.ServiceConfig.ToString(),
                tableStoreManager.ServiceConfigContainerInitials,
                tableStoreManager.StoreVersionTableName,
                tableStoreManager.StoreVersionTableInitials,
                StorageMode.PersistentOnly);

            var currentTableVersion = await azureTableStorage.QueryObjectAsync<StoreVersionEntity>(versionTable, tableStoreManager.StoreVersionKey, tableStoreManager.StoreVersionKey);

            // if version in store does not match oldVersion, then refuse to upgrade
            if (currentTableVersion.Version != oldVersion)
            {
                Console.WriteLine("Version mismatch in Azure table storage: original version in store is {0}", currentTableVersion.Version);
                return false;
            }

            currentTableVersion.Version = newVersion;
            var operation = Operation.Replace(versionTable, tableStoreManager.StoreVersionKey, tableStoreManager.StoreVersionKey, currentTableVersion);

            // perform the insert on Azure table storage
            await store.ExecuteOperationAsync(operation, ConsistencyMode.Strong);
            return true;
        }
    }
}
