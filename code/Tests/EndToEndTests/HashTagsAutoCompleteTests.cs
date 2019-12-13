// <copyright file="HashTagsAutoCompleteTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;

    /// <summary>
    /// Tests related to the Hash Tags section of Rest API services.
    /// </summary>
    [TestClass]
    public class HashTagsAutoCompleteTests
    {
        /// <summary>
        /// Tests for Autocomplete Hash Tags. It creates two users and then perform the following:
        /// 1- User 1 posts two topics with different hashtags (#food #NFL #sports).
        /// 2- The test uses GUID for another hashtag in topic 2.
        /// 3- User 2 searches for the hashtag GUID, the autocomplete feature should return a list of two strings GUID and GUIDGUID.
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task HashTagsAutoCompleteTest()
        {
            // Create users
            SocialPlusClient client1 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            SocialPlusClient client2 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            string firstName1 = "Stan";
            string lastName1 = "TopicMan";
            string bio1 = string.Empty;
            PostUserResponse postUserResponse1 = await TestUtilities.DoLogin(client1, firstName1, lastName1, bio1);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);

            string firstName2 = "Emily";
            string lastName2 = "Johnson";
            string bio2 = string.Empty;
            PostUserResponse postUserResponse2 = await TestUtilities.DoLogin(client2, firstName2, lastName2, bio2);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // First Topic by first user
            string topicTitle1 = "My Favorite Topic";
            string topicText1 = "It is all about sports! #sports #NFL";
            BlobType blobType1 = BlobType.Image;
            string blobHandle1 = "http://myBlobHandle/";
            string language1 = "en-US";
            string deepLink1 = "Sports!";
            string categories1 = "sports, ncurrency";
            string friendlyName1 = "Game On!";
            string group1 = "mygroup";
            PostTopicRequest postTopicRequest1 = new PostTopicRequest(
                publisherType: PublisherType.User,
                text: topicText1,
                title: topicTitle1,
                blobType: blobType1,
                blobHandle: blobHandle1,
                language: language1,
                deepLink: deepLink1,
                categories: categories1,
                friendlyName: friendlyName1,
                group: group1);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse1 = await client1.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest1, authorization: auth1);

            // If the first post topic operation failed, clean up
            if (!postTopicOperationResponse1.Response.IsSuccessStatusCode)
            {
                await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
                await client2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);
                Assert.Fail("Failed to post first topic");
            }

            // Delay to ensure that the topics are ordered correctly
            await Task.Delay(TestConstants.SearchDelay);

            // Second Topic by first user
            // Create a GUID for the word that we will test autocomplete against.
            // Reduce the size of the guid to fit into the query. Max size is 25.
            string guidstring = Guid.NewGuid().ToString().Substring(0, 24);
            string topicTitle2 = "My Second Favorite Topic";
            string topicText2 = "It is all about food! #food" + " #" + guidstring + " #" + guidstring + guidstring;
            BlobType blobType2 = BlobType.Custom;
            string blobHandle2 = "http://myBlobHandle/";
            string language2 = "en-US";
            string deepLink2 = "Food!";
            string categories2 = guidstring + " " + guidstring;
            string friendlyName2 = "Eat!";
            string group2 = "mygroup";
            PostTopicRequest postTopicRequest2 = new PostTopicRequest(
                publisherType: PublisherType.User,
                text: topicText2,
                title: topicTitle2,
                blobType: blobType2,
                blobHandle: blobHandle2,
                language: language2,
                deepLink: deepLink2,
                categories: categories2,
                friendlyName: friendlyName2,
                group: group2);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse2 = await client1.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest2, authorization: auth2);

            // If the second post topic operation failed, clean up
            if (!postTopicOperationResponse2.Response.IsSuccessStatusCode)
            {
                // clean up user 1 and topic 1
                await client1.Topics.DeleteTopicWithHttpMessagesAsync(postTopicOperationResponse1.Body.TopicHandle, auth1);
                await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);

                // clean up user 2
                await client2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);
                Assert.Fail("Failed to post second topic");
            }

            await Task.Delay(TestConstants.SearchDelay);

            // Get Autocomplete
            HttpOperationResponse<IList<string>> autocompleteResponse1 = await client2.Hashtags.GetAutocompletedHashtagsWithHttpMessagesAsync(authorization: auth2, query: "#" + guidstring);

            // Clean up topics
            await client1.Topics.DeleteTopicWithHttpMessagesAsync(postTopicOperationResponse1.Body.TopicHandle, auth1);
            await client1.Topics.DeleteTopicWithHttpMessagesAsync(postTopicOperationResponse2.Body.TopicHandle, auth2);

            // Clean up users
            HttpOperationResponse<object> deleteUser1 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse<object> deleteUser2 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);

            // Validate the Autocomplete part
            Assert.IsTrue(autocompleteResponse1.Response.IsSuccessStatusCode);
            Assert.IsNotNull(autocompleteResponse1, "Failed to get autocomplete hashtags");
            Assert.IsNotNull(autocompleteResponse1.Body, "Failed to get autocomplete hashtags");

            Assert.AreEqual(2, autocompleteResponse1.Body.Count);
            Assert.AreEqual("#" + guidstring, autocompleteResponse1.Body[0].ToString());
            Assert.AreEqual("#" + guidstring + guidstring, autocompleteResponse1.Body[1].ToString());
        }
    }
}
