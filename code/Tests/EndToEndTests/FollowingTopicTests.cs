// <copyright file="FollowingTopicTests.cs" company="Microsoft">
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
    /// Basic functional end to end tests for Activity APIs
    /// </summary>
    [TestClass]
    public class FollowingTopicTests
    {
        /// <summary>
        /// User1 follows a topic.  This test checks that the topic appears in the user's combined following feed, and that
        /// all users following User1 receive an activity of type Following when this happens.
        /// </summary>
        /// <returns>Task that will throw an exception if the test fails</returns>
        [TestMethod]
        public async Task FollowingTopicActivityTest()
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

            // user2 requests to follow user1
            PostFollowingUserRequest followingUserReq = new PostFollowingUserRequest()
            {
                UserHandle = user1.UserHandle
            };
            HttpOperationResponse postFollowingResponse = await client2.MyFollowing.PostFollowingUserWithHttpMessagesAsync(followingUserReq, authorization: auth2);

            // user3 requests to follow user1
            PostFollowingUserRequest followingUserReq2 = new PostFollowingUserRequest()
            {
                UserHandle = user1.UserHandle
            };
            HttpOperationResponse postFollowingResponse2 = await client3.MyFollowing.PostFollowingUserWithHttpMessagesAsync(followingUserReq2, authorization: auth3);

            // user2 creates a topic
            var postTopicResponse = await TestUtilities.PostGenericTopic(client2, auth2);
            string topicHandle = postTopicResponse.TopicHandle;

            // user1 follows the topic created by user2
            PostFollowingTopicRequest followingTopicReq = new PostFollowingTopicRequest()
            {
                TopicHandle = topicHandle
            };
            HttpOperationResponse postFollowingResponse3 = await client1.MyFollowing.PostFollowingTopicWithHttpMessagesAsync(followingTopicReq, authorization: auth1);

            // fetch user1's combined following topics feed
            HttpOperationResponse<FeedResponseTopicView> combinedFollowingTopics = await client1.MyFollowing.GetTopicsWithHttpMessagesAsync(authorization: auth1);

            // check that both user2 and user3 receive the activity generated when user1 follows the topic
            HttpOperationResponse<FeedResponseActivityView> activitiesResponse1 = null;
            HttpOperationResponse<FeedResponseActivityView> activitiesResponse2 = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    activitiesResponse1 = await client2.MyFollowing.GetActivitiesWithHttpMessagesAsync(authorization: auth2);
                    activitiesResponse2 = await client3.MyFollowing.GetActivitiesWithHttpMessagesAsync(authorization: auth3);
                }, () =>
                {
                    // verify that user2 and user3 both see 1 activity, whose type is Following
                    Assert.AreEqual(1, activitiesResponse1.Body.Data.Count);
                    Assert.AreEqual(ActivityType.Following, activitiesResponse1.Body.Data[0].ActivityType);
                    Assert.AreEqual(1, activitiesResponse2.Body.Data.Count);
                    Assert.AreEqual(ActivityType.Following, activitiesResponse2.Body.Data[0].ActivityType);
                });

            // clean up the topic
            HttpOperationResponse deleteTopic = await client2.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth2);

            // clean up the three users we created
            HttpOperationResponse deleteUserOperationResponse1 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse deleteUserOperationResponse2 = await client2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);
            HttpOperationResponse deleteUserOperationResponse3 = await client3.Users.DeleteUserWithHttpMessagesAsync(authorization: auth3);

            // check the result of the combined following topics feed
            Assert.AreEqual(1, combinedFollowingTopics.Body.Data.Count);
            Assert.AreEqual(topicHandle, combinedFollowingTopics.Body.Data[0].TopicHandle);

            // check the activity results
            IList<ActivityView> activityList = activitiesResponse1.Body.Data;
            Assert.AreEqual(1, activityList.Count);
            Assert.AreEqual(ActivityType.Following, activityList[0].ActivityType);
            Assert.AreEqual(1, activityList[0].ActorUsers.Count);
            Assert.AreEqual(user1.UserHandle, activityList[0].ActorUsers[0].UserHandle);
            Assert.AreEqual(null, activityList[0].ActedOnUser);
            Assert.AreEqual(topicHandle, activityList[0].ActedOnContent.ContentHandle);
            Assert.AreEqual(ContentType.Topic, activityList[0].ActedOnContent.ContentType);
            Assert.AreEqual(1, activityList[0].TotalActions);
            Assert.AreEqual(true, activityList[0].Unread);
            activityList = activitiesResponse2.Body.Data;
            Assert.AreEqual(1, activityList.Count);
            Assert.AreEqual(ActivityType.Following, activityList[0].ActivityType);
            Assert.AreEqual(1, activityList[0].ActorUsers.Count);
            Assert.AreEqual(user1.UserHandle, activityList[0].ActorUsers[0].UserHandle);
            Assert.AreEqual(null, activityList[0].ActedOnUser);
            Assert.AreEqual(topicHandle, activityList[0].ActedOnContent.ContentHandle);
            Assert.AreEqual(ContentType.Topic, activityList[0].ActedOnContent.ContentType);
            Assert.AreEqual(1, activityList[0].TotalActions);
            Assert.AreEqual(true, activityList[0].Unread);
        }

        /// <summary>
        /// Comment activity test
        /// </summary>
        /// <returns>Task that will throw an exception if the test fails</returns>
        [TestMethod]
        public async Task FollowingTopicCommentActivityTest()
        {
            // Setup two users
            SocialPlusClient client1 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            SocialPlusClient client2 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user1 = await TestUtilities.PostGenericUser(client1);
            var user2 = await TestUtilities.PostGenericUser(client2);
            var auth1 = AuthHelper.CreateSocialPlusAuth(user1.SessionToken);
            var auth2 = AuthHelper.CreateSocialPlusAuth(user2.SessionToken);

            // user2 creates a topic
            var postTopicResponse = await TestUtilities.PostGenericTopic(client2, auth2);
            string topicHandle = postTopicResponse.TopicHandle;

            // user1 follows the topic created by user2
            PostFollowingTopicRequest followingTopicReq = new PostFollowingTopicRequest()
            {
                TopicHandle = topicHandle
            };
            HttpOperationResponse postFollowingResponse3 = await client1.MyFollowing.PostFollowingTopicWithHttpMessagesAsync(followingTopicReq, authorization: auth1);

            // user2 posts a comment on the topic -- this causes a Comment activity to be sent to the topic followers
            var postCommentResponse = await TestUtilities.PostGenericComment(client2, auth2, topicHandle);
            string commentHandle = postCommentResponse.CommentHandle;

            HttpOperationResponse<FeedResponseActivityView> activitiesResponse = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    // because user1 follows the topic, user1 should receive an activity indicating that user2 commented on a topic
                    activitiesResponse = await client1.MyFollowing.GetActivitiesWithHttpMessagesAsync(authorization: auth1);
                }, () =>
                {
                    // verify that user1 sees 1 activity, whose type is Comment
                    Assert.AreEqual(1, activitiesResponse.Body.Data.Count);
                    Assert.AreEqual(ActivityType.Comment, activitiesResponse.Body.Data[0].ActivityType);
                });

            // clean up the comment and the topic
            HttpOperationResponse deleteComment = await client2.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth2);
            HttpOperationResponse deleteTopic = await client2.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth2);

            // clean up the two users we created
            HttpOperationResponse deleteUserOperationResponse1 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse deleteUserOperationResponse2 = await client2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);

            IList<ActivityView> activityList = activitiesResponse.Body.Data;
            Assert.AreEqual(1, activityList.Count);
            Assert.AreEqual(ActivityType.Comment, activityList[0].ActivityType);
            Assert.AreEqual(1, activityList[0].ActorUsers.Count);
            Assert.AreEqual(user2.UserHandle, activityList[0].ActorUsers[0].UserHandle);
            Assert.AreEqual(user2.UserHandle, activityList[0].ActedOnUser.UserHandle);
            Assert.AreEqual(1, activityList[0].TotalActions);
            Assert.AreEqual(true, activityList[0].Unread);
            Assert.AreEqual(ContentType.Topic, activityList[0].ActedOnContent.ContentType);
            Assert.AreEqual(topicHandle, activityList[0].ActedOnContent.ContentHandle);
        }

        /// <summary>
        /// Reply activity test
        /// </summary>
        /// <returns>Task that will throw an exception if the test fails</returns>
        [TestMethod]
        public async Task FollowingTopicReplyActivityTest()
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

            // user3 creates a topic
            var postTopicResponse = await TestUtilities.PostGenericTopic(client3, auth3);
            string topicHandle = postTopicResponse.TopicHandle;

            // user1 follows the topic created by user3
            PostFollowingTopicRequest followingTopicReq = new PostFollowingTopicRequest()
            {
                TopicHandle = topicHandle
            };
            HttpOperationResponse postFollowingResponse = await client1.MyFollowing.PostFollowingTopicWithHttpMessagesAsync(followingTopicReq, authorization: auth1);

            // user2 posts a comment on the topic -- this causes a Comment activity to be sent to the topic followers
            var postCommentResponse = await TestUtilities.PostGenericComment(client2, auth2, topicHandle);
            string commentHandle = postCommentResponse.CommentHandle;

            // user3 replies to user2's comment -- this causes a Reply activity to be sent to the topic followers
            var postReplyResponse = await TestUtilities.PostGenericReply(client3, auth3, commentHandle);
            string replyHandle = postReplyResponse.ReplyHandle;

            HttpOperationResponse<FeedResponseActivityView> activitiesResponse = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    // because user1 follows the topic, user1 should receive an activity indicating that user3 replied to user2's comment
                    activitiesResponse = await client1.MyFollowing.GetActivitiesWithHttpMessagesAsync(authorization: auth1);
                }, () =>
                {
                    // verify that user1 sees 2 activities, the first (newest) activity type is Reply
                    Assert.AreEqual(2, activitiesResponse.Body.Data.Count);
                    Assert.AreEqual(ActivityType.Reply, activitiesResponse.Body.Data[0].ActivityType);
                });

            // clean up the reply, the comment and the topic
            HttpOperationResponse deleteReply = await client3.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth3);
            HttpOperationResponse deleteComment = await client2.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth2);
            HttpOperationResponse deleteTopic = await client3.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth3);

            // clean up the three users we created
            HttpOperationResponse deleteUserOperationResponse1 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse deleteUserOperationResponse2 = await client2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);
            HttpOperationResponse deleteUserOperationResponse3 = await client3.Users.DeleteUserWithHttpMessagesAsync(authorization: auth3);

            IList<ActivityView> activityList = activitiesResponse.Body.Data;
            Assert.AreEqual(2, activityList.Count);
            Assert.AreEqual(ActivityType.Reply, activityList[0].ActivityType);
            Assert.AreEqual(1, activityList[0].ActorUsers.Count);
            Assert.AreEqual(user3.UserHandle, activityList[0].ActorUsers[0].UserHandle);
            Assert.AreEqual(user2.UserHandle, activityList[0].ActedOnUser.UserHandle);
            Assert.AreEqual(1, activityList[0].TotalActions);
            Assert.AreEqual(true, activityList[0].Unread);
            Assert.AreEqual(ContentType.Comment, activityList[0].ActedOnContent.ContentType);
            Assert.AreEqual(commentHandle, activityList[0].ActedOnContent.ContentHandle);
        }

        /// <summary>
        /// Like activity test
        /// </summary>
        /// <returns>Task that will throw an exception if the test fails</returns>
        [TestMethod]
        public async Task FollowingTopicLikeActivityTest()
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

            // user1 creates a topic
            var postTopicResponse = await TestUtilities.PostGenericTopic(client1, auth1);
            string topicHandle = postTopicResponse.TopicHandle;

            // user2 requests to follow user1's topic
            PostFollowingTopicRequest followingTopicReq = new PostFollowingTopicRequest()
            {
                TopicHandle = topicHandle
            };
            HttpOperationResponse postFollowingResponse = await client2.MyFollowing.PostFollowingTopicWithHttpMessagesAsync(followingTopicReq, authorization: auth2);

            // user3 posts a like on user1's topic -- this causes a Like activity to be sent to users following the topic
            var postLikeOperationResponse = await client3.TopicLikes.PostLikeWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth3);

            HttpOperationResponse<FeedResponseActivityView> activitiesResponse = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    // because user2 follows the topic, user2 should receive an activity indicating that user3 liked a topic
                    activitiesResponse = await client2.MyFollowing.GetActivitiesWithHttpMessagesAsync(authorization: auth2);
                }, () =>
                {
                    // verify that user2 sees 1 activity, whose type is Like
                    Assert.AreEqual(1, activitiesResponse.Body.Data.Count);
                    Assert.AreEqual(ActivityType.Like, activitiesResponse.Body.Data[0].ActivityType);
                });

            // clean up the topic
            HttpOperationResponse deleteTopic = await client1.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth1);

            // clean up the two users we created
            HttpOperationResponse deleteUserOperationResponse1 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse deleteUserOperationResponse2 = await client2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);
            HttpOperationResponse deleteUserOperationResponse3 = await client3.Users.DeleteUserWithHttpMessagesAsync(authorization: auth3);

            IList<ActivityView> activityList = activitiesResponse.Body.Data;
            Assert.AreEqual(1, activityList.Count);
            Assert.AreEqual(ActivityType.Like, activityList[0].ActivityType);
            Assert.AreEqual(1, activityList[0].ActorUsers.Count);
            Assert.AreEqual(user3.UserHandle, activityList[0].ActorUsers[0].UserHandle);
            Assert.AreEqual(user1.UserHandle, activityList[0].ActedOnUser.UserHandle);
            Assert.AreEqual(1, activityList[0].TotalActions);
            Assert.AreEqual(true, activityList[0].Unread);
            Assert.AreEqual(ContentType.Topic, activityList[0].ActedOnContent.ContentType);
            Assert.AreEqual(topicHandle, activityList[0].ActedOnContent.ContentHandle);
        }

        /// <summary>
        /// User tries to unfollow a topic that they were not following.
        /// This test checks that the user gets back a 404 - not found.
        /// </summary>
        /// <returns>Task that will throw an exception if the test fails</returns>
        [TestMethod]
        public async Task UnfollowNotFollowedTopicTest()
        {
            // Create two users
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            PostUserResponse user1 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(user1.SessionToken);
            string auth2;
            try
            {
                PostUserResponse user2 = await TestUtilities.PostGenericUser(client);
                auth2 = AuthHelper.CreateSocialPlusAuth(user2.SessionToken);
            }
            catch (Exception e)
            {
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
                throw e;
            }

            // User 1 creates a topic
            string topicHandle;
            try
            {
                PostTopicResponse postTopicResponse = await TestUtilities.PostGenericTopic(client, auth1);
                topicHandle = postTopicResponse.TopicHandle;
            }
            catch (Exception e)
            {
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
                throw e;
            }

            // User 2 who is not following that topic, tries to unfollow the topic
            HttpOperationResponse deleteFollowingTopicResponse = await client.MyFollowing.DeleteFollowingTopicWithHttpMessagesAsync(topicHandle, auth2);

            // cleanup
            HttpOperationResponse deleteTopicResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth1);
            HttpOperationResponse deleteUserOperationResponse1 = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse deleteUserOperationResponse2 = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);

            // check results
            Assert.IsFalse(deleteFollowingTopicResponse.Response.IsSuccessStatusCode);
            Assert.AreEqual(deleteFollowingTopicResponse.Response.StatusCode, System.Net.HttpStatusCode.NotFound);
            Assert.IsTrue(deleteTopicResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse2.Response.IsSuccessStatusCode);
        }
    }
}
