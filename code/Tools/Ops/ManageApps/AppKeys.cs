// <copyright file="AppKeys.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageApps
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;
    using SocialPlus.Utils;

    /// <summary>
    /// portion of the program dealing with app keys
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// the appkey for an application
        /// </summary>
        private string appKey = null;

        /// <summary>
        /// the client-side appkey for an application
        /// </summary>
        private string clientSideAppKey = null;

        /// <summary>
        /// client configuration JSON
        /// </summary>
        private string clientConfigJson = null;

        /// <summary>
        /// the client name for an application (in case we need to split an app key)
        /// </summary>
        private string clientName = null;

        /// <summary>
        /// create an app key given an AppHandle and a DeveloperId
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task CreateAppKey(string[] args)
        {
            this.ParseAppKeyArgs(args);

            this.ValidateCreateAppKey();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine("Cannot find app with appHandle = {0}", this.appHandle);
                Environment.Exit(0);
            }

            if (appProfileEntity.DeveloperId == this.appDeveloperId)
            {
                // create an app key if the key isn't specified
                if (this.appKey == null)
                {
                    this.appKey = Guid.NewGuid().ToString();
                }

                await this.appsManager.CreateAppKey(ProcessType.Frontend, this.appHandle, this.appKey, DateTime.UtcNow);
                Console.WriteLine("AppKey = {0} created for appHandle = {1}", this.appKey, this.appHandle);
            }
            else
            {
                Console.WriteLine(
                    "Incorrect developerId: developerId is {0} for appHandle {1}",
                    appProfileEntity.DeveloperId,
                    this.appHandle);
            }
        }

        /// <summary>
        /// get the list of app keys associated with an AppHandle
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task GetAppKeys(string[] args)
        {
            this.ParseAppKeyArgs(args);

            this.ValidateGetAppKeys();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine("Cannot find app with appHandle = {0}", this.appHandle);
                Environment.Exit(0);
            }

            var entities = await this.appsManager.ReadAppKeys(this.appHandle);
            if (entities.Count == 0)
            {
                Console.WriteLine("No app keys found for appHandle {0}", this.appHandle);
            }
            else
            {
                Console.WriteLine("Keys for appHandle = {0}", this.appHandle);
                foreach (var entity in entities)
                {
                    Console.WriteLine("  Key = {0}", entity.AppKey);
                }
            }
        }

        /// <summary>
        /// delete an app key given an AppHandle, a DeveloperId, and the AppKey to delete
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task DeleteAppKey(string[] args)
        {
            this.ParseAppKeyArgs(args);

            this.ValidateDeleteAppKey();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine("Cannot find app with appHandle = {0}", this.appHandle);
                Environment.Exit(0);
            }

            if (appProfileEntity.DeveloperId == this.appDeveloperId)
            {
                await this.appsManager.DeleteAppKey(ProcessType.Frontend, this.appHandle, this.appKey);
                Console.WriteLine("AppKey = {0} deleted for appHandle = {1}", this.appKey, this.appHandle);
            }
            else
            {
                Console.WriteLine(
                    "Incorrect developerId: developerId is {0} for appHandle {1}",
                    appProfileEntity.DeveloperId,
                    this.appHandle);
            }
        }

        /// <summary>
        /// Create client name and config
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task CreateClientNameAndConfig(string[] args)
        {
            this.ParseAppKeyArgs(args);

            this.ValidateClientNameAndConfig();

            // If the caller did not specify a client-side app key, just create a random one
            if (this.clientSideAppKey == null)
            {
                this.clientSideAppKey = Guid.NewGuid().ToString();
            }

            // Server-side app key is created by xor-ing appKey with client-side app key
            Guid guid1 = Guid.Parse(this.appKey);
            Guid guid2 = Guid.Parse(this.clientSideAppKey);
            Guid guid3 = guid1.Xor(guid2);
            string serverSideAppKey = guid3.ToString();

            await this.appsManager.CreateClientNameAndConfig(this.appKey, this.clientName, serverSideAppKey, this.clientConfigJson);
            Console.WriteLine($"Client configuration for client name: {this.clientName} created.");
            Console.WriteLine($"Client-side app key: {this.clientSideAppKey}");
            Console.WriteLine($"Server-side app key: {serverSideAppKey}");
        }

        /// <summary>
        /// Delete client name and config
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task DeleteClientNameAndConfig(string[] args)
        {
            this.ParseAppKeyArgs(args);

            this.ValidateClientNameAndConfig();

            await this.appsManager.DeleteClientNameAndConfig(this.appKey, this.clientName);
            Console.WriteLine($"Client configuration for client name: {this.clientName} deleted.");
        }

        /// <summary>
        /// Check the required inputs for CreateAppKey
        /// </summary>
        private void ValidateCreateAppKey()
        {
            if (this.appDeveloperId == null)
            {
                Console.WriteLine("Usage error: Must specify a developer id to create an app key.");
                Usage();
                Environment.Exit(0);
            }

            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle to create an app key.");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Check the required inputs for GetAppKeys
        /// </summary>
        private void ValidateGetAppKeys()
        {
            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle to get the list of app keys.");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Check the required inputs for DeleteAppKey
        /// </summary>
        private void ValidateDeleteAppKey()
        {
            if (this.appDeveloperId == null)
            {
                Console.WriteLine("Usage error: Must specify a developer id to delete an app key.");
                Usage();
                Environment.Exit(0);
            }

            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle to delete an app key.");
                Usage();
                Environment.Exit(0);
            }

            if (this.appKey == null)
            {
                Console.WriteLine("Usage error: Must specify the appKey to delete.");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Check the required inputs for CreateClientNameAndConfig and DeleteClientNameAndConfig
        /// </summary>
        private void ValidateClientNameAndConfig()
        {
            if (this.appKey == null)
            {
                Console.WriteLine("Usage error: Must specify an app key.");
                Usage();
                Environment.Exit(0);
            }

            if (this.clientName == null)
            {
                Console.WriteLine("Usage error: Must specify a client name.");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// parse the action parameters for CreateAppKey, DeleteAppKey, GetAppKeys, CreateClientNameAndConfig, and DeleteClientNAmeAndConfig
        /// </summary>
        /// <param name="args">command line args</param>
        private void ParseAppKeyArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-AppKey=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppKey=".Length;
                    this.appKey = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-AppHandle=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppHandle=".Length;
                    this.appHandle = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-CreateAppKey", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-ClientConfigJson=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-ClientConfigJson=".Length;
                    this.clientConfigJson = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-ClientName=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-ClientName=".Length;
                    this.clientName = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-ClientSideAppKey=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-ClientSideAppKey=".Length;
                    this.clientSideAppKey = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-DeleteAppKey", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-DeveloperId=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-DeveloperId=".Length;
                    this.appDeveloperId = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-GetAppKeys", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-Name=", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].Equals("-CreateClientNameAndConfig", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].Equals("-DeleteClientNameAndConfig", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else
                {
                    // default
                    Console.WriteLine("Unrecognized parameter: {0}", args[i]);
                    Usage();
                    Environment.Exit(0);
                }
            }
        }
    }
}
