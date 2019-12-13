// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageApps
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
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;
    using SocialPlus.Utils;

    /// <summary>
    /// main program
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
        /// String used to name the setting of the service bus batch interval
        /// </summary>
        private static readonly string ServiceBusBatchIntervalMsSetting = "ServiceBusBatchIntervalMs";

        /// <summary>
        /// parameter that provides the name of the environment to operate on
        /// </summary>
        private static string environmentName = null;

        /// <summary>
        /// handle generator
        /// </summary>
        private readonly HandleGenerator handleGenerator = new HandleGenerator();

        /// <summary>
        /// apps manager
        /// </summary>
        private IAppsManager appsManager = null;

        /// <summary>
        /// users manager
        /// </summary>
        private IUsersManager usersManager = null;

        /// <summary>
        /// enum listing the possible actions
        /// </summary>
        private enum Action
        {
            CreateApp,
            CreateAppAdmin,
            CreateAppAndDeveloper,
            CreateAppKey,
            CreateClientNameAndConfig,
            CreateUserAsAppAdmin,
            DeleteApp,
            DeleteAppAdmin,
            DeleteAppKey,
            DeleteClientNameAndConfig,
            GetApp,
            GetAppAdmin,
            GetAppDeveloperId,
            GetAppHandle,
            GetAppKeys,
            GetAppList,
            GetIdentityProvider,
            GetPush,
            GetValidation,
            None,
            UpdateAppProfile,
            UpdateIdentityProvider,
            UpdatePush,
            UpdateValidation
        }

        /// <summary>
        /// Main program.
        /// </summary>
        /// <param name="args">command line arguments</param>
        public static void Main(string[] args)
        {
            // parse the arguments (into statics)
            var action = ParseAction(args);
            var p = new Program();
            p.Initialize();
            p.PerformAction(action, args).Wait();
        }

        /// <summary>
        /// Parse the command line arguments and perform some validation checks.
        /// </summary>
        /// <param name="args">command line arguments</param>
        /// <returns>the action to perform</returns>
        private static Action ParseAction(string[] args)
        {
            Action action = Action.None;

            if (args.Length > 0 && args[0].StartsWith("-Name=", StringComparison.CurrentCultureIgnoreCase))
            {
                int prefixLen = "-Name=".Length;
                environmentName = args[0].Substring(prefixLen);
            }
            else
            {
                Usage();
                Environment.Exit(0);
            }

            if (args.Length > 1)
            {
                if (args[1].Equals("-CreateApp", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.CreateApp;
                }
                else if (args[1].Equals("-CreateAppAdmin", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.CreateAppAdmin;
                }
                else if (args[1].Equals("-CreateAppKey", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.CreateAppKey;
                }
                else if (args[1].Equals("-CreateAppAndDeveloper", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.CreateAppAndDeveloper;
                }
                else if (args[1].Equals("-CreateClientNameAndConfig", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.CreateClientNameAndConfig;
                }
                else if (args[1].Equals("-CreateUserAsAppAdmin", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.CreateUserAsAppAdmin;
                }
                else if (args[1].Equals("-DeleteApp", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.DeleteApp;
                }
                else if (args[1].Equals("-DeleteAppAdmin", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.DeleteAppAdmin;
                }
                else if (args[1].Equals("-DeleteAppKey", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.DeleteAppKey;
                }
                else if (args[1].Equals("-DeleteClientNameAndConfig", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.DeleteClientNameAndConfig;
                }
                else if (args[1].Equals("-GetApp", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.GetApp;
                }
                else if (args[1].Equals("-GetAppAdmin", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.GetAppAdmin;
                }
                else if (args[1].Equals("-GetAppDeveloperId", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.GetAppDeveloperId;
                }
                else if (args[1].Equals("-GetAppHandle", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.GetAppHandle;
                }
                else if (args[1].Equals("-GetAppKeys", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.GetAppKeys;
                }
                else if (args[1].Equals("-GetAppList", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.GetAppList;
                }
                else if (args[1].Equals("-GetIdentityProvider", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.GetIdentityProvider;
                }
                else if (args[1].Equals("-GetPush", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.GetPush;
                }
                else if (args[1].Equals("-GetValidation", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.GetValidation;
                }
                else if (args[1].Equals("-UpdateAppProfile", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.UpdateAppProfile;
                }
                else if (args[1].Equals("-UpdateIdentityProvider", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.UpdateIdentityProvider;
                }
                else if (args[1].Equals("-UpdatePush", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.UpdatePush;
                }
                else if (args[1].Equals("-UpdateValidation", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.UpdateValidation;
                }
                else
                {
                    Usage();
                    Environment.Exit(0);
                }
            }
            else
            {
                Usage();
                Environment.Exit(0);
            }

            if (string.IsNullOrWhiteSpace(environmentName))
            {
                Console.WriteLine("Usage error: environment name not specified.");
                Usage();
                Environment.Exit(0);
            }

            if (action == Action.None)
            {
                Console.WriteLine("Usage error: must specify one action to perform.");
                Usage();
                Environment.Exit(0);
            }

            return action;
        }

        /// <summary>
        /// Print message describing the command line options
        /// </summary>
        private static void Usage()
        {
            Console.WriteLine("Usage: ManageApps.exe -Name=<environment-name> <actions> <action-parameters>");
            Console.WriteLine("Actions: -CreateApp|-CreateAppAdmin|-CreateAppAndDeveloper|-CreateAppKey|-CreateUserAsAppAdmin|-DeleteApp|-DeleteAppAdmin|-DeleteAppKey|-GetApp|-GetAppAdmin|-GetAppDeveloperId|-GetAppHandle|-GetAppKeys|-GetAppList|-GetIdentityProvider|-GetPush|-GetValidation|-UpdateAppProfile|-UpdateIdentityProvider|-UpdatePush|-UpdateValidation");
            Console.WriteLine("Note: -Name=<environment-name> must be the first parameter, and the <action> must be the second parameter.");
            Console.WriteLine();
            Console.WriteLine("Action parameters:");
            Console.WriteLine(" For -CreateApp: -DeveloperId=<id> -AppName=<app-name> -PlatformType=<Windows|Android|IOS> [-AppDeepLink=<url>] [-AppIconUrl=<url>] [-AppStoreUrl=<url>] [-DisableHandleValidation=<true|false>]");
            Console.WriteLine(" For -CreateAppAdmin -AppHandle=<app-handle> -UserHandle=<user-handle>");
            Console.WriteLine(" For -CreateAppAndDeveloper: -AppName=<app-name> -PlatformType=<Windows|Android|IOS> [-AppDeepLink=<url>] [-AppIconUrl=<url>] [-AppStoreLink=<url>] [-DisableHandleValidation=<true|false>]");
            Console.WriteLine(" For -CreateAppKey: -DeveloperId=<id> -AppHandle=<app-handle> [-AppKey=<app-key>]");
            Console.WriteLine(" For -CreateClientNameAndConfig: -AppKey=<app-key> -ClientName=<client-name> [-ClientSideAppKey=<app-key>] [-ClientConfigJson=<client-config-json>]");
            Console.WriteLine(" For -CreateUserAsAppAdmin: -AppHandle=<app-handle> -FirstName=<first name> -LastName=<last name> -IdentityProvider=<Google|Facebook|Microsoft|Twitter|AADS2S> [-IdentityProviderAccountId=<id>] [-Bio=<bio>]");
            Console.WriteLine(" For -DeleteApp: -DeveloperId=<id> -AppHandle=<app-handle>");
            Console.WriteLine(" For -DeleteAppAdmin -AppHandle=<app-handle> -UserHandle=<user-handle>");
            Console.WriteLine(" For -DeleteAppKey: -DeveloperId=<id> -AppHandle=<app-handle> -AppKey=<app-key>");
            Console.WriteLine(" For -DeleteClientNameAndConfig: -AppKey=<app-key> -ClientName=<client-name>");
            Console.WriteLine(" For -GetApp: -AppHandle=<app-handle>");
            Console.WriteLine(" For -GetAppAdmin: -AppHandle=<app-handle> -UserHandle=<user-handle>");
            Console.WriteLine(" For -GetAppDeveloperId: -AppHandle=<app-handle>");
            Console.WriteLine(" For -GetAppHandle: -AppKey=<app-key>");
            Console.WriteLine(" For -GetAppKeys: -AppHandle=<app-handle>");
            Console.WriteLine(" For -GetAppList: -DeveloperId=<id>");
            Console.WriteLine(" For -GetIdentityProvider: -AppHandle=<app-handle> -IdentityProvider=<Google|Facebook|Microsoft|Twitter|AADS2S>");
            Console.WriteLine(" For -GetPush: -AppHandle=<app-handle> -PlatformType=<Windows|Android|IOS>");
            Console.WriteLine(" For -GetValidation: -AppHandle=<app-handle>");
            Console.WriteLine(" For -UpdateAppProfile: -DeveloperId=<id> -AppHandle=<app-handle> -PlatformType=<Windows|Android|IOS> [-AppDeepLink=<url>] [-AppIconUrl=<url>] [-AppStoreLink=<url>] [-DisableHandleValidation=<true|false>]");
            Console.WriteLine(" For -UpdateIdentityProvider: -AppHandle=<app-handle> -DeveloperId=<id> -IdentityProvider=<Google|Facebook|Microsoft|Twitter|AADS2S> -ClientId=<id> -ClientSecret=<key> -RedirectUri=<path>");
            Console.WriteLine(" For -UpdatePush: -AppHandle=<app-handle> -DeveloperId=<id> [-EnablePush|-DisablePush] -PlatformType=<Windows|Android|IOS> [-GoogleApiKey=<key>] [-AppleCertKey=<key>] [-AppleCertPath=<path>] [-WindowsSecretKey=<key>] [-WindowsPackageSID=<sid>]");
            Console.WriteLine(" For -UpdateValidation: -AppHandle=<app-handle> -DeveloperId=<id> [-EnableValidation|-DisableValidation] [-ValidateText=<true|false>] [-ValidateImages=<true|false>] [-AllowMatureContent=<true|false>] [-UserReportThreshold=<#>] [-ContentReportThreshold=<#>]");
        }

        /// <summary>
        /// Initialization routine
        /// </summary>
        private void Initialize()
        {
            // load the environment configuration file from UtilsInternal
            var sr = new FileSettingsReader(ConfigurationManager.AppSettings["ConfigRelativePath"] + Path.DirectorySeparatorChar + environmentName + ".config");
            var certThumbprint = sr.ReadValue(SocialPlusCertThumbprint);
            var clientID = sr.ReadValue(EmbeddedSocialClientIdSetting);
            var storeLocation = StoreLocation.CurrentUser;
            var vaultUrl = sr.ReadValue(SocialPlusVaultUrlSetting);
            ICertificateHelper cert = new CertificateHelper(certThumbprint, clientID, storeLocation);
            IKeyVaultClient client = new AzureKeyVaultClient(cert);

            var log = new Log(LogDestination.Console, Log.DefaultCategoryName);
            var kv = new KV(log, clientID, vaultUrl, certThumbprint, storeLocation, client);
            var kvReader = new KVSettingsReader(sr, kv);
            IConnectionStringProvider connectionStringProvider = new ConnectionStringProvider(kvReader);
            int queueBatchIntervalMs = int.Parse(sr.ReadValue(ServiceBusBatchIntervalMsSetting));

            // Lots of things need to be created to create an appsManager.
            ICTStoreManager tableStoreManager = new CTStoreManager(connectionStringProvider);
            bool tableInit = false;
            Exception exception = null;
            try
            {
                // use Task.Run to ensure that the async Initialize routine runs on a threadpool thread
                Task<bool> task = Task<bool>.Run(() => tableStoreManager.Initialize());

                // task.Result blocks until the result is ready
                tableInit = task.Result;
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (tableInit == false)
            {
                string errorMessage = "CTstore version number does not match the expected version number." + Environment.NewLine +
                    "If your tables are empty, then you probably forgot to provision storage." + Environment.NewLine +
                    "If not, then you need to convert the data format and update the storage version number.";
                Console.WriteLine(errorMessage);
                if (exception != null)
                {
                    Console.WriteLine("Exception message:" + exception.Message);
                }

                Environment.Exit(0);
            }

            ICBStoreManager blobStoreManager = new CBStoreManager(connectionStringProvider);
            AppsStore appsStore = new AppsStore(tableStoreManager);
            UsersStore usersStore = new UsersStore(tableStoreManager);
            ViewsManager viewsManager = new ViewsManager(
                log,
                appsStore,
                usersStore,
                new UserRelationshipsStore(tableStoreManager),
                new TopicsStore(tableStoreManager),
                new TopicRelationshipsStore(tableStoreManager),
                new CommentsStore(tableStoreManager),
                new RepliesStore(tableStoreManager),
                new LikesStore(tableStoreManager),
                new PinsStore(tableStoreManager),
                new BlobsStore(blobStoreManager));
            PushNotificationsManager pushManager = new PushNotificationsManager(log, new PushRegistrationsStore(tableStoreManager), appsStore, viewsManager, connectionStringProvider);
            this.appsManager = new AppsManager(appsStore, pushManager);
            SearchManager searchManager = new SearchManager(log, connectionStringProvider);
            PopularUsersManager popularUsersManager = new PopularUsersManager(usersStore);
            QueueManager queueManager = new QueueManager(connectionStringProvider, queueBatchIntervalMs);
            SearchQueue searchQueue = new SearchQueue(queueManager);
            this.usersManager = new UsersManager(usersStore, pushManager, popularUsersManager, searchQueue);
        }

        /// <summary>
        /// Execute the action requested by the user
        /// </summary>
        /// <param name="action">action to perform</param>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task PerformAction(Action action, string[] args)
        {
            switch (action)
            {
                case Action.CreateApp:
                    await this.CreateApp(args, false);
                    break;
                case Action.CreateAppAdmin:
                    await this.CreateAppAdmin(args);
                    break;
                case Action.CreateAppAndDeveloper:
                    await this.CreateApp(args, true);
                    break;
                case Action.CreateAppKey:
                    await this.CreateAppKey(args);
                    break;
                case Action.CreateClientNameAndConfig:
                    await this.CreateClientNameAndConfig(args);
                    break;
                case Action.CreateUserAsAppAdmin:
                    await this.CreateUserAsAppAdmin(args);
                    break;
                case Action.DeleteApp:
                    await this.DeleteApp(args);
                    break;
                case Action.DeleteAppAdmin:
                    await this.DeleteAppAdmin(args);
                    break;
                case Action.DeleteAppKey:
                    await this.DeleteAppKey(args);
                    break;
                case Action.DeleteClientNameAndConfig:
                    await this.DeleteClientNameAndConfig(args);
                    break;
                case Action.GetApp:
                    await this.GetApp(args);
                    break;
                case Action.GetAppAdmin:
                    await this.GetAppAdmin(args);
                    break;
                case Action.GetAppDeveloperId:
                    await this.GetAppDeveloperId(args);
                    break;
                case Action.GetAppHandle:
                    await this.GetAppHandle(args);
                    break;
                case Action.GetAppKeys:
                    await this.GetAppKeys(args);
                    break;
                case Action.GetAppList:
                    await this.GetAppList(args);
                    break;
                case Action.GetIdentityProvider:
                    await this.GetIdentityProvider(args);
                    break;
                case Action.GetPush:
                    await this.GetPush(args);
                    break;
                case Action.GetValidation:
                    await this.GetValidation(args);
                    break;
                case Action.UpdateAppProfile:
                    await this.UpdateAppProfile(args);
                    break;
                case Action.UpdateIdentityProvider:
                    await this.UpdateIdentityProvider(args);
                    break;
                case Action.UpdatePush:
                    await this.UpdatePushConfig(args);
                    break;
                case Action.UpdateValidation:
                    await this.UpdateValidationConfig(args);
                    break;
                default:
                    Console.WriteLine("Action not specified.");
                    break;
            }
        }
    }
}
