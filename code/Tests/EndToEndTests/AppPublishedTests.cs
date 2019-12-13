// <copyright file="AppPublishedTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;
    using SocialPlus.Utils;

    /// <summary>
    /// Basic functional end to end tests for app published topics
    /// </summary>
    [TestClass]
    public class AppPublishedTests
    {
        /// <summary>
        /// Create an app published topic, get it, and delete.
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task AppPublishedCreateVerifyDeleteTopicTest()
        {
            // get the app handle
            string appHandle = ManageAppsUtils.GetAppHandle(TestConstants.EnvironmentName);
            if (appHandle == null)
            {
                // fail the test
                Assert.Fail("Failed to lookup appHandle");
            }

            // Set up initial login etc
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user = await TestUtilities.PostGenericUser(client);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);

            string userHandle = user.UserHandle;

            // add user as admin
            bool added = ManageAppsUtils.AddAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
            if (!added)
            {
                // delete the user and fail the test
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to set user as administrator");
            }

            // create a topic
            string topicTitle = "My Favorite Topic";
            string topicText = "It is all about sports!";
            BlobType blobType = BlobType.Image;
            string blobHandle = "http://myBlobHandle/";
            string language = "en-US";
            string deepLink = "Sports!";
            string categories = "sports, ncurrency";
            string group = "mygroup";
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.App, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, categories: categories, language: language, deepLink: deepLink, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);

            // if create topic was a success, grab the topic handle, the topic, and delete the topic and user to cleanup
            string topicHandle = string.Empty;
            HttpOperationResponse<TopicView> getTopicOperationResponse = null;
            HttpOperationResponse<object> deleteTopicOperationResponse = null;
            HttpOperationResponse<object> deleteUserOperationResponse = null;
            bool? deleteAdminResult = null;
            if (postTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
                getTopicOperationResponse = await client.Topics.GetTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                deleteTopicOperationResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            }

            // otherwise, delete the admin and the user
            deleteAdminResult = ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
            deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // check everything went well
            Assert.IsTrue(postTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(getTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteAdminResult.HasValue);
            Assert.IsTrue(deleteAdminResult.Value);
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);
            TopicView getTopicResponse = getTopicOperationResponse.Body;
            Assert.AreEqual(getTopicResponse.BlobHandle, blobHandle);
            Assert.AreEqual(getTopicResponse.BlobType, blobType);
            if (getTopicResponse.BlobUrl.Contains("images/" + blobHandle) == false)
            {
                Assert.Fail(blobHandle + "should be contained in this: " + getTopicResponse.BlobUrl);
            }

            Assert.AreEqual(getTopicResponse.Categories, categories);
            Assert.IsTrue(getTopicResponse.ContentStatus == ContentStatus.Active || getTopicResponse.ContentStatus == ContentStatus.Clean);
            Assert.AreEqual(getTopicResponse.DeepLink, deepLink);
            Assert.AreEqual(getTopicResponse.Group, group);
            Assert.AreEqual(getTopicResponse.Language, language);
            Assert.AreEqual(getTopicResponse.Liked, false);
            Assert.AreEqual(getTopicResponse.Pinned, false);
            Assert.AreEqual(getTopicResponse.PublisherType, PublisherType.App);
            Assert.AreEqual(getTopicResponse.Text, topicText);
            Assert.AreEqual(getTopicResponse.Title, topicTitle);
            Assert.AreEqual(getTopicResponse.TopicHandle, topicHandle);
            Assert.AreEqual(getTopicResponse.TotalComments, 0);
            Assert.AreEqual(getTopicResponse.TotalLikes, 0);

            // for app-published topics, the user is null
            Assert.AreEqual(getTopicResponse.User, null);
        }

        /// <summary>
        /// Test pinning and unpinning an app published topic
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task AppPublishedPinUnpinTest()
        {
            // get the app handle
            string appHandle = ManageAppsUtils.GetAppHandle(TestConstants.EnvironmentName);
            if (appHandle == null)
            {
                // fail the test
                Assert.Fail("Failed to lookup appHandle");
            }

            var pt = new PinTests();
            await pt.PinUnPinDeletePinTestHelper(true, appHandle);
        }

        /// <summary>
        /// Test the pin feed for app published topics
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task AppPublishedGetPinFeedTest()
        {
            // get the app handle
            string appHandle = ManageAppsUtils.GetAppHandle(TestConstants.EnvironmentName);
            if (appHandle == null)
            {
                // fail the test
                Assert.Fail("Failed to lookup appHandle");
            }

            var pt = new PinTests();
            await pt.GetPinFeedTestHelper(true, appHandle);
        }

        /// <summary>
        /// Variant of the like topic test that is app published
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task AppPublishedLikeTopicTest()
        {
            // get the app handle
            string appHandle = ManageAppsUtils.GetAppHandle(TestConstants.EnvironmentName);
            if (appHandle == null)
            {
                // fail the test
                Assert.Fail("Failed to lookup appHandle");
            }

            var lt = new LikeTests();
            await lt.LikeTopicHelper(true, appHandle);
        }

        /// <summary>
        /// Variant of the get likes topic test that is app published
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task AppPublishedGetLikesTopicTest()
        {
            // get the app handle
            string appHandle = ManageAppsUtils.GetAppHandle(TestConstants.EnvironmentName);
            if (appHandle == null)
            {
                // fail the test
                Assert.Fail("Failed to lookup appHandle");
            }

            var lt = new LikeTests();
            await lt.GetLikesTopicTestHelper(true, appHandle);
        }

        /// <summary>
        /// Variant of the like topic comment reply delete test where the topic is app published
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task AppPublishedLikeCommentReplyDeleteTest()
        {
            // get the app handle
            string appHandle = ManageAppsUtils.GetAppHandle(TestConstants.EnvironmentName);
            if (appHandle == null)
            {
                // fail the test
                Assert.Fail("Failed to lookup appHandle");
            }

            var lt = new LikeTests();
            await lt.LikeCommentReplyDeleteTestHelper(true, appHandle);
        }

        /// <summary>
        /// Variant of the get put count notification test where the topic is app published
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task AppPublishedGetPutCountNotificationTest()
        {
            // get the app handle
            string appHandle = ManageAppsUtils.GetAppHandle(TestConstants.EnvironmentName);
            if (appHandle == null)
            {
                // fail the test
                Assert.Fail("Failed to lookup appHandle");
            }

            var nt = new NotificationsTests();
            await nt.GetPutCountNotificationTestHelper(true, appHandle);
        }

        /// <summary>
        /// This method tests the ability to search for a topic that was created by the app.
        /// It creates one user, elevates that user to admin, and posts one topic as an app.
        /// It then submits a search with a query string for that topic.
        /// It then deletes the topic and user.
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task AppPublishedTopicSearchTest()
        {
            // create the client
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // get the app handle
            string appHandle = ManageAppsUtils.GetAppHandle(TestConstants.EnvironmentName);
            if (string.IsNullOrWhiteSpace(appHandle))
            {
                // fail the test
                Assert.Fail("Failed to lookup appHandle");
            }

            // create a user
            var user = await TestUtilities.PostGenericUser(client);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);

            // get the user handle
            string userHandle = user.UserHandle;
            if (string.IsNullOrWhiteSpace(userHandle))
            {
                // fail the test
                Assert.Fail("Failed to get userHandle");
            }

            // elevate the user to admin
            bool added = ManageAppsUtils.AddAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
            if (!added)
            {
                // delete the user and fail the test
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to set user as administrator");
            }

            // create a unique string to search on
            string unique = Guid.NewGuid().ToString().Replace("-", string.Empty);

            // post a topic published by the app
            string topicTitle = unique;
            string topicText = "Something";
            BlobType blobType = BlobType.Unknown;
            string blobHandle = string.Empty;
            string language = "en-US";
            string deepLink = string.Empty;
            string categories = string.Empty;
            string friendlyName = string.Empty;
            string group = string.Empty;
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.App, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, language: language, deepLink: deepLink, categories: categories, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);

            // If the post topic operation failed, clean up
            if (postTopicOperationResponse == null || !postTopicOperationResponse.Response.IsSuccessStatusCode ||
                postTopicOperationResponse.Body == null || string.IsNullOrWhiteSpace(postTopicOperationResponse.Body.TopicHandle))
            {
                ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to post topic");
            }

            // Delay a bit to allow data to get into the search
            await Task.Delay(TestConstants.SearchDelay);

            // search for the single result
            HttpOperationResponse<FeedResponseTopicView> search = await client.Search.GetTopicsWithHttpMessagesAsync(query: unique, cursor: null, limit: 5, authorization: auth);

            // Clean up topic
            HttpOperationResponse<object> deleteTopic = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: postTopicOperationResponse.Body.TopicHandle, authorization: auth);

            // Clean up first user
            bool deleteAdminResult = ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
            HttpOperationResponse<object> deleteUser = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // Verify search result
            Assert.IsNotNull(search);
            Assert.IsNotNull(search.Body);
            Assert.IsNotNull(search.Body.Data);
            Assert.AreEqual(1, search.Body.Data.Count);
            Assert.AreEqual(postTopicOperationResponse.Body.TopicHandle, search.Body.Data[0].TopicHandle);
            Assert.AreEqual(unique, search.Body.Data[0].Title);

            // Verify deletions
            Assert.IsNotNull(deleteTopic);
            Assert.IsTrue(deleteTopic.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteAdminResult);
            Assert.IsNotNull(deleteUser);
            Assert.IsTrue(deleteUser.Response.IsSuccessStatusCode);
        }
    }
}
