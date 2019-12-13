// <copyright file="LikeTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;

    /// <summary>
    /// Tests related to the Like features
    /// </summary>
    [TestClass]
    public class LikeTests
    {
        /// <summary>
        /// Create a topic, like it, verify by doing a GetTopic, then delete the like
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task LikeTopicTest()
        {
            await this.LikeTopicHelper(false, null);
        }

        /// <summary>
        /// Create a topic, like it, verify by doing a GetTopic, then delete the like
        /// </summary>
        /// <param name="appPublished">flag to indicate that topic is appPublished</param>
        /// <param name="appHandle">app handle</param>
        /// <returns>Fail if an exception is hit</returns>
        public async Task LikeTopicHelper(bool appPublished, string appHandle)
        {
            // Set up initial login etc
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserResponse postUserResponse;
            string firstName = "R2D2";
            string lastName = "Robot";
            string bio = string.Empty;
            postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
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

            // create a topic
            string topicTitle = "Topic for Like Test";
            string topicText = "Verify the likes!";
            BlobType blobType = BlobType.Unknown;
            string blobHandle = "http://myBlobHandle/";
            string language = "en-US";
            string deepLink = "Like It!";
            string categories = "#likes";
            string friendlyName = "LT";
            string group = "mygroup";

            string topicHandle = string.Empty;
            var postTopicRequest = new PostTopicRequest() { Title = topicTitle, Text = topicText, BlobType = blobType, BlobHandle = blobHandle, Language = language, DeepLink = deepLink, Categories = categories, FriendlyName = friendlyName, Group = group };
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
                // extract topic handle from the response
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
            }
            else
            {
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

                Assert.Fail("Post topic failed.");
            }

            // Post a like on the Topic
            var postLikeOperationResponse = await client.TopicLikes.PostLikeWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (!postLikeOperationResponse.Response.IsSuccessStatusCode)
            {
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Like of topic failed.");
            }

            // get the topic and verify by Get Topic
            var getTopicOperationResponse = await client.Topics.GetTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (!getTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                await client.TopicLikes.DeleteLikeWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Get topic failed.");
            }

            // Delete the like
            var deleteLikeOperationResponse = await client.TopicLikes.DeleteLikeWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (!deleteLikeOperationResponse.Response.IsSuccessStatusCode)
            {
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Delete like failed.");
            }

            // Clean up first before verifying
            await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (appPublished)
            {
                ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
            }

            await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            var getTopicResponse = getTopicOperationResponse.Body;

            // Verify as the last step
            Assert.AreEqual(getTopicResponse.Liked, true);
            Assert.AreEqual(getTopicResponse.Text, topicText);
            Assert.AreEqual(getTopicResponse.Title, topicTitle);
            Assert.AreEqual(getTopicResponse.TopicHandle, topicHandle);
            Assert.AreEqual(getTopicResponse.TotalComments, 0);
            Assert.AreEqual(getTopicResponse.TotalLikes, 1);
        }

        /// <summary>
        /// Create a topic, and issue 100 likes on it from the same user in rapid succession
        /// </summary>
        /// <returns>Fail if any of the likes result in an HTTP error</returns>
        [TestMethod]
        public async Task RapidLikeTopicTest()
        {
            await this.RapidLikeTopicHelper(false);
        }

        /// <summary>
        /// Create a topic, and issue 100 likes and unlikes on it from the same user in rapid succession
        /// </summary>
        /// <returns>Fail if any of the likes or unlikes result in an HTTP error</returns>
        [TestMethod]
        public async Task RapidLikeUnlikeTopicTest()
        {
            await this.RapidLikeTopicHelper(true);
        }

        /// <summary>
        /// Create a topic, like it, verify by doing GetLikes, delete it, and verify deletion
        /// by doing another GetLikes.
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task GetLikesTopicTest()
        {
            await this.GetLikesTopicTestHelper(false, null);
        }

        /// <summary>
        /// Helper route to perform the main actions of the test.
        /// Create a topic, like it, verify by doing GetLikes, delete it, and verify deletion
        /// by doing another GetLikes.
        /// </summary>
        /// <param name="appPublished">flag to indicate if the topic is app published</param>
        /// <param name="appHandle">app handle</param>
        /// <returns>Fail if an exception is hit</returns>
        public async Task GetLikesTopicTestHelper(bool appPublished, string appHandle)
        {
            // Set up initial login etc
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserResponse postUserResponse;
            string firstName = "C3P0";
            string lastName = "MrRobot";
            string bio = string.Empty;
            postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
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

            string topicTitle = "Topic for Get Like ";
            string topicText = "Verify the likes!";
            BlobType blobType = BlobType.Image;
            string blobHandle = "http://myBlobHandle/";
            string language = "en-US";
            string deepLink = "Like It!";
            string categories = "#likes";
            string friendlyName = "LT";
            string group = "mygroup";

            string topicHandle = string.Empty;
            var postTopicRequest = new PostTopicRequest() { Title = topicTitle, Text = topicText, BlobType = blobType, BlobHandle = blobHandle, Language = language, DeepLink = deepLink, Categories = categories, FriendlyName = friendlyName, Group = group };
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
                // extract topic handle from the response
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
            }
            else
            {
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Post topic failed.");
            }

            // Post a like on the Topic
            var postLikeOperationResponse = await client.TopicLikes.PostLikeWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (!postLikeOperationResponse.Response.IsSuccessStatusCode)
            {
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Like of topic failed.");
            }

            // use GetLikes to verify the post like
            var getLikesOperationResponse = await client.TopicLikes.GetLikesWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (!getLikesOperationResponse.Response.IsSuccessStatusCode)
            {
                await client.TopicLikes.DeleteLikeWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Get likes failed.");
            }

            // Delete the like
            var deleteLikeOperationResponse = await client.TopicLikes.DeleteLikeWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (!deleteLikeOperationResponse.Response.IsSuccessStatusCode)
            {
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Delete like failed.");
            }

            // use GetLikes to verify the deleted like
            var getLikesOperationResponse2 = await client.TopicLikes.GetLikesWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (!getLikesOperationResponse2.Response.IsSuccessStatusCode)
            {
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Get likes failed.");
            }

            // Clean up first before verifying
            await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (appPublished)
            {
                ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
            }

            await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            var getLikesResponse = getLikesOperationResponse.Body;
            var getLikesResponse2 = getLikesOperationResponse2.Body;

            // Verify after cleaning up so don't leave junk behind if fails
            Assert.AreEqual(getLikesResponse.Data.Count, 1);
            Assert.AreEqual(getLikesResponse.Data[0].UserHandle, postUserResponse.UserHandle);
            Assert.AreEqual(getLikesResponse.Data[0].FirstName, firstName);
            Assert.AreEqual(getLikesResponse.Data[0].LastName, lastName);
            Assert.AreEqual(getLikesResponse2.Data.Count, 0);
        }

        /// <summary>
        /// Create a topic, comment and reply.  Like the comment and the reply. Then delete
        /// both the comment like and the reply like.
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task LikeCommentReplyDeleteTest()
        {
            await this.LikeCommentReplyDeleteTestHelper(false, null);
        }

        /// <summary>
        /// Helper routine to perform the main actions of the test
        /// Create a topic, comment and reply.  Like the comment and the reply. Then delete
        /// both the comment like and the reply like.
        /// </summary>
        /// <param name="appPublished">flag to indicate if topic is app published</param>
        /// <param name="appHandle">app handle</param>
        /// <returns>Fail if an exception is hit</returns>
        public async Task LikeCommentReplyDeleteTestHelper(bool appPublished, string appHandle)
        {
            // Set up initial login etc
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserResponse postUserResponse;
            string firstName = "R2D2";
            string lastName = "Robot";
            string bio = string.Empty;
            postUserResponse = await TestUtilities.DoLogin(client, firstName, lastName, bio);
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

            string topicTitle = "Topic for Like Reply Test";
            string topicText = "Verify the Like Reply!";
            BlobType blobType = BlobType.Unknown;
            string blobHandle = "http://myBlobHandle/";
            string language = "en-US";
            string deepLink = "Like It!";
            string categories = "#likes; #reply";
            string friendlyName = "LT";
            string group = "mygroup";

            string topicHandle = string.Empty;
            var postTopicRequest = new PostTopicRequest() { Title = topicTitle, Text = topicText, BlobType = blobType, BlobHandle = blobHandle, Language = language, DeepLink = deepLink, Categories = categories, FriendlyName = friendlyName, Group = group };
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
                // extract topic handle from the response
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
            }
            else
            {
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Post topic failed.");
            }

            // Post a comment to topic
            string commentHandle = string.Empty;
            var postCommentRequest = new PostCommentRequest() { Text = "My First Comment!", Language = "en-US" };
            var postCommentOperationResponse = await client.TopicComments.PostCommentWithHttpMessagesAsync(topicHandle: topicHandle, request: postCommentRequest, authorization: auth);
            if (postCommentOperationResponse.Response.IsSuccessStatusCode)
            {
                commentHandle = postCommentOperationResponse.Body.CommentHandle;
            }
            else
            {
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Post comment failed.");
            }

            // Post a reply to the comment to topic
            string replyHandle = string.Empty;
            var postReplyRequest = new PostReplyRequest() { Text = "My First Reply", Language = "en-US" };
            var postReplyOperationResponse = await client.CommentReplies.PostReplyWithHttpMessagesAsync(commentHandle, request: postReplyRequest, authorization: auth);
            if (postReplyOperationResponse.Response.IsSuccessStatusCode)
            {
                replyHandle = postReplyOperationResponse.Body.ReplyHandle;
            }
            else
            {
                await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Post reply failed.");
            }

            // Post a like on the Comment
            var postLikeOperationResponse = await client.CommentLikes.PostLikeWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
            if (!postLikeOperationResponse.Response.IsSuccessStatusCode)
            {
                await client.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
                await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Post like on comment failed.");
            }

            // Post a like on the Reply
            var postLikeOperationResponse2 = await client.ReplyLikes.PostLikeWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
            if (!postLikeOperationResponse2.Response.IsSuccessStatusCode)
            {
                await client.CommentLikes.DeleteLikeWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
                await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Post like on reply failed.");
            }

            // Get topic to later verify
            var getTopicOperationResponse = await client.Topics.GetTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (!getTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                await client.ReplyLikes.DeleteLikeWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
                await client.CommentLikes.DeleteLikeWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
                await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Get topic failed.");
            }

            // Get comment to later verify the like is there
            var getCommentOperationResponse = await client.Comments.GetCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
            if (!getCommentOperationResponse.Response.IsSuccessStatusCode)
            {
                await client.ReplyLikes.DeleteLikeWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
                await client.CommentLikes.DeleteLikeWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
                await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Get comment failed.");
            }

            // Get reply to later verify the like is there
            var getReplyOperationResponse = await client.Replies.GetReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
            if (!getReplyOperationResponse.Response.IsSuccessStatusCode)
            {
                await client.ReplyLikes.DeleteLikeWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
                await client.CommentLikes.DeleteLikeWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
                await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Get reply failed.");
            }

            var deleteReplyLikeOperationResult = await client.ReplyLikes.DeleteLikeWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
            if (!deleteReplyLikeOperationResult.Response.IsSuccessStatusCode)
            {
                await client.CommentLikes.DeleteLikeWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
                await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Delete reply like failed.");
            }

            var deleteCommentLikeOperationResult = await client.CommentLikes.DeleteLikeWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
            if (!deleteCommentLikeOperationResult.Response.IsSuccessStatusCode)
            {
                await client.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
                await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Delete comment like failed.");
            }

            // Get comment a second time to verify the like is gone
            var getCommentOperationResponse2 = await client.Comments.GetCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
            if (!getCommentOperationResponse2.Response.IsSuccessStatusCode)
            {
                await client.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
                await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Get comment failed.");
            }

            // Get reply a second time to verify the like is gone
            var getReplyOperationResponse2 = await client.Replies.GetReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
            if (!getReplyOperationResponse2.Response.IsSuccessStatusCode)
            {
                await client.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
                await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
                await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (appPublished)
                {
                    ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
                }

                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
                Assert.Fail("Get reply failed.");
            }

            // perform standard cleanup of the operations before checking assertions
            await client.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);
            await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);
            await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            if (appPublished)
            {
                ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, userHandle);
            }

            await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            var getTopicResponse = getTopicOperationResponse.Body;
            var getCommentResponse = getCommentOperationResponse.Body;
            var getReplyResponse = getReplyOperationResponse.Body;
            var getCommentResponse2 = getCommentOperationResponse2.Body;
            var getReplyResponse2 = getReplyOperationResponse2.Body;

            // Verify topic info
            Assert.AreEqual(getTopicResponse.Liked, false);
            Assert.AreEqual(getTopicResponse.Text, topicText);
            Assert.AreEqual(getTopicResponse.BlobType, BlobType.Unknown);
            Assert.AreEqual(getTopicResponse.BlobHandle, blobHandle);
            Assert.AreEqual(getTopicResponse.Language, language);
            Assert.AreEqual(getTopicResponse.DeepLink, deepLink);
            Assert.AreEqual(getTopicResponse.Categories, categories);
            Assert.AreEqual(getTopicResponse.FriendlyName, friendlyName);
            Assert.AreEqual(getTopicResponse.Group, group);
            Assert.AreEqual(getTopicResponse.TotalComments, 1);
            Assert.AreEqual(getTopicResponse.TotalLikes, 0);

            // Verify comment info
            Assert.AreEqual(getCommentResponse.Liked, true);
            Assert.AreEqual(getCommentResponse.Text, "My First Comment!");
            Assert.AreEqual(getCommentResponse.Language, "en-US");
            Assert.AreEqual(getCommentResponse.TotalReplies, 1);
            Assert.AreEqual(getCommentResponse.TotalLikes, 1);

            // Verify reply info
            Assert.AreEqual(getReplyResponse.Liked, true);
            Assert.AreEqual(getReplyResponse.Text, "My First Reply");
            Assert.AreEqual(getReplyResponse.Language, "en-US");
            Assert.AreEqual(getReplyResponse.TotalLikes, 1);

            // Verify comment info
            Assert.AreEqual(getCommentResponse2.Liked, false);
            Assert.AreEqual(getCommentResponse2.Text, "My First Comment!");
            Assert.AreEqual(getCommentResponse2.Language, "en-US");
            Assert.AreEqual(getCommentResponse2.TotalReplies, 1);
            Assert.AreEqual(getCommentResponse2.TotalLikes, 0);

            // Verify reply info
            Assert.AreEqual(getReplyResponse2.Liked, false);
            Assert.AreEqual(getReplyResponse2.Text, "My First Reply");
            Assert.AreEqual(getReplyResponse2.Language, "en-US");
            Assert.AreEqual(getReplyResponse2.TotalLikes, 0);
        }

        /// <summary>
        /// Create a topic, and issue 100 likes and unlikes on it from the same user in rapid succession
        /// </summary>
        /// <param name="unlike">if true, will also issue unlikes</param>
        /// <returns>Fail if any of the likes or unlikes result in an HTTP error</returns>
        private async Task RapidLikeTopicHelper(bool unlike)
        {
            int numLikesToIssue = 100;
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // create a user
            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client, "Rapid", "Like", "Rapid Like Topic Test");
            string auth = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);
            string userHandle = postUserResponse.UserHandle;

            // create a topic
            PostTopicRequest postTopicRequest = new PostTopicRequest()
            {
                Title = "Rapid like topic test",
                Text = "Rapid like topic test",
                BlobType = BlobType.Unknown,
                BlobHandle = string.Empty,
                Language = string.Empty,
                DeepLink = string.Empty,
                Categories = string.Empty,
                FriendlyName = string.Empty,
                Group = string.Empty,
                PublisherType = PublisherType.User
            };
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);
            if (!postTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);
            }

            Assert.IsTrue(postTopicOperationResponse.Response.IsSuccessStatusCode);
            string topicHandle = postTopicOperationResponse.Body.TopicHandle;

            // issue repeated likes and unlikes
            HttpOperationResponse[] postLikeOperationResponses = new HttpOperationResponse[numLikesToIssue];
            HttpOperationResponse[] postUnLikeOperationResponses = new HttpOperationResponse[numLikesToIssue];
            for (int i = 0; i < numLikesToIssue; i++)
            {
                postLikeOperationResponses[i] = await client.TopicLikes.PostLikeWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                if (unlike)
                {
                    postUnLikeOperationResponses[i] = await client.TopicLikes.DeleteLikeWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
                }
            }

            // clean up
            HttpOperationResponse deleteTopicOperationResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth);
            HttpOperationResponse deleteUserOperationResponse = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // check for errors
            for (int i = 0; i < numLikesToIssue; i++)
            {
                Assert.IsTrue(postLikeOperationResponses[i].Response.IsSuccessStatusCode);
                if (unlike)
                {
                    Assert.IsTrue(postUnLikeOperationResponses[i].Response.IsSuccessStatusCode);
                }
            }

            Assert.IsTrue(deleteTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);
        }
    }
}
