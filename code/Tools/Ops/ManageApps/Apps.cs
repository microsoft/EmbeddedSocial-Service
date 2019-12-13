// <copyright file="Apps.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageApps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// portion of the program dealing with creating, getting, and deleting apps, and updating an app profile
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// a deep link for the application
        /// </summary>
        private string appDeepLink = null;

        /// <summary>
        /// a link to the app store for the application
        /// </summary>
        private string appStoreLink = null;

        /// <summary>
        /// the id of the developer who created this application
        /// </summary>
        private string appDeveloperId = null;

        /// <summary>
        /// the app handle (created by social plus) for this application
        /// </summary>
        private string appHandle = null;

        /// <summary>
        /// the name of the application
        /// </summary>
        private string appName = null;

        /// <summary>
        /// a link to an icon for the application
        /// </summary>
        private string appIconUrl = null;

        /// <summary>
        /// the application's platform type
        /// </summary>
        private PlatformType? appPlatformType = null;

        /// <summary>
        /// whether to disable validation of app-provided handles
        /// </summary>
        private bool disableHandleValidation = false;

        /// <summary>
        /// create an application (and also create a new developer if needed)
        /// </summary>
        /// <param name="args">command line args</param>
        /// <param name="createDeveloper">create a new developer id</param>
        /// <returns>a task</returns>
        private async Task CreateApp(string[] args, bool createDeveloper)
        {
            this.ParseAppProfileArgs(args);

            this.ValidateCreateApp(createDeveloper);

            await this.appsManager.CreateApp(
                ProcessType.Frontend,
                this.appHandle,
                this.appDeveloperId,
                this.appName,
                this.appIconUrl,
                (PlatformType)this.appPlatformType,
                this.appDeepLink,
                this.appStoreLink,
                DateTime.UtcNow,
                this.disableHandleValidation);
            Console.WriteLine($"Application {this.appName} created, appHandle = {this.appHandle} developerId = {this.appDeveloperId}");
        }

        /// <summary>
        /// delete an app (using the specified developerId and appHandle)
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task DeleteApp(string[] args)
        {
            this.ParseLookupAppArgs(args);

            this.ValidateDeleteApp();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine($"Cannot delete app for appHandle = {this.appHandle}");
                Environment.Exit(0);
            }

            if (appProfileEntity.DeveloperId == this.appDeveloperId)
            {
                await this.appsManager.DeleteApp(ProcessType.Frontend, this.appDeveloperId, this.appHandle);
                Console.WriteLine($"Application {appProfileEntity.Name} deleted, appHandle = {this.appHandle} developerId = {this.appDeveloperId}");
            }
            else
            {
                Console.WriteLine($"Incorrect developerId: developerId is {appProfileEntity.DeveloperId} for appHandle {this.appHandle}");
            }
        }

        /// <summary>
        /// Get details on a specific application
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task GetApp(string[] args)
        {
            this.ParseLookupAppArgs(args);

            this.ValidateGetApp();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine($"Cannot find app with appHandle = {this.appHandle}");
                Environment.Exit(0);
            }

            Console.WriteLine($"App profile for appHandle {0}", this.appHandle);
            Console.WriteLine($"  Name = {appProfileEntity.Name}");
            Console.WriteLine($"  PlatformType = {appProfileEntity.PlatformType}");
            Console.WriteLine($"  DeveloperId = {appProfileEntity.DeveloperId}");
            Console.WriteLine($"  DeepLink = {appProfileEntity.DeepLink}");
            Console.WriteLine($"  StoreLink = {appProfileEntity.StoreLink}");
            Console.WriteLine($"  IconHandle = {appProfileEntity.IconHandle}");
            Console.WriteLine($"  AppStatus = {appProfileEntity.AppStatus}");
            Console.WriteLine($"  CreatedTime = {appProfileEntity.CreatedTime}");
            Console.WriteLine($"  LastUpdateTime = {appProfileEntity.LastUpdatedTime}");
        }

        /// <summary>
        /// Gets the developer Id corresponding to this app handle
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task GetAppDeveloperId(string[] args)
        {
            this.ParseLookupAppArgs(args);

            this.ValidateGetApp();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine($"Cannot find app with appHandle = {this.appHandle}");
                Environment.Exit(0);
            }

            Console.WriteLine($"DeveloperId = {appProfileEntity.DeveloperId} for appHandle = {this.appHandle}");
        }

        /// <summary>
        /// Get the app handle of a specific application (lookup by appKey)
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task GetAppHandle(string[] args)
        {
            this.ParseGetAppHandleArgs(args);

            this.ValidateGetAppHandle();

            IAppLookupEntity appEntity = await this.appsManager.ReadAppByAppKey(this.appKey);
            if (appEntity != null)
            {
                Console.WriteLine($"AppHandle = {appEntity.AppHandle} for application with appKey = {this.appKey}");
            }
            else
            {
                Console.WriteLine($"Cannot find app with appKey = {this.appKey}");
            }
        }

        /// <summary>
        /// Get a list of the applications owned by the specified developerId
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task GetAppList(string[] args)
        {
            this.ParseLookupAppArgs(args);

            this.ValidateGetAppList();

            var appFeedEntities = await this.appsManager.ReadDeveloperApps(this.appDeveloperId);
            if (appFeedEntities.Count == 0)
            {
                Console.WriteLine($"No apps found for developerId = {this.appDeveloperId}");
            }
            else
            {
                IEnumerable<Task<IAppProfileEntity>> tasks = from appFeedEntity in appFeedEntities select this.appsManager.ReadAppProfile(appFeedEntity.AppHandle);
                IAppProfileEntity[] profileEntities = await Task.WhenAll(tasks.ToArray());
                for (int i = 0; i < profileEntities.Length; i++)
                {
                    Console.WriteLine($"AppHandle: {appFeedEntities[i].AppHandle}, AppName: {profileEntities[i].Name}, PlatformType: {profileEntities[i].PlatformType.ToString()}");
                }
            }
        }

        /// <summary>
        /// Update the application profile data
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task UpdateAppProfile(string[] args)
        {
            this.ParseAppProfileArgs(args);

            this.ValidateUpdateAppProfile();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine($"Cannot find app with appHandle = {this.appHandle}");
                Environment.Exit(0);
            }

            if (appProfileEntity.DeveloperId == this.appDeveloperId)
            {
                await this.appsManager.UpdateAppProfileInfo(
                    ProcessType.Frontend,
                    this.appHandle,
                    this.appName,
                    this.appIconUrl,
                    (PlatformType)this.appPlatformType,
                    this.appDeepLink,
                    this.appStoreLink,
                    DateTime.UtcNow,
                    this.disableHandleValidation,
                    appProfileEntity);
                Console.WriteLine($"Updated app profile for appHandle {this.appHandle} platform type {(PlatformType)this.appPlatformType}");
            }
            else
            {
                Console.WriteLine($"Incorrect developerId: developerId is {appProfileEntity.DeveloperId} for appHandle {this.appHandle}");
            }
        }

        /// <summary>
        /// Check the required inputs for CreateApp
        /// </summary>
        /// <param name="createDeveloper">create a new developer id</param>
        private void ValidateCreateApp(bool createDeveloper)
        {
            if (this.appDeveloperId != null && createDeveloper == true)
            {
                Console.WriteLine("Usage error: Must not specify a developer id with the -CreateAppAndDeveloper action.");
                Usage();
                Environment.Exit(0);
            }

            if (this.appDeveloperId == null)
            {
                if (createDeveloper == true)
                {
                    this.appDeveloperId = Guid.NewGuid().ToString();
                }
                else
                {
                    Console.WriteLine("Usage error: Must specify a developer id.  Use the -CreateAppAndDeveloper action otherwise.");
                    Usage();
                    Environment.Exit(0);
                }
            }

            if (this.appPlatformType == null)
            {
                Console.WriteLine("Usage error: Must specify platform type when creating an application.");
                Usage();
                Environment.Exit(0);
            }

            if (this.appName == null)
            {
                Console.WriteLine("Usage error: Must specify app name when creating an application.");
                Usage();
                Environment.Exit(0);
            }

            if (this.appHandle == null)
            {
                // generate a new app handle if the user does not specify the appHandle
                this.appHandle = this.handleGenerator.GenerateShortHandle();
            }
        }

        /// <summary>
        /// Check the required inputs for DeleteApp
        /// </summary>
        private void ValidateDeleteApp()
        {
            if (this.appDeveloperId == null)
            {
                Console.WriteLine("Usage error: Must specify a developer id to delete an application.");
                Usage();
                Environment.Exit(0);
            }

            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle to delete an application.");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Check the required inputs for GetApp
        /// </summary>
        private void ValidateGetApp()
        {
            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle to get an application profile.");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Check the required inputs for GetAppHandle
        /// </summary>
        private void ValidateGetAppHandle()
        {
            if (this.appKey == null)
            {
                Console.WriteLine("Usage error: Must specify an appKey to get an appHandle.");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Check the required inputs for GetAppList
        /// </summary>
        private void ValidateGetAppList()
        {
            if (this.appDeveloperId == null)
            {
                Console.WriteLine("Usage error: Must specify a developer id to get a list of registered applications");
                Usage();
                Environment.Exit(0);
            }

            if (this.appHandle != null)
            {
                Console.WriteLine("Usage error: Cannot specify an app handle when getting a list of registered applications");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Check the required inputs for UpdateAppProfile
        /// </summary>
        private void ValidateUpdateAppProfile()
        {
            if (this.appDeveloperId == null)
            {
                Console.WriteLine("Usage error: Must specify a developer id to update an application profile.");
                Usage();
                Environment.Exit(0);
            }

            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle to update an application profile.");
                Usage();
                Environment.Exit(0);
            }

            if (this.appPlatformType == null)
            {
                Console.WriteLine("Usage error: Must specify platform type when updating an application profile.");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Parse the command line for the actions:  DeleteApp, GetApp, GetAppList
        /// </summary>
        /// <param name="args">command line args</param>
        private void ParseLookupAppArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-AppHandle=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppHandle=".Length;
                    this.appHandle = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-DeleteApp", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-DeveloperId=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-DeveloperId=".Length;
                    this.appDeveloperId = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-GetAppDeveloperId", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].Equals("-GetApp", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].Equals("-GetAppList", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-Name=", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else
                {
                    // default
                    Console.WriteLine($"Unrecognized parameter: {args[i]}");
                    Usage();
                    Environment.Exit(0);
                }
            }
        }

        /// <summary>
        /// Parse the command line for the GetAppHandle action
        /// </summary>
        /// <param name="args">command line args</param>
        private void ParseGetAppHandleArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-AppKey=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppKey=".Length;
                    this.appKey = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-GetAppHandle", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-Name=", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else
                {
                    // default
                    Console.WriteLine($"Unrecognized parameter: {args[i]}");
                    Usage();
                    Environment.Exit(0);
                }
            }
        }

        /// <summary>
        /// Parse the command line for the actions:  CreateApp, CreateAppAndDeveloper, UpdateAppProfile
        /// </summary>
        /// <param name="args">command line args</param>
        private void ParseAppProfileArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-AppDeepLink=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppDeepLink=".Length;
                    this.appDeepLink = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-AppHandle=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppHandle=".Length;
                    this.appHandle = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-AppIconUrl=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppIconUrl=".Length;
                    this.appIconUrl = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-AppName=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppName=".Length;
                    this.appName = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-AppStoreLink=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppStoreLink=".Length;
                    this.appStoreLink = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-DisableHandleValidation=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-DisableHandleValidation=".Length;
                    var value = args[i].Substring(prefixLen);

                    // disableHandleValidation remains false, unless the next argument is "true".
                    if (value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.disableHandleValidation = true;
                    }
                }
                else if (args[i].Equals("-CreateApp", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].Equals("-CreateAppAndDeveloper", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-DeveloperId=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-DeveloperId=".Length;
                    this.appDeveloperId = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-Name=", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-PlatformType=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-PlatformType=".Length;
                    var value = args[i].Substring(prefixLen);
                    if (value.Equals("Windows", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.appPlatformType = PlatformType.Windows;
                    }
                    else if (value.Equals("Android", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.appPlatformType = PlatformType.Android;
                    }
                    else if (value.Equals("IOS", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.appPlatformType = PlatformType.IOS;
                    }
                    else
                    {
                        Console.WriteLine($"Unrecognized platform type: {value}");
                        Usage();
                        Environment.Exit(0);
                    }
                }
                else if (args[i].Equals("-UpdateAppProfile", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else
                {
                    // default
                    Console.WriteLine($"Unrecognized parameter: {args[i]}");
                    Usage();
                    Environment.Exit(0);
                }
            }
        }
    }
}
