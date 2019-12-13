// <copyright file="ActivityTests.cs" company="Microsoft">
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
    /// Basic functional end to end tests for Activity APIs
    /// </summary>
    [TestClass]
    public class ActivityTests
    {
        /// <summary>
        /// Following activity test
        /// </summary>
        /// <returns>Task that will throw an exception if the test fails</returns>
        [TestMethod]
        public async Task FollowingActivityTest()
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

            // user1 requests to follow user2
            PostFollowingUserRequest followingUserReq1 = new PostFollowingUserRequest()
            {
                UserHandle = user2.UserHandle
            };
            HttpOperationResponse postFollowingResponse1 = await client1.MyFollowing.PostFollowingUserWithHttpMessagesAsync(followingUserReq1, authorization: auth1);

            // user2 requests to follow user3
            PostFollowingUserRequest followingUserReq2 = new PostFollowingUserRequest()
            {
                UserHandle = user3.UserHandle
            };
            HttpOperationResponse postFollowingResponse2 = await client2.MyFollowing.PostFollowingUserWithHttpMessagesAsync(followingUserReq2, authorization: auth2);

            HttpOperationResponse<FeedResponseActivityView> activitiesResponse = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    // because user1 follows user2, user1 should receive an activity indicating that user2 follows user3
                    activitiesResponse = await client1.MyFollowing.GetActivitiesWithHttpMessagesAsync(authorization: auth1);
                }, () =>
                {
                    // verify that user1 sees 1 activity, whose type is Following
                    Assert.AreEqual(1, activitiesResponse.Body.Data.Count);
                    Assert.AreEqual(ActivityType.Following, activitiesResponse.Body.Data[0].ActivityType);
                });

            // clean up the three users we created
            HttpOperationResponse deleteUserOperationResponse1 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse deleteUserOperationResponse2 = await client2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);
            HttpOperationResponse deleteUserOperationResponse3 = await client3.Users.DeleteUserWithHttpMessagesAsync(authorization: auth3);

            IList<ActivityView> activityList = activitiesResponse.Body.Data;
            Assert.AreEqual(1, activityList.Count);
            Assert.AreEqual(ActivityType.Following, activityList[0].ActivityType);
            Assert.AreEqual(1, activityList[0].ActorUsers.Count);
            Assert.AreEqual(user2.UserHandle, activityList[0].ActorUsers[0].UserHandle);
            Assert.AreEqual(user3.UserHandle, activityList[0].ActedOnUser.UserHandle);
            Assert.AreEqual(1, activityList[0].TotalActions);
            Assert.AreEqual(true, activityList[0].Unread);
        }

        /// <summary>
        /// Comment activity test
        /// </summary>
        /// <returns>Task that will throw an exception if the test fails</returns>
        [TestMethod]
        public async Task CommentActivityTest()
        {
            // Setup two users
            SocialPlusClient client1 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            SocialPlusClient client2 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user1 = await TestUtilities.PostGenericUser(client1);
            var user2 = await TestUtilities.PostGenericUser(client2);
            var auth1 = AuthHelper.CreateSocialPlusAuth(user1.SessionToken);
            var auth2 = AuthHelper.CreateSocialPlusAuth(user2.SessionToken);

            // user1 requests to follow user2
            PostFollowingUserRequest followingUserReq1 = new PostFollowingUserRequest()
            {
                UserHandle = user2.UserHandle
            };
            HttpOperationResponse postFollowingResponse = await client1.MyFollowing.PostFollowingUserWithHttpMessagesAsync(followingUserReq1, authorization: auth1);

            // user1 creates a topic
            var postTopicResponse = await TestUtilities.PostGenericTopic(client1, auth1);
            string topicHandle = postTopicResponse.TopicHandle;

            // user2 posts a comment on the topic -- this causes a Comment activity to be sent to users following user2
            var postCommentResponse = await TestUtilities.PostGenericComment(client2, auth2, topicHandle);
            string commentHandle = postCommentResponse.CommentHandle;

            HttpOperationResponse<FeedResponseActivityView> activitiesResponse = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    // because user1 follows user2, user1 should receive an activity indicating that user2 commented on a topic
                    activitiesResponse = await client1.MyFollowing.GetActivitiesWithHttpMessagesAsync(authorization: auth1);
                }, () =>
                {
                    // verify that user1 sees 1 activity, whose type is Comment
                    Assert.AreEqual(1, activitiesResponse.Body.Data.Count);
                    Assert.AreEqual(ActivityType.Comment, activitiesResponse.Body.Data[0].ActivityType);
                });

            // clean up the comment and the topic
            HttpOperationResponse deleteComment = await client2.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth2);
            HttpOperationResponse deleteTopic = await client1.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth1);

            // clean up the two users we created
            HttpOperationResponse deleteUserOperationResponse1 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse deleteUserOperationResponse2 = await client2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);

            IList<ActivityView> activityList = activitiesResponse.Body.Data;
            Assert.AreEqual(1, activityList.Count);
            Assert.AreEqual(ActivityType.Comment, activityList[0].ActivityType);
            Assert.AreEqual(1, activityList[0].ActorUsers.Count);
            Assert.AreEqual(user2.UserHandle, activityList[0].ActorUsers[0].UserHandle);
            Assert.AreEqual(user1.UserHandle, activityList[0].ActedOnUser.UserHandle);
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
        public async Task ReplyActivityTest()
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

            // user2 requests to follow user3
            PostFollowingUserRequest followingUserReq = new PostFollowingUserRequest()
            {
                UserHandle = user3.UserHandle
            };
            HttpOperationResponse postFollowingResponse = await client2.MyFollowing.PostFollowingUserWithHttpMessagesAsync(followingUserReq, authorization: auth2);

            // user1 creates a topic
            var postTopicResponse = await TestUtilities.PostGenericTopic(client1, auth1);
            string topicHandle = postTopicResponse.TopicHandle;

            // user2 posts a comment on the topic -- this causes a Comment activity to be sent to users following user2
            var postCommentResponse = await TestUtilities.PostGenericComment(client2, auth2, topicHandle);
            string commentHandle = postCommentResponse.CommentHandle;

            // user3 replies to user2's comment
            var postReplyOperationResponse = await TestUtilities.PostGenericReply(client3, auth3, commentHandle);
            string replyHandle = postReplyOperationResponse.ReplyHandle;

            HttpOperationResponse<FeedResponseActivityView> activitiesResponse = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    // because user2 follows user3, user2 should receive an activity indicating that user3 replied to user2's comment
                    activitiesResponse = await client2.MyFollowing.GetActivitiesWithHttpMessagesAsync(authorization: auth2);
                }, () =>
                {
                    // verify that user1 sees 1 activity, whose type is Following
                    Assert.AreEqual(1, activitiesResponse.Body.Data.Count);
                    Assert.AreEqual(ActivityType.Reply, activitiesResponse.Body.Data[0].ActivityType);
                });

            // clean up the reply, the comment and the topic
            HttpOperationResponse deleteReply = await client3.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth3);
            HttpOperationResponse deleteComment = await client2.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth2);
            HttpOperationResponse deleteTopic = await client1.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth1);

            // clean up the three users we created
            HttpOperationResponse deleteUserOperationResponse1 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse deleteUserOperationResponse2 = await client2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);
            HttpOperationResponse deleteUserOperationResponse3 = await client3.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);

            IList<ActivityView> activityList = activitiesResponse.Body.Data;
            Assert.AreEqual(1, activityList.Count);
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
        public async Task LikeActivityTest()
        {
            // Setup two users
            SocialPlusClient client1 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            SocialPlusClient client2 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user1 = await TestUtilities.PostGenericUser(client1);
            var user2 = await TestUtilities.PostGenericUser(client2);
            var auth1 = AuthHelper.CreateSocialPlusAuth(user1.SessionToken);
            var auth2 = AuthHelper.CreateSocialPlusAuth(user2.SessionToken);

            // user1 requests to follow user2
            // user1 requests to follow user2
            PostFollowingUserRequest followingUserReq1 = new PostFollowingUserRequest()
            {
                UserHandle = user2.UserHandle
            };
            HttpOperationResponse postFollowingResponse = await client1.MyFollowing.PostFollowingUserWithHttpMessagesAsync(followingUserReq1, authorization: auth1);

            // user1 creates a topic
            var postTopicResponse = await TestUtilities.PostGenericTopic(client1, auth1);
            string topicHandle = postTopicResponse.TopicHandle;

            // user2 posts a like on user1's topic -- this causes a Like activity to be sent to users following user2
            var postLikeOperationResponse = await client2.TopicLikes.PostLikeWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth2);

            HttpOperationResponse<FeedResponseActivityView> activitiesResponse = null;
            await TestUtilities.AutoRetryServiceBusHelper(
                async () =>
                {
                    // because user1 follows user2, user1 should receive an activity indicating that user2 liked a topic
                    activitiesResponse = await client1.MyFollowing.GetActivitiesWithHttpMessagesAsync(authorization: auth1);
                }, () =>
                {
                    // verify that user1 sees 1 activity, whose type is Like
                    Assert.AreEqual(1, activitiesResponse.Body.Data.Count);
                    Assert.AreEqual(ActivityType.Like, activitiesResponse.Body.Data[0].ActivityType);
                });

            // clean up: delete the topic
            HttpOperationResponse deleteTopic = await client1.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth1);

            // clean up the two users we created
            HttpOperationResponse deleteUserOperationResponse1 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse deleteUserOperationResponse2 = await client2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);

            IList<ActivityView> activityList = activitiesResponse.Body.Data;
            Assert.AreEqual(1, activityList.Count);
            Assert.AreEqual(ActivityType.Like, activityList[0].ActivityType);
            Assert.AreEqual(1, activityList[0].ActorUsers.Count);
            Assert.AreEqual(user2.UserHandle, activityList[0].ActorUsers[0].UserHandle);
            Assert.AreEqual(user1.UserHandle, activityList[0].ActedOnUser.UserHandle);
            Assert.AreEqual(1, activityList[0].TotalActions);
            Assert.AreEqual(true, activityList[0].Unread);
            Assert.AreEqual(ContentType.Topic, activityList[0].ActedOnContent.ContentType);
            Assert.AreEqual(topicHandle, activityList[0].ActedOnContent.ContentHandle);
        }
    }
}
