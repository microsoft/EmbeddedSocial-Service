// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageRedis
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Server.Tables;
    using SocialPlus.Utils;
    using StackExchange.Redis;

    /// <summary>
    /// specifies whether to use the volatile or persistent instance of the redis cache
    /// </summary>
    internal enum RedisType
    {
        /// <summary>
        /// volatile redis cache
        /// </summary>
        Volatile,

        /// <summary>
        /// persistent redis cache
        /// </summary>
        Persistent
    }

    /// <summary>
    /// Command line utility to connect to a redis cache and directly manipulate certain values in that cache
    /// </summary>
    public class Program
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
        /// Environment name
        /// </summary>
        private static string environmentName;

        /// <summary>
        /// object used to talk to the azure key vault
        /// </summary>
        private static KV kv = null;

        /// <summary>
        /// Log
        /// </summary>
        private static Log log = null;

        /// <summary>
        /// name of ctstore container
        /// </summary>
        private static string containerName = null;

        /// <summary>
        /// name of ctstore table
        /// </summary>
        private static string tableName = null;

        /// <summary>
        /// object key
        /// </summary>
        private static string objKey = null;

        /// <summary>
        /// feed key
        /// </summary>
        private static string feedKey = null;

        /// <summary>
        /// partition key
        /// </summary>
        private static string partitionKey = null;

        /// <summary>
        /// order for rank feed (ascending or descending)
        /// </summary>
        private static Order rankFeedOrder = Order.Descending;

        /// <summary>
        /// action to perform
        /// </summary>
        private static string action;

        /// <summary>
        /// enum indicating which redis instance to use
        /// </summary>
        private static RedisType redisType = RedisType.Volatile;

        /// <summary>
        /// Main program
        /// </summary>
        /// <param name="args">command line args</param>
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

            RedisCache redisCache = null;
            if (redisType == RedisType.Volatile)
            {
                var volatileRedis = await kvReader.ReadValueAsync("VolatileRedisConnectionString");
                if (action == "get-info")
                {
                    volatileRedis += ", allowAdmin=1";
                }

                redisCache = new RedisCache(volatileRedis);
            }
            else if (redisType == RedisType.Persistent)
            {
                var persistentRedis = await kvReader.ReadValueAsync("PersistentRedisConnectionString");
                if (action == "get-info")
                {
                    persistentRedis += ", allowAdmin=1";
                }

                redisCache = new RedisCache(persistentRedis);
            }

            // note that the get-info command does not use a container or a table
            if (action == "get-info")
            {
                var actions = new Actions(redisCache, null, null);
                await actions.GetInfo();
                return;
            }

            var container = LookupContainer(containerName);
            var table = LookupTable(tableName, container);
            if (container != null && table != null)
            {
                var actions = new Actions(redisCache, container, table);
                if (action == "get")
                {
                    await actions.GetObject(partitionKey, objKey);
                }
                else if (action == "get-rank-feed")
                {
                    await actions.GetRankFeed(partitionKey, feedKey, rankFeedOrder);
                }
                else if (action == "delete")
                {
                    await actions.DeleteObject(partitionKey, objKey);
                }
            }
        }

        /// <summary>
        /// Parse the command line arguments and perform some validation checks.
        /// </summary>
        /// <param name="args">command line arguments</param>
        private static void ParseArgs(string[] args)
        {
            string order = null;
            int i = 0;

            while (i < args.Length)
            {
                if (args[i].StartsWith("-Action=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-Action=".Length;
                    action = args[i].Substring(prefixLen);
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-Container=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-Container=".Length;
                    containerName = args[i].Substring(prefixLen);
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-FK=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-FK=".Length;
                    feedKey = args[i].Substring(prefixLen);
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
                else if (args[i].StartsWith("-OK=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-OK=".Length;
                    objKey = args[i].Substring(prefixLen);
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-Order=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-Order=".Length;
                    order = args[i].Substring(prefixLen);

                    // if order is specified, make sure it is a valid choice
                    if (!(order.Equals("ascending", StringComparison.CurrentCultureIgnoreCase) ||
                          order.Equals("descending", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        Console.WriteLine("Usage error: order must be ascending or descending");
                        Usage();
                        Environment.Exit(0);
                    }

                    i++;
                    continue;
                }
                else if (args[i].Equals("-Persistent", StringComparison.CurrentCultureIgnoreCase))
                {
                    redisType = RedisType.Persistent;
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-PK=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-PK=".Length;
                    partitionKey = args[i].Substring(prefixLen);
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-Table=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-Table=".Length;
                    tableName = args[i].Substring(prefixLen);
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Volatile"))
                {
                    redisType = RedisType.Volatile;
                    i++;
                    continue;
                }
                else
                {
                    // default case
                    Console.WriteLine($"Unrecognized parameter: {args[i]}");
                    i++;
                }
            }

            if (string.IsNullOrWhiteSpace(environmentName))
            {
                Console.WriteLine("Usage error: must specify name of environment");
                Usage();
                Environment.Exit(0);
            }

            if (action != "delete" && action != "get" && action != "get-info" && action != "get-rank-feed")
            {
                Console.WriteLine("Usage error: invalid action.");
                Console.WriteLine("             Supported actions are delete, get, get-info, get-rank-feed");
                Usage();
                Environment.Exit(0);
            }

            if (action == "get-rank-feed")
            {
                if (order == null)
                {
                    Console.WriteLine("Usage error: must specify ordering for get-rank-feed");
                    Usage();
                    Environment.Exit(0);
                }

                if (order.Equals("ascending", StringComparison.CurrentCultureIgnoreCase))
                {
                    rankFeedOrder = Order.Ascending;
                }
                else if (order.Equals("descending", StringComparison.CurrentCultureIgnoreCase))
                {
                    rankFeedOrder = Order.Descending;
                }

                if (feedKey == null)
                {
                    Console.WriteLine("Usage error: must specify feed key for get-rank-feed");
                    Usage();
                    Environment.Exit(0);
                }
            }

            if (action == "get" || action == "delete")
            {
                if (objKey == null)
                {
                    Console.WriteLine("Usage error: must specify object key for get or delete");
                    Usage();
                    Environment.Exit(0);
                }
            }

            if (action != "get-info" && (containerName == null || tableName == null || partitionKey == null))
            {
                Console.WriteLine("Usage error: must specify container name, table name, partition key.");
                Usage();
                Environment.Exit(0);
            }

            // avoid accidental modification to a persistent Redis instance in a production environment
            if (action == "delete" && redisType == RedisType.Persistent && ProdConfiguration.IsProduction(environmentName))
            {
                Console.WriteLine("Error! Your configuration modifies a production service. Aborting...");
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Prints a summary of how to correctly use this utility
        /// </summary>
        private static void Usage()
        {
            Console.WriteLine("Usage error:");
            Console.WriteLine("ManageRedis.exe -Name=<environment-name> -Action=<delete|get|get-info|get-rank-feed> [-Volatile|-Persistent]");
            Console.WriteLine("  Action-specific parameters:");
            Console.WriteLine("    delete: -Container=<container-name> -Table=<table-name> -PK=<primary-key-name> -OK=<object-key-name>");
            Console.WriteLine("    get: -Container=<container-name> -Table=<table-name> -PK=<primary-key-name> -OK=<object-key-name>");
            Console.WriteLine("    get-info: none.");
            Console.WriteLine("    get-rank-feed: -Container=<container-name> -Table=<table-name> -PK=<primary-key-name> -FK=<feed-key-name> -Order=<ascending|descending>");
            Console.WriteLine("The -Volatile and -Persistent parameters are optional, if neither is specified then -Volatile is the default.");
        }

        /// <summary>
        /// Lookup the container by name
        /// </summary>
        /// <param name="containerName">container name</param>
        /// <returns>container descriptor object if it exists</returns>
        private static ContainerDescriptor LookupContainer(string containerName)
        {
            // first, scan to find a matching container
            foreach (ContainerIdentifier containerId in Enum.GetValues(typeof(ContainerIdentifier)))
            {
                if (containerId.ToString() == containerName)
                {
                    Console.WriteLine($"Found container {containerName}");
                    return ContainerTableDescriptorProvider.Containers[containerId];
                }
            }

            Console.WriteLine($"Container {containerName} not found.");
            return null;
        }

        /// <summary>
        /// Lookup the table by name
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="desc">container descriptor</param>
        /// <returns>table descriptor object if it exists</returns>
        private static TableDescriptor LookupTable(string tableName, ContainerDescriptor desc)
        {
            if (desc == null)
            {
                return null;
            }

            foreach (TableIdentifier tableId in desc.Tables.Keys)
            {
                if (desc.Tables[tableId].TableName == tableName)
                {
                    Console.WriteLine($"Found table {tableName}.");
                    return desc.Tables[tableId];
                }
            }

            Console.WriteLine($"Table {tableName} not found.");
            return null;
        }
    }
}
