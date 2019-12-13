// <copyright file="SocialFollowTests.cs" company="Microsoft">
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
    /// Tests of social relationships.
    /// These tests cover following and followers relationships, blocking and pending users, as well as
    /// the contents of the combined following topics feed.
    /// </summary>
    [TestClass]
    public class SocialFollowTests
    {
        /// <summary>
        /// Test of one user following another. The sequence of steps is:
        /// - create user1 and user2
        /// - user2 gets the feed of users that he is following
        /// - user2 follows user1
        /// - user2 gets the feed of users that he is following
        /// - user1 gets the profile of user2
        /// - user2 gets the profile of user1
        /// - clean up: delete both users
        /// - validate: check that following1 is empty, following2 contains user1, and both user profiles have the correct state
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task SocialFollowingUserTest()
        {
            // create a client
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // create user1 and user2
            var postUserResponse1 = await TestUtilities.PostGenericUser(client);
            var postUserResponse2 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // user2 gets the feed of users that he is following
            FeedResponseUserCompactView following1 = await client.MyFollowing.GetFollowingUsersAsync(auth2, null, 10);

            // user2 follows user1
            PostFollowingUserRequest postFollowingRequest = new PostFollowingUserRequest(postUserResponse1.UserHandle);
            await client.MyFollowing.PostFollowingUserAsync(postFollowingRequest, auth2);

            // user2 gets the feed of users that he is following
            var following2 = await client.UserFollowing.GetFollowingAsync(postUserResponse2.UserHandle, auth2, null, 10);

            // user1 gets the profile of user2
            // user2 gets the profile of user1
            var userProfile1 = await client.Users.GetUserAsync(postUserResponse2.UserHandle, auth1);
            var userProfile2 = await client.Users.GetUserAsync(postUserResponse1.UserHandle, auth2);

            // clean up: delete both users
            await TestUtilities.DeleteUser(client, auth1);
            await TestUtilities.DeleteUser(client, auth2);

            // validate: check that following1 is empty, following2 contains user1, and both user profiles have the correct state
            Assert.AreEqual(0, following1.Data.Count);
            Assert.AreEqual(1, following2.Data.Count);
            Assert.AreEqual(postUserResponse1.UserHandle, following2.Data[0].UserHandle);
            Assert.AreEqual(FollowerStatus.Follow, following2.Data[0].FollowerStatus);

            Assert.AreEqual(FollowerStatus.None, userProfile1.FollowerStatus);
            Assert.AreEqual(FollowingStatus.Follow, userProfile1.FollowingStatus);
            Assert.AreEqual(0, userProfile1.TotalFollowers);
            Assert.AreEqual(1, userProfile1.TotalFollowing);

            Assert.AreEqual(FollowerStatus.Follow, userProfile2.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, userProfile2.FollowingStatus);
            Assert.AreEqual(1, userProfile2.TotalFollowers);
            Assert.AreEqual(0, userProfile2.TotalFollowing);
        }

        /// <summary>
        /// Test of one user following another, verifying the contents of the combined following topics feed,
        /// and then unfollowing that user. The sequence of steps is:
        /// - create user1 and user2
        /// - user1 creates topic1 and topic2
        /// - user2 follows user1
        /// - user2 gets the combined following topics feed
        /// - wait until two topics are returned or the auto-retry helper times out
        /// - user2 unfollows user1
        /// - user2 gets the combined following topics feed after user2 unfollows user1
        /// - clean up: delete topics and users
        /// - validate: check that followingTopics1 contains both topics and followingTopics2 is empty
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task SocialFollowUnfollowTest()
        {
            // create a client
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // create user1 and user2
            var postUserResponse1 = await TestUtilities.PostGenericUser(client);
            var postUserResponse2 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // user1 creates topic1 and topic2
            var postTopic1 = await TestUtilities.PostGenericTopic(client, auth1);
            var postTopic2 = await TestUtilities.PostGenericTopic(client, auth1);

            // user2 follows user1
            PostFollowingUserRequest postFollowingRequest = new PostFollowingUserRequest(postUserResponse1.UserHandle);
            await client.MyFollowing.PostFollowingUserAsync(postFollowingRequest, auth2);

            // when one user follows another, import of existing topics into the following topics feed
            // is done by a worker
            FeedResponseTopicView followingTopics1 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    // user2 gets the combined following topics feed
                    followingTopics1 = await client.MyFollowing.GetTopicsAsync(auth2, null, 10);
                }, () =>
                {
                    // wait until two topics are returned or the auto-retry helper times out
                    Assert.AreEqual(2, followingTopics1.Data.Count);
                });

            // user2 unfollows user1
            await client.MyFollowing.DeleteFollowingUserAsync(postUserResponse1.UserHandle, auth2);

            // user2 gets the combined following topics feed after user2 unfollows user1
            var followingTopics2 = await client.MyFollowing.GetTopicsAsync(auth2, null, 10);

            // clean up: delete topics and users
            await TestUtilities.DeleteTopic(client, postTopic1.TopicHandle, auth1);
            await TestUtilities.DeleteTopic(client, postTopic2.TopicHandle, auth1);
            await TestUtilities.DeleteUser(client, auth1);
            await TestUtilities.DeleteUser(client, auth2);

            // validate: check that followingTopics1 contains both topics and followingTopics2 is empty
            Assert.AreEqual(2, followingTopics1.Data.Count);
            Assert.AreEqual(0, followingTopics2.Data.Count);

            Assert.AreEqual(postTopic2.TopicHandle, followingTopics1.Data[0].TopicHandle);
            Assert.AreEqual(FollowerStatus.Follow, followingTopics1.Data[0].User.FollowerStatus);
            Assert.AreEqual(postTopic1.TopicHandle, followingTopics1.Data[1].TopicHandle);
            Assert.AreEqual(FollowerStatus.Follow, followingTopics1.Data[1].User.FollowerStatus);
        }

        /// <summary>
        /// Test of user2 follows user1, then user2 unfollows (hides) a topic created by user1.
        /// This causes only that one topic to disappear from user2's combined following topics feed.
        /// The sequence of steps is:
        /// - create user1 and user2
        /// - user1 creates topic1 and topic2
        /// - user2 follows user1
        /// - user2 gets the combined following topics feed
        /// - wait until two topics are returned or the auto-retry helper times out
        /// - user2 unfollows topic2 from the combined following topics feed
        /// - user2 gets the combined following topics feed
        /// - clean up: delete topics and users
        /// - validate: check that user2 sees topic1 and topic2 in the following topics feed on the first get,
        ///   and user2 sees topic1 in its following topics feed on the second get.
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task SocialUnfollowTopicTest()
        {
            // create the client
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // create user1 and user2
            var postUserResponse1 = await TestUtilities.PostGenericUser(client);
            var postUserResponse2 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // user1 creates two topics
            var postTopic1 = await TestUtilities.PostGenericTopic(client, auth1);
            var postTopic2 = await TestUtilities.PostGenericTopic(client, auth1);

            // user2 follows user1
            PostFollowingUserRequest postFollowingRequest = new PostFollowingUserRequest(postUserResponse1.UserHandle);
            await client.MyFollowing.PostFollowingUserAsync(postFollowingRequest, auth2);

            // when one user follows another, import of existing topics into the following topics feed
            // is done by a worker
            FeedResponseTopicView followingTopics1 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    // user2 gets the combined following topics feed
                    followingTopics1 = await client.MyFollowing.GetTopicsAsync(auth2, null, 10);
                }, () =>
                {
                    // wait until two topics are returned or the auto-retry helper times out
                    Assert.AreEqual(2, followingTopics1.Data.Count);
                });

            // user2 unfollows topic2 from the combined following topics feed
            await client.MyFollowing.DeleteFollowingTopicAsync(topicHandle: postTopic2.TopicHandle, authorization: auth2);

            // user2 gets the combined following topics feed
            var followingTopics2 = await client.MyFollowing.GetTopicsAsync(auth2, null, 10);

            // clean up: delete topics and users
            await TestUtilities.DeleteTopic(client, postTopic1.TopicHandle, auth1);
            await TestUtilities.DeleteTopic(client, postTopic2.TopicHandle, auth1);
            await TestUtilities.DeleteUser(client, auth1);
            await TestUtilities.DeleteUser(client, auth2);

            // validate: check that user2 sees topic1 and topic2 in the following topics feed on the first get,
            // and user2 sees topic1 in its following topics feed on the second get.
            Assert.AreEqual(2, followingTopics1.Data.Count);
            Assert.AreEqual(postTopic2.TopicHandle, followingTopics1.Data[0].TopicHandle);
            Assert.AreEqual(FollowerStatus.Follow, followingTopics1.Data[0].User.FollowerStatus);
            Assert.AreEqual(postTopic1.TopicHandle, followingTopics1.Data[1].TopicHandle);
            Assert.AreEqual(FollowerStatus.Follow, followingTopics1.Data[1].User.FollowerStatus);

            Assert.AreEqual(1, followingTopics2.Data.Count);
            Assert.AreEqual(postTopic1.TopicHandle, followingTopics2.Data[0].TopicHandle);
            Assert.AreEqual(FollowerStatus.Follow, followingTopics2.Data[0].User.FollowerStatus);
        }

        /// <summary>
        /// Test following a private user. The sequence of steps is:
        /// - create user1 and user2
        /// - make user2 a private user
        /// - user2 gets the feed of his followers
        /// - user1 requests to follow user2
        /// - user2 gets his feed of pending users
        /// - user2 accepts user1's follow request
        /// - user2 gets the feed of his followers with GET /users/{userhandle}/followers
        /// - user2 gets the feed of his followers with GET /users/me/followers - result is same, just using a different API
        /// - user1 gets the feed of his following users
        /// - clean up: delete both users
        /// - validate:
        ///   check that user2's list of pending users includes user1
        ///   check that user2's followers feed includes user1
        ///   check that user1's list of following users includes user2
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task SocialFollowPrivateUserTest()
        {
            // create the client
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // create user1 and user2
            var postUserResponse1 = await TestUtilities.PostGenericUser(client);
            var postUserResponse2 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // make user2 a private user
            PutUserVisibilityRequest putUserVisibilityRequest = new PutUserVisibilityRequest(Visibility.Private);
            await client.Users.PutUserVisibilityAsync(putUserVisibilityRequest, auth2);

            // user2 gets the feed of his followers
            var followersUsers1 = await client.MyFollowers.GetFollowersAsync(auth2, null, 10);

            // user1 requests to follow user2
            PostFollowingUserRequest postFollowingRequest1 = new PostFollowingUserRequest(postUserResponse2.UserHandle);
            await client.MyFollowing.PostFollowingUserAsync(postFollowingRequest1, auth1);

            // user2 gets his feed of pending users
            var pendingUsers = await client.MyPendingUsers.GetPendingUsersAsync(auth2, null, 10);

            // user2 accepts user1's follow request
            PostFollowerRequest postFollowerRequest2 = new PostFollowerRequest(postUserResponse1.UserHandle);
            await client.MyFollowers.PostFollowerAsync(postFollowerRequest2, auth2);

            // user2 gets the feed of his followers with GET /users/{userhandle}/followers
            var followersUsers2 = await client.UserFollowers.GetFollowersAsync(postUserResponse2.UserHandle, auth2, null, 10);

            // user2 gets the feed of his followers with GET /users/me/followers - result is same, just using a different API
            var followersUsers3 = await client.MyFollowers.GetFollowersAsync(auth2, null, 10);

            // user1 gets the feed of his following users
            var followingUsers = await client.MyFollowing.GetFollowingUsersAsync(auth1, null, 10);

            // clean up: delete both users
            await TestUtilities.DeleteUser(client, auth1);
            await TestUtilities.DeleteUser(client, auth2);

            // validate:
            // check that user2's list of pending users includes user1
            // check that user2's followers feed includes user1
            // check that user1's list of following users includes user2
            Assert.AreEqual(0, followersUsers1.Data.Count);
            Assert.AreEqual(1, followersUsers2.Data.Count);
            Assert.AreEqual(1, followersUsers3.Data.Count);
            Assert.AreEqual(1, pendingUsers.Data.Count);
            Assert.AreEqual(1, followingUsers.Data.Count);

            Assert.AreEqual(postUserResponse1.UserHandle, followersUsers2.Data[0].UserHandle);
            Assert.AreEqual(FollowerStatus.None, followersUsers2.Data[0].FollowerStatus);
            Assert.AreEqual(postUserResponse1.UserHandle, followersUsers3.Data[0].UserHandle);
            Assert.AreEqual(FollowerStatus.None, followersUsers3.Data[0].FollowerStatus);
            Assert.AreEqual(postUserResponse1.UserHandle, pendingUsers.Data[0].UserHandle);
            Assert.AreEqual(FollowerStatus.None, pendingUsers.Data[0].FollowerStatus);
            Assert.AreEqual(postUserResponse2.UserHandle, followingUsers.Data[0].UserHandle);
            Assert.AreEqual(FollowerStatus.Follow, followingUsers.Data[0].FollowerStatus);
        }

        /// <summary>
        /// Test #2 following a private user.
        /// In this case, the follow request is rejected rather than accepted. The sequence of steps is:
        /// - create user1 and user2
        /// - user1 gets the profile of user2
        /// - user2 gets the profile of user1
        /// - make user2 a private user
        /// - user1 requests to follow user2
        /// - user2 gets the count of his pending users
        /// - user2 gets his feed of pending users
        /// - user1 gets the profile of user2
        /// - user2 gets the profile of user1
        /// - user2 rejects user1's follow request
        /// - user2 gets the count of his pending users
        /// - user2 gets his feed of pending users
        /// - user1 gets the profile of user2
        /// - user2 gets the profile of user1
        /// - clean up: delete both users
        /// - validate:
        ///   check that user2's list of pending users includes user1
        ///   check that user2's list of pending users is empty after the reject
        ///   check the user profiles to see that follower status and following status are updated correctly
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task SocialFollowPrivateUserRejectTest()
        {
            // create a client
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // create user1 and user2
            var postUserResponse1 = await TestUtilities.PostGenericUser(client);
            var postUserResponse2 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // user1 gets the profile of user2
            // user2 gets the profile of user1
            var userProfile1 = await client.Users.GetUserAsync(postUserResponse2.UserHandle, auth1);
            var userProfile2 = await client.Users.GetUserAsync(postUserResponse1.UserHandle, auth2);

            // make user2 a private user
            PutUserVisibilityRequest putUserVisibilityRequest = new PutUserVisibilityRequest(Visibility.Private);
            await client.Users.PutUserVisibilityAsync(putUserVisibilityRequest, auth2);

            // user1 requests to follow user2
            PostFollowingUserRequest postFollowingRequest = new PostFollowingUserRequest(postUserResponse2.UserHandle);
            await client.MyFollowing.PostFollowingUserAsync(postFollowingRequest, auth1);

            CountResponse getPendingUserCount1 = null;
            FeedResponseUserCompactView getPendingUsers1 = null;

            // user2 gets the count of his pending users
            // user2 gets his feed of pending users
            getPendingUserCount1 = await client.MyPendingUsers.GetPendingUsersCountAsync(auth2);
            getPendingUsers1 = await client.MyPendingUsers.GetPendingUsersAsync(auth2, null, 10);

            // user1 gets the profile of user2
            // user2 gets the profile of user1
            var userProfile3 = await client.Users.GetUserAsync(postUserResponse2.UserHandle, auth1);
            var userProfile4 = await client.Users.GetUserAsync(postUserResponse1.UserHandle, auth2);

            // user2 rejects user1's follow request
            await client.MyPendingUsers.DeletePendingUserAsync(postUserResponse1.UserHandle, auth2);

            // user2 gets the count of his pending users
            // user2 gets his feed of pending users
            CountResponse getPendingUserCount2 = await client.MyPendingUsers.GetPendingUsersCountAsync(auth2);
            var getPendingUsers2 = await client.MyPendingUsers.GetPendingUsersAsync(auth2, null, 10);

            // user1 gets the profile of user2
            // user2 gets the profile of user1
            var userProfile5 = await client.Users.GetUserAsync(postUserResponse2.UserHandle, auth1);
            var userProfile6 = await client.Users.GetUserAsync(postUserResponse1.UserHandle, auth2);

            // clean up: delete both users
            await TestUtilities.DeleteUser(client, auth1);
            await TestUtilities.DeleteUser(client, auth2);

            // validate:
            // check that user2's list of pending users includes user1
            // check that user2's list of pending users is empty after the reject
            // check the user profiles to see that follower status and following status are updated correctly
            Assert.AreEqual(1, getPendingUserCount1.Count);
            Assert.AreEqual(0, getPendingUserCount2.Count);

            Assert.AreEqual(postUserResponse1.UserHandle, getPendingUsers1.Data[0].UserHandle);
            Assert.AreEqual(FollowerStatus.None, getPendingUsers1.Data[0].FollowerStatus);
            Assert.AreEqual(Visibility.Public, getPendingUsers1.Data[0].Visibility);

            // Validate User Profiles
            Assert.AreEqual(FollowerStatus.None, userProfile1.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, userProfile1.FollowingStatus);
            Assert.AreEqual(Visibility.Public, userProfile1.Visibility);

            Assert.AreEqual(FollowerStatus.None, userProfile2.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, userProfile2.FollowingStatus);
            Assert.AreEqual(Visibility.Public, userProfile2.Visibility);

            Assert.AreEqual(FollowerStatus.Pending, userProfile3.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, userProfile3.FollowingStatus);
            Assert.AreEqual(Visibility.Private, userProfile3.Visibility);

            Assert.AreEqual(FollowerStatus.None, userProfile4.FollowerStatus);
            Assert.AreEqual(FollowingStatus.Pending, userProfile4.FollowingStatus);
            Assert.AreEqual(Visibility.Public, userProfile4.Visibility);

            Assert.AreEqual(FollowerStatus.None, userProfile5.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, userProfile5.FollowingStatus);
            Assert.AreEqual(Visibility.Private, userProfile5.Visibility);

            Assert.AreEqual(FollowerStatus.None, userProfile6.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, userProfile6.FollowingStatus);
            Assert.AreEqual(Visibility.Public, userProfile6.Visibility);
        }

        /// <summary>
        /// Test deleting following and followers. The sequence of steps is:
        ///  - create user1, user2, and user3
        ///  - user2 follows user1
        ///  - user3 follows user1
        ///  - user1 gets the feed of his followers
        ///  - user2 unfollows user1
        ///  - user1 gets the updated list of his followers
        ///  - user1 removes user3 as a follower
        ///  - user1 gets the updated list of his followers
        ///  - clean up: delete all three users
        ///  - validate:
        ///    check that user1's list of followers includes user2 and user3 the first time.
        ///    check that user1's list of followers includes user3 the second time.
        ///    check that user1's list of following users is empty the third time.
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task SocialDeleteFollowTest()
        {
            // create a client
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // create user1, user2, and user3
            var postUserResponse1 = await TestUtilities.PostGenericUser(client);
            var postUserResponse2 = await TestUtilities.PostGenericUser(client);
            var postUserResponse3 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);
            string auth3 = AuthHelper.CreateSocialPlusAuth(postUserResponse3.SessionToken);

            // user2 follows user1
            PostFollowingUserRequest postFollowingRequest = new PostFollowingUserRequest(postUserResponse1.UserHandle);
            await client.MyFollowing.PostFollowingUserAsync(postFollowingRequest, auth2);

            // user3 follows user1
            await client.MyFollowing.PostFollowingUserAsync(postFollowingRequest, auth3);

            // user1 gets the feed of his followers
            var followersUsers1 = await client.MyFollowers.GetFollowersAsync(auth1, null, 10);

            // user2 unfollows user1
            await client.MyFollowing.DeleteFollowingUserAsync(postUserResponse1.UserHandle, auth2);

            // user1 gets the updated list of his followers
            var followersUsers2 = await client.MyFollowers.GetFollowersAsync(auth1, null, 10);

            // user1 removes user3 as a follower
            await client.MyFollowers.DeleteFollowerAsync(postUserResponse3.UserHandle, auth1);

            // user1 gets the updated list of his followers
            var followersUsers3 = await client.MyFollowers.GetFollowersAsync(auth1, null, 10);

            // clean up: delete all three users
            await TestUtilities.DeleteUser(client, auth1);
            await TestUtilities.DeleteUser(client, auth2);
            await TestUtilities.DeleteUser(client, auth3);

            // - validate:
            //   check that user1's list of followers includes user2 and user3 the first time.
            //   check that user1's list of followers includes user3 the second time.
            //   check that user1's list of following users is empty the third time.
            Assert.AreEqual(2, followersUsers1.Data.Count);
            Assert.AreEqual(1, followersUsers2.Data.Count);
            Assert.AreEqual(0, followersUsers3.Data.Count);

            Assert.AreEqual(postUserResponse3.UserHandle, followersUsers1.Data[0].UserHandle);
            Assert.AreEqual(FollowerStatus.None, followersUsers1.Data[0].FollowerStatus);
            Assert.AreEqual(postUserResponse2.UserHandle, followersUsers1.Data[1].UserHandle);
            Assert.AreEqual(FollowerStatus.None, followersUsers1.Data[1].FollowerStatus);
            Assert.AreEqual(postUserResponse3.UserHandle, followersUsers2.Data[0].UserHandle);
            Assert.AreEqual(FollowerStatus.None, followersUsers2.Data[0].FollowerStatus);
        }

        /// <summary>
        /// Test blocking a user. The sequence of steps is:
        ///  - create user1 and user2
        ///  - user1 gets user2's profile
        ///  - user2 gets user1's profile
        ///  - user2 blocks user1
        ///  - user2 gets his list of blocked users
        ///  - user1 gets user2's profile
        ///  - user2 gets user1's profile
        ///  - user2 unblocks user1
        ///  - user1 gets user2's profile
        ///  - user2 gets user1's profile
        ///  - user2 gets his updated list of blocked users
        ///  - cleanup: delete both users
        ///  - validate:
        ///    check that user1 appears as blocked in the profile followerstatus and followingstatus
        ///    check that users2's list of blocked users has user1 the first time
        ///    check that users2's list of blocked users is empty the second time
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task SocialBlockUnblockUserTest()
        {
            // create a client
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // create user1 and user2
            var postUserResponse1 = await TestUtilities.PostGenericUser(client);
            var postUserResponse2 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // user1 gets user2's profile
            // user2 gets user1's profile
            var userProfile1 = await client.Users.GetUserAsync(postUserResponse2.UserHandle, auth1);
            var userProfile2 = await client.Users.GetUserAsync(postUserResponse1.UserHandle, auth2);

            // user2 blocks user1
            PostBlockedUserRequest postBlockedUserRequest = new PostBlockedUserRequest(postUserResponse1.UserHandle);
            await client.MyBlockedUsers.PostBlockedUserAsync(postBlockedUserRequest, auth2);

            // user2 gets his list of blocked users
            var getBlockedUsers1 = await client.MyBlockedUsers.GetBlockedUsersAsync(auth2, null, 10);

            // user1 gets user2's profile
            // user2 gets user1's profile
            var userProfile3 = await client.Users.GetUserAsync(postUserResponse2.UserHandle, auth1);
            var userProfile4 = await client.Users.GetUserAsync(postUserResponse1.UserHandle, auth2);

            // user2 unblocks user1
            await client.MyBlockedUsers.DeleteBlockedUserAsync(postUserResponse1.UserHandle, auth2);

            // user1 gets user2's profile
            // user2 gets user1's profile
            var userProfile5 = await client.Users.GetUserAsync(postUserResponse2.UserHandle, auth1);
            var userProfile6 = await client.Users.GetUserAsync(postUserResponse1.UserHandle, auth2);

            // user2 gets his updated list of blocked users
            var getBlockedUsers2 = await client.MyBlockedUsers.GetBlockedUsersAsync(auth2, null, 10);

            // cleanup: delete both users
            await TestUtilities.DeleteUser(client, auth1);
            await TestUtilities.DeleteUser(client, auth2);

            // validate:
            // check that user1 appears as blocked in the profile followerstatus and followingstatus
            // check that users2's list of blocked users has user1 the first time
            // check that users2's list of blocked users is empty the second time
            Assert.AreEqual(FollowerStatus.None, userProfile1.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, userProfile1.FollowingStatus);

            Assert.AreEqual(FollowerStatus.None, userProfile2.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, userProfile2.FollowingStatus);

            Assert.AreEqual(FollowerStatus.Blocked, userProfile3.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, userProfile3.FollowingStatus);

            Assert.AreEqual(FollowerStatus.None, userProfile4.FollowerStatus);
            Assert.AreEqual(FollowingStatus.Blocked, userProfile4.FollowingStatus);

            Assert.AreEqual(FollowerStatus.None, userProfile5.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, userProfile5.FollowingStatus);

            Assert.AreEqual(FollowerStatus.None, userProfile6.FollowerStatus);
            Assert.AreEqual(FollowingStatus.None, userProfile6.FollowingStatus);

            Assert.AreEqual(1, getBlockedUsers1.Data.Count);
            Assert.AreEqual(postUserResponse1.UserHandle, getBlockedUsers1.Data[0].UserHandle);
            Assert.AreEqual(FollowerStatus.None, getBlockedUsers1.Data[0].FollowerStatus);

            Assert.AreEqual(0, getBlockedUsers2.Data.Count);
        }

        /// <summary>
        /// Tests whether a user can block a follower. The sequence of steps is:
        ///  - create user1 and user2
        ///  - user1 follows user2
        ///  - user2 posts topic1
        ///  - user1 gets topic1
        ///  - user2 blocks user1
        ///  - user2 posts topic2
        ///  - user1 gets topic1
        ///  - user1 gets topic2
        ///  - cleanup: delete both users and both topics
        ///  - validate:
        ///    check that getting topic1 worked the first time
        ///    check that getting topic1 worked the second time
        ///    check that getting topic2 did not work
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        /// <remarks>This test currently fails. Bug #697 in VSO has been entered (and it's currently open)</remarks>
        [TestMethod]
        public async Task BlockAfterFollow()
        {
            // create a client
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // create user1 and user2
            var postUserResponse1 = await TestUtilities.PostGenericUser(client);
            var postUserResponse2 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // user1 follows user2
            PostFollowingUserRequest postFollowingRequest = new PostFollowingUserRequest(postUserResponse2.UserHandle);
            await client.MyFollowing.PostFollowingUserAsync(postFollowingRequest, auth1);

            // user2 posts topic1
            var postTopicResponse1 = await TestUtilities.PostGenericTopic(client, auth2);

            // user1 gets topic1
            TopicView topicView1 = await client.Topics.GetTopicAsync(postTopicResponse1.TopicHandle, auth1);

            // user2 blocks user1
            PostBlockedUserRequest postBlockedUserRequest = new PostBlockedUserRequest(postUserResponse1.UserHandle);
            await client.MyBlockedUsers.PostBlockedUserAsync(postBlockedUserRequest, auth2);

            // user2 posts topic2
            var postTopicResponse2 = await TestUtilities.PostGenericTopic(client, auth2);

            // user1 gets topic1
            TopicView topicView2 = await client.Topics.GetTopicAsync(postTopicResponse1.TopicHandle, auth1);

            // user1 fetches topic2
            TopicView topicView3 = await client.Topics.GetTopicAsync(postTopicResponse2.TopicHandle, auth1);

            // cleanup: delete both users and both topics
            await client.Topics.DeleteTopicAsync(postTopicResponse1.TopicHandle, auth2);
            await client.Topics.DeleteTopicAsync(postTopicResponse2.TopicHandle, auth2);
            await TestUtilities.DeleteUser(client, auth1);
            await TestUtilities.DeleteUser(client, auth2);

            // validate:
            // check that getting topic1 worked the first time
            // check that getting topic1 worked the second time
            // check that getting topic2 did not work
            Assert.AreEqual(postTopicResponse1.TopicHandle, topicView1.TopicHandle);
            Assert.AreEqual(postTopicResponse1.TopicHandle, topicView2.TopicHandle);
            Assert.AreEqual(null, topicView3.TopicHandle);
        }

        /// <summary>
        /// Get friends suggestions
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task GetFriendSuggestions()
        {
            // Facebook Access token. Have to get it manually
            // Get Access Token from here: https://developers.facebook.com/tools/explorer/ -- make sure to select the scope "user_friends"
            string fbAccessToken = "EAACEdEose0cBAIbxq9XeXJVQBNS02S2ytfGPACMvVWZCEyhq0TpxSrlQedGW2UyZAUIm0vh7Sr24xwnAnZChBHuPGLspLSRoZCFWGCvZAKe6VxLc8umrDQxSZAk7KQZAS1py7xudtAYgBmms7h19kIaEZBNT1DAZCp3p48dTK3moTq8yJX9nksgCkIpDRLoNnYC0ZD";
            string auth = "Facebook AK = " + TestConstants.AppKey + "| TK = " + fbAccessToken;

            // Set up initial stuff
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserRequest postUserRequest =
                new PostUserRequest(instanceId: TestConstants.InstanceId, firstName: "Joseph", lastName: "Johnson", bio: "Some Bio");
            HttpOperationResponse<PostUserResponse> postUserOperationResponse =
                await client.Users.PostUserWithHttpMessagesAsync(request: postUserRequest, authorization: auth);

            // Assert correct HTTP error codes. If incorrect, we do not need to delete the user any longer
            Assert.IsTrue(postUserOperationResponse.Response.IsSuccessStatusCode);

            // PostUser also returns a non-empty session token and the user handle
            Assert.IsFalse(string.IsNullOrEmpty(postUserOperationResponse.Body.SessionToken));
            Assert.IsFalse(string.IsNullOrEmpty(postUserOperationResponse.Body.UserHandle));

            // Get friend suggestions using the Facebook auth
            HttpOperationResponse<IList<UserCompactView>> friendList = await client.MyFollowing.GetSuggestionsUsersWithHttpMessagesAsync(authorization: auth);

            // Assert call went throuhg
            Assert.IsTrue(friendList.Response.IsSuccessStatusCode);

            // Delete the user
            auth = AuthHelper.CreateSocialPlusAuth(postUserOperationResponse.Body.SessionToken);
            HttpOperationResponse<object> deleteUserOperationResponse =
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // Assert delete was successful
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);
        }
    }
}
