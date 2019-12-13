// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageServerState
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Server;
    using SocialPlus.Server.Blobs;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Server.Managers;
    using SocialPlus.Utils;

    /// <summary>
    /// This program will create (provision) or delete Azure server state
    /// </summary>
    public partial class Program
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
        /// flag that indicates whether the clean or create operation applies to all Azure services
        /// </summary>
        private static bool doAll = false;

        /// <summary>
        /// flag that indicates whether the clean or create operation is performed on Azure blobs
        /// </summary>
        private static bool doBlobs = false;

        /// <summary>
        /// flag that indicates whether the clean or create operation is performed on Azure service bus queues
        /// </summary>
        private static bool doQueues = false;

        /// <summary>
        /// flag that indicates whether the clean or create operation is performed on redis caches
        /// </summary>
        private static bool doRedis = false;

        /// <summary>
        /// flag that indicates whether the clean or create operation is performed on Azure tables
        /// </summary>
        private static bool doTables = false;

        /// <summary>
        /// flag that indicates whether the clean or create operation is performed on the WAD* tables
        /// </summary>
        private static bool doLogs = false;

        /// <summary>
        /// flag that indicates whether the clean or create operation is performed on Azure search indices
        /// </summary>
        private static bool doSearch = false;

        /// <summary>
        /// flag that indicates whether to perform a create operation
        /// </summary>
        private static bool doCreate = false;

        /// <summary>
        /// flag that indicates whether to perform a clean operation
        /// </summary>
        private static bool doClean = false;

        /// <summary>
        /// flag that indicates whether to upgrade the storage version in Azure tables and Redis persistent storage
        /// </summary>
        private static bool doUpgradeStoreVersion = false;

        /// <summary>
        /// version string for new version
        /// </summary>
        private static string newVersion = null;

        /// <summary>
        /// version string for old version
        /// </summary>
        private static string oldVersion = null;

        /// <summary>
        /// flag that indicates whether to perform the requested action without interactive prompts
        /// </summary>
        private static bool forceOperation = false;

        /// <summary>
        /// parameter that provides the name of the environment to operate on
        /// </summary>
        private static string environmentName = null;

        /// <summary>
        /// object used to talk to the azure key vault
        /// </summary>
        private static KV kv = null;

        /// <summary>
        /// Log
        /// </summary>
        private static Log log = null;

        /// <summary>
        /// Main program. Will delete all server state.
        /// Usage: ManageServerState.exe -Name=environment-name [-Clean|-Create|-UpgradeStoreVersion] [-Force] [-OldVersion=version-num] [-NewVersion=version-num] [-All|-Blobs|-Queues|-Redis|-Search|-Tables]
        /// </summary>
        /// <param name="args">command line arguments</param>
        public static void Main(string[] args)
        {
            AsyncMain(args).Wait();
        }

        /// <summary>
        /// Async version of the Main program
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        public static async Task AsyncMain(string[] args)
        {
            ParseArgs(args);

            var sr = new FileSettingsReader(ConfigurationManager.AppSettings["ConfigRelativePath"] + Path.DirectorySeparatorChar + environmentName + ".config");
            var certThumbprint = sr.ReadValue(SocialPlusCertThumbprint);
            var clientID = sr.ReadValue(EmbeddedSocialClientIdSetting);
            var storeLocation = StoreLocation.CurrentUser;
            var vaultUrl = sr.ReadValue(SocialPlusVaultUrlSetting);
            ICertificateHelper cert = new CertificateHelper(certThumbprint, clientID, storeLocation);
            IKeyVaultClient client = new AzureKeyVaultClient(cert);

            log = new Log(LogDestination.Console, Log.DefaultCategoryName);
            kv = new KV(log, clientID, vaultUrl, certThumbprint, storeLocation, client);
            var kvReader = new KVSettingsReader(sr, kv);

            // Create a null connection string provider needed for blobStoreManager and tableStoreManager
            NullConnectionStringProvider connectionStringProvider = new NullConnectionStringProvider();

            if (doUpgradeStoreVersion)
            {
                if (!forceOperation)
                {
                    Console.WriteLine("You must specify the -Force option when using -UpgradeStoreVersion");
                    Console.WriteLine("The -UpgradeStoreVersion option is only intended to be used by our version upgrade scripts");
                    Console.WriteLine("If you are trying to use this by hand, you're probably doing something wrong.");
                    return;
                }

                CTStoreManager tableStoreManager = new CTStoreManager(connectionStringProvider);
                string redisPersistentConnectionString = await kvReader.ReadValueAsync("PersistentRedisConnectionString");
                string azureTableStorageConnectionString = await kvReader.ReadValueAsync("AzureStorageConnectionString");
                await UpgradeStoreVersion(tableStoreManager, azureTableStorageConnectionString, redisPersistentConnectionString);
                return;
            }

            if (doClean)
            {
                DisplayWarning();
            }

            // display current configuration
            await ValidateAndPrintConfiguration(environmentName, kvReader);

            if (forceOperation == false)
            {
                // get user approval
                Console.Write("Are you sure you want to proceed? [y/n] : ");
                ConsoleKeyInfo keyInfo = Console.ReadKey(false);
                if (keyInfo.KeyChar != 'y')
                {
                    return;
                }
            }

            // Mr Clean!!
            Console.WriteLine();
            Console.WriteLine();

            if (doAll || doSearch)
            {
                string searchServiceName = await kvReader.ReadValueAsync("SearchServiceName");
                string searchServiceAdminKey = await kvReader.ReadValueAsync("SearchServiceAdminKey");

                if (doClean)
                {
                    // delete search indices
                    await DeleteSearch(searchServiceName, searchServiceAdminKey);
                }

                if (doCreate)
                {
                    // create search indices
                    await ProvisionSearch(searchServiceName, searchServiceAdminKey);
                }
            }

            if (doAll || doQueues)
            {
                string serviceBusConnectionString = await kvReader.ReadValueAsync("ServiceBusConnectionString");

                if (doClean)
                {
                    // Delete queues
                    await DeleteServiceBusQueues(serviceBusConnectionString);
                }

                if (doCreate)
                {
                    // Create queues
                    await ProvisionServiceBusQueues(serviceBusConnectionString);
                }
            }

            if (doAll || doTables)
            {
                CTStoreManager tableStoreManager = new CTStoreManager(connectionStringProvider);
                string azureTableStorageConnectionString = await kvReader.ReadValueAsync("AzureStorageConnectionString");

                if (doClean)
                {
                    // Delete tables
                    await DeleteAzureTables(azureTableStorageConnectionString);
                }

                if (doCreate)
                {
                    await ProvisionAzureStorageTables(tableStoreManager, azureTableStorageConnectionString);
                }
            }

            if (doAll || doBlobs)
            {
                CBStoreManager blobStoreManager = new CBStoreManager(connectionStringProvider);
                string azureBlobStorageConnectionString = await kvReader.ReadValueAsync("AzureBlobStorageConnectionString");

                if (doClean)
                {
                    // Delete blobs
                    await DeleteAzureBlobs(blobStoreManager, azureBlobStorageConnectionString);
                }

                if (doCreate)
                {
                    await ProvisionAzureStorageBlobs(blobStoreManager, azureBlobStorageConnectionString);
                }
            }

            if (doAll || doRedis)
            {
                if (doClean)
                {
                    // Delete redis cache
                    string redisVolatileConnectionString = await kvReader.ReadValueAsync("VolatileRedisConnectionString") + ", allowAdmin=1";
                    string redisPersistentConnectionString = await kvReader.ReadValueAsync("PersistentRedisConnectionString") + ", allowAdmin=1";
                    DeleteRedisCaches(redisVolatileConnectionString, redisPersistentConnectionString);
                }

                if (doCreate)
                {
                    string redisPersistentConnectionString = await kvReader.ReadValueAsync("PersistentRedisConnectionString");
                    CTStoreManager tableStoreManager = new CTStoreManager(connectionStringProvider);
                    await ProvisionRedisCaches(redisPersistentConnectionString, tableStoreManager);
                }
            }

            if (doAll || doLogs)
            {
                CBStoreManager blobStoreManager = new CBStoreManager(connectionStringProvider);
                string azureBlobStorageConnectionString = await kvReader.ReadValueAsync("AzureBlobStorageConnectionString");

                if (doClean)
                {
                    // Delete logs
                    await DeleteAzureLogs(azureBlobStorageConnectionString);
                }

                if (doCreate)
                {
                    // No need to create the Azure logs (aka WAD* tables). Azure Diagnostics creates them automatically.
                }
            }

            // bye
            Console.WriteLine();
            Console.WriteLine("All done! Bye!");
            Console.WriteLine();
        }

        /// <summary>
        /// Parse the command line arguments and perform some validation checks.
        /// </summary>
        /// <param name="args">command line arguments</param>
        private static void ParseArgs(string[] args)
        {
            int i = 0;
            while (i < args.Length)
            {
                if (args[i].Equals("-All", StringComparison.CurrentCultureIgnoreCase))
                {
                    doAll = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Blobs", StringComparison.CurrentCultureIgnoreCase))
                {
                    doBlobs = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Clean", StringComparison.CurrentCultureIgnoreCase))
                {
                    doClean = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Create", StringComparison.CurrentCultureIgnoreCase))
                {
                    doCreate = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Force", StringComparison.CurrentCultureIgnoreCase))
                {
                    forceOperation = true;
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-Name=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-Name=".Length;
                    environmentName = args[i].Substring(prefixLen);
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-NewVersion=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-NewVersion=".Length;
                    newVersion = args[i].Substring(prefixLen);
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-OldVersion=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-OldVersion=".Length;
                    oldVersion = args[i].Substring(prefixLen);
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Queues", StringComparison.CurrentCultureIgnoreCase))
                {
                    doQueues = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Redis", StringComparison.CurrentCultureIgnoreCase))
                {
                    doRedis = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Search", StringComparison.CurrentCultureIgnoreCase))
                {
                    doSearch = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-UpgradeStoreVersion", StringComparison.CurrentCultureIgnoreCase))
                {
                    doUpgradeStoreVersion = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Tables", StringComparison.CurrentCultureIgnoreCase))
                {
                    doTables = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Logs", StringComparison.CurrentCultureIgnoreCase))
                {
                    doLogs = true;
                    i++;
                    continue;
                }
                else
                {
                    // default case
                    Console.WriteLine("Unrecognized parameter: {0}", args[i]);
                    i++;
                }
            }

            if (string.IsNullOrWhiteSpace(environmentName))
            {
                Console.WriteLine("Usage error: must specify name of environment");
                Usage();
                Environment.Exit(0);
            }

            if (!(doClean || doCreate || doUpgradeStoreVersion))
            {
                Console.WriteLine("Usage error: must specify an action of clean, create, or upgrade store version");
                Usage();
                Environment.Exit(0);
            }

            if (doClean && doCreate)
            {
                Console.WriteLine("Usage error: cannot perform clean and create actions");
                Usage();
                Environment.Exit(0);
            }

            if (doClean && doUpgradeStoreVersion)
            {
                Console.WriteLine("Usage error: cannot perform clean and upgrade actions");
                Usage();
                Environment.Exit(0);
            }

            if (doCreate && doUpgradeStoreVersion)
            {
                Console.WriteLine("Usage error: cannot perform create and upgrade actions");
                Usage();
                Environment.Exit(0);
            }

            // if action is clean or create, then must specify target objects for the action
            if (!doUpgradeStoreVersion && !(doAll || doBlobs || doQueues || doRedis || doSearch || doTables || doLogs))
            {
                Console.WriteLine("Usage error: must specify which objects to clean or create");
                Usage();
                Environment.Exit(0);
            }

            if (doUpgradeStoreVersion && (oldVersion == null || newVersion == null))
            {
                Console.WriteLine("Usage error: must specify old version and new version when upgrading the store version");
                Usage();
                Environment.Exit(0);
            }

            return;
        }

        /// <summary>
        /// print usage error message
        /// </summary>
        private static void Usage()
        {
            Console.WriteLine("Usage: ManageServerState.exe -Name=<environment-name> [-Clean | -Create | -UpgradeStoreVersion] [-Force] [-OldVersion=<version>] [-NewVersion=<version] [-All | -Blobs | -Queues | -Redis | -Search | -Tables | -Logs]");
        }

        /// <summary>
        /// Display a warning message for cleanup actions
        /// </summary>
        private static void DisplayWarning()
        {
            // display warning
            Console.WriteLine();
            if (doClean && doSearch)
            {
                Console.WriteLine("Warning!! this program will erase all Social Plus state from the Azure Search Instance.");
            }
            else if (doClean && doBlobs)
            {
                Console.WriteLine("Warning!! this program will erase all Social Plus state from the Azure Blob storage.");
            }
            else if (doClean && doRedis)
            {
                Console.WriteLine("Warning!! this program will erase all Social Plus state from the Redis caches.");
            }
            else if (doClean && doQueues)
            {
                Console.WriteLine("Warning!! this program will erase all Social Plus state from the Azure Service Bus queues.");
            }
            else if (doClean && doTables)
            {
                Console.WriteLine("Warning!! this program will erase all Social Plus state from the Azure Table storage.");
            }
            else if (doClean && doLogs)
            {
                Console.WriteLine("Warning!! this program will erase all Social Plus logs from the Azure WAD* tables.");
            }
            else if (doClean && doAll)
            {
                Console.WriteLine("Warning!! this program will erase all Social Plus state from Azure.  Everything!  Really!!");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// gets the current config from app.config, make sure it is not a production environment, prints the configuration
        /// </summary>
        /// <param name="environmentName">name of the environment that will be created or cleaned</param>
        /// <param name="kvr">key vault settings reader</param>
        /// <returns>validate and print task</returns>
        private static async Task ValidateAndPrintConfiguration(string environmentName, ISettingsReaderAsync kvr)
        {
            if (doAll || doSearch)
            {
                // check that all of the required settings are not null or whitespace
                if (string.IsNullOrWhiteSpace(await kvr.ReadValueAsync("SearchServiceName")))
                {
                    Console.WriteLine("Error! SearchServiceName in your configuration is null or whitespace. Aborting...");
                    System.Environment.Exit(-1);
                }

                if (string.IsNullOrWhiteSpace(await kvr.ReadValueAsync("SearchServiceAdminKey")))
                {
                    Console.WriteLine("Error! SearchServiceAdminKey in your configuration is null or whitespace. Aborting...");
                    System.Environment.Exit(-1);
                }
            }

            if (doAll || doQueues)
            {
                if (string.IsNullOrWhiteSpace(await kvr.ReadValueAsync("ServiceBusConnectionString")))
                {
                    Console.WriteLine("Error! ServiceBusConnectionString in your configuration is null or whitespace. Aborting...");
                    System.Environment.Exit(-1);
                }
            }

            if (doAll || doTables)
            {
                if (string.IsNullOrWhiteSpace(await kvr.ReadValueAsync("AzureStorageConnectionString")))
                {
                    Console.WriteLine("Error! AzureStorageConnectionString in your configuration is null or whitespace. Aborting...");
                    System.Environment.Exit(-1);
                }
            }

            if (doAll || doBlobs || doLogs)
            {
                if (string.IsNullOrWhiteSpace(await kvr.ReadValueAsync("AzureBlobStorageConnectionString")))
                {
                    Console.WriteLine("Error! AzureBlobStorageConnectionString in your configuration is null or whitespace. Aborting...");
                    System.Environment.Exit(-1);
                }
            }

            if (doAll || doRedis)
            {
                if (string.IsNullOrWhiteSpace(await kvr.ReadValueAsync("VolatileRedisConnectionString")))
                {
                    Console.WriteLine("Error! VolatileRedisConnectionString in your configuration is null or whitespace. Aborting...");
                    System.Environment.Exit(-1);
                }

                if (string.IsNullOrWhiteSpace(await kvr.ReadValueAsync("PersistentRedisConnectionString")))
                {
                    Console.WriteLine("Error! PersistentRedisConnectionString in your configuration is null or whitespace. Aborting...");
                    System.Environment.Exit(-1);
                }
            }

            // for clean operations, make sure we are not operating on a production service
            if (doClean && (ProdConfiguration.IsProduction(await kvr.ReadValueAsync("SearchServiceName")) ||
                ProdConfiguration.IsProduction(await kvr.ReadValueAsync("ServiceBusConnectionString")) ||
                ProdConfiguration.IsProduction(await kvr.ReadValueAsync("AzureStorageConnectionString")) ||
                ProdConfiguration.IsProduction(await kvr.ReadValueAsync("AzureBlobStorageConnectionString")) ||
                ProdConfiguration.IsProduction(await kvr.ReadValueAsync("VolatileRedisConnectionString")) ||
                ProdConfiguration.IsProduction(await kvr.ReadValueAsync("PersistentRedisConnectionString"))))
            {
                Console.WriteLine("Error! Your configuration includes a production service. Aborting...");
                System.Environment.Exit(-1);
            }

            Console.WriteLine();
            Console.Write("Environment name: ");
            Console.WriteLine(environmentName);
            Console.WriteLine();
            Console.WriteLine("Current configuration:");
            if (doAll || doSearch)
            {
                Console.WriteLine("\tsearch service name: " + await kvr.ReadValueAsync("SearchServiceName"));
                Console.WriteLine("\tsearch admin key: " + await kvr.ReadValueAsync("SearchServiceAdminKey"));
            }

            if (doAll || doQueues)
            {
                Console.WriteLine("\tservice bus connection string: " + await kvr.ReadValueAsync("ServiceBusConnectionString"));
            }

            if (doAll || doTables)
            {
                Console.WriteLine("\tazure table storage string: " + await kvr.ReadValueAsync("AzureStorageConnectionString"));
            }

            if (doAll || doBlobs || doLogs)
            {
                Console.WriteLine("\tazure blob storage string: " + await kvr.ReadValueAsync("AzureBlobStorageConnectionString"));
            }

            if (doAll || doRedis)
            {
                Console.WriteLine("\tredis connection strings: ");
                Console.WriteLine("\t Volatile = {0}", await kvr.ReadValueAsync("VolatileRedisConnectionString"));
                Console.WriteLine("\t Persistent = {0}", await kvr.ReadValueAsync("PersistentRedisConnectionString"));
            }

            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// upgrade the store version number in Azure table storage and persistent redis
        /// </summary>
        /// <param name="tableStoreManager">ctstore manager</param>
        /// <param name="azureConnectionString">azure connection string</param>
        /// <param name="persistentCacheConnectionString">persistent redis connection string</param>
        /// <returns>upgrade version task</returns>
        private static async Task UpgradeStoreVersion(CTStoreManager tableStoreManager, string azureConnectionString, string persistentCacheConnectionString)
        {
            bool result = await UpgradeStoreVersionAzureTables(tableStoreManager, azureConnectionString);
            bool result2 = await UpgradeStoreVersionRedis(tableStoreManager, persistentCacheConnectionString);

            if (!result)
            {
                Console.WriteLine("Version upgrade failed for azure table storage");
            }

            if (!result2)
            {
                Console.WriteLine("Version upgrade failed for persistent redis storage");
            }

            if (result && result2)
            {
                Console.WriteLine("Store version upgrade succeeded.");
            }

            return;
        }
    }
}
