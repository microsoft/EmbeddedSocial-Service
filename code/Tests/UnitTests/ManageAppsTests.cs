// <copyright file="ManageAppsTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System.Configuration;
    using System.IO;
    using System.Text.RegularExpressions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Utils;

    /// <summary>
    /// Tests the behavior of the ManageApps command line utility
    /// </summary>
    [TestClass]
    public class ManageAppsTests
    {
        // create the application using the specified environment

        /// <summary>
        /// using ManageApps.exe with the specified environment
        /// </summary>
        private string environment = TestConstants.EnvironmentName;

        /// <summary>
        /// path to the manage apps executable
        /// </summary>
        private string manageAppsExecutable = ConfigurationManager.AppSettings["ManageAppsRelativePath"] + Path.DirectorySeparatorChar + "ManageApps.exe";

        /// <summary>
        /// Tests creating a developer id, creating an application, and creating an app key for the application.
        /// Deletes the app and keys.
        /// </summary>
        [TestMethod]
        public void CreateAndDeleteDevAppKeys()
        {
            // do NOT use the standard appKey from TestConstants.AppKey.  Instead, we want to create an app key that will
            // only be used by this test
            string appKey = "7147f211-d768-411a-b87f-1cab4d83042f";
            string appName = "UnitTestApp";

            string command1 = this.manageAppsExecutable + " -Name=" + this.environment + " -CreateAppAndDeveloper -AppName=" + appName + " -PlatformType=Windows";
            string command1Output = RunCommand.Execute(command1);

            // expected output:  "Application UnitTestApp created, appHandle = <app-handle> developerId = <guid>"
            string expectedOutputPattern = "^Application " + appName + @" created, appHandle = ([\w-]+) developerId = ([\w-]+)";

            // use a regular expression to extract the appHandle and developerId from the output of ManageApps.exe
            string appHandle = null;
            string developerId = null;
            var match = Regex.Match(command1Output, expectedOutputPattern, RegexOptions.Multiline);
            if (match.Success == true)
            {
                 appHandle = match.Groups[1].Value;
                 developerId = match.Groups[2].Value;
            }
            else
            {
                Assert.Fail("Test failed: failed to find expected output from ManageApps.exe");
            }

            string command2 = this.manageAppsExecutable + " -Name=" + this.environment + " -CreateAppKey -DeveloperId=" + developerId + " -AppHandle=" + appHandle + " -AppKey=" + appKey;
            string command2Output = RunCommand.Execute(command2);

            expectedOutputPattern = "^AppKey = " + appKey + " created for appHandle = " + appHandle;
            match = Regex.Match(command2Output, expectedOutputPattern, RegexOptions.Multiline);
            if (match.Success == false)
            {
                Assert.Fail("Test failed: failed to find expected output from ManageApps.exe");
            }

            string command3 = this.manageAppsExecutable + " -Name=" + this.environment + " -DeleteAppKey -DeveloperId=" + developerId + " -AppHandle=" + appHandle + " -AppKey=" + appKey;
            string command3Output = RunCommand.Execute(command3);

            expectedOutputPattern = "^AppKey = " + appKey + " deleted for appHandle = " + appHandle;
            match = Regex.Match(command3Output, expectedOutputPattern, RegexOptions.Multiline);
            if (match.Success == false)
            {
                Assert.Fail("Test failed: failed to find expected output from ManageApps.exe");
            }

            string command4 = this.manageAppsExecutable + " -Name=" + this.environment + " -DeleteApp -DeveloperId=" + developerId + " -AppHandle=" + appHandle;
            string command4Output = RunCommand.Execute(command4);

            expectedOutputPattern = "^Application " + appName + " deleted, appHandle = " + appHandle + " developerId = " + developerId;
            match = Regex.Match(command4Output, expectedOutputPattern, RegexOptions.Multiline);
            if (match.Success == false)
            {
                Assert.Fail("Test failed: failed to find expected output from ManageApps.exe");
            }
        }
    }
}
