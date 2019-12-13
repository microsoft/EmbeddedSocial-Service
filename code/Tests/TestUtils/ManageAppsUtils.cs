// <copyright file="ManageAppsUtils.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.TestUtils
{
    using System.Configuration;
    using System.Text.RegularExpressions;

    using SocialPlus.Utils;

    /// <summary>
    /// Utils for using ManageApps in our tests
    /// </summary>
    public static class ManageAppsUtils
    {
        /// <summary>
        /// name of the manage apps executable
        /// </summary>
        private static string manageAppsExecutable = ".\\ManageApps.exe";

        /// <summary>
        /// run ManageApps.exe to get the app handle
        /// </summary>
        /// <param name="environment">name of service environment</param>
        /// <returns>the app handle</returns>
        public static string GetAppHandle(string environment)
        {
            // lookup the apphandle associated with the appkey
            string command1 = "cd " + ConfigurationManager.AppSettings["ManageAppsRelativePath"] + " & " + manageAppsExecutable + " -Name=" + environment + " -GetAppHandle -AppKey=" + TestConstants.AppKey;
            string command1Output = RunCommand.Execute(command1);

            // expected output:  "AppHandle = <app-handle> for application with appKey = <app-key>"
            string expectedOutputPattern = @"^AppHandle = ([\w-]+) for application with appKey = ([\w-]+)";
            var match = Regex.Match(command1Output, expectedOutputPattern, RegexOptions.Multiline);
            if (match.Success == true)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// add the user as an adminstrator for this app
        /// </summary>
        /// <param name="environment">name of service environment</param>
        /// <param name="appHandle">app handle</param>
        /// <param name="userHandle">user handle</param>
        /// <returns>true if user is now an admin</returns>
        public static bool AddAdmin(string environment, string appHandle, string userHandle)
        {
            // use the manageapps utility to register the user as an administrator
            string command1 = "cd " + ConfigurationManager.AppSettings["ManageAppsRelativePath"] + " & " + manageAppsExecutable + " -Name=" + environment + " -CreateAppAdmin -AppHandle=" + appHandle + " -UserHandle=" + userHandle;
            string command1Output = RunCommand.Execute(command1);

            // expected output:  "UserHandle = <user-handle> is now an admin for appHandle = <app-handle>"
            string expectedOutputPattern = @"^UserHandle = ([\w-]+) is now an admin for appHandle = ([\w-]+)";
            var match = Regex.Match(command1Output, expectedOutputPattern, RegexOptions.Multiline);
            if (match.Success == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// delete the user as an adminstrator for this app
        /// </summary>
        /// <param name="environment">name of service environment</param>
        /// <param name="appHandle">app handle</param>
        /// <param name="userHandle">user handle</param>
        /// <returns>true if user is deleted from admin list</returns>
        public static bool DeleteAdmin(string environment, string appHandle, string userHandle)
        {
            // step 3: use the manageapps utility to register the user as an administrator
            string command1 = "cd " + ConfigurationManager.AppSettings["ManageAppsRelativePath"] + " & " + manageAppsExecutable + " -Name=" + environment + " -DeleteAppAdmin -AppHandle=" + appHandle + " -UserHandle=" + userHandle;
            string command1Output = RunCommand.Execute(command1);

            // expected output:  "UserHandle = <user-handle> deleted from list of admins for appHandle = <app-handle>"
            string expectedOutputPattern = @"^UserHandle = ([\w-]+) deleted from list of admins for appHandle = ([\w-]+)";
            var match = Regex.Match(command1Output, expectedOutputPattern, RegexOptions.Multiline);
            if (match.Success == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// run ManageApps.exe to get the developer id
        /// </summary>
        /// <param name="environment">name of service environment</param>
        /// <returns>the developer id</returns>
        public static string GetDeveloperId(string environment)
        {
            string appHandle = ManageAppsUtils.GetAppHandle(environment);

            // lookup the developer id associated with the app handle
            string command1 = "cd " + ConfigurationManager.AppSettings["ManageAppsRelativePath"] + " & " + manageAppsExecutable + " -Name=" + environment + " -GetAppDeveloperId -AppHandle=" + appHandle;
            string command1Output = RunCommand.Execute(command1);

            // expected output:  "DeveloperId = <developer-id> for appHandle = <app-handle>"
            string expectedOutputPattern = @"^DeveloperId = ([\w-]+) for appHandle = ([\w-]+)";
            var match = Regex.Match(command1Output, expectedOutputPattern, RegexOptions.Multiline);
            if (match.Success == true)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// run ManageApps.exe to create client name and config
        /// </summary>
        /// <param name="clientName">client name</param>
        /// <param name="clientSideAppKey">client-side appKey</param>
        /// <param name="clientConfigJson">client configuration</param>
        /// <returns>server-side app key</returns>
        public static string CreateClientNameAndConfig(string clientName, string clientSideAppKey, string clientConfigJson)
        {
            // use the manageapps utility to create a client name and configuration
            string command1 = $"cd {ConfigurationManager.AppSettings["ManageAppsRelativePath"]} & {manageAppsExecutable} -Name={TestConstants.EnvironmentName}";
            command1 += $" -CreateClientNameAndConfig -AppKey={TestConstants.AppKey} -ClientName={clientName}";
            if (!string.IsNullOrEmpty(clientSideAppKey))
            {
                command1 += $" -ClientSideAppKey={clientSideAppKey}";
            }

            if (!string.IsNullOrEmpty(clientConfigJson))
            {
                command1 += $" -ClientConfigJson=\"{clientConfigJson}\"";
            }

            string command1Output = RunCommand.Execute(command1);

            // expected output:  "Client-side app key: <client-side-app-key>\nServer-side app key: <server-side-app-key>"
            string expectedOutputPattern1 = @"^Client-side app key: ([\w-]+)";
            string expectedOutputPattern2 = @"^Server-side app key: ([\w-]+)";
            var match1 = Regex.Match(command1Output, expectedOutputPattern1, RegexOptions.Multiline);
            var match2 = Regex.Match(command1Output, expectedOutputPattern2, RegexOptions.Multiline);

            if (!match1.Success || !match2.Success)
            {
                return null;
            }

            return match2.Groups[1].Value;
        }

        /// <summary>
        /// deletes client name and its corresponding configuration
        /// </summary>
        /// <param name="clientName">client name</param>
        /// <returns>true if client name deleted successfully</returns>
        public static bool DeleteClientNameAndConfig(string clientName)
        {
            // use the manageapps utility to delete the client name and its corresponding configuration
            string command1 = $"cd {ConfigurationManager.AppSettings["ManageAppsRelativePath"]} & {manageAppsExecutable} -Name={TestConstants.EnvironmentName}";
            command1 += $" -DeleteClientNameAndConfig -AppKey={TestConstants.AppKey} -ClientName={clientName}";

            string command1Output = RunCommand.Execute(command1);

            // expected output:  "Client configuration for client name: <client-name> deleted."
            string expectedOutputPattern = @"^Client configuration for client name: ([\w-]+) deleted.";
            var match1 = Regex.Match(command1Output, expectedOutputPattern, RegexOptions.Multiline);

            return match1.Success;
        }
    }
}
