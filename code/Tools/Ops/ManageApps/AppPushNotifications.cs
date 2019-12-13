// <copyright file="AppPushNotifications.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageApps
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// portion of the program dealing with configuring push notifications for an app
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// indicates whether push notifications are enabled for this platform type
        /// </summary>
        private bool? pushNotificationsEnable = null;

        /// <summary>
        /// windows platform auth info
        /// </summary>
        private string windowsPackageSid = null;

        /// <summary>
        /// windows platform auth info
        /// </summary>
        private string windowsSecretKey = null;

        /// <summary>
        /// android platform auth info
        /// </summary>
        private string googleApiKey = null;

        /// <summary>
        /// ios platform auth info
        /// </summary>
        private string appleCertKey = null;

        /// <summary>
        /// ios platform auth info
        /// </summary>
        private string appleCertPath = null;

        /// <summary>
        /// gets the push configuration for an app
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task GetPush(string[] args)
        {
            this.ParseLookupPushArgs(args);

            this.ValidateGetPush();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine("Cannot find app with appHandle = {0}", this.appHandle);
                Environment.Exit(0);
            }

            IPushNotificationsConfigurationEntity entity = await this.appsManager.ReadPushNotificationsConfiguration(this.appHandle, (PlatformType)this.appPlatformType);
            if (entity != null)
            {
                Console.WriteLine("Push notifications config for appHandle {0} on platform {1}", this.appHandle, (PlatformType)this.appPlatformType);
                Console.WriteLine("  Enabled = {0}", entity.Enabled);
                Console.WriteLine("  Key = {0}", entity.Key);
                Console.WriteLine("  Path = {0}", entity.Path);
            }
            else
            {
                Console.WriteLine("Cannot find push config for appHandle {0} on platform {1}", this.appHandle, (PlatformType)this.appPlatformType);
            }
        }

        /// <summary>
        /// update the push notifications configuration for an app
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task UpdatePushConfig(string[] args)
        {
            this.ParsePushArgs(args);

            this.ValidateUpdatePushConfig();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine("Cannot find app with appHandle = {0}", this.appHandle);
                Environment.Exit(0);
            }

            IPushNotificationsConfigurationEntity entity = await this.appsManager.ReadPushNotificationsConfiguration(this.appHandle, (PlatformType)this.appPlatformType);
            if (entity != null)
            {
                string path = null;
                string key = null;

                // Validate routine above ensures that appPlatformType is set to one of the three platforms listed below
                if (this.appPlatformType == PlatformType.Windows)
                {
                    path = this.windowsPackageSid;
                    key = this.windowsSecretKey;
                }
                else if (this.appPlatformType == PlatformType.Android)
                {
                    key = this.googleApiKey;
                }
                else if (this.appPlatformType == PlatformType.IOS)
                {
                    path = this.appleCertPath;
                    key = this.appleCertKey;
                }

                if (appProfileEntity.DeveloperId == this.appDeveloperId)
                {
                    await this.appsManager.UpdatePushNotificationsConfiguration(
                        ProcessType.Frontend,
                        this.appHandle,
                        (PlatformType)this.appPlatformType,
                        (bool)this.pushNotificationsEnable,
                        path,
                        key,
                        entity);
                    Console.WriteLine("Updated push notifications configuration for appHandle {0} on platform {1}", this.appHandle, this.appPlatformType);
                }
                else
                {
                    Console.WriteLine(
                        "Incorrect developerId: developerId is {0} for appHandle {1} on platform type {2}",
                        appProfileEntity.DeveloperId,
                        this.appHandle,
                        (PlatformType)this.appPlatformType);
                }
            }
            else
            {
                Console.WriteLine("Cannot find push config for appHandle {0} on platform {1}", this.appHandle, (PlatformType)this.appPlatformType);
            }
        }

        /// <summary>
        /// Check the required inputs for GetPush
        /// </summary>
        private void ValidateGetPush()
        {
            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle to get the push notification config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.appPlatformType == null)
            {
                Console.WriteLine("Usage error: Must provide the platform type to get the push notifications config for an application");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Check the required inputs for UpdatePush
        /// </summary>
        private void ValidateUpdatePushConfig()
        {
            if (this.appDeveloperId == null)
            {
                Console.WriteLine("Usage error: Must specify a developer id to update the push notification config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle to update the push notification config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.pushNotificationsEnable == null)
            {
                Console.WriteLine("Usage error: Must specify -EnablePush or -DisablePush when updating push notifications config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.appPlatformType == null)
            {
                Console.WriteLine("Usage error: Must provide the platform type when updating push notifications config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.appPlatformType == PlatformType.Windows)
            {
                if (this.pushNotificationsEnable.Value && this.windowsPackageSid == null)
                {
                    Console.WriteLine("Usage error: Must specify WindowsPackageSID to enable push notifications on Windows");
                    Usage();
                    Environment.Exit(0);
                }

                if (this.pushNotificationsEnable.Value && this.windowsSecretKey == null)
                {
                    Console.WriteLine("Usage error: Must specify WindowsSecretKey to enable push notifications on Windows");
                    Usage();
                    Environment.Exit(0);
                }
            }
            else if (this.appPlatformType == PlatformType.Android)
            {
                if (this.pushNotificationsEnable.Value && this.googleApiKey == null)
                {
                    Console.WriteLine("Usage error: Must specify GoogleApiKey to enable push notifications on Android");
                    Usage();
                    Environment.Exit(0);
                }
            }
            else if (this.appPlatformType == PlatformType.IOS)
            {
                if (this.pushNotificationsEnable.Value && this.appleCertPath == null)
                {
                    Console.WriteLine("Usage error: Must specify AppleCertPath to enable push notifications on IOS");
                    Usage();
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.WriteLine("Usage error: Must specify a platform type to configure push notifications");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// parse the action parameters for UpdatePush
        /// </summary>
        /// <param name="args">command line args</param>
        private void ParsePushArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-AppHandle=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppHandle=".Length;
                    this.appHandle = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-AppleCertKey=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppleCertKey=".Length;
                    this.appleCertKey = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-AppleCertPath=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppleCertPath=".Length;
                    this.appleCertPath = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-DeveloperId=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-DeveloperId=".Length;
                    this.appDeveloperId = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-DisablePush", StringComparison.CurrentCultureIgnoreCase))
                {
                    this.pushNotificationsEnable = false;
                }
                else if (args[i].Equals("-EnablePush", StringComparison.CurrentCultureIgnoreCase))
                {
                    this.pushNotificationsEnable = true;
                }
                else if (args[i].StartsWith("-GoogleApiKey=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-GoogleApiKey=".Length;
                    this.googleApiKey = args[i].Substring(prefixLen);
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
                        Console.WriteLine("Unrecognized platform type: {0}", value);
                        Usage();
                        Environment.Exit(0);
                    }
                }
                else if (args[i].Equals("-UpdatePush", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-WindowsPackageSID=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-WindowsPackageSID=".Length;
                    this.windowsPackageSid = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-WindowsSecretKey=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-WindowsSecretKey=".Length;
                    this.windowsSecretKey = args[i].Substring(prefixLen);
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

        /// <summary>
        /// parse the action parameters for GetPush
        /// </summary>
        /// <param name="args">command line args</param>
        private void ParseLookupPushArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-AppHandle=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppHandle=".Length;
                    this.appHandle = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-GetPush", StringComparison.CurrentCultureIgnoreCase))
                {
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
                        Console.WriteLine("Unrecognized platform type: {0}", value);
                        Usage();
                        Environment.Exit(0);
                    }
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
