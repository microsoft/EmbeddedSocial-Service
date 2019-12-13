// <copyright file="NuGetPackageTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.NuGetTest
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.HttpMessageHandlers;
    using SocialPlus.Client.Models;
    using SocialPlus.Logging;
    using SocialPlus.Server.Email;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Server.Messaging;
    using SocialPlus.TestUtils;
    using SocialPlus.Utils;

    /// <summary>
    /// This class encapsulates tests for our NuGet packages.
    /// These tests are not meant to be exhaustive (that is the goal of the E2E tests). Instead, each of these tests make
    /// one single call to one of our NuGet packages and checks that the call completed successfully.
    /// </summary>
    [TestClass]
    public class NuGetPackageTests
    {
        /// <summary>
        /// handle generator
        /// </summary>
        private static readonly HandleGenerator HandleGenerator = new HandleGenerator();

        /// <summary>
        /// Test SocialPlus.Client by issuing a few simple requests as a batch
        /// </summary>
        /// <returns>Batch task</returns>
        [TestMethod]
        public async Task TestNuGetClient()
        {
            // instantiate the handler
            DelegatingHandler[] handlers = new DelegatingHandler[1];
            Uri batchURL = new Uri(TestConstants.ServerApiBaseUrl.OriginalString + "batch");
            BatchHttpMessageHandler batchHandler = new BatchHttpMessageHandler(HttpMethod.Post, batchURL);
            handlers[0] = batchHandler;

            // instantiate the client, passing in the handler
            SocialPlusClient myClient = new SocialPlusClient(TestConstants.ServerApiBaseUrl, handlers);

            // request server build info
            Task<HttpOperationResponse<GetBuildInfoResponse>> getBuildsTask = myClient.Config.GetBuildInfoWithHttpMessagesAsync();

            // request login
            string userHandle = HandleGenerator.GenerateShortHandle();
            string auth = await TestUtilities.GetAADAuth(userHandle);
            PostUserRequest postUserRequest = new PostUserRequest(instanceId: "test", firstName: "Bob", lastName: "Smith", bio: "mybio");
            Task<HttpOperationResponse<PostUserResponse>> postUserTask = myClient.Users.PostUserWithHttpMessagesAsync(request: postUserRequest, authorization: auth);

            // issue the batch
            await batchHandler.IssueBatch();

            // verify server build info
            HttpOperationResponse<GetBuildInfoResponse> buildsResponse = await getBuildsTask;
            VerifyBuildsResponse(buildsResponse);

            // verify user login info
            HttpOperationResponse<PostUserResponse> postUserResponse = await postUserTask;
            VerifyPostUserResponse(postUserResponse);

            // delete login
            SocialPlusClient myClient2 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            HttpOperationResponse<object> deleteUserResponse = await myClient2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            Assert.IsTrue(deleteUserResponse.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// Test SocialPlus.Logging nuget package by logging a unique string and check the string appears in the log
        /// </summary>
        [TestMethod]
        public void TestNuGetLogging()
        {
            // setup trace listener
            var stringBuilder = new StringBuilder();
            var writer = new StringWriter(stringBuilder);
            var listener = new TextWriterTraceListener(writer);

            var log = new Log(LogDestination.TraceSource, Log.DefaultCategoryName, listener);

            // Create a unique string and log it
            string unique = TestUtilities.CreateUniqueDigits();
            log.LogInformation(unique);

            // Check the unique string has been logged successfully
            string loggedOutput = stringBuilder.ToString();
            Assert.IsTrue(loggedOutput.Contains(unique));
        }

        /// <summary>
        /// Test SocialPlus.Server.Email nuget package by sending email with a To: field set to null.
        /// This behavior should raise an exception
        /// </summary>
        /// <returns>task awaiting the e-mail</returns>
        [TestMethod]
        public async Task TestNuGetEmail()
        {
            // Create debug log and fake SendGrid key
            var debugLog = new Log(LogDestination.Debug, Log.DefaultCategoryName);
            var fakeSendGridKey = TestUtilities.CreateUniqueDigits();

            // Initalize email class
            var emailSender = new SendGridEmail(debugLog, fakeSendGridKey);

            // Create an email message with a To: field set to null
            IEmail emailMsg = new Email()
            {
                To = null
            };

            // Check that this raises the correct exception. Since EMail raises a generic exception,
            // we catch the exception here and test that it includes the expected error message
            try
            {
                await emailSender.SendEmail(emailMsg);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message.Contains("got empty email address list"));
                return;
            }

            // Code should never each here
            Assert.IsTrue(false);
        }

        /// <summary>
        /// Test SocialPlus.Server.KV nuget package by creating a KV instance with null URLs and credentials.
        /// This behavior should raise an exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNuGetKV()
        {
            // Create debug log
            var debugLog = new Log(LogDestination.Debug, Log.DefaultCategoryName);

            // Create a KV with null values for its URL and client credentials. This should raise an exception
            var kv = new KV(debugLog, null, null, null, StoreLocation.CurrentUser, null);
        }

        /// <summary>
        /// Test SocialPlus.Server.Messaging nuget package by testing its initialization
        /// </summary>
        [TestMethod]
        public void TestNuGetMessaging()
        {
            var connectionString = TestUtilities.CreateUniqueDigits();
            var queueName = TestUtilities.CreateUniqueDigits();
            int batchIntervalInMs = 100;
            var sbQueue = new ServiceBusQueue(connectionString, queueName, batchIntervalInMs);

            // Check that the queue was initialized with the correct parameters
            Assert.AreEqual(connectionString, sbQueue.ConnectionString);
            Assert.AreEqual(queueName, sbQueue.QueueName);
        }

        /// <summary>
        /// Test SocialPlus.Utils nuget package by generating short handle.
        /// </summary>
        [TestMethod]
        public void TestNuGetUtils()
        {
            string handle = HandleGenerator.GenerateShortHandle();
            Assert.IsFalse(string.IsNullOrWhiteSpace(handle));
        }

        /// <summary>
        /// Checks that builds response is valid
        /// </summary>
        /// <param name="buildInfoResponse">builds response from the server</param>
        private static void VerifyBuildsResponse(HttpOperationResponse<GetBuildInfoResponse> buildInfoResponse)
        {
            Assert.IsTrue(buildInfoResponse.Response.IsSuccessStatusCode);
            Assert.IsFalse(string.IsNullOrWhiteSpace(buildInfoResponse.Body.CommitHash));
            Assert.IsFalse(string.IsNullOrWhiteSpace(buildInfoResponse.Body.DateAndTime));
            Assert.IsFalse(string.IsNullOrWhiteSpace(buildInfoResponse.Body.Hostname));
        }

        /// <summary>
        /// Checks that the post user response is valid
        /// </summary>
        /// <param name="postUserResponse">post user response from the server</param>
        private static void VerifyPostUserResponse(HttpOperationResponse<PostUserResponse> postUserResponse)
        {
            Assert.IsTrue(postUserResponse.Response.IsSuccessStatusCode);
            Assert.IsFalse(string.IsNullOrWhiteSpace(postUserResponse.Body.UserHandle));
            Assert.IsFalse(string.IsNullOrWhiteSpace(postUserResponse.Body.SessionToken));
        }
    }
}
