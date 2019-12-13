// <copyright file="NamedTopicsTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.Configuration;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;
    using SocialPlus.Utils;

    /// <summary>
    /// Basic functional end to end tests for everything to do with Named Topics
    /// </summary>
    [TestClass]
    public class NamedTopicsTests
    {
        /// <summary>
        /// name of the server environment for use with ManageApps.exe
        /// </summary>
        private string environment = null;

        /// <summary>
        /// Create a named topic, read it, and then delete.
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task CreateVerifyDeleteNamedTopicTest()
        {
            HttpOperationResponse<object> deleteUserOperationResponse = null;
            HttpOperationResponse<object> deleteTopicOperationResponse = null;
            HttpOperationResponse<object> deleteTopicNameOperationResponse = null;
            DeleteTopicNameRequest deleteTopicNameReq = new DeleteTopicNameRequest(publisherType: PublisherType.App);
            int endIndex = TestConstants.ConfigFileName.IndexOf(".");
            this.environment = TestConstants.ConfigFileName.Substring(0, endIndex);

            // create a user
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var t = await this.CreateUserForTest(client);
            PostUserResponse postUserResponse = t.Item1;
            string auth = t.Item2;
            string userHandle = postUserResponse.UserHandle;

            string appHandle = ManageAppsUtils.GetAppHandle(this.environment);
            if (appHandle == null)
            {
                // delete the user and fail the test
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to lookup appHandle");
            }

            // add user as admin
            bool added = ManageAppsUtils.AddAdmin(this.environment, appHandle, userHandle);
            if (!added)
            {
                // delete the user and fail the test
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to set user as administrator");
            }

            // create a topic
            string topicTitle = "Test topic for named topics";
            string topicText = "This sure is a fine topic.";
            BlobType blobType = BlobType.Image;
            string blobHandle = "http://myBlobHandle/";
            string language = "en-US";
            string group = "mygroup";
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.User, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, language: language, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);

            // if create topic was a success, grab the topic handle
            string topicHandle = string.Empty;
            if (postTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
            }
            else
            {
                // otherwise, delete the admin, the user and fail the test
                ManageAppsUtils.DeleteAdmin(this.environment, appHandle, userHandle);
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Create topic failed, cannot finish the CreateVerifyUpdateDeleteNamedTopicTest");
            }

            string topicName = "UnitTestTopicName";

            // create a topic name
            PostTopicNameRequest postTopicNameReq = new PostTopicNameRequest(publisherType: PublisherType.App, topicName: topicName, topicHandle: topicHandle);
            HttpOperationResponse<object> postTopicNameOperationResponse = await client.Topics.PostTopicNameWithHttpMessagesAsync(request: postTopicNameReq, authorization: auth);

            // if creating the topic name fails, delete the topic, the user, and fail the test
            if (!postTopicNameOperationResponse.Response.IsSuccessStatusCode)
            {
                deleteTopicOperationResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(authorization: auth, topicHandle: topicHandle);
                ManageAppsUtils.DeleteAdmin(this.environment, appHandle, userHandle);
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Create topic name failed, cannot finish the CreateVerifyUpdateDeleteNamedTopicTest");
            }

            // get the topic name
            HttpOperationResponse<GetTopicByNameResponse> getTopicNameResponse = await client.Topics.GetTopicByNameWithHttpMessagesAsync(topicName: topicName, publisherType: PublisherType.App, authorization: auth);
            if (!getTopicNameResponse.Response.IsSuccessStatusCode)
            {
                // if get topic name fails, cleanup: delete the topic name, the admin, the user, and fail the test
                deleteTopicNameOperationResponse = await client.Topics.DeleteTopicNameWithHttpMessagesAsync(request: deleteTopicNameReq, authorization: auth, topicName: topicName);
                deleteTopicOperationResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(authorization: auth, topicHandle: topicHandle);
                ManageAppsUtils.DeleteAdmin(this.environment, appHandle, userHandle);
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("get topic name failed, cannot finish the CreateVerifyUpdateDeleteNamedTopicTest");
            }

            // delete the topic name we just created
            deleteTopicNameOperationResponse = await client.Topics.DeleteTopicNameWithHttpMessagesAsync(request: deleteTopicNameReq, authorization: auth, topicName: topicName);

            // if deleting the topic name fails, delete the topic, the admin, the user, and fail the test
            if (!deleteTopicNameOperationResponse.Response.IsSuccessStatusCode)
            {
                deleteTopicOperationResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(authorization: auth, topicHandle: topicHandle);
                ManageAppsUtils.DeleteAdmin(this.environment, appHandle, userHandle);
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Delete topic name failed, cannot finish the CreateVerifyUpdateDeleteNamedTopicTest");
            }

            // do standard cleanup : delete the topic, delete the admin, and then delete the user
            deleteTopicOperationResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(authorization: auth, topicHandle: topicHandle);
            ManageAppsUtils.DeleteAdmin(this.environment, appHandle, userHandle);
            deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // if we reach here, the test was successful.
            return;
        }

        /// <summary>
        /// Create a named topic, read it, and then delete.
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task CreateUpdateDeleteNamedTopicTest()
        {
            var deleteRequest = new DeleteTopicNameRequest() { PublisherType = PublisherType.App };
            int endIndex = TestConstants.ConfigFileName.IndexOf(".");
            this.environment = TestConstants.ConfigFileName.Substring(0, endIndex);

            // create a user
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var t = await this.CreateUserForTest(client);
            PostUserResponse postUserResponse = t.Item1;
            string auth = t.Item2;
            string userHandle = postUserResponse.UserHandle;

            // get the app handle
            string appHandle = ManageAppsUtils.GetAppHandle(this.environment);
            if (appHandle == null)
            {
                // delete the user and fail the test
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to lookup appHandle");
            }

            // add user as admin
            bool added = ManageAppsUtils.AddAdmin(this.environment, appHandle, userHandle);
            if (!added)
            {
                // delete the user and fail the test
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to set user as administrator");
            }

            // create a topic
            string topicTitle = "Test topic for named topics";
            string topicText = "This sure is a fine topic.";
            BlobType blobType = BlobType.Image;
            string blobHandle = "http://myBlobHandle/";
            string language = "en-US";
            string group = "mygroup";
            string topicHandle = null;
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.User, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, language: language, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);
            if (postTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
            }

            // create another topic
            string topicTitle2 = "Test topic #2 for named topics";
            string topicText2 = "This one also sure is a fine topic.";
            string language2 = "en-US";
            string group2 = "mygroup2";
            string topicHandle2 = null;
            PostTopicRequest postTopicRequest2 = new PostTopicRequest(publisherType: PublisherType.User, text: topicText2, title: topicTitle2, language: language2, group: group2);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse2 = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest2, authorization: auth);
            if (postTopicOperationResponse2.Response.IsSuccessStatusCode)
            {
                topicHandle2 = postTopicOperationResponse2.Body.TopicHandle;
            }

            if (!(postTopicOperationResponse.Response.IsSuccessStatusCode && postTopicOperationResponse2.Response.IsSuccessStatusCode))
            {
                // if either topic creation fails, cleanup:
                // delete both topics, the admin, the user and fail the test
                await client.Topics.DeleteTopicWithHttpMessagesAsync(authorization: auth, topicHandle: topicHandle);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(authorization: auth, topicHandle: topicHandle2);
                ManageAppsUtils.DeleteAdmin(this.environment, appHandle, userHandle);
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Create topics failed, cannot finish the CreateVerifyUpdateDeleteNamedTopicTest");
            }

            // create a topic name for topic #1
            string topicName = "SpecialTopicName";
            PostTopicNameRequest postTopicNameReq = new PostTopicNameRequest(publisherType: PublisherType.App, topicName: topicName, topicHandle: topicHandle);
            HttpOperationResponse<object> postTopicNameOperationResponse = await client.Topics.PostTopicNameWithHttpMessagesAsync(request: postTopicNameReq, authorization: auth);
            if (!postTopicNameOperationResponse.Response.IsSuccessStatusCode)
            {
                // if creating the topic name fails, cleanup:
                // delete both topics, the admin, the user, and fail the test
                await client.Topics.DeleteTopicWithHttpMessagesAsync(authorization: auth, topicHandle: topicHandle);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(authorization: auth, topicHandle: topicHandle2);
                ManageAppsUtils.DeleteAdmin(this.environment, appHandle, userHandle);
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Create topic name failed, cannot finish the CreateVerifyUpdateDeleteNamedTopicTest");
            }

            // update the topic name so that it now refers to topic #2
            PutTopicNameRequest putTopicNameReq = new PutTopicNameRequest(publisherType: PublisherType.App, topicHandle: topicHandle2);
            HttpOperationResponse<object> putTopicNameOperationResponse = await client.Topics.PutTopicNameWithHttpMessagesAsync(topicName: topicName, request: putTopicNameReq, authorization: auth);
            if (!putTopicNameOperationResponse.Response.IsSuccessStatusCode)
            {
                // if updating the topic name fails, cleanup
                await client.Topics.DeleteTopicNameWithHttpMessagesAsync(topicName: topicName, request: deleteRequest, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(authorization: auth, topicHandle: topicHandle);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(authorization: auth, topicHandle: topicHandle2);
                ManageAppsUtils.DeleteAdmin(this.environment, appHandle, userHandle);
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Put topic name failed, cannot finish the CreateVerifyUpdateDeleteNamedTopicTest");
            }

            // do the standard cleanup
            await client.Topics.DeleteTopicNameWithHttpMessagesAsync(topicName: topicName, request: deleteRequest, authorization: auth);
            await client.Topics.DeleteTopicWithHttpMessagesAsync(authorization: auth, topicHandle: topicHandle);
            await client.Topics.DeleteTopicWithHttpMessagesAsync(authorization: auth, topicHandle: topicHandle2);
            ManageAppsUtils.DeleteAdmin(this.environment, appHandle, userHandle);
            await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
        }

        /// <summary>
        /// helper routine to create a user for named topics tests
        /// </summary>
        /// <param name="client">social plus client</param>
        /// <returns>a post user response and an authorization header value</returns>
        private async Task<Tuple<PostUserResponse, string>> CreateUserForTest(SocialPlusClient client)
        {
            string firstName = "Joe";
            string lastName = "Blow";
            string bio = "Joe Joe Joe";
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            return new Tuple<PostUserResponse, string>(postUserResponse, auth);
        }
    }
}
