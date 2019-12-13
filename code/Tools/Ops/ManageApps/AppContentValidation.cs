// <copyright file="AppContentValidation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageApps
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// portion of the program dealing with content validation configuration for an app
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// indicates whether or not the app wants content validation
        /// </summary>
        private bool? validationEnable = null;

        /// <summary>
        /// indicates whether or not to validate images
        /// </summary>
        private bool? validateImages = null;

        /// <summary>
        /// indicates whether or not to validate text
        /// </summary>
        private bool? validateText = null;

        /// <summary>
        /// threshold for how many times a user can be reported
        /// </summary>
        private int? userReportThreshold = null;

        /// <summary>
        /// threshold for how many times a specific piece of content can be reported
        /// </summary>
        private int? contentReportThreshold = null;

        /// <summary>
        /// indicates whether or not mature content is allowed
        /// </summary>
        private bool? allowMatureContent = null;

        /// <summary>
        /// lookup the content validation configuration for an app
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task GetValidation(string[] args)
        {
            this.ParseLookupValidationArgs(args);

            this.ValidateGetValidation();

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine("Cannot find app with appHandle = {0}", this.appHandle);
                Environment.Exit(0);
            }

            IValidationConfigurationEntity validationEntity = await this.appsManager.ReadValidationConfiguration(this.appHandle);
            if (validationEntity != null)
            {
                Console.WriteLine("Content validation config for appHandle {0}", this.appHandle);
                Console.WriteLine("  Validation Enabled = {0}", validationEntity.Enabled);
                Console.WriteLine("  AllowMatureContent = {0}", validationEntity.AllowMatureContent);
                Console.WriteLine("  ContentReportThreshold = {0}", validationEntity.ContentReportThreshold);
                Console.WriteLine("  UserReportThreshold = {0}", validationEntity.UserReportThreshold);
                Console.WriteLine("  ValidateImages = {0}", validationEntity.ValidateImages);
                Console.WriteLine("  ValidateText = {0}", validationEntity.ValidateText);
            }
            else
            {
                Console.WriteLine("Cannot find validation config for appHandle {0}", this.appHandle);
            }
        }

        /// <summary>
        /// Update the content validation configuration for an app
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        private async Task UpdateValidationConfig(string[] args)
        {
            this.ParseValidationArgs(args);

            this.ValidateUpdateValidationConfig();

            // if we are disabling content validation, supply default values for all the other fields
            if (this.validationEnable == false)
            {
                this.validateText = false;
                this.validateImages = false;
                this.userReportThreshold = 0;
                this.contentReportThreshold = 0;
                this.allowMatureContent = false;
            }

            IAppProfileEntity appProfileEntity = await this.appsManager.ReadAppProfile(this.appHandle);
            if (appProfileEntity == null)
            {
                Console.WriteLine("Cannot find app with appHandle = {0}", this.appHandle);
                Environment.Exit(0);
            }

            IValidationConfigurationEntity validationEntity = await this.appsManager.ReadValidationConfiguration(this.appHandle);
            if (validationEntity != null)
            {
                if (appProfileEntity.DeveloperId == this.appDeveloperId)
                {
                    await this.appsManager.UpdateValidationConfiguration(
                        ProcessType.Frontend,
                        this.appHandle,
                        (bool)this.validationEnable,
                        (bool)this.validateText,
                        (bool)this.validateImages,
                        (int)this.userReportThreshold,
                        (int)this.contentReportThreshold,
                        (bool)this.allowMatureContent,
                        validationEntity);
                    Console.WriteLine("Updated content validation configuration for appHandle = {0}, developerId = {1}", this.appHandle, this.appDeveloperId);
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
                Console.WriteLine("Cannot find validation config for appHandle {0}", this.appHandle);
            }
        }

        /// <summary>
        /// Check the required inputs for GetValidation
        /// </summary>
        private void ValidateGetValidation()
        {
            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle to update the content validation config for an application");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Check the required inputs for UpdateValidationConfig
        /// </summary>
        private void ValidateUpdateValidationConfig()
        {
            if (this.appDeveloperId == null)
            {
                Console.WriteLine("Usage error: Must specify a developer id to update the content validation config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.appHandle == null)
            {
                Console.WriteLine("Usage error: Must specify an appHandle to update the content validation config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.validationEnable == null)
            {
                Console.WriteLine("Usage error: Must use either -EnableValidation or -DisableValidation when updating the content validation config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.validationEnable == true && this.validateText == null)
            {
                Console.WriteLine("Usage error: Must use -ValidateText=true or -ValidateText=false when updating the content validation config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.validationEnable == true && this.validateImages == null)
            {
                Console.WriteLine("Usage error: Must use -ValidateImages=true or -ValidateImages=false when updating the content validation config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.validationEnable == true && this.userReportThreshold == null)
            {
                Console.WriteLine("Usage error: Must set the value of -UserReportThreshold=<value> when updating the content validation config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.validationEnable == true && this.contentReportThreshold == null)
            {
                Console.WriteLine("Usage error: Must set the value of -ContentReportThreshold=<value> when updating the content validation config for an application");
                Usage();
                Environment.Exit(0);
            }

            if (this.validationEnable == true && this.allowMatureContent == null)
            {
                Console.WriteLine("Usage error: Must use -AllowMatureContent=true or -AllowMatureContent=false when updating the content validation config for an application");
                Usage();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// parses the command line arguments for the UpdateValidation action
        /// </summary>
        /// <param name="args">command line args</param>
        private void ParseValidationArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-AllowMatureContent=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AllowMatureContent=".Length;
                    var value = args[i].Substring(prefixLen);
                    if (value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.allowMatureContent = true;
                    }
                    else if (value.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.allowMatureContent = false;
                    }
                    else
                    {
                        Console.WriteLine("Unrecognized value for AllowMatureContent: {0}", value);
                        Usage();
                        Environment.Exit(0);
                    }
                }
                else if (args[i].StartsWith("-AppHandle=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppHandle=".Length;
                    this.appHandle = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-ContentReportThreshold=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-ContentReportThreshold=".Length;
                    var value = args[i].Substring(prefixLen);
                    int result;
                    bool success = int.TryParse(value, out result);
                    if (success)
                    {
                        this.contentReportThreshold = result;
                    }
                }
                else if (args[i].StartsWith("-DeveloperId=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-DeveloperId=".Length;
                    this.appDeveloperId = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-DisableValidation", StringComparison.CurrentCultureIgnoreCase))
                {
                    this.validationEnable = false;
                }
                else if (args[i].Equals("-EnableValidation", StringComparison.CurrentCultureIgnoreCase))
                {
                    this.validationEnable = true;
                }
                else if (args[i].StartsWith("-Name=", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].Equals("-UpdateValidation", StringComparison.CurrentCultureIgnoreCase))
                {
                }
                else if (args[i].StartsWith("-UserReportThreshold=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-UserReportThreshold=".Length;
                    var value = args[i].Substring(prefixLen);
                    int result;
                    bool success = int.TryParse(value, out result);
                    if (success)
                    {
                        this.userReportThreshold = result;
                    }
                }
                else if (args[i].StartsWith("-ValidateText=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-ValidateText=".Length;
                    var value = args[i].Substring(prefixLen);
                    if (value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.validateText = true;
                    }
                    else if (value.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.validateText = false;
                    }
                    else
                    {
                        Console.WriteLine("Unrecognized value for ValidateText: {0}", value);
                        Usage();
                        Environment.Exit(0);
                    }
                }
                else if (args[i].StartsWith("-ValidateImages=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-ValidateImages=".Length;
                    var value = args[i].Substring(prefixLen);
                    if (value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.validateImages = true;
                    }
                    else if (value.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.validateImages = false;
                    }
                    else
                    {
                        Console.WriteLine("Unrecognized value for ValidateImages: {0}", value);
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

        /// <summary>
        /// parses the command line arguments for the GetValidation action
        /// </summary>
        /// <param name="args">command line args</param>
        private void ParseLookupValidationArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-AppHandle=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-AppHandle=".Length;
                    this.appHandle = args[i].Substring(prefixLen);
                }
                else if (args[i].Equals("-GetValidation", StringComparison.CurrentCultureIgnoreCase))
                {
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
