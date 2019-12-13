// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageQueues
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Server;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Server.Messaging;
    using SocialPlus.Server.Queues;
    using SocialPlus.Utils;

    /// <summary>
    /// main program
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// One or more arguments are not correct.
        /// This value comes from the MSDN documented System Error Codes.
        /// </summary>
        private const int ErrorBadArguments = 0xA0;

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
        /// queue name
        /// </summary>
        private static string queueName = null;

        /// <summary>
        /// queue identifier for the specified queue
        /// </summary>
        private static QueueIdentifier? selectedQueueId = null;

        /// <summary>
        /// sequence number of a queued message
        /// </summary>
        private static long? seqNum = null;

        /// <summary>
        /// queue specified by command line args
        /// </summary>
        private Queue selectedQueue;

        /// <summary>
        /// service bus
        /// </summary>
        private ServiceBus sb;

        /// <summary>
        /// service bus queue
        /// </summary>
        private ServiceBusQueue sbQueue;

        /// <summary>
        /// actions
        /// </summary>
        private Actions actions = new Actions();

        /// <summary>
        /// enum listing the possible actions
        /// </summary>
        private enum Action
        {
            None,
            DeleteMessage,
            ListQueues,
            QueueStats,
            ShowMessages,
            ShowDeadLetterMessages
        }

        /// <summary>
        /// Main program.
        /// </summary>
        /// <param name="args">command line arguments</param>
        public static void Main(string[] args)
        {
            // parse the arguments (into statics)
            var action = ParseArgs(args);
            var p = new Program();
            p.Initialize(action).Wait();
            p.PerformAction(action, args).Wait();
        }

        /// <summary>
        /// Parse the command line arguments and perform some validation checks.
        /// </summary>
        /// <param name="args">command line arguments</param>
        /// <returns>the action to perform</returns>
        private static Action ParseArgs(string[] args)
        {
            var action = Action.None;

            int i = 0;
            while (i < args.Length)
            {
                if (args[i].StartsWith("-DeleteMessage", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.DeleteMessage;
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-ListQueues", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.ListQueues;
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
                else if (args[i].StartsWith("-QueueName=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-QueueName=".Length;
                    queueName = args[i].Substring(prefixLen);
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-QueueStats", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.QueueStats;
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-SeqNum=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-SeqNum=".Length;
                    seqNum = long.Parse(args[i].Substring(prefixLen));
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-ShowDeadLetterMessages", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.ShowDeadLetterMessages;
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-ShowMessages", StringComparison.CurrentCultureIgnoreCase))
                {
                    action = Action.ShowMessages;
                    i++;
                    continue;
                }
                else
                {
                    Usage();
                    Environment.Exit(ErrorBadArguments);
                }
            }

            if (string.IsNullOrWhiteSpace(environmentName))
            {
                Console.WriteLine("Usage error: environment name not specified.");
                Usage();
                Environment.Exit(ErrorBadArguments);
            }

            // ensure that a queue name is specified (for all actions other than ListQueues)
            if (action != Action.ListQueues && string.IsNullOrWhiteSpace(queueName))
            {
                Console.WriteLine("Usage error: queue name not specified.");
                Usage();
                Environment.Exit(ErrorBadArguments);
            }

            // check that specified queue name is valid (for all actions other than ListQueues)
            if (action != Action.ListQueues && queueName != null)
            {
                bool found = false;
                foreach (QueueIdentifier queueId in Enum.GetValues(typeof(QueueIdentifier)))
                {
                    if (queueId.ToString().ToLower().Equals(queueName.ToLower()))
                    {
                        selectedQueueId = queueId;
                        found = true;
                    }
                }

                if (found == false)
                {
                    Console.WriteLine($"Specified queue name {queueName} does not exist.");
                    Usage();
                    Environment.Exit(ErrorBadArguments);
                }
            }

            // check that delete specifies a sequence number
            if (action == Action.DeleteMessage && !seqNum.HasValue)
            {
                Console.WriteLine("DeleteMessage action requires a sequence number for the message to delete.");
                Usage();
                Environment.Exit(ErrorBadArguments);
            }

            return action;
        }

        /// <summary>
        /// Print message describing the command line options
        /// </summary>
        private static void Usage()
        {
            Console.WriteLine("Usage: ManageQueues.exe -Name=<environment-name>  <action> <action-params>");
            Console.WriteLine("Actions: -DeleteMessage|-ListQueues|-QueueStats|-ShowMessages|-ShowDeadLetterMessages");
            Console.WriteLine("Action parameters:");
            Console.WriteLine("  DeleteMessage: -QueueName=<queue-name> -SeqNum=<sequence-number>");
            Console.WriteLine("  QueueStats: -QueueName=<queue-name>");
            Console.WriteLine("  ShowMessages: -QueueName=<queue-name>");
            Console.WriteLine("  ShowDeadLetterMessages: -QueueName=<queue-name>");
        }

        /// <summary>
        /// Initialization routine
        /// </summary>
        /// <param name="action">action</param>
        /// <returns>initialization task</returns>
        private async Task Initialize(Action action)
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
            QueueManager queueManager = new QueueManager(connectionStringProvider, queueBatchIntervalMs);
            var sbConnect = await connectionStringProvider.GetServiceBusConnectionString(ServiceBusInstanceType.Default);

            // the ListQueues action requires an instance of a ServiceBus object
            if (action == Action.ListQueues)
            {
                this.sb = new ServiceBus(sbConnect);
            }

            // all the remaining actions operate on an instance of a ServiceBusQueue object
            if (action != Action.ListQueues)
            {
                // ParseArgs() ensures that queueName is valid here
                this.sbQueue = await ServiceBusQueue.Create(sbConnect, queueName, queueBatchIntervalMs);
                this.selectedQueue = await queueManager.GetQueue((QueueIdentifier)selectedQueueId);
            }
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
                case Action.DeleteMessage:
                    await this.actions.DeleteDeadLetterMessage(queueName, this.sbQueue, (long)seqNum);
                    break;
                case Action.QueueStats:
                    await this.actions.QueueStats(queueName, this.sbQueue);
                    break;
                case Action.ShowDeadLetterMessages:
                    await this.actions.ShowDeadLetterMessages(queueName, this.sbQueue);
                    break;
                case Action.ShowMessages:
                    await this.actions.ShowMessages(queueName, this.sbQueue);
                    break;
                case Action.ListQueues:
                    await this.actions.ListQueues(this.sb);
                    break;
                default:
                    Console.WriteLine("Action not specified.");
                    break;
            }
        }
    }
}
