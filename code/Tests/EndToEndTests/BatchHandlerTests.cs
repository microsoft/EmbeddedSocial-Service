// <copyright file="BatchHandlerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.HttpMessageHandlers;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;
    using SocialPlus.Utils;

    /// <summary>
    /// Basic functional end to end tests for HTTP batch handler
    /// </summary>
    [TestClass]
    public class BatchHandlerTests
    {
        /// <summary>
        /// Instance ID to be used in all calls
        /// </summary>
        private const string InstanceId = "BatchHandlerTests";

        /// <summary>
        /// handle generator
        /// </summary>
        private static readonly HandleGenerator HandleGenerator = new HandleGenerator();

        /// <summary>
        /// Issues a few simple requests in non batch mode
        /// </summary>
        /// <returns>task</returns>
        [TestMethod]
        public async Task IssueNonBatch()
        {
            // instantiate the handler
            DelegatingHandler[] handlers = new DelegatingHandler[1];
            InstrumentedHttpMessageHandler testHandler = new InstrumentedHttpMessageHandler();
            handlers[0] = testHandler;

            // instantiate the client
            SocialPlusClient myClient = new SocialPlusClient(TestConstants.ServerApiBaseUrl, handlers);

            // request server build info
            HttpOperationResponse<GetBuildInfoResponse> buildsResponse = await myClient.Config.GetBuildInfoWithHttpMessagesAsync();
            VerifyBuildsResponse(buildsResponse);

            // login
            PostUserRequest postUserRequest = new PostUserRequest(InstanceId, "Bob", "Smith", "mybio", null);
            string userHandle = HandleGenerator.GenerateShortHandle();
            string auth = await TestUtilities.GetAADAuth(userHandle);
            HttpOperationResponse<PostUserResponse> postUserResponse = await myClient.Users.PostUserWithHttpMessagesAsync(request: postUserRequest, authorization: auth);
            VerifyPostUserResponse(postUserResponse);

            // delete login
            auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.Body.SessionToken);
            HttpOperationResponse<object> deleteUserResponse = await myClient.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            Assert.IsTrue(deleteUserResponse.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// Issues a few simple requests as a batch
        /// </summary>
        /// <returns>Batch task</returns>
        [TestMethod]
        public async Task IssueBatch()
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
            // Get the access token for the Embedded Social Test Client 1 AAD application
            string userHandle = HandleGenerator.GenerateShortHandle();
            string auth = await TestUtilities.GetAADAuth(userHandle);
            PostUserRequest postUserRequest = new PostUserRequest(InstanceId, "Bob", "Smith", "mybio", null);
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
            auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.Body.SessionToken);
            HttpOperationResponse<object> deleteUserResponse = await myClient2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            Assert.IsTrue(deleteUserResponse.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// Issues a bad batch call to test exception handling
        /// </summary>
        /// <returns>Batch task</returns>
        [TestMethod]
        public async Task IssueBadBatch()
        {
            // instantiate the handler using a bad batch URL
            DelegatingHandler[] handlers = new DelegatingHandler[1];
            Uri batchURL = new Uri(TestConstants.ServerApiBaseUrl.OriginalString + "batch" + Guid.NewGuid());
            BatchHttpMessageHandler batchHandler = new BatchHttpMessageHandler(HttpMethod.Post, batchURL);
            handlers[0] = batchHandler;

            // instantiate the client, passing in the handler
            SocialPlusClient myClient = new SocialPlusClient(TestConstants.ServerApiBaseUrl, handlers);

            // request server build info
            Task<HttpOperationResponse<GetBuildInfoResponse>> getBuildsTask = myClient.Config.GetBuildInfoWithHttpMessagesAsync();

            // request login
            string userHandle = HandleGenerator.GenerateShortHandle();
            string auth = await TestUtilities.GetAADAuth(userHandle);
            PostUserRequest postUserRequest = new PostUserRequest(InstanceId, "Bob", "Smith", "mybio", null);
            Task<HttpOperationResponse<PostUserResponse>> postUserTask = myClient.Users.PostUserWithHttpMessagesAsync(request: postUserRequest, authorization: auth);

            // issue the batch and check that it failed
            bool failure = false;
            try
            {
                await batchHandler.IssueBatch();
            }
            catch
            {
                failure = true;
            }

            Assert.IsTrue(failure);
        }

        /// <summary>
        /// Issues a few simple requests in two batches
        /// </summary>
        /// <returns>Batch task</returns>
        [TestMethod]
        public async Task IssueTwoBatches()
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
            PostUserRequest postUserRequest = new PostUserRequest(InstanceId, "Bob", "Smith", "mybio", null);
            Task<HttpOperationResponse<PostUserResponse>> postUserTask = myClient.Users.PostUserWithHttpMessagesAsync(request: postUserRequest, authorization: auth);

            // issue the batch
            await batchHandler.IssueBatch();

            // verify server build info
            HttpOperationResponse<GetBuildInfoResponse> buildsResponse = await getBuildsTask;
            VerifyBuildsResponse(buildsResponse);

            // verify user login info
            HttpOperationResponse<PostUserResponse> postUserResponse = await postUserTask;
            VerifyPostUserResponse(postUserResponse);
            auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.Body.SessionToken);

            // reset the batch handler
            batchHandler.Reset();

            // request bad login to demonstrate error code
            PostUserRequest postUserRequest2 = new PostUserRequest(InstanceId, "Bob", "Smith", "mybio", null);
            Task<HttpOperationResponse<PostUserResponse>> postUserTask2 = myClient.Users.PostUserWithHttpMessagesAsync(request: postUserRequest2, authorization: auth);

            // request server build info
            Task<HttpOperationResponse<GetBuildInfoResponse>> getBuildsTask2 = myClient.Config.GetBuildInfoWithHttpMessagesAsync();

            // issue the batch
            await batchHandler.IssueBatch();

            // verify server build info
            HttpOperationResponse<GetBuildInfoResponse> buildsResponse2 = await getBuildsTask2;
            VerifyBuildsResponse(buildsResponse2);

            // verify user login failed
            HttpOperationResponse<PostUserResponse> postUserResponse2 = await postUserTask2;
            Assert.IsFalse(postUserResponse2.Response.IsSuccessStatusCode);

            // delete login
            SocialPlusClient myClient2 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            HttpOperationResponse<object> deleteUserResponse = await myClient2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            Assert.IsTrue(deleteUserResponse.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// Issues many simple requests as a batch with two handlers
        /// </summary>
        /// <returns>Batch task</returns>
        [TestMethod]
        public async Task IssueManyBatches()
        {
            // instantiate the handlers
            DelegatingHandler[] handlers = new DelegatingHandler[2];
            Uri batchURL = new Uri(TestConstants.ServerApiBaseUrl.OriginalString + "batch");
            BatchHttpMessageHandler batchHandler = new BatchHttpMessageHandler(HttpMethod.Post, batchURL);
            InstrumentedHttpMessageHandler testHandler = new InstrumentedHttpMessageHandler();
            handlers[0] = batchHandler; // this will be the inner handler
            handlers[1] = testHandler; // this will be the outer handler

            // instantiate the client, passing in the handler
            SocialPlusClient myClient = new SocialPlusClient(TestConstants.ServerApiBaseUrl, handlers);

            // request server build info many times
            int numRequests = 50;
            List<Task<HttpOperationResponse<GetBuildInfoResponse>>> getBuildsTasks = new List<Task<HttpOperationResponse<GetBuildInfoResponse>>>();
            for (int i = 0; i < numRequests; i++)
            {
                getBuildsTasks.Add(myClient.Config.GetBuildInfoWithHttpMessagesAsync());
            }

            // issue the batch
            await batchHandler.IssueBatch();

            // verify server build info responses
            for (int i = 0; i < numRequests; i++)
            {
                HttpOperationResponse<GetBuildInfoResponse> buildsResponse = await getBuildsTasks[i];
                VerifyBuildsResponse(buildsResponse);
            }
        }

        /// <summary>
        /// Checks that builds response is valid
        /// </summary>
        /// <param name="buildsResponse">builds response from the server</param>
        private static void VerifyBuildsResponse(HttpOperationResponse<GetBuildInfoResponse> buildsResponse)
        {
            Assert.IsTrue(buildsResponse.Response.IsSuccessStatusCode);
            Assert.IsFalse(string.IsNullOrWhiteSpace(buildsResponse.Body.CommitHash));
            Assert.IsFalse(string.IsNullOrWhiteSpace(buildsResponse.Body.DateAndTime));
            Assert.IsFalse(string.IsNullOrWhiteSpace(buildsResponse.Body.Hostname));
            Assert.IsFalse(string.IsNullOrWhiteSpace(buildsResponse.Body.Tag));
            Assert.IsNotNull(buildsResponse.Body.DirtyFiles);
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
