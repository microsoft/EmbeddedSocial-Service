// <copyright file="AppIdentityProviders.cs" company="Microsoft">
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
    /// portion of the program dealing with identity provider configuration for an app
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// type of the identity provider
        /// </summary>
        private IdentityProviderType? identityProvider = null;

        /// <summary>
        /// oauth client id
        /// </summary>
        private string clientId = null;

        /// <summary>
        /// oauth client secret
        /// </summary>
        private string clientSecret = null;

        /// <summary>
        /// oauth redirect uri
        /// </summary>
        private string redirectUri = null;

        /// <summary>
        /// Read the identity provider configuration for an app
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task GetIdentityProvider(string[] args)
        {
            this.ParseIdentityLookupArgs(args);

            this.ValidateGetIdentityProvider();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine("Cannot find app with appHandle = {0}", this.appHandle);
                Environment.Exit(0);
            }

            IIdentityProviderCredentialsEntity credentialsEntity = await this.appsManager.ReadIdentityProviderCredentials(this.appHandle, (IdentityProviderType)this.identityProvider);
            if (credentialsEntity != null)
            {
                Console.WriteLine("Identity provider config for appHandle {0}, identity provider type {1}", this.appHandle, (IdentityProviderType)this.identityProvider);
                Console.WriteLine("  Client Id = {0}", credentialsEntity.ClientId);
                Console.WriteLine("  Client secret = {0}", credentialsEntity.ClientSecret);
                Console.WriteLine("  Redirect Uri = {0}", credentialsEntity.ClientRedirectUri);
            }
            else
            {
                Console.WriteLine(
                    "Cannot find identity provider config for appHandle {0}, identity provider type {1}",
                    this.appHandle,
                    (IdentityProviderType)this.identityProvider);
            }
        }

        /// <summary>
        /// Update the identity provider configuration for an app
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task UpdateIdentityProvider(string[] args)
        {
            this.ParseIdentityArgs(args);

            this.ValidateUpdateIdentityProvider();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine("Cannot find app with appHandle = {0}", this.appHandle);
                Environment.Exit(0);
            }

            IIdentityProviderCredentialsEntity credentialsEntity = await this.appsManager.ReadIdentityProviderCredentials(this.appHandle, (IdentityProviderType)this.identityProvider);
            if (credentialsEntity != null)
            {
                if (appProfileEntity.DeveloperId == this.appDeveloperId)
                {
                    await this.appsManager.UpdateIdentityProviderCredentials(
                        ProcessType.Frontend,
                        this.appHandle,
                        (IdentityProviderType)this.identityProvider,
                        this.clientId,
                        this.clientSecret,
                        this.redirectUri,
                        credentialsEntity);
                    Console.WriteLine("Updated identity provider config for appHandle {0}, identity provider type {1}", this.appHandle, (IdentityProviderType)this.identityProvider);
                }
                else
                {
                    Console.WriteLine(
                        "Incorrect developerId: developerId is {0} for appHandle {1}",
                        appProfileEntity.DeveloperId,
                        this.appHandle);
                }
            }
            else
            {
                Console.WriteLine(
                    "Cannot find identity provider config for appHandle {0}, identity provider type {1}",
                    this.appHandle,
                    (IdentityProviderType)this.identityProvider);
            }
        }

        /// <summary>
        /// Check the required inputs for GetIdentityProvider
        /// </summary>
        private void ValidateGetIdentityProvider()
        {
            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle to read the identity provider config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.identityProvider == null)
            {
                Console.WriteLine("Usage error: Must specify an identity provider type to read the identity provider config for an application");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Check the required inputs for UpdateIdentityProvider
        /// </summary>
        private void ValidateUpdateIdentityProvider()
        {
            if (this.appDeveloperId == null)
            {
                Console.WriteLine("Usage error: Must specify a developer id to update the identity provider config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle to update the identity provider config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.identityProvider == null)
            {
                Console.WriteLine("Usage error: Must specify the identity provider type to update the identity provider config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.clientId == null)
            {
                Console.WriteLine("Usage error: Must specify a value for ClientId to update the identity provider config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.clientSecret == null)
            {
                Console.WriteLine("Usage error: Must specify a value for ClientSecret to update the identity provider config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.redirectUri == null)
            {
                Console.WriteLine("Usage error: Must specify a value for RedirectUri to update the identity provider config for an application");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// parse the command line args for updating identity provider configuration
        /// </summary>
        /// <param name="args">command line args</param>
        private void ParseIdentityArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-AppHandle=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppHandle=".Length;
                    this.appHandle = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-ClientId=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-ClientId=".Length;
                    this.clientId = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-ClientSecret=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-ClientSecret=".Length;
                    this.clientSecret = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-DeveloperId=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-DeveloperId=".Length;
                    this.appDeveloperId = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-IdentityProvider=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-IdentityProvider=".Length;
                    var value = args[i].Substring(prefixLen);
                    if (value.Equals("Google", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.identityProvider = IdentityProviderType.Google;
                    }
                    else if (value.Equals("Facebook", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.identityProvider = IdentityProviderType.Facebook;
                    }
                    else if (value.Equals("Microsoft", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.identityProvider = IdentityProviderType.Microsoft;
                    }
                    else if (value.Equals("Twitter", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.identityProvider = IdentityProviderType.Twitter;
                    }
                    else if (value.Equals("AADS2S", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.identityProvider = IdentityProviderType.AADS2S;
                    }
                    else
                    {
                        Console.WriteLine("Unrecognized identity provider: {0}", value);
                        Usage();
                        Environment.Exit(0);
                    }
                }
                else if (args[i].StartsWith("-Name=", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-RedirectUri=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-RedirectUri=".Length;
                    this.redirectUri = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-UpdateIdentityProvider", StringComparison.CurrentCultureIgnoreCase))
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

        /// <summary>
        /// parse the command line args for action GetIdentityProvider
        /// </summary>
        /// <param name="args">command line args</param>
        private void ParseIdentityLookupArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-AppHandle=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppHandle=".Length;
                    this.appHandle = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-GetIdentityProvider", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-IdentityProvider=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-IdentityProvider=".Length;
                    var value = args[i].Substring(prefixLen);
                    if (value.Equals("Google", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.identityProvider = IdentityProviderType.Google;
                    }
                    else if (value.Equals("Facebook", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.identityProvider = IdentityProviderType.Facebook;
                    }
                    else if (value.Equals("Microsoft", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.identityProvider = IdentityProviderType.Microsoft;
                    }
                    else if (value.Equals("Twitter", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.identityProvider = IdentityProviderType.Twitter;
                    }
                    else if (value.Equals("AADS2S", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.identityProvider = IdentityProviderType.AADS2S;
                    }
                    else
                    {
                        Console.WriteLine("Unrecognized identity provider: {0}", value);
                        Usage();
                        Environment.Exit(0);
                    }
                }
                else if (args[i].StartsWith("-Name=", StringComparison.CurrentCultureIgnoreCase))
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