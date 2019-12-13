// <copyright file="TopicTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using Microsoft.Rest.TransientFaultHandling;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;
    using SocialPlus.Utils;

    /// <summary>
    /// Basic functional end to end tests for everything to do with Topics
    /// </summary>
    [TestClass]
    public class TopicTests
    {
        /// <summary>
        /// Create a topic, get properties and delete.
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task CreateVerifyDeleteTopicTest()
        {
            // Set up initial login etc
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            string firstName = "Stan";
            string lastName = "TopicMan";
            string bio = string.Empty;
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            // create a topic
            string topicTitle = "My Favorite Topic";
            string topicText = "It is all about sports!";
            BlobType blobType = BlobType.Image;
            string blobHandle = "http://myBlobHandle/";
            string language = "en-US";
            string deepLink = "Sports!";
            string categories = "sports, ncurrency";
            string friendlyName = "Game On!";
            string group = "mygroup";
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.User, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);

            // if create topic was a success, grab the topic handle, the topic, and delete the topic and user to cleanup
            string topicHandle = string.Empty;
            HttpOperationResponse<TopicView> getTopicOperationResponse = null;
            HttpOperationResponse<object> deleteTopicOperationResponse = null;
            HttpOperationResponse<object> deleteUserOperationResponse = null;
            if (postTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
                getTopicOperationResponse = await client.Topics.GetTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                deleteTopicOperationResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            }

            // otherwise, delete the user
            else
            {
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            }

            // check everything went well
            Assert.IsTrue(getTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse.Response.IsSuccessStatusCode);
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
            Assert.AreEqual(getTopicResponse.FriendlyName, friendlyName);
            Assert.AreEqual(getTopicResponse.Group, group);
            Assert.AreEqual(getTopicResponse.Language, language);
            Assert.AreEqual(getTopicResponse.Liked, false);
            Assert.AreEqual(getTopicResponse.Pinned, false);
            Assert.AreEqual(getTopicResponse.PublisherType, PublisherType.User);
            Assert.AreEqual(getTopicResponse.Text, topicText);
            Assert.AreEqual(getTopicResponse.Title, topicTitle);
            Assert.AreEqual(getTopicResponse.TopicHandle, topicHandle);
            Assert.AreEqual(getTopicResponse.TotalComments, 0);
            Assert.AreEqual(getTopicResponse.TotalLikes, 0);
            Assert.AreEqual(getTopicResponse.User.FirstName, firstName);
            Assert.AreEqual(getTopicResponse.User.LastName, lastName);
            Assert.AreEqual(getTopicResponse.User.UserHandle, postUserResponse.UserHandle);
        }

        /// <summary>
        /// Create topics and Gets them
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task GetTopicsTest()
        {
            // Set up initial login etc
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            string firstName = "Stan";
            string lastName = "TopicMan";
            string bio = string.Empty;
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            // create a topic
            string topicTitle = "My Favorite Topic";
            string topicText = "It is all about sports!";
            BlobType blobType = BlobType.Custom;
            string blobHandle = "http://myBlobHandle/";
            string language = string.Empty;
            string deepLink = string.Empty;
            string categories = string.Empty;
            string friendlyName = string.Empty;
            string group = string.Empty;
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.User, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);

            // create a second one
            string topicTitle2 = "My Second Favorite Topic";
            string topicText2 = "It is all about food!";
            BlobType blobType2 = BlobType.Video;
            string blobHandle2 = "http://myFood/food.wma/";
            PostTopicRequest postTopicRequest2 = new PostTopicRequest(publisherType: PublisherType.User, text: topicText2, title: topicTitle2, blobType: blobType2, blobHandle: blobHandle2, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse2 = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest2, authorization: auth);

            // if create topics was a success, grab the topic handles, the topics, and delete the topics and user to cleanup
            string topicHandle = string.Empty;
            string topicHandle2 = string.Empty;
            HttpOperationResponse<FeedResponseTopicView> getTopicsOperationResponse = null;
            HttpOperationResponse<object> deleteTopicOperationResponse = null;
            HttpOperationResponse<object> deleteTopicOperationResponse2 = null;
            HttpOperationResponse<object> deleteUserOperationResponse = null;
            if (postTopicOperationResponse.Response.IsSuccessStatusCode && postTopicOperationResponse2.Response.IsSuccessStatusCode)
            {
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
                topicHandle2 = postTopicOperationResponse2.Body.TopicHandle;

                // get the feed
                string cursor = null;
                int limit = 5;
                getTopicsOperationResponse = await client.MyTopics.GetTopicsWithHttpMessagesAsync(authorization: auth, cursor: cursor, limit: limit);
                deleteTopicOperationResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                deleteTopicOperationResponse2 = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth);
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            }

            // otherwise, clean up
            else if (!postTopicOperationResponse.Response.IsSuccessStatusCode && postTopicOperationResponse2.Response.IsSuccessStatusCode)
            {
                topicHandle2 = postTopicOperationResponse2.Body.TopicHandle;
                deleteTopicOperationResponse2 = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth);
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            }
            else if (postTopicOperationResponse.Response.IsSuccessStatusCode && !postTopicOperationResponse2.Response.IsSuccessStatusCode)
            {
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
                deleteTopicOperationResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            }
            else
            {
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            }

            // check everything went well
            Assert.IsTrue(getTopicsOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);
            IList<TopicView> topicListResponse = getTopicsOperationResponse.Body.Data;
            Assert.AreEqual(topicListResponse[0].BlobHandle, blobHandle2);
            Assert.AreEqual(topicListResponse[0].BlobType, blobType2);
            Assert.AreEqual(topicListResponse[0].Title, topicTitle2);
            Assert.AreEqual(topicListResponse[0].Text, topicText2);
            Assert.AreEqual(topicListResponse[1].BlobHandle, blobHandle);
            Assert.AreEqual(topicListResponse[1].BlobType, blobType);
            Assert.AreEqual(topicListResponse[1].Title, topicTitle);
            Assert.AreEqual(topicListResponse[1].Text, topicText);
        }

        /// <summary>
        /// Create a topic and tries to get it using an empty cursor
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task GetTopicTestWithEmptyString()
        {
            Assert.Fail("Bug #433 - Entering a string.Empty for GetTopics should act same as Null");

            // Set up initial login etc
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            string firstName = "Dave";
            string lastName = "Niehaus";
            string bio = string.Empty;
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            // create a topic
            string topicTitle = "Mariners";
            string topicText = "It is all about sports!";
            BlobType blobType = BlobType.Custom;
            string blobHandle = "http://myBlobHandle/";
            string language = string.Empty;
            string deepLink = string.Empty;
            string categories = string.Empty;
            string friendlyName = string.Empty;
            string group = string.Empty;
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.User, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);

            // if create topic was a success, grab the topic handle, the topic, and delete the topic and user to cleanup
            string topicHandle = string.Empty;
            HttpOperationResponse<FeedResponseTopicView> getTopicsOperationResponse = null;
            HttpOperationResponse<object> deleteTopicOperationResponse = null;
            HttpOperationResponse<object> deleteUserOperationResponse = null;
            if (postTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                topicHandle = postTopicOperationResponse.Body.TopicHandle;

                // get the feed
                string cursor = string.Empty;
                int limit = 5;
                getTopicsOperationResponse = await client.MyTopics.GetTopicsWithHttpMessagesAsync(authorization: auth, cursor: cursor, limit: limit);
                deleteTopicOperationResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            }

            // otherwise, delete the user
            else
            {
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            }

            // check everything went well
            Assert.IsTrue(getTopicsOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);
            IList<TopicView> topicListResponse = getTopicsOperationResponse.Body.Data;
            Assert.AreEqual(topicListResponse[0].BlobHandle, blobHandle);
            Assert.AreEqual(topicListResponse[0].BlobType, blobType);
            Assert.AreEqual(topicListResponse[0].Title, topicTitle);
            Assert.AreEqual(topicListResponse[0].Text, topicText);
        }

        /// <summary>
        /// Create a topic, change properties and delete.
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task PutTopicTest()
        {
            // Set up initial login etc
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            string firstName = "Stan";
            string lastName = "TopicMan";
            string bio = string.Empty;
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            // create a topic
            string topicTitle = "A";
            string topicText = "B";
            BlobType blobType = BlobType.Unknown;
            string blobHandle = "/C/";
            string language = "D";
            string deepLink = "E";
            string categories = "F";
            string friendlyName = "G";
            string group = "H";
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.User, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);

            // Change Values
            string topicTitleCHANGE = "CHANGE My Favorite Topic";
            string topicTextCHANGE = "CHANGE It is all about sports!";
            string categoriesCHANGE = "CHANGEsports, ncurrency";

            // if create topic was a success, grab the topic handle, change the topic, get the topic, and delete the topic and user to cleanup
            string topicHandle = string.Empty;
            HttpOperationResponse<object> putTopicOperationResponse = null;
            HttpOperationResponse<TopicView> getTopicOperationResponse = null;
            HttpOperationResponse<object> deleteTopicOperationResponse = null;
            HttpOperationResponse<object> deleteUserOperationResponse = null;
            if (postTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
                PutTopicRequest putTopicRequest = new PutTopicRequest(text: topicTextCHANGE, title: topicTitleCHANGE, categories: categoriesCHANGE);
                putTopicOperationResponse = await client.Topics.PutTopicWithHttpMessagesAsync(topicHandle: topicHandle, request: putTopicRequest, authorization: auth);
                getTopicOperationResponse = await client.Topics.GetTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                deleteTopicOperationResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            }

            // otherwise, cleanup
            else
            {
                deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            }

            // check everything went well
            Assert.IsTrue(putTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(getTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);
            TopicView getTopicResponse = getTopicOperationResponse.Body;
            Assert.AreEqual(getTopicResponse.BlobHandle, blobHandle);
            Assert.AreEqual(getTopicResponse.BlobType, blobType);
            if (getTopicResponse.BlobUrl.Contains("blobs/" + blobHandle) == false)
            {
                Assert.Fail(blobHandle + "should be contained in this: " + getTopicResponse.BlobUrl);
            }

            Assert.AreEqual(getTopicResponse.Categories, categoriesCHANGE);
            Assert.IsTrue(getTopicResponse.ContentStatus == ContentStatus.Active || getTopicResponse.ContentStatus == ContentStatus.Clean);
            Assert.AreEqual(getTopicResponse.DeepLink, deepLink);
            Assert.AreEqual(getTopicResponse.FriendlyName, friendlyName);
            Assert.AreEqual(getTopicResponse.Group, group);
            Assert.AreEqual(getTopicResponse.Language, language);
            Assert.AreEqual(getTopicResponse.Liked, false);
            Assert.AreEqual(getTopicResponse.Pinned, false);
            Assert.AreEqual(getTopicResponse.PublisherType, PublisherType.User);
            Assert.AreEqual(getTopicResponse.Text, topicTextCHANGE);
            Assert.AreEqual(getTopicResponse.Title, topicTitleCHANGE);
            Assert.AreEqual(getTopicResponse.TopicHandle, topicHandle);
            Assert.AreEqual(getTopicResponse.TotalComments, 0);
            Assert.AreEqual(getTopicResponse.TotalLikes, 0);
            Assert.AreEqual(getTopicResponse.User.FirstName, firstName);
            Assert.AreEqual(getTopicResponse.User.LastName, lastName);
            Assert.AreEqual(getTopicResponse.User.UserHandle, postUserResponse.UserHandle);
        }

        /// <summary>
        /// Handles All Time Ranges for the Popular Topics test
        /// </summary>
        /// <returns>Task that throws an exception if the test fails</returns>
        [TestMethod]
        public async Task PopularTopicsTest()
        {
            // ensure that popular topics feeds are empty; otherwise this test cannot proceed
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            string anonAuth = TestUtilities.GetAnonAuth();
            HttpOperationResponse<FeedResponseTopicView> precheck1 = await client.Topics.GetPopularTopicsWithHttpMessagesAsync(timeRange: TimeRange.Today, authorization: anonAuth, cursor: null, limit: 10);
            HttpOperationResponse<FeedResponseTopicView> precheck2 = await client.Topics.GetPopularTopicsWithHttpMessagesAsync(timeRange: TimeRange.ThisWeek, authorization: anonAuth, cursor: null, limit: 10);
            HttpOperationResponse<FeedResponseTopicView> precheck3 = await client.Topics.GetPopularTopicsWithHttpMessagesAsync(timeRange: TimeRange.ThisMonth, authorization: anonAuth, cursor: null, limit: 10);
            HttpOperationResponse<FeedResponseTopicView> precheck4 = await client.Topics.GetPopularTopicsWithHttpMessagesAsync(timeRange: TimeRange.AllTime, authorization: anonAuth, cursor: null, limit: 10);

            try
            {
                Assert.IsTrue(precheck1.Response.IsSuccessStatusCode);
                Assert.IsTrue(precheck2.Response.IsSuccessStatusCode);
                Assert.IsTrue(precheck3.Response.IsSuccessStatusCode);
                Assert.IsTrue(precheck4.Response.IsSuccessStatusCode);
                Assert.AreEqual(precheck1.Body.Data.Count, 0);
                Assert.AreEqual(precheck2.Body.Data.Count, 0);
                Assert.AreEqual(precheck3.Body.Data.Count, 0);
                Assert.AreEqual(precheck4.Body.Data.Count, 0);
            }
            catch (Exception)
            {
                Console.WriteLine("PopularTopicsTest: precheck conditions failed.");
                Console.WriteLine("   If you are running this test in a dev environment, use CleanAndRecreateEnv.ps1");
                Console.WriteLine("   Otherwise, you may need to manually clean up the popular topics state.");
                throw;
            }

            // create user1 and user2
            var postUserResponse1 = await TestUtilities.PostGenericUser(client);
            var postUserResponse2 = await TestUtilities.PostGenericUser(client);
            var postUserResponse3 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);
            string auth3 = AuthHelper.CreateSocialPlusAuth(postUserResponse3.SessionToken);

            // each user creates a topic
            var postTopicOperationResponse1 = await TestUtilities.PostGenericTopic(client, auth1);
            var postTopicOperationResponse2 = await TestUtilities.PostGenericTopic(client, auth2);
            var postTopicOperationResponse3 = await TestUtilities.PostGenericTopic(client, auth3);

            // extract topic handles
            string topicHandle1 = postTopicOperationResponse1.TopicHandle;
            string topicHandle2 = postTopicOperationResponse2.TopicHandle;
            string topicHandle3 = postTopicOperationResponse3.TopicHandle;

            // issue likes: user2 and user3 like topic1 and user3 likes topic2
            HttpOperationResponse<object> likeTopicOperationResponse1 = await client.TopicLikes.PostLikeWithHttpMessagesAsync(topicHandle: topicHandle1, authorization: auth2);
            HttpOperationResponse<object> likeTopicOperationResponse2 = await client.TopicLikes.PostLikeWithHttpMessagesAsync(topicHandle: topicHandle1, authorization: auth3);
            HttpOperationResponse<object> likeTopicOperationResponse3 = await client.TopicLikes.PostLikeWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth3);

            // Get popular topics for today, while waiting for likes to persist
            HttpOperationResponse<FeedResponseTopicView> popularTopicsOperationResponse1 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    popularTopicsOperationResponse1 = await client.Topics.GetPopularTopicsWithHttpMessagesAsync(timeRange: TimeRange.Today, authorization: anonAuth, cursor: null, limit: 10);
                }, () =>
                {
                    Assert.IsTrue(popularTopicsOperationResponse1.Response.IsSuccessStatusCode);
                    Assert.IsTrue(popularTopicsOperationResponse1.Body.Data.Count >= 2);
                    Assert.AreEqual(2, popularTopicsOperationResponse1.Body.Data[0].TotalLikes);
                    Assert.AreEqual(1, popularTopicsOperationResponse1.Body.Data[1].TotalLikes);
                });

            // Get popular topics for today with smaller limit
            HttpOperationResponse<FeedResponseTopicView> popularTopicsOperationResponse2 = await client.Topics.GetPopularTopicsWithHttpMessagesAsync(timeRange: TimeRange.Today, authorization: anonAuth, cursor: null, limit: 1);

            // Get popular topics for today with different cursor
            HttpOperationResponse<FeedResponseTopicView> popularTopicsOperationResponse3 = await client.Topics.GetPopularTopicsWithHttpMessagesAsync(timeRange: TimeRange.Today, authorization: anonAuth, cursor: int.Parse(popularTopicsOperationResponse2.Body.Cursor), limit: 1);

            // Get popular topics for week
            HttpOperationResponse<FeedResponseTopicView> popularTopicsOperationResponse4 = await client.Topics.GetPopularTopicsWithHttpMessagesAsync(timeRange: TimeRange.ThisWeek, authorization: anonAuth, cursor: null, limit: 10);

            // Get popular topics for month
            HttpOperationResponse<FeedResponseTopicView> popularTopicsOperationResponse5 = await client.Topics.GetPopularTopicsWithHttpMessagesAsync(timeRange: TimeRange.ThisMonth, authorization: anonAuth, cursor: null, limit: 10);

            // Get popular topics for all time
            HttpOperationResponse<FeedResponseTopicView> popularTopicsOperationResponse6 = await client.Topics.GetPopularTopicsWithHttpMessagesAsync(timeRange: TimeRange.AllTime, authorization: anonAuth, cursor: null, limit: 10);

            // Delete topic1
            HttpOperationResponse<object> deleteTopicOperationResponse1 = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle1, authorization: auth1);

            // Wait to see that topic1 is deleted from the popular topics feed
            HttpOperationResponse<FeedResponseTopicView> popularTopicsOperationResponse7 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    popularTopicsOperationResponse7 = await client.Topics.GetPopularTopicsWithHttpMessagesAsync(timeRange: TimeRange.Today, authorization: anonAuth, cursor: null, limit: 10);
                }, () =>
                {
                    Assert.IsTrue(popularTopicsOperationResponse7.Response.IsSuccessStatusCode);
                    Assert.IsTrue(popularTopicsOperationResponse7.Body.Data.Count <= 2);
                });

            // Delete topic2
            HttpOperationResponse<object> deleteTopicOperationResponse2 = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth2);

            // Wait to see that topic2 is deleted from the popular topics feed
            HttpOperationResponse<FeedResponseTopicView> popularTopicsOperationResponse8 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    popularTopicsOperationResponse8 = await client.Topics.GetPopularTopicsWithHttpMessagesAsync(timeRange: TimeRange.Today, authorization: anonAuth, cursor: null, limit: 10);
                }, () =>
                {
                    Assert.IsTrue(popularTopicsOperationResponse8.Response.IsSuccessStatusCode);
                    Assert.IsTrue(popularTopicsOperationResponse8.Body.Data.Count <= 1);
                });

            // delete topic3
            HttpOperationResponse<object> deleteTopicOperationResponse3 = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle3, authorization: auth3);

            // Wait to see that topic3 is deleted from the popular topics feed
            HttpOperationResponse<FeedResponseTopicView> popularTopicsOperationResponse9 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    popularTopicsOperationResponse9 = await client.Topics.GetPopularTopicsWithHttpMessagesAsync(timeRange: TimeRange.Today, authorization: anonAuth, cursor: null, limit: 10);
                }, () =>
                {
                    Assert.IsTrue(popularTopicsOperationResponse9.Response.IsSuccessStatusCode);
                    Assert.AreEqual(0, popularTopicsOperationResponse9.Body.Data.Count);
                });

            // cleanup users
            var deleteUserOperationResponse1 = await TestUtilities.DeleteUser(client, auth1);
            var deleteUserOperationResponse2 = await TestUtilities.DeleteUser(client, auth2);
            var deleteUserOperationResponse3 = await TestUtilities.DeleteUser(client, auth3);

            // check failure conditions
            Assert.IsTrue(deleteTopicOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse3.Response.IsSuccessStatusCode);
            Assert.IsTrue(likeTopicOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(likeTopicOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(likeTopicOperationResponse3.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse3.Response.IsSuccessStatusCode);
            Assert.IsTrue(popularTopicsOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(popularTopicsOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(popularTopicsOperationResponse3.Response.IsSuccessStatusCode);
            Assert.IsTrue(popularTopicsOperationResponse4.Response.IsSuccessStatusCode);
            Assert.IsTrue(popularTopicsOperationResponse5.Response.IsSuccessStatusCode);
            Assert.IsTrue(popularTopicsOperationResponse6.Response.IsSuccessStatusCode);
            Assert.IsTrue(popularTopicsOperationResponse7.Response.IsSuccessStatusCode);
            Assert.IsTrue(popularTopicsOperationResponse8.Response.IsSuccessStatusCode);
            Assert.IsTrue(popularTopicsOperationResponse9.Response.IsSuccessStatusCode);

            // Verify things now that all cleaned up - start with basic "Today"
            FeedResponseTopicView popularTopics1 = popularTopicsOperationResponse1.Body;
            Assert.IsTrue(popularTopics1.Data.Count >= 2);
            Assert.AreEqual(2, popularTopics1.Data[0].TotalLikes);
            Assert.AreEqual(0, popularTopics1.Data[0].TotalComments);
            Assert.AreEqual(topicHandle1, popularTopics1.Data[0].TopicHandle);
            Assert.AreEqual(1, popularTopics1.Data[1].TotalLikes);
            Assert.AreEqual(0, popularTopics1.Data[1].TotalComments);
            Assert.AreEqual(topicHandle2, popularTopics1.Data[1].TopicHandle);

            FeedResponseTopicView popularTopics2 = popularTopicsOperationResponse2.Body;
            Assert.AreEqual(1, popularTopics2.Data.Count);
            Assert.AreEqual(2, popularTopics2.Data[0].TotalLikes);
            Assert.AreEqual(0, popularTopics2.Data[0].TotalComments);
            Assert.AreEqual(topicHandle1, popularTopics2.Data[0].TopicHandle);

            FeedResponseTopicView popularTopics3 = popularTopicsOperationResponse3.Body;
            Assert.AreEqual(1, popularTopics3.Data.Count);
            Assert.AreEqual(1, popularTopics3.Data[0].TotalLikes);
            Assert.AreEqual(0, popularTopics3.Data[0].TotalComments);
            Assert.AreEqual(topicHandle2, popularTopics3.Data[0].TopicHandle);

            FeedResponseTopicView popularTopics7 = popularTopicsOperationResponse7.Body;
            Assert.IsTrue(popularTopics7.Data.Count <= 2);
            Assert.AreEqual(1, popularTopics7.Data[0].TotalLikes);
            Assert.AreEqual(0, popularTopics7.Data[0].TotalComments);
            Assert.AreEqual(topicHandle2, popularTopics7.Data[0].TopicHandle);

            FeedResponseTopicView popularTopics8 = popularTopicsOperationResponse8.Body;
            Assert.IsTrue(popularTopics8.Data.Count <= 1);

            FeedResponseTopicView popularTopics9 = popularTopicsOperationResponse9.Body;
            Assert.AreEqual(0, popularTopics9.Data.Count);

            // Verify things for the other time ranges
            FeedResponseTopicView popularTopics_week = popularTopicsOperationResponse4.Body;
            Assert.IsTrue(popularTopics_week.Data.Count >= 2);
            Assert.AreEqual(2, popularTopics_week.Data[0].TotalLikes);
            Assert.AreEqual(0, popularTopics_week.Data[0].TotalComments);
            Assert.AreEqual(topicHandle1, popularTopics_week.Data[0].TopicHandle);
            Assert.AreEqual(1, popularTopics_week.Data[1].TotalLikes);
            Assert.AreEqual(0, popularTopics_week.Data[1].TotalComments);
            Assert.AreEqual(topicHandle2, popularTopics_week.Data[1].TopicHandle);

            FeedResponseTopicView popularTopics_month = popularTopicsOperationResponse5.Body;
            Assert.IsTrue(popularTopics_month.Data.Count >= 2);
            Assert.AreEqual(2, popularTopics_month.Data[0].TotalLikes);
            Assert.AreEqual(0, popularTopics_month.Data[0].TotalComments);
            Assert.AreEqual(topicHandle1, popularTopics_month.Data[0].TopicHandle);
            Assert.AreEqual(1, popularTopics_month.Data[1].TotalLikes);
            Assert.AreEqual(0, popularTopics_month.Data[1].TotalComments);
            Assert.AreEqual(topicHandle2, popularTopics_month.Data[1].TopicHandle);

            FeedResponseTopicView popularTopics_all = popularTopicsOperationResponse6.Body;
            Assert.IsTrue(popularTopics_all.Data.Count >= 2);
            Assert.AreEqual(2, popularTopics_all.Data[0].TotalLikes);
            Assert.AreEqual(0, popularTopics_all.Data[0].TotalComments);
            Assert.AreEqual(topicHandle1, popularTopics_all.Data[0].TopicHandle);
            Assert.AreEqual(1, popularTopics_all.Data[1].TotalLikes);
            Assert.AreEqual(0, popularTopics_all.Data[1].TotalComments);
            Assert.AreEqual(topicHandle2, popularTopics_all.Data[1].TopicHandle);
        }

        /// <summary>
        /// Create topics from many users and Gets them
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task GetTopicsFromManyUsersTest()
        {
            // create 3 users
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            string firstName = "Stan";
            string lastName = "TopicMan";
            string bio = string.Empty;
            PostUserResponse postUserResponse1 = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);

            string firstName2 = "NotPopularNelly";
            string lastName2 = "Von Jerk";
            string bio2 = "Nelly is not very popular. :(";
            PostUserResponse postUserResponse2 = await TestUtilities.DoLogin(client, firstName2, lastName2, bio2);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            string firstName3 = "J.J.";
            string lastName3 = "Abrams";
            string bio3 = "Something creative goes here";
            PostUserResponse postUserResponse3 = await TestUtilities.DoLogin(client, firstName3, lastName3, bio3);
            string auth3 = AuthHelper.CreateSocialPlusAuth(postUserResponse3.SessionToken);

            // create a topic1 from user 1
            string topicTitle = "My Favorite Topic";
            string topicText = "It is all about sports!";
            BlobType blobType = BlobType.Custom;
            string blobHandle = "http://myBlobHandle/";
            string language = string.Empty;
            string deepLink = string.Empty;
            string categories = string.Empty;
            string friendlyName = string.Empty;
            string group = string.Empty;
            PostTopicRequest postTopicRequest1 = new PostTopicRequest(publisherType: PublisherType.User, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse1 = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest1, authorization: auth1);

            // create a topic2 from user 1
            string topicTitle2 = "My Second Favorite Topic";
            string topicText2 = "It is all about food!";
            BlobType blobType2 = BlobType.Video;
            string blobHandle2 = "http://myFood/food.wma/";
            PostTopicRequest postTopicRequest2 = new PostTopicRequest(publisherType: PublisherType.User, text: topicText2, title: topicTitle2, blobType: blobType2, blobHandle: blobHandle2, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse2 = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest2, authorization: auth1);

            // create a topic3 from user 2
            string topicTitle3 = "User 2's fav topic";
            string topicText3 = "Nothing about food";
            BlobType blobType3 = BlobType.Unknown;
            string blobHandle3 = string.Empty;
            PostTopicRequest postTopicRequest3 = new PostTopicRequest(publisherType: PublisherType.User, text: topicText3, title: topicTitle3, blobType: blobType3, blobHandle: blobHandle3, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse3 = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest3, authorization: auth2);

            // create a topic4 from user 1
            string topicTitle4 = "User 2's second fav topic";
            string topicText4 = "Cars cars cars";
            BlobType blobType4 = BlobType.Unknown;
            string blobHandle4 = string.Empty;
            PostTopicRequest postTopicRequest4 = new PostTopicRequest(publisherType: PublisherType.User, text: topicText4, title: topicTitle4, blobType: blobType4, blobHandle: blobHandle4, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse4 = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest4, authorization: auth1);

            // create a topic5 from user 1
            string topicTitle5 = "User 2's second fav topic";
            string topicText5 = "Cars cars cars";
            BlobType blobType5 = BlobType.Custom;
            string blobHandle5 = "BlobHandleStringGoesHere";
            PostTopicRequest postTopicRequest5 = new PostTopicRequest(publisherType: PublisherType.User, text: topicText5, title: topicTitle5, blobType: blobType5, blobHandle: blobHandle5, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse5 = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest5, authorization: auth1);

            // get the all topics
            string cursor = null;
            int limit = 15;
            HttpOperationResponse<FeedResponseTopicView> getTopicsOperationResponse = await client.Topics.GetTopicsWithHttpMessagesAsync(cursor: cursor, limit: limit, authorization: auth1);

            // extract topic handles and delete topics
            string topicHandle1 = string.Empty;
            HttpOperationResponse<object> deleteTopicOperationResponse1 = null;
            if (postTopicOperationResponse1.Response.IsSuccessStatusCode)
            {
                topicHandle1 = postTopicOperationResponse1.Body.TopicHandle;
                deleteTopicOperationResponse1 = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle1, authorization: auth1);
            }

            string topicHandle2 = string.Empty;
            HttpOperationResponse<object> deleteTopicOperationResponse2 = null;
            if (postTopicOperationResponse2.Response.IsSuccessStatusCode)
            {
                topicHandle2 = postTopicOperationResponse2.Body.TopicHandle;
                deleteTopicOperationResponse2 = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth1);
            }

            string topicHandle3 = string.Empty;
            HttpOperationResponse<object> deleteTopicOperationResponse3 = null;
            if (postTopicOperationResponse3.Response.IsSuccessStatusCode)
            {
                topicHandle3 = postTopicOperationResponse3.Body.TopicHandle;
                deleteTopicOperationResponse3 = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle3, authorization: auth2);
            }

            string topicHandle4 = string.Empty;
            HttpOperationResponse<object> deleteTopicOperationResponse4 = null;
            if (postTopicOperationResponse4.Response.IsSuccessStatusCode)
            {
                topicHandle4 = postTopicOperationResponse4.Body.TopicHandle;
                deleteTopicOperationResponse4 = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle4, authorization: auth1);
            }

            string topicHandle5 = string.Empty;
            HttpOperationResponse<object> deleteTopicOperationResponse5 = null;
            if (postTopicOperationResponse5.Response.IsSuccessStatusCode)
            {
                topicHandle5 = postTopicOperationResponse5.Body.TopicHandle;
                deleteTopicOperationResponse5 = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle5, authorization: auth1);
            }

            // cleanup users
            HttpOperationResponse<object> deleteUserOperationResponse1 = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse<object> deleteUserOperationResponse2 = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);
            HttpOperationResponse<object> deleteUserOperationResponse3 = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth3);

            // check failure conditions
            Assert.IsTrue(postTopicOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(postTopicOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(postTopicOperationResponse3.Response.IsSuccessStatusCode);
            Assert.IsTrue(postTopicOperationResponse4.Response.IsSuccessStatusCode);
            Assert.IsTrue(postTopicOperationResponse5.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse3.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse4.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse5.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse3.Response.IsSuccessStatusCode);
            Assert.IsTrue(getTopicsOperationResponse.Response.IsSuccessStatusCode);

            // Verify - even if there are existing topics from old data, the new ones will be in these spots
            FeedResponseTopicView topicListResponse = getTopicsOperationResponse.Body;
            Assert.IsTrue(topicListResponse.Data.Count >= 5, string.Format("We only have {0} topics, when we should have at least 5 topics.", topicListResponse.Data.Count));
            Assert.AreEqual(topicListResponse.Data[0].BlobHandle, blobHandle5);
            Assert.AreEqual(topicListResponse.Data[0].BlobType, blobType5);
            Assert.AreEqual(topicListResponse.Data[0].Title, topicTitle5);
            Assert.AreEqual(topicListResponse.Data[0].Text, topicText5);

            Assert.AreEqual(topicListResponse.Data[1].BlobHandle, blobHandle4);
            Assert.AreEqual(topicListResponse.Data[1].BlobType, blobType4);
            Assert.AreEqual(topicListResponse.Data[1].Title, topicTitle4);
            Assert.AreEqual(topicListResponse.Data[1].Text, topicText4);

            Assert.AreEqual(topicListResponse.Data[2].BlobHandle, blobHandle3);
            Assert.AreEqual(topicListResponse.Data[2].BlobType, blobType3);
            Assert.AreEqual(topicListResponse.Data[2].Title, topicTitle3);
            Assert.AreEqual(topicListResponse.Data[2].Text, topicText3);

            Assert.AreEqual(topicListResponse.Data[3].BlobHandle, blobHandle2);
            Assert.AreEqual(topicListResponse.Data[3].BlobType, blobType2);
            Assert.AreEqual(topicListResponse.Data[3].Title, topicTitle2);
            Assert.AreEqual(topicListResponse.Data[3].Text, topicText2);

            Assert.AreEqual(topicListResponse.Data[4].BlobHandle, blobHandle);
            Assert.AreEqual(topicListResponse.Data[4].BlobType, blobType);
            Assert.AreEqual(topicListResponse.Data[4].Title, topicTitle);
            Assert.AreEqual(topicListResponse.Data[4].Text, topicText);
        }

        /// <summary>
        /// Test back-to-back create topics.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task LoadCreateDeleteTopicTest()
        {
            // number of topics
            int numTopics = 100;

            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user = await TestUtilities.PostGenericUser(client);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);

            // Disable client retries by setting the retry policy to a FixedInterval strategy with a retryCount of 0
            int retryCount = 0;
            var retryPolicy = new RetryPolicy<HttpStatusCodeErrorDetectionStrategy>(new FixedIntervalRetryStrategy(retryCount));
            client.SetRetryPolicy(retryPolicy);

            Task<PostTopicResponse>[] t = new Task<PostTopicResponse>[numTopics];
            PostTopicResponse[] responses = new PostTopicResponse[numTopics];

            for (int i = 0; i < numTopics; i += 1)
            {
                t[i] = TestUtilities.PostGenericTopic(client, auth);
            }

            for (int i = 0; i < numTopics; i += 1)
            {
                t[i].Wait();
                responses[i] = (PostTopicResponse)t[i].Result;

                Console.WriteLine("{0}: TopicHandle: {1} created.", i, responses[i].TopicHandle);
            }

            // Cleanup
            for (int i = 0; i < numTopics; i += 1)
            {
                // final clean up
                await client.Topics.DeleteTopicAsync(responses[i].TopicHandle, auth);
            }

            await client.Users.DeleteUserAsync(auth);
        }

        /// <summary>
        /// Test that topic handles are case sensitive.
        /// Step 1. Create topic
        /// Step 2. Change the case of the first letter of the topic handle
        /// Step 3. Get topic. (this should return not found)
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task CheckTopicHandlesCaseSensitivity()
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user = await TestUtilities.PostGenericUser(client);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);

            // Post a generic topic to get a topic handle
            var topicResponse = await TestUtilities.PostGenericTopic(client, auth);

            // Change the case of the first letter of the topic handle, and call get topic
            string topicHandleWithCaseChange = topicResponse.TopicHandle.ChangeCaseOfFirstLetter();
            var similarTopicResponse = await client.Topics.GetTopicWithHttpMessagesAsync(topicHandleWithCaseChange, auth);

            // Delete generic topic
            await client.Topics.DeleteTopicAsync(topicResponse.TopicHandle, auth);

            // Delete generic user
            await client.Users.DeleteUserAsync(auth);

            // Check that GetTopic returned NotFound
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, similarTopicResponse.Response.StatusCode);
        }
    }
}
