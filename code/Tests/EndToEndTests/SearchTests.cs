// <copyright file="SearchTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;

    /// <summary>
    /// Basic tests around search
    /// </summary>
    [TestClass]
    public class SearchTests
    {
        /// <summary>
        /// This method tests the search users feature. It creates three users, then performs
        /// the following searches:
        /// 1. Search for user 1's first name
        /// 2. Search for user 2's last name
        /// 3. Search for a common substring of user 2 and user 3's first names (should find both)
        /// 4. Search for a common substring of user 1's first and last names (should find user 1 once)
        /// It then deletes all three of the users. Finally, it performs one more search:
        /// 5. Search for user 1's first name (should be empty)
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task SearchUsersTest()
        {
            // trying to make it more robust by adding on extra so don't have chance of bad data affecting next test run
            string dateString = DateTime.UtcNow.ToFileTime().ToString();
            string searchWord = dateString.Substring(dateString.Length - 7);

            // Create users
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserResponse postUserResponse;
            string firstName = "FirstYouThere" + searchWord;
            string lastName = "YouThere" + searchWord;
            string bio = string.Empty;
            postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            PostUserResponse postUserResponse2;
            string firstName2 = "Larry" + searchWord;
            string lastName2 = "GoWhere" + searchWord;
            string bio2 = string.Empty;
            postUserResponse2 = await TestUtilities.DoLogin(client, firstName2, lastName2, bio2);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            PostUserResponse postUserResponse3;
            string firstName3 = "Larry" + searchWord;
            string lastName3 = "NotHere" + searchWord;
            string bio3 = string.Empty;
            postUserResponse3 = await TestUtilities.DoLogin(client, firstName3, lastName3, bio3);
            string auth3 = AuthHelper.CreateSocialPlusAuth(postUserResponse3.SessionToken);

            // Delay a bit to allow data to get into the search
            await Task.Delay(TestConstants.SearchDelay);

            // Search on first name
            HttpOperationResponse<FeedResponseUserCompactView> search1 = await client.Search.GetUsersWithHttpMessagesAsync(query: firstName, cursor: null, limit: 5, authorization: auth1);

            // Search on last name
            HttpOperationResponse<FeedResponseUserCompactView> search2 = await client.Search.GetUsersWithHttpMessagesAsync(query: lastName2, cursor: null, limit: 5, authorization: auth1);

            // Search on something that results more than one user
            HttpOperationResponse<FeedResponseUserCompactView> search3 = await client.Search.GetUsersWithHttpMessagesAsync(query: "Larry" + searchWord, cursor: null, limit: 3, authorization: auth1);

            // Search on one that hits in multiple fields in one entry
            HttpOperationResponse<FeedResponseUserCompactView> search4 = await client.Search.GetUsersWithHttpMessagesAsync(query: "YouThere" + searchWord, cursor: null, limit: 5, authorization: auth1);

            // Clean up first user
            HttpOperationResponse<object> deleteUser1 = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);

            // search on that first user shouldn't come back anything
            string anon = TestUtilities.GetAnonAuth();
            HttpOperationResponse<FeedResponseUserCompactView> search5 = await client.Search.GetUsersWithHttpMessagesAsync(query: firstName, cursor: null, limit: 10, authorization: anon);

            // Clean up second user
            HttpOperationResponse<object> deleteUser2 = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);

            // Clean up third user
            HttpOperationResponse<object> deleteUser3 = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth3);

            // *** Verify section - do this after deleting because left behind users affect results
            // Verify first search
            Assert.IsTrue(search1.Response.IsSuccessStatusCode);
            Assert.AreEqual(search1.Body.Data.Count, 1);
            Assert.AreEqual(search1.Body.Data[0].FirstName, firstName);
            Assert.AreEqual(search1.Body.Data[0].LastName, lastName);
            Assert.AreEqual(search1.Body.Data[0].UserHandle, postUserResponse.UserHandle);

            // Verify second search
            Assert.IsTrue(search2.Response.IsSuccessStatusCode);
            Assert.AreEqual(search2.Body.Data.Count, 1);
            Assert.AreEqual(search2.Body.Data[0].FirstName, firstName2);
            Assert.AreEqual(search2.Body.Data[0].LastName, lastName2);
            Assert.AreEqual(search2.Body.Data[0].UserHandle, postUserResponse2.UserHandle);

            // Verify third search
            Assert.IsTrue(search3.Response.IsSuccessStatusCode);
            List<UserCompactView> search3OrderedData = search3.Body.Data.OrderBy(x => x.LastName).ToList();
            Assert.AreEqual(search3OrderedData.Count, 2);
            Assert.AreEqual(search3OrderedData[0].FirstName, firstName2);
            Assert.AreEqual(search3OrderedData[0].LastName, lastName2);
            Assert.AreEqual(search3OrderedData[0].UserHandle, postUserResponse2.UserHandle);
            Assert.AreEqual(search3OrderedData[1].FirstName, firstName3);
            Assert.AreEqual(search3OrderedData[1].LastName, lastName3);
            Assert.AreEqual(search3OrderedData[1].UserHandle, postUserResponse3.UserHandle);

            // Verify fourth search
            Assert.IsTrue(search4.Response.IsSuccessStatusCode);
            Assert.AreEqual(search4.Body.Data.Count, 1);
            Assert.AreEqual(search4.Body.Data[0].FirstName, firstName);
            Assert.AreEqual(search4.Body.Data[0].LastName, lastName);
            Assert.AreEqual(search4.Body.Data[0].UserHandle, postUserResponse.UserHandle);

            // Verify fifth search - should be 0 since user was deleted
            Assert.IsTrue(search5.Response.IsSuccessStatusCode);
            Assert.AreEqual(search5.Body.Data.Count, 0);

            // Verify deletions
            Assert.IsTrue(deleteUser1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUser2.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUser3.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// This method tests the search topic feature. It creates one user and posts two topics
        /// with that user, with both topics including a common keyword in the text. It then
        /// performs the following searches:
        /// 1. Search on the keyword with a limit of 1 (should return the most recent topic only)
        /// 2. Search on the keyword with a limit of 1 using the cursor from the previous search
        ///     (should return the older topic only)
        /// 3. Search on the keyword with a limit of 2 (should return both topics)
        /// It then deletes both topics and the user. Finally, it performs one more search:
        /// 4. Search on the keyword (should be empty)
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task SearchTopicsTest()
        {
            // generate unique string
            string searchWord = "#" + Guid.NewGuid().ToString().Replace("-", string.Empty);

            // create user
            SocialPlusClient client1 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            PostUserResponse postUserResponse;
            string firstName = "FirstUser";
            string lastName = "FirstUserLastName";
            string bio = string.Empty;
            postUserResponse = await TestUtilities.DoLogin(client1, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            // First Topic
            string topicTitle = string.Empty;
            string topicText = searchWord;
            BlobType blobType = BlobType.Unknown;
            string blobHandle = string.Empty;
            string language = string.Empty;
            string deepLink = string.Empty;
            string categories = string.Empty;
            string friendlyName = string.Empty;
            string group = string.Empty;
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.User, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, language: language, deepLink: deepLink, categories: categories, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client1.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);

            // If the first post topic operation failed, clean up
            if (postTopicOperationResponse == null || postTopicOperationResponse.Body == null || string.IsNullOrWhiteSpace(postTopicOperationResponse.Body.TopicHandle))
            {
                await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to post first topic");
            }

            // Large delay to increase the difference in freshness between the two topics.
            // Search results are influenced by weight (not used right now), freshness, and relevance.
            // Azure Search's relevance score seems to vary a little bit even though both topics here
            // are exactly the same. By increasing the freshness difference, these small variations in
            // relevance score get washed out, and this test will pass deterministically.
            await Task.Delay(10 * TestConstants.SearchDelay);

            // create a second Topic
            string topicTitle2 = string.Empty;
            string topicText2 = searchWord;
            PostTopicRequest postTopicRequest2 = new PostTopicRequest(publisherType: PublisherType.User, text: topicText2, title: topicTitle2, blobType: blobType, blobHandle: blobHandle, language: language, deepLink: deepLink, categories: categories, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse2 = await client1.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest2, authorization: auth);

            // If the second post topic operation failed, clean up
            if (postTopicOperationResponse2 == null || postTopicOperationResponse2.Body == null || string.IsNullOrWhiteSpace(postTopicOperationResponse2.Body.TopicHandle))
            {
                await client1.Topics.DeleteTopicWithHttpMessagesAsync(postTopicOperationResponse.Body.TopicHandle, auth);
                await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to post second topic");
            }

            // Delay a bit to allow data to get into the search
            await Task.Delay(TestConstants.SearchDelay);

            // Only one result
            HttpOperationResponse<FeedResponseTopicView> search1 = await client1.Search.GetTopicsWithHttpMessagesAsync(query: searchWord, cursor: null, limit: 1, authorization: auth);

            // Now get the second one after that cursor
            HttpOperationResponse<FeedResponseTopicView> search2 = await client1.Search.GetTopicsWithHttpMessagesAsync(query: searchWord, cursor: int.Parse(search1.Body.Cursor), limit: 1, authorization: auth);

            // Now get all in one
            HttpOperationResponse<FeedResponseTopicView> search3 = await client1.Search.GetTopicsWithHttpMessagesAsync(query: searchWord, cursor: null, limit: 2, authorization: auth);

            // Delete topics and see if search works
            HttpOperationResponse<object> deleteTopic1 = await client1.Topics.DeleteTopicWithHttpMessagesAsync(postTopicOperationResponse.Body.TopicHandle, auth);
            HttpOperationResponse<object> deleteTopic2 = await client1.Topics.DeleteTopicWithHttpMessagesAsync(postTopicOperationResponse2.Body.TopicHandle, auth);

            // now search to see if works after deleted
            HttpOperationResponse<FeedResponseTopicView> search4 = await client1.Search.GetTopicsWithHttpMessagesAsync(query: searchWord, cursor: null, limit: 10, authorization: auth);

            // Clean up first user
            HttpOperationResponse<object> deleteUser1 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // Verify now - verify after all is cleaned up so any failures isn't leaving behind stuff to cause failure next time test is ran
            // Verify Search 1
            Assert.IsTrue(search1.Response.IsSuccessStatusCode);
            Assert.AreEqual(search1.Body.Data.Count, 1);
            Assert.AreEqual(search1.Body.Data[0].TopicHandle, postTopicOperationResponse2.Body.TopicHandle);
            Assert.AreEqual(search1.Body.Data[0].Title, topicTitle2);
            Assert.AreEqual(search1.Body.Data[0].Text, topicText2);
            Assert.AreEqual(search1.Body.Data[0].BlobType, blobType);
            Assert.AreEqual(search1.Body.Data[0].BlobHandle, blobHandle);
            Assert.AreEqual(search1.Body.Data[0].Language, language);
            Assert.AreEqual(search1.Body.Data[0].DeepLink, deepLink);
            Assert.AreEqual(search1.Body.Data[0].Categories, categories);
            Assert.AreEqual(search1.Body.Data[0].FriendlyName, friendlyName);
            Assert.AreEqual(search1.Body.Data[0].Group, group);

            // Verify Search 2
            Assert.IsTrue(search2.Response.IsSuccessStatusCode);
            Assert.AreEqual(search2.Body.Data.Count, 1);
            Assert.AreEqual(search2.Body.Data[0].TopicHandle, postTopicOperationResponse.Body.TopicHandle);
            Assert.AreEqual(search2.Body.Data[0].Title, topicTitle);
            Assert.AreEqual(search2.Body.Data[0].Text, topicText);
            Assert.AreEqual(search2.Body.Data[0].BlobType, blobType);
            Assert.AreEqual(search2.Body.Data[0].BlobHandle, blobHandle);
            Assert.AreEqual(search2.Body.Data[0].Language, language);
            Assert.AreEqual(search2.Body.Data[0].DeepLink, deepLink);
            Assert.AreEqual(search2.Body.Data[0].Categories, categories);
            Assert.AreEqual(search2.Body.Data[0].FriendlyName, friendlyName);
            Assert.AreEqual(search2.Body.Data[0].Group, group);

            // Verify Search 3
            Assert.IsTrue(search3.Response.IsSuccessStatusCode);
            Assert.AreEqual(search3.Body.Data.Count, 2);
            Assert.AreEqual(search3.Body.Data[0].TopicHandle, postTopicOperationResponse2.Body.TopicHandle);
            Assert.AreEqual(search3.Body.Data[0].Title, topicTitle2);
            Assert.AreEqual(search3.Body.Data[0].Text, topicText2);
            Assert.AreEqual(search3.Body.Data[0].BlobType, blobType);
            Assert.AreEqual(search3.Body.Data[0].BlobHandle, blobHandle);
            Assert.AreEqual(search3.Body.Data[0].Language, language);
            Assert.AreEqual(search3.Body.Data[0].DeepLink, deepLink);
            Assert.AreEqual(search3.Body.Data[0].Categories, categories);
            Assert.AreEqual(search3.Body.Data[0].FriendlyName, friendlyName);
            Assert.AreEqual(search3.Body.Data[0].Group, group);

            Assert.AreEqual(search3.Body.Data[1].TopicHandle, postTopicOperationResponse.Body.TopicHandle);
            Assert.AreEqual(search3.Body.Data[1].Title, topicTitle);
            Assert.AreEqual(search3.Body.Data[1].Text, topicText);
            Assert.AreEqual(search3.Body.Data[1].BlobType, blobType);
            Assert.AreEqual(search3.Body.Data[1].BlobHandle, blobHandle);
            Assert.AreEqual(search3.Body.Data[1].Language, language);
            Assert.AreEqual(search3.Body.Data[1].DeepLink, deepLink);
            Assert.AreEqual(search3.Body.Data[1].Categories, categories);
            Assert.AreEqual(search3.Body.Data[1].FriendlyName, friendlyName);
            Assert.AreEqual(search3.Body.Data[1].Group, group);

            // Verify Search 4
            Assert.IsTrue(search4.Response.IsSuccessStatusCode);
            Assert.AreEqual(search4.Body.Data.Count, 0);

            // Verify deletions
            Assert.IsTrue(deleteTopic1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopic2.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUser1.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// This method tests the search topic feature when a topic has been updated.
        /// It creates one user, posts one topic with that user, and then immediately
        /// updates that topic. It then submits two searches, one on the original topic
        /// text and one on the updated topic text. The search on the original topic text
        /// should not return any results, while the search on the updated topic text
        /// should return the topic. Finally, it deletes the topic and user.
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task SearchUpdatedTopicTest()
        {
            string dateString = DateTime.UtcNow.ToFileTime().ToString();
            string searchWord1 = "OriginalText" + dateString.Substring(dateString.Length - 7);
            string searchWord2 = "UpdatedText" + dateString.Substring(dateString.Length - 7);

            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserResponse postUserResponse;
            string firstName = "FirstUser";
            string lastName = "FirstUserLastName";
            string bio = string.Empty;
            postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            string topicTitle = "My Favorite Topic";
            string originalText = "Sports. #" + searchWord1;
            BlobType blobType = BlobType.Image;
            string blobHandle = "http://myBlobHandle/";
            string language = "en-US";
            string deepLink = "Sports!";
            string categories = "sports, ncurrency";
            string friendlyName = "Game On!";
            string group = "mygroup";
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.User, text: originalText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, language: language, deepLink: deepLink, categories: categories, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);

            // If the post topic operation failed, clean up
            if (postTopicOperationResponse == null || !postTopicOperationResponse.Response.IsSuccessStatusCode || postTopicOperationResponse.Body == null || string.IsNullOrWhiteSpace(postTopicOperationResponse.Body.TopicHandle))
            {
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to post topic");
            }

            string topicHandle = postTopicOperationResponse.Body.TopicHandle;

            string updatedText = "Movies. #" + searchWord2;
            PutTopicRequest putTopicRequest = new PutTopicRequest(text: updatedText, title: topicTitle, categories: categories);
            HttpOperationResponse putTopicOperationResponse = await client.Topics.PutTopicWithHttpMessagesAsync(topicHandle, request: putTopicRequest, authorization: auth);

            // If the put topic operation failed, clean up
            if (putTopicOperationResponse == null || !putTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to update topic");
            }

            // Delay a bit to allow data to get into the search
            await Task.Delay(TestConstants.SearchDelay);

            // Search on original text
            HttpOperationResponse<FeedResponseTopicView> search1 = await client.Search.GetTopicsWithHttpMessagesAsync(query: searchWord1, cursor: null, authorization: auth);

            // Search on updated text
            HttpOperationResponse<FeedResponseTopicView> search2 = await client.Search.GetTopicsWithHttpMessagesAsync(query: searchWord2, cursor: null, authorization: auth);

            // Clean up topic
            HttpOperationResponse<object> deleteTopic = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);

            // Clean up user
            HttpOperationResponse<object> deleteUser = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // Verify now - verify after cleanup so that failed asserts don't cause data to be left behind and interfere with future tests
            // Verify Search 1
            Assert.IsTrue(search1.Response.IsSuccessStatusCode);
            Assert.AreEqual(search1.Body.Data.Count, 0);

            // Verify Search 2
            Assert.IsTrue(search2.Response.IsSuccessStatusCode);
            Assert.AreEqual(search2.Body.Data.Count, 1);
            Assert.AreEqual(search2.Body.Data[0].TopicHandle, topicHandle);
            Assert.AreEqual(search2.Body.Data[0].Title, topicTitle);
            Assert.AreEqual(search2.Body.Data[0].Text, updatedText);
            Assert.AreEqual(search2.Body.Data[0].BlobType, blobType);
            Assert.AreEqual(search2.Body.Data[0].BlobHandle, blobHandle);
            Assert.AreEqual(search2.Body.Data[0].Language, language);
            Assert.AreEqual(search2.Body.Data[0].DeepLink, deepLink);
            Assert.AreEqual(search2.Body.Data[0].Categories, categories);
            Assert.AreEqual(search2.Body.Data[0].FriendlyName, friendlyName);
            Assert.AreEqual(search2.Body.Data[0].Group, group);

            // Verify deletions
            Assert.IsTrue(deleteTopic.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUser.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// This method tests the search topic feature when an empty query string is provided.
        /// It creates one user and posts one topic with that user, and then submits a search
        /// with an empty query string. The search should receive an HTTP 400 "bad request"
        /// response. It then deletes the topic and user.
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task SearchTopicsTestUsingEmptyString()
        {
            // ***********************************************
            // ** NOTE ** - Verification might change if design is set to not return anything vs return everything
            // ***********************************************
            SocialPlusClient client1 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserResponse postUserResponse;
            string firstName = "FirstUser";
            string lastName = "FirstUserLastName";
            string bio = string.Empty;
            postUserResponse = await TestUtilities.DoLogin(client1, firstName, lastName, bio);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            // First Topic
            string topicTitle = "My Favorite Topic";
            string topicText = "Sports. ";
            BlobType blobType = BlobType.Image;
            string blobHandle = "http://myBlobHandle/";
            string language = "en-US";
            string deepLink = "Sports!";
            string categories = "sports, ncurrency";
            string friendlyName = "Game On!";
            string group = "mygroup";
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.User, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, language: language, deepLink: deepLink, categories: categories, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client1.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);

            // If the post topic operation failed, clean up
            if (postTopicOperationResponse == null || postTopicOperationResponse.Body == null || string.IsNullOrWhiteSpace(postTopicOperationResponse.Body.TopicHandle))
            {
                await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Failed to post topic");
            }

            // Delay a bit to allow data to get into the search
            await Task.Delay(TestConstants.SearchDelay);

            // Only one result
            HttpOperationResponse<FeedResponseTopicView> search1 = await client1.Search.GetTopicsWithHttpMessagesAsync(query: string.Empty, cursor: null, limit: 5, authorization: auth);

            // Clean up topic
            HttpOperationResponse<object> deleteTopic1 = await client1.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: postTopicOperationResponse.Body.TopicHandle, authorization: auth);

            // Clean up first user
            HttpOperationResponse<object> deleteUser1 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // Verify now - get info
            Assert.AreEqual(search1.Response.StatusCode, HttpStatusCode.BadRequest);

            // Verify deletions
            Assert.IsTrue(deleteTopic1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUser1.Response.IsSuccessStatusCode);
        }
    }
}
