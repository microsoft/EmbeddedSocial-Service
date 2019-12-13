// <copyright file="BatchTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
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
    /// Tests that use batch calls
    /// </summary>
    [TestClass]
    public class BatchTests
    {
        /// <summary>
        /// handle generator
        /// </summary>
        private static readonly HandleGenerator HandleGenerator = new HandleGenerator();

        /// <summary>
        /// Test a batch call with two get topic operations
        /// </summary>
        /// <returns>a task</returns>
        [TestMethod]
        public async Task BatchTestMultiGetTopics()
        {
            // Set up initial login etc
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            string firstName = "Stan";
            string lastName = "TopicMan";
            string bio = string.Empty;
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            string topicTitle1 = "My Favorite Topic";
            string topicText1 = "It is all about sports!";
            BlobType blobType = BlobType.Image;
            string blobHandle = "http://myBlobHandle/";
            string language = "en-US";
            string deepLink = "Sports!";
            string categories = "sports, ncurrency";
            string friendlyName = "Game On!";
            string group = "mygroup";

            PostTopicRequest postTopicRequest1 = new PostTopicRequest(publisherType: PublisherType.User, text: topicText1, title: topicTitle1, blobType: blobType, blobHandle: blobHandle, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse1 = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest1, authorization: auth);

            string topicTitle2 = "My Favorite Topic #2";
            string topicText2 = "Maybe it isn't all about sports?";
            PostTopicRequest postTopicRequest2 = new PostTopicRequest(publisherType: PublisherType.User, text: topicText2, title: topicTitle2, blobType: blobType, blobHandle: blobHandle, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse2 = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest2, authorization: auth);

            // extract topic handles from the responses
            var topicHandle1 = postTopicOperationResponse1.Body.TopicHandle;
            var topicHandle2 = postTopicOperationResponse2.Body.TopicHandle;

            // create a new batch operation
            Uri batchURL = new Uri(TestConstants.ServerApiBaseUrl.OriginalString + "batch");
            BatchHttpMessageHandler batchHandler = new BatchHttpMessageHandler(HttpMethod.Post, batchURL);

            // Create a batch client
            DelegatingHandler[] handlers = new DelegatingHandler[1];
            handlers[0] = batchHandler;
            SocialPlusClient batchClient = new SocialPlusClient(TestConstants.ServerApiBaseUrl, handlers);

            // put two calls to GetTopic inside the batch
            Task<HttpOperationResponse<TopicView>> getTopic1 = batchClient.Topics.GetTopicWithHttpMessagesAsync(topicHandle1, auth);
            Task<HttpOperationResponse<TopicView>> getTopic2 = batchClient.Topics.GetTopicWithHttpMessagesAsync(topicHandle2, auth);

            // send the batch to the server
            await batchHandler.IssueBatch();

            // process the individual results from the batch
            var topicResult1 = await getTopic1;
            var topicResult2 = await getTopic2;

            // clean up
            await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle1, auth);
            await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle2, auth);
            await client.Users.DeleteUserWithHttpMessagesAsync(auth);

            // after clean up, check that the get topic operations inside the batch were successful
            Assert.AreEqual(categories, topicResult1.Body.Categories);
            Assert.AreEqual(ContentStatus.Active, topicResult1.Body.ContentStatus);
            Assert.AreEqual(deepLink, topicResult1.Body.DeepLink);
            Assert.AreEqual(friendlyName, topicResult1.Body.FriendlyName);
            Assert.AreEqual(group, topicResult1.Body.Group);
            Assert.AreEqual(language, topicResult1.Body.Language);
            Assert.AreEqual(false, topicResult1.Body.Liked);
            Assert.AreEqual(false, topicResult1.Body.Pinned);
            Assert.AreEqual(PublisherType.User, topicResult1.Body.PublisherType);
            Assert.AreEqual(topicText1, topicResult1.Body.Text);
            Assert.AreEqual(topicTitle1, topicResult1.Body.Title);
            Assert.AreEqual(topicHandle1, topicResult1.Body.TopicHandle);
            Assert.AreEqual(0, topicResult1.Body.TotalComments);
            Assert.AreEqual(0, topicResult1.Body.TotalLikes);
            Assert.AreEqual(firstName, topicResult1.Body.User.FirstName);
            Assert.AreEqual(lastName, topicResult1.Body.User.LastName);
            Assert.AreEqual(postUserResponse.UserHandle, topicResult1.Body.User.UserHandle);

            Assert.AreEqual(categories, topicResult2.Body.Categories);
            Assert.AreEqual(ContentStatus.Active, topicResult2.Body.ContentStatus);
            Assert.AreEqual(deepLink, topicResult2.Body.DeepLink);
            Assert.AreEqual(friendlyName, topicResult2.Body.FriendlyName);
            Assert.AreEqual(group, topicResult2.Body.Group);
            Assert.AreEqual(language, topicResult2.Body.Language);
            Assert.AreEqual(false, topicResult2.Body.Liked);
            Assert.AreEqual(false, topicResult2.Body.Pinned);
            Assert.AreEqual(PublisherType.User, topicResult2.Body.PublisherType);
            Assert.AreEqual(topicText2, topicResult2.Body.Text);
            Assert.AreEqual(topicTitle2, topicResult2.Body.Title);
            Assert.AreEqual(topicHandle2, topicResult2.Body.TopicHandle);
            Assert.AreEqual(0, topicResult2.Body.TotalComments);
            Assert.AreEqual(0, topicResult2.Body.TotalLikes);
            Assert.AreEqual(firstName, topicResult2.Body.User.FirstName);
            Assert.AreEqual(lastName, topicResult2.Body.User.LastName);
            Assert.AreEqual(postUserResponse.UserHandle, topicResult2.Body.User.UserHandle);
        }

        /// <summary>
        /// Test a batch call that creates two users
        /// </summary>
        /// <returns>a task</returns>
        [TestMethod]
        public async Task BatchTestMultiPostUsers()
        {
            // create a new batch operation
            Uri batchURL = new Uri(TestConstants.ServerApiBaseUrl.OriginalString + "batch");
            BatchHttpMessageHandler batchHandler = new BatchHttpMessageHandler(HttpMethod.Post, batchURL);

            // Create a batch client
            DelegatingHandler[] handlers = new DelegatingHandler[1];
            handlers[0] = batchHandler;
            SocialPlusClient batchClient = new SocialPlusClient(TestConstants.ServerApiBaseUrl, handlers);

            // user1
            string instanceId = TestConstants.InstanceId;
            string firstName1 = "User1";
            string lastName1 = "Johnson";
            string bio1 = "Some Bio";

            // user2
            string firstName2 = "User2";
            string lastName2 = "Ralston";
            string bio2 = "Yet Another Bio";

            // put two calls to PostUser inside the batch
            PostUserRequest postUserRequest1 = new PostUserRequest(instanceId: instanceId, firstName: firstName1, lastName: lastName1, bio: bio1);
            PostUserRequest postUserRequest2 = new PostUserRequest(instanceId: instanceId, firstName: firstName2, lastName: lastName2, bio: bio2);

            string userHandle1 = HandleGenerator.GenerateShortHandle();
            string auth1 = await TestUtilities.GetAADAuth(userHandle1);
            string userHandle2 = HandleGenerator.GenerateShortHandle();
            string auth2 = await TestUtilities.GetAADAuth(userHandle2);
            Task<HttpOperationResponse<PostUserResponse>> postUserTask1 = batchClient.Users.PostUserWithHttpMessagesAsync(postUserRequest1, auth1);
            Task<HttpOperationResponse<PostUserResponse>> postUserTask2 = batchClient.Users.PostUserWithHttpMessagesAsync(postUserRequest2, auth2);

            // issue the batch
            await batchHandler.IssueBatch();

            // process the individual results from the batch
            var postUserResult1 = await postUserTask1;
            var postUserResult2 = await postUserTask2;

            // Use a regular (non-batch) client from now on
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // Call Get User
            UserProfileView userProfile1 = await client.Users.GetUserAsync(userHandle: userHandle1, authorization: auth1);
            UserProfileView userProfile2 = await client.Users.GetUserAsync(userHandle: userHandle2, authorization: auth2);

            // clean up (delete both users)
            await client.Users.DeleteUserAsync(auth1);
            await client.Users.DeleteUserAsync(auth2);

            Assert.AreEqual(bio1, userProfile1.Bio);
            Assert.AreEqual(firstName1, userProfile1.FirstName);
            Assert.AreEqual(lastName1, userProfile1.LastName);

            Assert.AreEqual(bio2, userProfile2.Bio);
            Assert.AreEqual(firstName2, userProfile2.FirstName);
            Assert.AreEqual(lastName2, userProfile2.LastName);
        }
    }
}
