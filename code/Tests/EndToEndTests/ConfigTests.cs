// <copyright file="ConfigTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;

    /// <summary>
    /// Tests config information
    /// </summary>
    [TestClass]
    public class ConfigTests
    {
        /// <summary>
        /// Get the current build information
        /// </summary>
        /// <returns>a task</returns>
        [TestMethod]
        public async Task BuildInfoTest()
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            var configBuildInfoResponse = await client.Config.GetBuildInfoAsync();

            Console.WriteLine("Current Build Information:");
            Console.WriteLine($"  Build Date and Time: {configBuildInfoResponse.DateAndTime}");
            Console.WriteLine($"  Build Hostname: {configBuildInfoResponse.Hostname}");
            Console.WriteLine($"  Build Branch Name: {configBuildInfoResponse.BranchName}");
            Console.WriteLine($"  Build Tag: {configBuildInfoResponse.Tag}");
            foreach (var file in configBuildInfoResponse.DirtyFiles)
            {
                Console.WriteLine($"  Dirty File: {file}");
            }
        }

        /// <summary>
        /// Get the current service information
        /// </summary>
        /// <returns>a task</returns>
        [TestMethod]
        public async Task ServiceInfoTest()
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            var configServiceInfoResponse = await client.Config.GetServiceInfoAsync();

            Console.WriteLine("Current Service Information:");
            Console.WriteLine($"  Service API Version: {configServiceInfoResponse.ServiceApiVersion}");
            Console.WriteLine($"  Service API All Versions: {configServiceInfoResponse.ServiceApiAllVersions}");
        }
    }
}
