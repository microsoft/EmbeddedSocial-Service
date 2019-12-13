// <copyright file="AppAdmins.cs" company="Microsoft">
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
    /// portion of the program dealing with app admins
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// the user handle of the app administrator
        /// </summary>
        private string userHandle;

        /// <summary>
        /// first name of a user
        /// </summary>
        private string firstName;

        /// <summary>
        /// last name of a user
        /// </summary>
        private string lastName;

        /// <summary>
        /// bio of a user
        /// </summary>
        private string userBio;

        /// <summary>
        /// identity provider account id
        /// </summary>
        private string identityProviderAccountId;

        /// <summary>
        /// create an app administrator given an AppHandle and a UserHandle
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task CreateAppAdmin(string[] args)
        {
            this.ParseAppAdminArgs(args);

            this.ValidateAppAdmin();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine("Cannot find app with appHandle = {0}", this.appHandle);
                Environment.Exit(0);
            }

            IUserProfileEntity userProfileEntity = await this.usersManager.ReadUserProfile(this.userHandle, this.appHandle);
            if (userProfileEntity == null)
            {
                Console.WriteLine("Cannot find user with userHandle = {0} in app with appHandle = {1}", this.userHandle, this.appHandle);
                Environment.Exit(0);
            }

            bool isAdmin = await this.appsManager.IsAdminUser(this.appHandle, this.userHandle);
            if (isAdmin)
            {
                Console.WriteLine("Cannot create a new administrator: UserHandle = {0} is already an admin in app with appHandle = {1}", this.userHandle, this.appHandle);
            }
            else
            {
                await this.appsManager.InsertAdminUser(this.appHandle, this.userHandle);
                Console.WriteLine("UserHandle = {0} is now an admin for appHandle = {1}", this.userHandle, this.appHandle);
            }
        }

        /// <summary>
        /// create a new user and configure them to be an app administrator for the specified app
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task CreateUserAsAppAdmin(string[] args)
        {
            this.ParseCreateUserAsAppAdminArgs(args);

            this.ValidateCreateUserAsAppAdmin();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine("Cannot find app with appHandle = {0}", this.appHandle);
                Environment.Exit(0);
            }

            // create a user handle
            this.userHandle = this.handleGenerator.GenerateShortHandle();

            // for AADS2S, the user handle is the account id
            if (this.identityProvider == IdentityProviderType.AADS2S)
            {
                this.identityProviderAccountId = this.userHandle;
            }

            await this.usersManager.CreateUserAndUserProfile(
                ProcessType.Frontend,
                this.userHandle,
                (IdentityProviderType)this.identityProvider,
                this.identityProviderAccountId,
                this.appHandle,
                this.firstName,
                this.lastName,
                this.userBio,
                null, // we currently don't allow user photo to be set here
                DateTime.UtcNow,
                null);
            Console.WriteLine("Created user with UserHandle = {0} for appHandle = {1}", this.userHandle, this.appHandle);

            bool isAdmin = await this.appsManager.IsAdminUser(this.appHandle, this.userHandle);
            if (isAdmin)
            {
                Console.WriteLine("Cannot create a new administrator: UserHandle = {0} is already an admin in app with appHandle = {1}", this.userHandle, this.appHandle);
            }
            else
            {
                await this.appsManager.InsertAdminUser(this.appHandle, this.userHandle);
                Console.WriteLine("UserHandle = {0} is now an admin for appHandle = {1}", this.userHandle, this.appHandle);
            }
        }

        /// <summary>
        /// delete an app administrator given an AppHandle and a UserHandle
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task DeleteAppAdmin(string[] args)
        {
            this.ParseAppAdminArgs(args);

            this.ValidateAppAdmin();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine("Cannot find app with appHandle = {0}", this.appHandle);
                Environment.Exit(0);
            }

            IUserProfileEntity userProfileEntity = await this.usersManager.ReadUserProfile(this.userHandle, this.appHandle);
            if (userProfileEntity == null)
            {
                Console.WriteLine("Cannot find user with userHandle = {0} in app with appHandle = {1}", this.userHandle, this.appHandle);
                Environment.Exit(0);
            }

            bool isAdmin = await this.appsManager.IsAdminUser(this.appHandle, this.userHandle);
            if (isAdmin)
            {
                await this.appsManager.DeleteAdminUser(this.appHandle, this.userHandle);
                Console.WriteLine("UserHandle = {0} deleted from list of admins for appHandle = {1}", this.userHandle, this.appHandle);
            }
            else
            {
                Console.WriteLine("Cannot delete administrator: UserHandle = {0} is not an admin in app with appHandle = {1}", this.userHandle, this.appHandle);
            }
        }

        /// <summary>
        /// determine whether a user is an app administrator given an AppHandle and a UserHandle
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task GetAppAdmin(string[] args)
        {
            this.ParseAppAdminArgs(args);

            this.ValidateAppAdmin();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine("Cannot find app with appHandle = {0}", this.appHandle);
                Environment.Exit(0);
            }

            IUserProfileEntity userProfileEntity = await this.usersManager.ReadUserProfile(this.userHandle, this.appHandle);
            if (userProfileEntity == null)
            {
                Console.WriteLine("Cannot find user with userHandle = {0} in app with appHandle = {1}", this.userHandle, this.appHandle);
                Environment.Exit(0);
            }

            bool isAdmin = await this.appsManager.IsAdminUser(this.appHandle, this.userHandle);
            if (isAdmin)
            {
                Console.WriteLine("User handle {0} is an administrator for app with appHandle = {1}", this.userHandle, this.appHandle);
            }
            else
            {
                Console.WriteLine("User handle {0} is not an administrator for app with appHandle = {1}", this.userHandle, this.appHandle);
            }
        }

        /// <summary>
        /// Check the required inputs for CreateAppAdmin, DeleteAppAdmin, and GetAppAdmin
        /// </summary>
        private void ValidateAppAdmin()
        {
            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle for an app administrator.");
                Usage();
                Environment.Exit(0);
            }

            if (this.userHandle == null)
            {
                Console.WriteLine("Usage error: Must specify a userHandle for an app administrator.");
                Usage();
                Environment.Exit(0);
            }
        }

        private void ValidateCreateUserAsAppAdmin()
        {
            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle to create an app administrator.");
                Usage();
                Environment.Exit(0);
            }

            if (this.firstName == null)
            {
                Console.WriteLine("Usage error: Must specify a first name and last name to create a new user.");
                Usage();
                Environment.Exit(0);
            }

            if (this.lastName == null)
            {
                Console.WriteLine("Usage error: Must specify a first name and last name to create a new user.");
                Usage();
                Environment.Exit(0);
            }

            if (this.identityProvider == null)
            {
                Console.WriteLine("Usage error: Must specify an identity provider to create a new user.");
                Usage();
                Environment.Exit(0);
            }

            if (this.identityProvider != IdentityProviderType.AADS2S && this.identityProviderAccountId == null)
            {
                Console.WriteLine("Usage error: Must specify an identity provider account id for Facebook, Google, Twitter or Microsoft accounts.");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// parse the action parameters for CreateAppAdmin, DeleteAppAdmin, and GetAppAdmin
        /// </summary>
        /// <param name="args">command line args</param>
        private void ParseAppAdminArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-AppHandle=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppHandle=".Length;
                    this.appHandle = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-CreateAppAdmin", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].Equals("-DeleteAppAdmin", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-DeveloperId=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-DeveloperId=".Length;
                    this.appDeveloperId = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-GetAppAdmin", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-Name=", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-UserHandle=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-UserHandle=".Length;
                    this.userHandle = args[i].Substring(prefixLen);
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
        /// parse the action parameters for CreateUserAsAppAdmin
        /// </summary>
        /// <param name="args">command line args</param>
        private void ParseCreateUserAsAppAdminArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-AppHandle=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppHandle=".Length;
                    this.appHandle = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-Bio=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-Bio=".Length;
                    this.userBio = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-CreateUserAsAppAdmin", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-FirstName=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-FirstName=".Length;
                    this.firstName = args[i].Substring(prefixLen);
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
                else if (args[i].StartsWith("-IdentityProviderAccountId=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-IdentityProviderAccountId=".Length;
                    this.identityProviderAccountId = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-LastName=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-LastName=".Length;
                    this.lastName = args[i].Substring(prefixLen);
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
