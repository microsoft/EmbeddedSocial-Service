// <copyright file="HashTagsTrendingTests.cs" company="Microsoft">
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
    public class HashTagsTrendingTests
    {
        /// <summary>
        /// Tests for Trending Hash Tags. It creates two users and then perform the following:
        /// 1- User 1 posts two topics with different hashtags (#food #NFL #sports) as well as a hashtag with GUID.
        /// 2- User 2 searches for the trending hashtags. It should return a list of strings of hashtags provided by user 1.
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task TrendingHashTagsTest()
        {
            // Create users
            SocialPlusClient client1 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            SocialPlusClient client2 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            string firstName1 = "Stan";
            string lastName1 = "TopicMan";
            string bio1 = string.Empty;
            PostUserResponse postUserResponse1 = await TestUtilities.DoLogin(client1, firstName1, lastName1, bio1);
            string bearerToken1 = "Bearer " + postUserResponse1.SessionToken;

            string firstName2 = "Emily";
            string lastName2 = "Johnson";
            string bio2 = string.Empty;
            PostUserResponse postUserResponse2 = await TestUtilities.DoLogin(client2, firstName2, lastName2, bio2);
            string bearerToken2 = "Bearer " + postUserResponse2.SessionToken;

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
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse1 = await client1.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest1, authorization: bearerToken1);

            // If the first post topic operation failed, clean up
            if (!postTopicOperationResponse1.Response.IsSuccessStatusCode)
            {
                await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: bearerToken1);
                await client2.Users.DeleteUserWithHttpMessagesAsync(authorization: bearerToken2);
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
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse2 = await client1.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest2, authorization: bearerToken2);

            // If the second post topic operation failed, clean up
            if (!postTopicOperationResponse2.Response.IsSuccessStatusCode)
            {
                // clean up user 1 and topic 1
                await client1.Topics.DeleteTopicWithHttpMessagesAsync(postTopicOperationResponse1.Body.TopicHandle, bearerToken1);
                await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: bearerToken1);

                // clean up user 2
                await client2.Users.DeleteUserWithHttpMessagesAsync(authorization: bearerToken2);
                Assert.Fail("Failed to post second topic");
            }

            await Task.Delay(TestConstants.SearchDelay);

            // Get the trending hash tags
            HttpOperationResponse<IList<string>> trendingResponse1 = await client2.Hashtags.GetTrendingHashtagsWithHttpMessagesAsync(authorization: bearerToken2);

            // Clean up topics
            await client1.Topics.DeleteTopicWithHttpMessagesAsync(postTopicOperationResponse1.Body.TopicHandle, bearerToken1);
            await client1.Topics.DeleteTopicWithHttpMessagesAsync(postTopicOperationResponse2.Body.TopicHandle, bearerToken2);

            // Clean up users
            HttpOperationResponse<object> deleteUser1 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: bearerToken1);
            HttpOperationResponse<object> deleteUser2 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: bearerToken2);

            // Validate the trending part - don't assume order or counts as "dirty" data could affect verification
            Assert.IsTrue(trendingResponse1.Response.IsSuccessStatusCode);
            Assert.IsNotNull(trendingResponse1, "Failed to get trending hashtags");
            Assert.IsNotNull(trendingResponse1.Body, "Failed to get trending hashtags");

            Assert.IsTrue(trendingResponse1.Body.Contains("#" + guidstring), "#" + guidstring + " was not found in the results");
            Assert.IsTrue(trendingResponse1.Body.Contains("#food"), "#food was not found in the results");
            Assert.IsTrue(trendingResponse1.Body.Contains("#sports"), "#sports was not found in the results");
            Assert.IsTrue(trendingResponse1.Body.Contains("#NFL"), "#NFL was not found in the results");
        }
    }
}
