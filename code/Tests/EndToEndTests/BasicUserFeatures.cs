// <copyright file="BasicUserFeatures.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;
    using SocialPlus.Utils;

    /// <summary>
    /// All basic user related tests.
    /// </summary>
    [TestClass]
    public class BasicUserFeatures
    {
        /// <summary>
        /// handle generator
        /// </summary>
        private static readonly HandleGenerator HandleGenerator = new HandleGenerator();

        /// <summary>
        /// Creates and deletes user
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task CreateDeleteUserTest()
        {
            // Set up initial stuff
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, "Joseph", "Johnson", "Some Bio");
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);
            await client.Users.DeleteUserAsync(auth);

            // Test that PostUser returns a non-null and non-empty user handle and session token
            Assert.IsFalse(string.IsNullOrEmpty(postUserResponse.UserHandle));
            Assert.IsFalse(string.IsNullOrEmpty(postUserResponse.SessionToken));
        }

        /// <summary>
        /// Creates several users in a row and then deletes then
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task CreateMultipleDeleteMultipleUsersTest()
        {
            // Setup three users
            SocialPlusClient client1 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            SocialPlusClient client2 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            SocialPlusClient client3 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user1 = await TestUtilities.PostGenericUser(client1);
            var user2 = await TestUtilities.PostGenericUser(client2);
            var user3 = await TestUtilities.PostGenericUser(client3);
            var auth1 = AuthHelper.CreateSocialPlusAuth(user1.SessionToken);
            var auth2 = AuthHelper.CreateSocialPlusAuth(user2.SessionToken);
            var auth3 = AuthHelper.CreateSocialPlusAuth(user3.SessionToken);

            // Delete Users
            await client1.Users.DeleteUserAsync(auth1);
            await client2.Users.DeleteUserAsync(auth2);
            await client3.Users.DeleteUserAsync(auth3);

            // Test that PostUser returns a non-null and non-empty user handle and session token
            Assert.IsFalse(string.IsNullOrEmpty(user1.UserHandle));
            Assert.IsFalse(string.IsNullOrEmpty(user1.SessionToken));
            Assert.IsFalse(string.IsNullOrEmpty(user2.UserHandle));
            Assert.IsFalse(string.IsNullOrEmpty(user2.SessionToken));
            Assert.IsFalse(string.IsNullOrEmpty(user3.UserHandle));
            Assert.IsFalse(string.IsNullOrEmpty(user3.SessionToken));

            await Task.Delay(TestConstants.SearchDelay);

            // User Search to verify user was deleted
            string anon = TestUtilities.GetAnonAuth();
            var search = await client1.Search.GetUsersAsync(query: "XX", authorization: anon, cursor: null, limit: 2);
            Assert.AreEqual(search.Data.Count, 0);
            var search1 = await client1.Search.GetUsersAsync(query: "JGG", authorization: anon, cursor: null, limit: 2);
            Assert.AreEqual(search.Data.Count, 0);
            var search2 = await client1.Search.GetUsersAsync(query: "AAA", authorization: anon, cursor: null, limit: 2);
            Assert.AreEqual(search.Data.Count, 0);
        }

        /// <summary>
        /// Creates a simple user and gets the info
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task GetUserTest()
        {
            // Set up initial stuff
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, "Fred", "Flintstone", "Rocking bedrock ...");
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            // Call Get User
            UserProfileView getUserProfile = await client.Users.GetMyProfileAsync(auth);

            // Clean up first before verifying
            await client.Users.DeleteUserAsync(auth);

            Assert.AreEqual("Rocking bedrock ...", getUserProfile.Bio);
            Assert.AreEqual("Fred", getUserProfile.FirstName);
            Assert.AreEqual("Flintstone", getUserProfile.LastName);
            Assert.AreEqual(FollowerStatus.None, getUserProfile.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, getUserProfile.FollowingStatus);
            Assert.AreEqual(null, getUserProfile.PhotoHandle);
            Assert.AreEqual(null, getUserProfile.PhotoUrl);
            Assert.AreEqual(ProfileStatus.Active, getUserProfile.ProfileStatus);
            Assert.AreEqual(0, getUserProfile.TotalFollowers);
            Assert.AreEqual(0, getUserProfile.TotalFollowing);
            Assert.AreEqual(0, getUserProfile.TotalTopics);
            Assert.AreEqual(postUserResponse.UserHandle, getUserProfile.UserHandle);
            Assert.AreEqual(Visibility.Public, getUserProfile.Visibility);
        }

        /// <summary>
        /// Calls get user with a null user handle and checks that the call returns not found
        /// </summary>
        /// <remarks>
        /// Note: Do NOT include this test in the PassingTests list in Program.cs.
        /// This test is currently broken, because the AutoRest generated client checks for a null
        /// user handle on the client side and generates a ValidationException, rather than calling
        /// the server with a null user handle.  To work around this, we would need to bypass the AutoRest
        /// generated code.
        /// </remarks>
        /// <returns>Get null user task</returns>
        [TestMethod]
        public async Task GetNullUserTest()
        {
            // create a client
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // create a user
            PostUserResponse postUserResponse = await TestUtilities.PostGenericUser(client);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            // call get user with null user handle argument
            var getUserResponse = await client.Users.GetUserWithHttpMessagesAsync(null, auth);

            // clean up: delete the user
            await TestUtilities.DeleteUser(client, auth);

            // check that get user returns not found
            Assert.AreEqual(HttpStatusCode.NotFound, getUserResponse.Response.StatusCode);
        }

        /// <summary>
        /// Calls get user with anonymous auth and checks that the call returns successfully (for a public user)
        /// </summary>
        /// <returns>Anonymous get user task</returns>
        [TestMethod]
        public async Task AnonGetUserTest()
        {
            // create a client
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // create a user
            PostUserResponse postUserResponse = await TestUtilities.PostGenericUser(client);
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);
            string createdUser = postUserResponse.UserHandle;

            // fetch the profile of the user we just created using anonymous auth
            string anonAuth = TestUtilities.GetAnonAuth();
            var getUserResponse = await client.Users.GetUserWithHttpMessagesAsync(createdUser, anonAuth);

            // clean up: delete the user
            await TestUtilities.DeleteUser(client, auth);

            // check that user handle returned by post user matched that returned by get user
            Assert.AreEqual(getUserResponse.Response.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(getUserResponse.Body.UserHandle, createdUser);
        }

        /// <summary>
        /// Creates a couple users and get the info specific to one of them
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task GetUserTestUsingHandle()
        {
            // Set up initial stuff
            SocialPlusClient client1 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            SocialPlusClient client2 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            PostUserResponse postUserResponse1 = await TestUtilities.DoLogin(client1, "Fred", "Flintstone", "Rocking bedrock ...");
            PostUserResponse postUserResponse2 = await TestUtilities.DoLogin(client2, "Barney", "Rubble", "Being Fred's sidekick");
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // Call Get User for first one
            UserProfileView getUserProfile1 = await client1.Users.GetUserAsync(postUserResponse1.UserHandle, auth1);

            // Call Get User for Second one
            UserProfileView getUserProfile2 = await client1.Users.GetUserAsync(postUserResponse2.UserHandle, auth1);

            // Clean up first before verifying
            await client1.Users.DeleteUserAsync(auth1);
            await client2.Users.DeleteUserAsync(auth2);

            // Verify first one
            Assert.AreEqual("Rocking bedrock ...", getUserProfile1.Bio);
            Assert.AreEqual("Fred", getUserProfile1.FirstName);
            Assert.AreEqual("None", getUserProfile1.FollowerStatus.ToString());
            Assert.AreEqual("None", getUserProfile1.FollowingStatus.ToString());
            Assert.AreEqual("Flintstone", getUserProfile1.LastName);
            Assert.AreEqual(null, getUserProfile1.PhotoHandle);
            Assert.AreEqual(null, getUserProfile1.PhotoUrl);
            Assert.AreEqual(ProfileStatus.Active, getUserProfile1.ProfileStatus);
            Assert.AreEqual(0, getUserProfile1.TotalFollowers);
            Assert.AreEqual(0, getUserProfile1.TotalFollowing);
            Assert.AreEqual(0, getUserProfile1.TotalTopics);
            Assert.AreEqual(postUserResponse1.UserHandle, getUserProfile1.UserHandle);
            Assert.AreEqual(Visibility.Public, getUserProfile1.Visibility);

            // Verify second one
            Assert.AreEqual("Being Fred's sidekick", getUserProfile2.Bio);
            Assert.AreEqual("Barney", getUserProfile2.FirstName);
            Assert.AreEqual(FollowerStatus.None, getUserProfile2.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, getUserProfile2.FollowingStatus);
            Assert.AreEqual("Rubble", getUserProfile2.LastName);
            Assert.AreEqual(null, getUserProfile2.PhotoHandle);
            Assert.AreEqual(null, getUserProfile1.PhotoUrl);
            Assert.AreEqual(ProfileStatus.Active, getUserProfile2.ProfileStatus);
            Assert.AreEqual(0, getUserProfile2.TotalFollowers);
            Assert.AreEqual(0, getUserProfile2.TotalFollowing);
            Assert.AreEqual(0, getUserProfile2.TotalTopics);
            Assert.AreEqual(postUserResponse2.UserHandle, getUserProfile2.UserHandle);
            Assert.AreEqual(Visibility.Public, getUserProfile2.Visibility);
        }

        /// <summary>
        /// Creates a simple user and puts the info
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task PutUserTest()
        {
            // Set up initial stuff
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, "ü", "§", "╚");
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            PutUserInfoRequest putUserInfoRequest = new PutUserInfoRequest(firstName: "Wilman", lastName: "Flinstone", bio: "Changed it up!");
            await client.Users.PutUserInfoAsync(putUserInfoRequest, auth);

            // Call Get User
            UserProfileView getUserProfile = await client.Users.GetUserAsync(postUserResponse.UserHandle, auth);

            // Clean up first before verifying
            await client.Users.DeleteUserAsync(auth);

            // Verify changes ... also verify rest to make sure nothing else wiped out
            Assert.AreEqual("Changed it up!", getUserProfile.Bio);
            Assert.AreEqual("Wilman", getUserProfile.FirstName);
            Assert.AreEqual("Flinstone", getUserProfile.LastName);
            Assert.AreEqual(FollowerStatus.None, getUserProfile.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, getUserProfile.FollowingStatus);
            Assert.AreEqual(null, getUserProfile.PhotoHandle);
            Assert.AreEqual(null, getUserProfile.PhotoUrl);
            Assert.AreEqual(ProfileStatus.Active, getUserProfile.ProfileStatus);
            Assert.AreEqual(0, getUserProfile.TotalFollowers);
            Assert.AreEqual(0, getUserProfile.TotalFollowing);
            Assert.AreEqual(0, getUserProfile.TotalTopics);
            Assert.AreEqual(postUserResponse.UserHandle, getUserProfile.UserHandle);
            Assert.AreEqual(Visibility.Public, getUserProfile.Visibility);
        }

        /// <summary>
        /// Creates a simple user and updates the photo info
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task UpdateUserPhotoTest()
        {
            // Set up initial stuff
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, "ü", "§", "╚");
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            // Call Put User
            string photoURL = "myPics.org//Selfie.jpg";
            PutUserPhotoRequest putUserPhotoRequest = new PutUserPhotoRequest(photoURL);
            await client.Users.PutUserPhotoAsync(putUserPhotoRequest, auth);

            // Call Get User
            UserProfileView getUserProfile = await client.Users.GetUserAsync(postUserResponse.UserHandle, auth);

            // Clean up first before verifying
            await client.Users.DeleteUserAsync(auth);

            // Verify changes ... also verify rest to make sure nothing else wiped out
            Assert.AreEqual("╚", getUserProfile.Bio);
            Assert.AreEqual("ü", getUserProfile.FirstName);
            Assert.AreEqual("§", getUserProfile.LastName);
            Assert.AreEqual(FollowerStatus.None, getUserProfile.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, getUserProfile.FollowingStatus);
            Assert.AreEqual(photoURL, getUserProfile.PhotoHandle);
            if (getUserProfile.PhotoUrl.Contains("images/" + photoURL) == false)
            {
                Assert.Fail("'images'" + photoURL + " should be contained in this: " + getUserProfile.PhotoUrl);
            }

            Assert.AreEqual(ProfileStatus.Active, getUserProfile.ProfileStatus);
            Assert.AreEqual(0, getUserProfile.TotalFollowers);
            Assert.AreEqual(0, getUserProfile.TotalFollowing);
            Assert.AreEqual(0, getUserProfile.TotalTopics);
            Assert.AreEqual(postUserResponse.UserHandle, getUserProfile.UserHandle);
            Assert.AreEqual(Visibility.Public, getUserProfile.Visibility);
        }

        /// <summary>
        /// Creates a simple user and updates the visibility info
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task UpdateUserVisibilityTest()
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user = await TestUtilities.PostGenericUser(client);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);

            // Call Put User
            PutUserVisibilityRequest putUserVisibilityRequest = new PutUserVisibilityRequest(Visibility.Private);
            await client.Users.PutUserVisibilityAsync(putUserVisibilityRequest, auth);

            // Call Get User
            UserProfileView getUserProfile = await client.Users.GetUserAsync(user.UserHandle, auth);

            // Clean up first before verifying
            await client.Users.DeleteUserAsync(auth);

            // Verify changes ... also verify rest to make sure nothing else wiped out
            Assert.AreEqual(FollowerStatus.None, getUserProfile.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, getUserProfile.FollowingStatus);
            Assert.AreEqual(null, getUserProfile.PhotoHandle);
            Assert.AreEqual(null, getUserProfile.PhotoUrl);
            Assert.AreEqual(ProfileStatus.Active, getUserProfile.ProfileStatus);
            Assert.AreEqual(0, getUserProfile.TotalFollowers);
            Assert.AreEqual(0, getUserProfile.TotalFollowing);
            Assert.AreEqual(0, getUserProfile.TotalTopics);
            Assert.AreEqual(user.UserHandle, getUserProfile.UserHandle);
            Assert.AreEqual(Visibility.Private, getUserProfile.Visibility);
        }

        /// <summary>
        /// Creates a simple user and gets the identity provider
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task GetLinkAccountsTest()
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user = await TestUtilities.PostGenericUser(client);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);

            // Call Get User
            IList<LinkedAccountView> getLinkedAccounts = await client.MyLinkedAccounts.GetLinkedAccountsAsync(auth);

            // Clean up
            await client.Users.DeleteUserAsync(auth);

            // Verify changes ... also verify rest to make sure nothing else wiped out
            Assert.AreEqual(2, getLinkedAccounts.Count);
            Assert.AreEqual(IdentityProvider.AADS2S, getLinkedAccounts[0].IdentityProvider);
        }

        /// <summary>
        /// creates topics for a user and gets them.
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task GetTopicsForUserTest()
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user = await TestUtilities.PostGenericUser(client);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);

            var httpResponse1 = await TestUtilities.PostGenericTopic(client, auth);
            string topicHandle1 = httpResponse1.TopicHandle;

            var httpResponse2 = await TestUtilities.PostGenericTopic(client, auth);
            string topicHandle2 = httpResponse2.TopicHandle;

            // get the topics for this user
            FeedResponseTopicView topicListResponse = await client.MyTopics.GetTopicsAsync(auth, cursor: null, limit: 2);

            // Delete Topics
            await client.Topics.DeleteTopicAsync(topicHandle1, auth);
            await client.Topics.DeleteTopicAsync(topicHandle2, auth);

            // Delete user
            await TestUtilities.DeleteUser(client, auth);

            Assert.AreEqual(2, topicListResponse.Data.Count);
        }

        /// <summary>
        /// creates topics for a user and gets them.
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task GetTopicsForUserUsingHandleTest()
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user = await TestUtilities.PostGenericUser(client);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);

            var httpResponse1 = await TestUtilities.PostGenericTopic(client, auth);
            string topicHandle1 = httpResponse1.TopicHandle;

            var httpResponse2 = await TestUtilities.PostGenericTopic(client, auth);
            string topicHandle2 = httpResponse2.TopicHandle;

            // get the topics for this user
            FeedResponseTopicView topicListResponse = await client.UserTopics.GetTopicsAsync(user.UserHandle, auth, cursor: null, limit: 2);

            // Delete Topics
            await client.Topics.DeleteTopicAsync(topicHandle1, auth);
            await client.Topics.DeleteTopicAsync(topicHandle2, auth);

            // Delete user
            await TestUtilities.DeleteUser(client, auth);

            Assert.AreEqual(2, topicListResponse.Data.Count);
        }

        /// <summary>
        /// Test for popular users call
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task PopularUsersTest()
        {
            // This test is susceptable to data left over from previous tests. Run CleanServerState before running all the tests and this will work fine.

            // Create Users
            SocialPlusClient client1 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            SocialPlusClient client2 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            SocialPlusClient client3 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            PostUserResponse postUserResponse1 = await TestUtilities.DoLogin(client1, "Stan", "TopicMan", string.Empty);
            PostUserResponse postUserResponse2 = await TestUtilities.DoLogin(client2, "Bill", "Top Man", string.Empty);
            PostUserResponse postUserResponse3 = await TestUtilities.DoLogin(client3, "Jill", "TopicWoman", "Third man in");
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);
            string auth3 = AuthHelper.CreateSocialPlusAuth(postUserResponse3.SessionToken);

            // Get Followings
            PostFollowingUserRequest postFollowingRequest1 = new PostFollowingUserRequest(postUserResponse1.UserHandle);
            PostFollowingUserRequest postFollowingRequest2 = new PostFollowingUserRequest(postUserResponse1.UserHandle);
            PostFollowingUserRequest postFollowingRequest3 = new PostFollowingUserRequest(postUserResponse2.UserHandle);

            await client2.MyFollowing.PostFollowingUserAsync(postFollowingRequest1, auth2);
            await client3.MyFollowing.PostFollowingUserAsync(postFollowingRequest2, auth3);
            await client3.MyFollowing.PostFollowingUserAsync(postFollowingRequest3, auth3);

            // Use the first one as the gating factor. If that works then others will be ready too
            FeedResponseUserProfileView popularUsers1 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
            {
                // Various Popular Users stuff
                popularUsers1 = await client3.Users.GetPopularUsersAsync(auth3, null, 10);
            }, () =>
            {
                // verify
                Assert.AreEqual("Stan", popularUsers1.Data[0].FirstName);
                Assert.AreEqual("TopicMan", popularUsers1.Data[0].LastName);
            });

            // Various Popular Users stuff - check the others and use cursor
            FeedResponseUserProfileView popularUsers2 = await client1.Users.GetPopularUsersAsync(auth1, null, 1);
            FeedResponseUserProfileView popularUsers3 = await client1.Users.GetPopularUsersAsync(auth1, int.Parse(popularUsers2.Cursor), 1);

            await client2.MyFollowing.DeleteFollowingUserAsync(postUserResponse1.UserHandle, auth2);
            await client3.MyFollowing.DeleteFollowingUserAsync(postUserResponse1.UserHandle, auth3);

            // Since following stuff deleted need to wait for service bus
            FeedResponseUserProfileView popularUsers4 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
            {
                // Various Popular Users stuff
                popularUsers4 = await client1.Users.GetPopularUsersAsync(auth1, null, 10);
            }, () =>
            {
                // verify
                Assert.AreEqual("Bill", popularUsers4.Data[0].FirstName);
                Assert.AreEqual("Top Man", popularUsers4.Data[0].LastName);
            });
            await client3.Users.DeleteUserAsync(auth3);

            var popularUsers5 = await client1.Users.GetPopularUsersAsync(auth1, null, 10);
            await client2.Users.DeleteUserAsync(auth2);

            var popularUsers6 = await client1.Users.GetPopularUsersAsync(auth1, null, 10);
            await client1.Users.DeleteUserAsync(auth1);

            var popularUsers7 = await client1.Users.GetPopularUsersAsync(auth1, null, 10);

            // Verify
            Assert.AreEqual(2, popularUsers1.Data.Count);
            Assert.AreEqual("Stan", popularUsers1.Data[0].FirstName);
            Assert.AreEqual("TopicMan", popularUsers1.Data[0].LastName);
            Assert.AreEqual(postUserResponse1.UserHandle, popularUsers1.Data[0].UserHandle);
            Assert.AreEqual("Bill", popularUsers1.Data[1].FirstName);
            Assert.AreEqual("Top Man", popularUsers1.Data[1].LastName);
            Assert.AreEqual(postUserResponse2.UserHandle, popularUsers1.Data[1].UserHandle);

            Assert.AreEqual(1, popularUsers2.Data.Count);
            Assert.AreEqual("Stan", popularUsers2.Data[0].FirstName);
            Assert.AreEqual("TopicMan", popularUsers2.Data[0].LastName);

            Assert.AreEqual(1, popularUsers3.Data.Count);
            Assert.AreEqual("Bill", popularUsers3.Data[0].FirstName);
            Assert.AreEqual("Top Man", popularUsers3.Data[0].LastName);

            Assert.AreEqual(2, popularUsers4.Data.Count);
            Assert.AreEqual("Bill", popularUsers4.Data[0].FirstName);
            Assert.AreEqual("Top Man", popularUsers4.Data[0].LastName);
            Assert.AreEqual(postUserResponse2.UserHandle, popularUsers4.Data[0].UserHandle);
            Assert.AreEqual("Stan", popularUsers4.Data[1].FirstName);
            Assert.AreEqual("TopicMan", popularUsers4.Data[1].LastName);
            Assert.AreEqual(postUserResponse1.UserHandle, popularUsers4.Data[1].UserHandle);

            Assert.AreEqual(2, popularUsers5.Data.Count);
            Assert.AreEqual("Bill", popularUsers5.Data[0].FirstName);
            Assert.AreEqual("Top Man", popularUsers5.Data[0].LastName);
            Assert.AreEqual(postUserResponse2.UserHandle, popularUsers5.Data[0].UserHandle);
            Assert.AreEqual("Stan", popularUsers5.Data[1].FirstName);
            Assert.AreEqual("TopicMan", popularUsers5.Data[1].LastName);
            Assert.AreEqual(postUserResponse1.UserHandle, popularUsers5.Data[1].UserHandle);

            Assert.AreEqual(1, popularUsers6.Data.Count);
            Assert.AreEqual("Stan", popularUsers6.Data[0].FirstName);
            Assert.AreEqual("TopicMan", popularUsers6.Data[0].LastName);
            Assert.AreEqual(postUserResponse1.UserHandle, popularUsers6.Data[0].UserHandle);

            Assert.AreEqual(0, popularUsers7.Data.Count);
        }

        /// <summary>
        /// Tests for popular user topics
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task PopularUserTopicsTest()
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user = await TestUtilities.PostGenericUser(client);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);

            var httpResponse1 = await TestUtilities.PostGenericTopic(client, auth);
            string topicHandle1 = httpResponse1.TopicHandle;

            var httpResponse2 = await TestUtilities.PostGenericTopic(client, auth);
            string topicHandle2 = httpResponse2.TopicHandle;

            await client.TopicLikes.PostLikeAsync(topicHandle1, auth);

            // Wait for Service Bus to update things since topic just deleted
            FeedResponseTopicView popularTopics1 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
            {
                popularTopics1 = await client.MyTopics.GetPopularTopicsAsync(auth, null, 10);
            }, () =>
            {
                Assert.AreEqual(2, popularTopics1.Data.Count);
            });

            // Get Popular User Topics
            FeedResponseTopicView popularTopics2 = await client.MyTopics.GetPopularTopicsAsync(auth, null, 10);
            FeedResponseTopicView popularTopics3 = await client.MyTopics.GetPopularTopicsAsync(auth, int.Parse(popularTopics2.Cursor), 1);

            // Delete then check
            await client.Topics.DeleteTopicAsync(topicHandle1, auth);

            // Wait for Service Bus to update things since topic just deleted
            FeedResponseTopicView popularTopics4 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
            {
                popularTopics4 = await client.MyTopics.GetPopularTopicsAsync(auth, null, 10);
            }, () =>
            {
                Assert.AreEqual(1, popularTopics4.Data.Count);
            });

            // Delete then check
            await client.Topics.DeleteTopicAsync(topicHandle2, auth);

            // Check but wait for Service Bus
            FeedResponseTopicView popularTopics5 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
            {
                popularTopics5 = await client.MyTopics.GetPopularTopicsAsync(auth, null, 10);
            }, () =>
            {
                Assert.AreEqual(1, popularTopics5.Data.Count);
            });

            await client.Users.DeleteUserAsync(auth);

            // Verify
            Assert.AreEqual(2, popularTopics1.Data.Count);
            Assert.AreEqual(1, popularTopics1.Data[0].TotalLikes);
            Assert.AreEqual(true, popularTopics1.Data[0].Liked);
            Assert.AreEqual(0, popularTopics1.Data[1].TotalLikes);
            Assert.AreEqual(false, popularTopics1.Data[1].Liked);

            Assert.AreEqual(1, popularTopics2.Data.Count);
            Assert.AreEqual(1, popularTopics2.Data[0].TotalLikes);
            Assert.AreEqual(true, popularTopics2.Data[0].Liked);

            Assert.AreEqual(1, popularTopics3.Data.Count);
            Assert.AreEqual(0, popularTopics3.Data[0].TotalLikes);
            Assert.AreEqual(false, popularTopics3.Data[0].Liked);

            Assert.AreEqual(1, popularTopics4.Data.Count);
            Assert.AreEqual(0, popularTopics4.Data[0].TotalLikes);
            Assert.AreEqual(false, popularTopics4.Data[0].Liked);

            Assert.AreEqual(0, popularTopics5.Data.Count);
        }

        /// <summary>
        /// Tests for popular user topics but using the user handle
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task PopularUserTopicsTest_UsingUserHandle()
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user = await TestUtilities.PostGenericUser(client);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);

            var httpResponse1 = await TestUtilities.PostGenericTopic(client, auth);
            string topicHandle1 = httpResponse1.TopicHandle;

            var httpResponse2 = await TestUtilities.PostGenericTopic(client, auth);
            string topicHandle2 = httpResponse2.TopicHandle;

            await client.TopicLikes.PostLikeAsync(topicHandle1, auth);

            // Wait for Service Bus - wait for the one and other checks will be ok as just a check of same data
            FeedResponseTopicView popularTopics1 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                // Get before delete it
                popularTopics1 = await client.MyTopics.GetPopularTopicsAsync(auth, null, 10);
                }, () =>
                {
                    Assert.AreEqual(2, popularTopics1.Data.Count);
                });

            FeedResponseTopicView popularTopics2 = await client.UserTopics.GetPopularTopicsAsync(user.UserHandle, auth, null, 1);
            FeedResponseTopicView popularTopics3 = await client.UserTopics.GetPopularTopicsAsync(user.UserHandle, auth, int.Parse(popularTopics2.Cursor), 1);

            // Now Delete and check it
            await client.Topics.DeleteTopicAsync(topicHandle1, auth);

            // Wait for Service Bus
            FeedResponseTopicView popularTopics4 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                // Get before delete it
                popularTopics4 = await client.UserTopics.GetPopularTopicsAsync(user.UserHandle, auth, null, 10);
                }, () =>
                {
                    Assert.AreEqual(1, popularTopics4.Data.Count);
                });

            // Delete another one and check again
            await client.Topics.DeleteTopicAsync(topicHandle2, auth);

            // Wait for Service Bus
            FeedResponseTopicView popularTopics5 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                // Get before delete it
                popularTopics5 = await client.UserTopics.GetPopularTopicsAsync(user.UserHandle, auth, null, 10);
                }, () =>
                {
                    Assert.AreEqual(0, popularTopics5.Data.Count);
                });

            // final clean up
            await client.Users.DeleteUserAsync(auth);

            // Verify everything
            Assert.AreEqual(2, popularTopics1.Data.Count);
            Assert.AreEqual(1, popularTopics1.Data[0].TotalLikes);

            Assert.AreEqual(1, popularTopics2.Data.Count);
            Assert.AreEqual(1, popularTopics2.Data[0].TotalLikes);

            Assert.AreEqual(1, popularTopics3.Data.Count);
            Assert.AreEqual(0, popularTopics3.Data[0].TotalLikes);

            Assert.AreEqual(1, popularTopics4.Data.Count);
            Assert.AreEqual(0, popularTopics4.Data[0].TotalLikes);

            Assert.AreEqual(0, popularTopics5.Data.Count);
        }

        /// <summary>
        /// Test back-to-back create users.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task LoadCreateDeleteUserTest()
        {
            // number of users
            int numUsers = 100;

            TimeSpan threeMinutes = new TimeSpan(0, 3, 0);

            SocialPlusClient[] clients = new SocialPlusClient[numUsers];
            Task<PostUserResponse>[] t = new Task<PostUserResponse>[numUsers];
            PostUserResponse[] responses = new PostUserResponse[numUsers];

            for (int i = 0; i < numUsers; i += 1)
            {
                clients[i] = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

                // Add another three minutes to the timeout defaults of these clients. We don't want them to timeout early.
                ////api[i].IncrementHttpTimeout(threeMinutes);
            }

            for (int i = 0; i < numUsers; i += 1)
            {
                t[i] = TestUtilities.PostGenericUser(clients[i]);
            }

            for (int i = 0; i < numUsers; i += 1)
            {
                t[i].Wait();
                responses[i] = (PostUserResponse)t[i].Result;

                // Write to the console the user handle and the last 10 chars of the session token (no need to write the whole session token)
                string sessionTokenSuffix = responses[i].SessionToken.Substring(responses[i].SessionToken.Length - 10);
                Console.WriteLine("{0}: UserHandle: {1}; SessionToken: ...{2} created.", i, responses[i].UserHandle, sessionTokenSuffix);
            }

            // Cleanup
            for (int i = 0; i < numUsers; i += 1)
            {
                // final clean up
                var auth = AuthHelper.CreateSocialPlusAuth(responses[i].SessionToken);
                await clients[i].Users.DeleteUserAsync(auth);
            }
        }

        /// <summary>
        /// Test that user handles are case sensitive
        /// Step 1. Create user
        /// Step 2. Change the case of the first letter of the user handle
        /// Step 3. Get user. (this should return not found)
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task CheckUserHandlesCaseSensitivity()
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user = await TestUtilities.PostGenericUser(client);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);

            // Change the case of the first letter of the user handle, and call get user
            string similarUserHandle = user.UserHandle.ChangeCaseOfFirstLetter();
            var similarUserResponse = await client.Users.GetUserWithHttpMessagesAsync(similarUserHandle, auth);

            // Delete generic user
            await client.Users.DeleteUserAsync(auth);

            // Check that GetUser returned NotFound
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, similarUserResponse.Response.StatusCode);
        }
    }
}