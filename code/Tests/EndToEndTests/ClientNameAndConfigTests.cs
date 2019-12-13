// <copyright file="ClientNameAndConfigTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.Configuration;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.TestUtils;
    using SocialPlus.Utils;

    /// <summary>
    /// Basic functional end to end tests for client name and config
    /// </summary>
    [TestClass]
    public class ClientNameAndConfigTests
    {
        /// <summary>
        /// Create a client name and configuration, read it, verify it, and delete it
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task CreateVerifyDeleteClientNameAndConfigTest()
        {
            // retrieve the developerId using ManageApps
            string developerId = ManageAppsUtils.GetDeveloperId(TestConstants.EnvironmentName);

            // create a client name and config using ManageApps
            string uniqueSuffix = TestUtilities.CreateUniqueDigits();
            string clientName = $"{TestConstants.EnvironmentName}-ClientNameTest{uniqueSuffix}";
            string clientSideAppKey = Guid.NewGuid().ToString();
            string clientConfigJson = "{}";
            string serverSideAppKey = ManageAppsUtils.CreateClientNameAndConfig(clientName, clientSideAppKey, clientConfigJson);

            // create a test client
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // create user1
            var postUserResponse1 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);

            if (serverSideAppKey == null)
            {
                // delete the user and fail the test
                await TestUtilities.DeleteUser(client, auth1);
                Assert.Fail("Failed to create client name and config");
            }

            // retrieve the client configuration
            var clientConfig = await client.Config.GetClientConfigWithHttpMessagesAsync(developerId, clientName);

            // delete client name and configuration
            bool deleted = ManageAppsUtils.DeleteClientNameAndConfig(clientName);
            if (!deleted)
            {
                // delete the user and fail the test
                await TestUtilities.DeleteUser(client, auth1);
                Assert.Fail("Failed to delete client name and config");
            }

            // delete the user
            await TestUtilities.DeleteUser(client, auth1);

            // Check the retrieved configuration is correct
            Assert.AreEqual(serverSideAppKey, clientConfig.Body.ServerSideAppKey);
            Assert.AreEqual(clientConfigJson, clientConfig.Body.ClientConfigJson);

            // if we reach here, the test was successful.
            return;
        }
    }
}