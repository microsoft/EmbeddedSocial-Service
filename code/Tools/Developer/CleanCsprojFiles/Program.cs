// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.CleanCsprojFiles
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Build.Evaluation;

    /// <summary>
    /// This program parses VS2017 csproj files and cleans them up.
    /// It makes all non-system references private and it removes the designer subtype.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The operation completed successfully.
        /// This value comes from the MSDN documented System Error Codes.
        /// </summary>
        private const int ErrorSuccess = 0x0;

        /// <summary>
        /// One or more arguments are not correct.
        /// This value comes from the MSDN documented System Error Codes.
        /// </summary>
        private const int ErrorBadArguments = 0xA0;

        /// <summary>
        /// Main code that this program executes
        /// </summary>
        /// <param name="args">file name</param>
        public static void Main(string[] args)
        {
            // check input argument
            if (args.Length != 1)
            {
                PrintUsage();
                Environment.Exit(ErrorBadArguments);
            }

            // process a single file
            if (File.Exists(args[0]))
            {
                ProcessOneFile(args[0]);
                Environment.Exit(ErrorSuccess);
            }

            // process a directory
            if (Directory.Exists(args[0]))
            {
                string[] filenames = Directory.GetFiles(args[0], "*.csproj", SearchOption.AllDirectories);
                foreach (string filename in filenames)
                {
                    ProcessOneFile(filename);
                }

                Environment.Exit(ErrorSuccess);
            }

            // fall through error condition
            PrintUsage();
            Environment.Exit(ErrorBadArguments);
        }

        /// <summary>
        /// Prints usage of this executable on the commandline
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine("Usage: " + AppDomain.CurrentDomain.FriendlyName + " <csproj filename or solution root directory>");
        }

        /// <summary>
        /// Process a csproj file
        /// </summary>
        /// <param name="filename">name of the csproj file</param>
        private static void ProcessOneFile(string filename)
        {
            // load the csproj file
            ProjectCollection projectCollection = new ProjectCollection();

            // 4.0 is the version of csproj file format that modern versions of VS use
            Project project = projectCollection.LoadProject(filename, "4.0");

            // update private setting for references
            bool privateSettingChanged = false;
            project = UpdatePrivateSettingForReferences(project, out privateSettingChanged);

            // remove all designer subtypes
            bool designerSubTypeChanged = false;
            project = RemoveAllDesignerSubTypes(project, out designerSubTypeChanged);

            // save
            if (privateSettingChanged || designerSubTypeChanged)
            {
                project.Save();
                Console.WriteLine("updated " + filename);
            }
            else
            {
                Console.WriteLine("no changes to " + filename);
            }
        }

        /// <summary>
        /// Set all non-system references in the project to private
        /// and remove private tag for all system references.
        /// </summary>
        /// <param name="project">input csproj</param>
        /// <param name="changed">were any changes applied?</param>
        /// <returns>output csproj with updated private setting for all references</returns>
        private static Project UpdatePrivateSettingForReferences(Project project, out bool changed)
        {
            changed = false;

            // pull out all references
            IEnumerable<ProjectItem> references =
                from projectItem in project.AllEvaluatedItems
                where projectItem.ItemType == "Reference"
                select projectItem;

            foreach (ProjectItem reference in references)
            {
                // remove private if it begins with "system" or "microsoft.csharp"
                if (reference.UnevaluatedInclude.StartsWith("system", StringComparison.InvariantCultureIgnoreCase) ||
                    reference.UnevaluatedInclude.StartsWith("microsoft.csharp", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (reference.HasMetadata("Private"))
                    {
                        reference.RemoveMetadata("Private");
                        changed = true;
                    }

                    continue;
                }

                // set each non-system reference to private
                if (reference.HasMetadata("Private"))
                {
                    if (reference.GetMetadataValue("Private") == "false")
                    {
                        reference.SetMetadataValue("Private", "true");
                        changed = true;
                    }
                }
                else
                {
                    reference.SetMetadataValue("Private", "true");
                    changed = true;
                }
            }

            return project;
        }

        /// <summary>
        /// Remove all instances of <SubType>Designer</SubType>
        /// </summary>
        /// <param name="project">input csproj</param>
        /// <param name="changed">were any changes applied?</param>
        /// <returns>output csproj with all designer subtypes removed</returns>
        private static Project RemoveAllDesignerSubTypes(Project project, out bool changed)
        {
            changed = false;

            // pull out all items with a subtype
            IEnumerable<ProjectItem> itemsWithSubType =
                from projectItem in project.AllEvaluatedItems
                where projectItem.HasMetadata("SubType")
                select projectItem;

            // remove designer subtype
            foreach (ProjectItem itemWithSubType in itemsWithSubType)
            {
                if (itemWithSubType.GetMetadataValue("SubType") == "Designer")
                {
                    itemWithSubType.RemoveMetadata("SubType");
                    changed = true;
                }
            }

            return project;
        }
    }
}