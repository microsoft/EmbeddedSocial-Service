// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.SimpleStats
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This program will print statistics
    /// </summary>
    public class Program
    {
        /// <summary>
        /// name of environment
        /// </summary>
        private static string environmentName;

        /// <summary>
        /// flag -- if on no json file will be created
        /// </summary>
        private static bool noJson = false;

        /// <summary>
        /// optional DateTime; if specified, will list userHandles where profile has been updated after this DateTime
        /// </summary>
        private static DateTime? userProfileDateTime = null;

        /// <summary>
        /// Main program. Will print simple statistics
        /// </summary>
        /// <param name="args">command line arguments</param>
        public static void Main(string[] args)
        {
            ParseParams(args);

            var appStats = AppsStats.Instance(environmentName);
            var userStats = UsersStats.Instance(environmentName);
            var userHandlesWithUpdatedProfiles = UserHandlesWithUpdatedProfiles.Instance(environmentName);
            var jsonCommandArgs = new JProperty("CommandArgs", JsonConvert.SerializeObject(args));

            Console.ResetColor();

            // 1. Get all the app Handles
            List<string> appHandles = appStats.GetAllAppHandles();
            Console.WriteLine("Number of AppHandles: " + appHandles.Count);
            JObject statsManifest = new JObject(jsonCommandArgs);
            statsManifest.Add(new JProperty("AppHandles", JsonConvert.SerializeObject(appHandles)));

            // 2. For each app handle, get its profile and all its user handles
            JObject jsonFragmentAppNames = new JObject();
            JObject jsonFragmentUserHandles = new JObject();
            foreach (var appHandle in appHandles)
            {
                string appName = appStats.GetAppName(appHandle);
                List<string> userHandles = userStats.GetAllUserProfiles(appHandle);

                Console.WriteLine("  AppHandle: {0}", appHandle);
                Console.WriteLine("    Name: {0}", appName);
                Console.WriteLine("    Number of UserHandles: {0}", userHandles.Count);
                jsonFragmentAppNames.Add(appHandle, appName);

                JArray jsonUserHandles = new JArray();
                foreach (string userHandle in userHandles)
                {
                    jsonUserHandles.Add(userHandle);
                }

                jsonFragmentUserHandles[appHandle] = jsonUserHandles;
            }

            // 3. For each app handle, get user profiles that have been updated after the specified DateTime
            JObject jsonFragmentUpdatedUserHandles = new JObject();
            if (userProfileDateTime.HasValue)
            {
                foreach (var appHandle in appHandles)
                {
                    List<string> updatedUserHandles = userHandlesWithUpdatedProfiles.GetUpdatedUserHandles(appHandle, userProfileDateTime.Value);
                    Console.WriteLine("  AppHandle: {0}", appHandle);
                    Console.WriteLine("    Number of updated UserHandles: {0}", updatedUserHandles.Count);
                    JArray jsonUserHandles = new JArray();
                    foreach (string userHandle in updatedUserHandles)
                    {
                        jsonUserHandles.Add(userHandle);
                    }

                    jsonFragmentUpdatedUserHandles[appHandle] = jsonUserHandles;
                }
            }

            statsManifest.Add("AppNames", jsonFragmentAppNames);
            statsManifest.Add("UserHandlesPerApp", jsonFragmentUserHandles);
            statsManifest.Add("UpdatedUserHandlesPerApp", jsonFragmentUpdatedUserHandles);

            // write json to file
            if (noJson == false)
            {
                DateTime ts = DateTime.Now;
                string fileName = string.Format(@".\stats_manifest_{0}_{1}-{2}-{3}_{4}-{5}-{6}.json", environmentName, ts.Year, ts.Month.ToString("00"), ts.Day.ToString("00"), ts.Hour.ToString("00"), ts.Minute.ToString("00"), ts.Second.ToString("00"));
                System.IO.File.WriteAllText(fileName, statsManifest.ToString());
                Console.WriteLine("\nManifest file written to " + fileName);
            }
        }

        /// <summary>
        /// Parse the command line arguments and perform some validation checks.
        /// </summary>
        /// <param name="args">command line arguments</param>
        private static void ParseParams(string[] args)
        {
            if (args.Length > 0 && args[0].StartsWith("-name=", StringComparison.CurrentCultureIgnoreCase))
            {
                int prefixLen = "-name=".Length;
                environmentName = args[0].Substring(prefixLen);
            }
            else
            {
                Usage();
                Environment.Exit(0);
            }

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].Equals("-nojson", StringComparison.CurrentCultureIgnoreCase))
                {
                    noJson = true;
                }
                else if (args[i].StartsWith("-userprofiledatetime=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-userprofiledatetime=".Length;
                    try
                    {
                        userProfileDateTime = DateTime.Parse(args[i].Substring(prefixLen));
                    }
                    catch (ArgumentNullException)
                    {
                        Console.WriteLine("Empty DateTime supplied to -userprofiledatetime= argument");
                        Environment.Exit(-1);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("DateTime supplied to -userprofiledatetime= argument could not be parsed");
                        Environment.Exit(-1);
                    }
                }
                else
                {
                    Usage();
                    Environment.Exit(0);
                }
            }
        }

        /// <summary>
        /// Print message describing the command line options
        /// </summary>
        private static void Usage()
        {
            Console.WriteLine("Usage: SimpleStats.exe -Name=<environment-name> [-NoJson] [-UserProfileDateTime=<DateTime>]");
            Console.WriteLine("\n");
            Console.WriteLine("\t -UserProfileDateTime=<DateTime> lists the userHandles that have updated their user profiles after the specified DateTime");
        }
    }
}
