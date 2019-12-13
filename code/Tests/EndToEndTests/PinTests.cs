// <copyright file="PinTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;

    /// <summary>
    /// All basic tests related to Pins
    /// </summary>
    [TestClass]
    public class PinTests
    {
        /// <summary>
        /// Create a topic.  Get the initial topic pin status, check that it is unpinned.  Pin it.  Check the pin status is true.
        /// Unpin it. Then check pin status is back to false.
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task PinUnPinDeletePinTest()
        {
            await this.PinUnPinDeletePinTestHelper(false, null);
        }

        /// <summary>
        /// Helper routine that performs the main actions of the test.
        /// Create a topic.  Get the initial topic pin status, check that it is unpinned.  Pin it.  Check the pin status is true.
        /// Unpin it. Then check pin status is back to false.
        /// </summary>
        /// <param name="appPublished">flag to indicate if topic is app published</param>
        /// <param name="appHandle">app handle</param>
        /// <returns>Fail if an exception is hit</returns>
        public async Task PinUnPinDeletePinTestHelper(bool appPublished, string appHandle)
        {
            // Set up initial login etc
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            string firstName = "J.J.";
            string lastName = "Z";
            string bio = string.Empty;
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);
            string userHandle = postUserResponse.UserHandle;

            if (appPublished)
            {
                // add user as admin
                bool added = ManageAppsUtils.AddAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                if (!added)
                {
                    // delete the user and fail the test
                    await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                    Assert.Fail("Failed to set user as administrator");
                }
            }

            string topicTitle = "Rarest coin";
            string topicText = "Egyptian coin";
            BlobType blobType = BlobType.Image;
            string blobHandle = "http://myBlobHandle/";
            string language = "en-US";
            string deepLink = "coins:abcdef";
            string categories = "photo, ncurrency";
            string friendlyName = "abcde";
            string group = "mygroup";
            string topicHandle = string.Empty;

            // step 1, create a topic
            var postTopicRequest = new PostTopicRequest() { Text = topicText, Title = topicTitle, BlobType = blobType, BlobHandle = blobHandle, Categories = categories, Language = language, DeepLink = deepLink, FriendlyName = friendlyName, Group = group };
            if (appPublished)
            {
                postTopicRequest.PublisherType = PublisherType.App;
            }
            else
            {
                postTopicRequest.PublisherType = PublisherType.User;
            }

            var postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);
            if (postTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
            }
            else
            {
                // cleanup: delete the user
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to post topic.");
            }

            bool? initialPinValue = null;
            bool? secondPinValue = null;
            bool? finalPinValue = null;

            // step 2, get the topic (and its pin status)
            var getTopicOperationResponse = await client.Topics.GetTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (getTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                initialPinValue = getTopicOperationResponse.Body.Pinned;
            }
            else
            {
                // cleanup: delete the topic and the user
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to get topic.");
            }

            // step 3, pin topic
            PostPinRequest postPinRequest = new PostPinRequest() { TopicHandle = topicHandle };
            var postPinOperationResponse = await client.MyPins.PostPinWithHttpMessagesAsync(request: postPinRequest, authorization: auth);
            if (!postPinOperationResponse.Response.IsSuccessStatusCode)
            {
                // cleanup: delete the topic and the user
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to pin topic.");
            }

            // step 4, get topic and its pin status again
            getTopicOperationResponse = await client.Topics.GetTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (getTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                secondPinValue = getTopicOperationResponse.Body.Pinned;
            }
            else
            {
                // cleanup: delete the topic and the user
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to get topic.");
            }

            // step 5, Delete Pin
            var deletePinOperationResponse = await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (!deletePinOperationResponse.Response.IsSuccessStatusCode)
            {
                // cleanup: delete the topic and the user
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to delete pin.");
            }

            // step 6, get topic yet again, and check pin status to see that it is now back to false
            getTopicOperationResponse = await client.Topics.GetTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (getTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                finalPinValue = getTopicOperationResponse.Body.Pinned;
            }
            else
            {
                // cleanup: delete the topic and the user
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to get pin.");
            }

            // cleanup: delete the topic and the user
            await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            bool? adminDeleted = null;
            if (appPublished)
            {
                adminDeleted = ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
            }

            await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            if (appPublished)
            {
                Assert.IsTrue(adminDeleted.HasValue);
                Assert.IsTrue(adminDeleted.Value);
            }

            Assert.IsTrue(initialPinValue.HasValue);
            Assert.IsFalse(initialPinValue.Value);
            Assert.IsTrue(secondPinValue.HasValue);
            Assert.IsTrue(secondPinValue.Value);
            Assert.IsTrue(finalPinValue.HasValue);
            Assert.IsFalse(finalPinValue.Value);
        }

        /// <summary>
        /// Create multiple topics.  Pin them.  Then get the pin feed, and check that all the pinned topics show up.
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task GetPinFeedTest()
        {
            await this.GetPinFeedTestHelper(false, null);
        }

        /// <summary>
        /// Helper routine that performs the main actions of the test.
        /// Create multiple topics.  Pin them.  Then get the pin feed, and check that all the pinned topics show up.
        /// </summary>
        /// <param name="appPublished">flag to indicate if topic is app published</param>
        /// <param name="appHandle">app handle</param>
        /// <returns>Fail if an exception is hit</returns>
        public async Task GetPinFeedTestHelper(bool appPublished, string appHandle)
        {
            // Set up initial login etc
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            string firstName = "J.J.";
            string lastName = "Z";
            string bio = string.Empty;
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);
            string userHandle = postUserResponse.UserHandle;

            if (appPublished)
            {
                // add user as admin
                bool added = ManageAppsUtils.AddAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                if (!added)
                {
                    // delete the user and fail the test
                    await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                    Assert.Fail("Failed to set user as administrator");
                }
            }

            // create topic #1
            string topicTitle = "topic number 1";
            string topicText = "the jukebox needs to take a leak";
            string language = "en-US";
            string deepLink = "http://dummy/";
            string categories = "cat1, cat6";
            string group = "mygroup";

            // step 1, create a topic and pin it
            string topicHandle = string.Empty;
            var postTopicRequest = new PostTopicRequest() { Text = topicText, Title = topicTitle, Categories = categories, Language = language, DeepLink = deepLink, Group = group };
            if (appPublished)
            {
                postTopicRequest.PublisherType = PublisherType.App;
            }
            else
            {
                postTopicRequest.PublisherType = PublisherType.User;
            }

            var postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);
            if (postTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
            }
            else
            {
                // cleanup: delete the user
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to post topic.");
            }

            var pinRequest = new PostPinRequest() { TopicHandle = topicHandle };
            var postPinOperationResponse = await client.MyPins.PostPinWithHttpMessagesAsync(authorization: auth, request: pinRequest);
            if (!postPinOperationResponse.Response.IsSuccessStatusCode)
            {
                // cleanup: delete topic #1 and delete the user
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to pin topic #1.");
            }

            // create topic #2 and pin it
            topicTitle = "topic number 2";
            topicText = "the piano has been drinking";
            language = "en-US";
            deepLink = "http://dummy/";
            categories = "cat1, cat6";
            group = "mygroup";

            string topicHandle2 = string.Empty;
            PostTopicRequest postTopicRequest2 = new PostTopicRequest() { Text = topicText, Title = topicTitle, Categories = categories, Language = language, DeepLink = deepLink, Group = group };
            if (appPublished)
            {
                postTopicRequest2.PublisherType = PublisherType.App;
            }
            else
            {
                postTopicRequest2.PublisherType = PublisherType.User;
            }

            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse2 = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest2, authorization: auth);
            if (postTopicOperationResponse2.Response.IsSuccessStatusCode)
            {
                topicHandle2 = postTopicOperationResponse2.Body.TopicHandle;
            }
            else
            {
                // cleanup: delete topic #1 and delete the user
                await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to post topic #2.");
            }

            pinRequest = new PostPinRequest() { TopicHandle = topicHandle2 };
            postPinOperationResponse = await client.MyPins.PostPinWithHttpMessagesAsync(authorization: auth, request: pinRequest);
            if (!postPinOperationResponse.Response.IsSuccessStatusCode)
            {
                // cleanup: delete topic #1, topic #2, and delete the user
                await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth);
                await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to pin topic #2.");
            }

            // create topic #3
            topicTitle = "topic number 3";
            topicText = "the carpet needs a haircut";
            language = "en-US";
            deepLink = "http://dummy/";
            categories = "cat1, cat6";
            group = "mygroup";

            string topicHandle3 = string.Empty;
            PostTopicRequest postTopicRequest3 = new PostTopicRequest() { Text = topicText, Title = topicTitle, Categories = categories, Language = language, DeepLink = deepLink, Group = group };
            if (appPublished)
            {
                postTopicRequest3.PublisherType = PublisherType.App;
            }
            else
            {
                postTopicRequest3.PublisherType = PublisherType.User;
            }

            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse3 = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest3, authorization: auth);
            if (postTopicOperationResponse3.Response.IsSuccessStatusCode)
            {
                topicHandle3 = postTopicOperationResponse3.Body.TopicHandle;
            }
            else
            {
                // cleanup: delete topic #1, topic #2, and delete the user
                await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth);
                await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to post topic #3.");
            }

            pinRequest = new PostPinRequest() { TopicHandle = topicHandle3 };
            postPinOperationResponse = await client.MyPins.PostPinWithHttpMessagesAsync(authorization: auth, request: pinRequest);
            if (!postPinOperationResponse.Response.IsSuccessStatusCode)
            {
                // cleanup: delete topic #1, topic #2, topic #3 and delete the user
                await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle3, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle3, authorization: auth);
                await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth);
                await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to pin topic #3.");
            }

            var pinFeedOperationResponse = await client.MyPins.GetPinsWithHttpMessagesAsync(authorization: auth);
            IList<TopicView> pinFeedResponse = null;
            if (pinFeedOperationResponse.Response.IsSuccessStatusCode)
            {
                pinFeedResponse = pinFeedOperationResponse.Body.Data;
            }
            else
            {
                // cleanup: delete topic #1, topic #2, topic #3 and delete the user
                await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle3, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle3, authorization: auth);
                await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth);
                await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to get the pin feed.");
            }

            // after getting the pin feed, clean up
            await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle3, authorization: auth);
            await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle3, authorization: auth);
            await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth);
            await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle2, authorization: auth);
            await client.MyPins.DeletePinWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            bool? adminDeleted = null;
            if (appPublished)
            {
                adminDeleted = ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
            }

            await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            if (appPublished)
            {
                Assert.IsTrue(adminDeleted.HasValue);
                Assert.IsTrue(adminDeleted.Value);
            }

            // after clean up, check the content of the pin feed
            Assert.AreEqual(pinFeedResponse.Count, 3);
            Assert.AreEqual(pinFeedResponse[0].Title, "topic number 3");
            Assert.AreEqual(pinFeedResponse[1].Title, "topic number 2");
            Assert.AreEqual(pinFeedResponse[2].Title, "topic number 1");
        }
    }
}
