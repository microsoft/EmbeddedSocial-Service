// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageCVS
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Server;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Server.Tables;
    using SocialPlus.Utils;

    /// <summary>
    /// Utility to manage requests submitted to CVS
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
        /// Url to contact CVS service
        /// </summary>
        private static string cvsUrl = null;

        /// <summary>
        /// Url to contact CVS service
        /// </summary>
        private static string cvsSubscriptionKey = null;

        /// <summary>
        /// parameter that provides the name of the environment to operate on
        /// </summary>
        private static string environmentName = null;

        private static string jobId = null;

        private Actions actions = new Actions();

        /// <summary>
        /// enum listing the possible actions
        /// </summary>
        private enum Action
        {
            SubmitJob,
            QueryJob,
            None
        }

        /// <summary>
        /// main entry point
        /// </summary>
        /// <param name="args">command line args</param>
        public static void Main(string[] args)
        {
            // parse the arguments
            var action = ParseArgs(args);
            var p = new Program();
            p.Initialize().Wait();
            p.PerformAction(action, args).Wait();
        }

        /// <summary>
        /// Parse the command line arguments and perform some validation checks.
        /// </summary>
        /// <param name="args">command line arguments</param>
        /// <returns>the action to perform</returns>
        private static Action ParseArgs(string[] args)
        {
            Action action = Action.None;

            int i = 0;
            while (i < args.Length)
            {
                if (args[i].StartsWith("-JobId=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-JobId=".Length;
                    jobId = args[i].Substring(prefixLen);
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
                else if (args[1].Equals("-QueryJob", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.QueryJob;
                    i++;
                    continue;
                }
                else
                {
                    Usage();
                    Environment.Exit(0);
                }
            }

            if (string.IsNullOrWhiteSpace(environmentName))
            {
                Console.WriteLine("Usage error: environment name not specified.");
                Usage();
                Environment.Exit(0);
            }

            if (action == Action.QueryJob && string.IsNullOrWhiteSpace(jobId))
            {
                Console.WriteLine("Usage error: environment name not specified.");
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
            Console.WriteLine("Usage: ManageCVS.exe -Name=<environment-name> <action> <action-params>");
            Console.WriteLine("Actions: -QueryJob");
            Console.WriteLine("Action parameters:");
            Console.WriteLine("  QueryJob: -JobId=<job-id>");
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
                case Action.QueryJob:
                    await this.actions.QueryJob(cvsUrl, cvsSubscriptionKey, jobId);
                    break;
            }
        }

        /// <summary>
        /// Initialization routine
        /// </summary>
        /// <returns>init task</returns>
        private async Task Initialize()
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

            cvsUrl = await connectionStringProvider.GetCVSUrl(CVSInstanceType.Default);
            cvsSubscriptionKey = await connectionStringProvider.GetCVSKey(CVSInstanceType.Default);
        }
    }
}
