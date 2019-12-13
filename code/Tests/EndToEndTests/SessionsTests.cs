// <copyright file="SessionsTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;
    using SocialPlus.Utils;

    /// <summary>
    /// Tests for the Session API
    /// </summary>
    [TestClass]
    public class SessionsTests
    {
        /// <summary>
        /// handle generator
        /// </summary>
        private static readonly HandleGenerator HandleGenerator = new HandleGenerator();

        /// <summary>
        /// Creates and deletes sesion
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task CreateDeleteSessionTest()
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, "Barack", "Obama", "president");
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            // Delete session (corresponding to log off)
            await client.Sessions.DeleteSessionAsync(authorization: auth);

            // Create session (corresponding to log on)
            PostSessionRequest postSessionRequest = new PostSessionRequest("E2Etests", postUserResponse.UserHandle);
            var postSessionResponse = await client.Sessions.PostSessionWithHttpMessagesAsync(postSessionRequest, auth);

            // Delete user
            string newAuth = AuthHelper.CreateSocialPlusAuth(postSessionResponse.Body.SessionToken);
            await client.Users.DeleteUserAsync(authorization: newAuth);
        }
    }
}