// <copyright file="RepliesTests.cs" company="Microsoft">
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
    /// All basic end to end tests for the Reply feature
    /// </summary>
    [TestClass]
    public class RepliesTests
    {
        /// <summary>
        /// This test performs the following operations in order:
        /// PostUser
        /// PostTopic
        /// PostComment
        /// GetComment
        /// PostReply
        /// GetComment
        /// GetReply
        /// DeleteReply
        /// DeleteComment
        /// DeleteTopic
        /// DeleteUser
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task PostGetRepliesTest()
        {
            // Set up initial login etc
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user = await TestUtilities.PostGenericUser(client);
            string auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);

            // throw an error if null
            Assert.IsFalse(string.IsNullOrEmpty(auth), "User creation failed");

            HttpOperationResponse<object> deleteUserOperationResponse = null;

            // create topic, cleanup and throw an error if null
            var postTopicResponse = await TestUtilities.PostGenericTopic(client, auth);
            string topicHandle = postTopicResponse.TopicHandle;
            HttpOperationResponse<object> deleteTopicResponse = null;
            if (string.IsNullOrWhiteSpace(topicHandle))
            {
                deleteUserOperationResponse = await TestUtilities.DeleteUser(client, auth);
                Assert.Fail("Topic posting failed, or handle was invalid.");
            }

            // Post a comment to Topic
            var postCommentResponse = await TestUtilities.PostGenericComment(client, auth, topicHandle);
            string commentHandle = postCommentResponse.CommentHandle;
            HttpOperationResponse<object> deleteCommentResponse = null;
            if (string.IsNullOrWhiteSpace(commentHandle))
            {
                deleteTopicResponse = await TestUtilities.DeleteTopic(client: client, topicHandle: topicHandle, authorization: auth);
                deleteUserOperationResponse = await TestUtilities.DeleteUser(client: client, authorization: auth);
                Assert.Fail("Comment posting failed, or handle was invalid.");
            }

            // Get comment before reply
            HttpOperationResponse<CommentView> getCommentResponse = await client.Comments.GetCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);

            // Post a Reply
            string replyString = "Hail to the Queen!";
            string replyLanguage = "EN-GB";
            PostReplyRequest postReplyRequest = new PostReplyRequest(text: replyString, language: replyLanguage);
            HttpOperationResponse<PostReplyResponse> postReplyResponse = await client.CommentReplies.PostReplyWithHttpMessagesAsync(commentHandle: commentHandle, request: postReplyRequest, authorization: auth);

            // Get Comment after reply
            HttpOperationResponse<CommentView> getCommentResponse2 = await client.Comments.GetCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);

            // cleanup and throw an error if null
            if (postReplyResponse == null || !postReplyResponse.Response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(postReplyResponse.Body.ReplyHandle))
            {
                deleteCommentResponse = await TestUtilities.DeleteComment(client: client, commentHandle: commentHandle, authorization: auth);
                deleteTopicResponse = await TestUtilities.DeleteTopic(client: client, topicHandle: topicHandle, authorization: auth);
                deleteUserOperationResponse = await TestUtilities.DeleteUser(client: client, authorization: auth);
                Assert.Fail("Reply posting failed, or handle was invalid.");
            }

            // Get Reply info
            string replyHandle = postReplyResponse.Body.ReplyHandle;
            HttpOperationResponse<ReplyView> getReplyResponse = await client.Replies.GetReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: auth);

            // Clean up first before verifying
            HttpOperationResponse<object> deleteReplyResponse = await TestUtilities.DeleteReply(client: client, replyHandle: replyHandle, authorization: auth);
            deleteCommentResponse = await TestUtilities.DeleteComment(client: client, commentHandle: commentHandle, authorization: auth);
            deleteTopicResponse = await TestUtilities.DeleteTopic(client: client, topicHandle: topicHandle, authorization: auth);
            deleteUserOperationResponse = await TestUtilities.DeleteUser(client: client, authorization: auth);

            // Verify after cleaning up so don't leave junk behind if fails

            // Double check comment to show it counts right
            Assert.AreEqual(getCommentResponse.Body.TotalReplies, 0);  // before
            Assert.AreEqual(getCommentResponse2.Body.TotalReplies, 1); // after reply

            // Verify reply info
            Assert.AreEqual(getReplyResponse.Body.CommentHandle, commentHandle);
            Assert.AreEqual(getReplyResponse.Body.ContentStatus, ContentStatus.Active);
            Assert.AreEqual(getReplyResponse.Body.Language, replyLanguage);
            Assert.AreEqual(getReplyResponse.Body.Liked, false);
            Assert.AreEqual(getReplyResponse.Body.ReplyHandle, postReplyResponse.Body.ReplyHandle);
            Assert.AreEqual(getReplyResponse.Body.Text, replyString);
            Assert.AreEqual(getReplyResponse.Body.TopicHandle, topicHandle);
            Assert.AreEqual(getReplyResponse.Body.TotalLikes, 0);

            // Verify deletion
            Assert.IsTrue(deleteReplyResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteCommentResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// This test performs the following operations in order:
        /// PostUser
        /// PostTopic
        /// PostComment
        /// 3x PostReply
        /// 3x GetReply
        /// GetComment
        /// 3x DeleteReply
        /// DeleteComment
        /// DeleteTopic
        /// DeleteUser
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task GetRepliesForACommentTest()
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user = await TestUtilities.PostGenericUser(client);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);
            HttpOperationResponse<object> deleteUserOperationResponse = null;

            // create topic, cleanup and throw an error if null
            var postTopicResponse = await TestUtilities.PostGenericTopic(client, auth);
            string topicHandle = postTopicResponse.TopicHandle;
            HttpOperationResponse<object> deleteTopicResponse = null;
            if (string.IsNullOrWhiteSpace(topicHandle))
            {
                deleteUserOperationResponse = await TestUtilities.DeleteUser(client, auth);
                Assert.Fail("Topic posting failed, or handle was invalid.");
            }

            // Post a comment
            var postCommentResponse = await TestUtilities.PostGenericComment(client, auth, topicHandle);
            string commentHandle = postCommentResponse.CommentHandle;
            HttpOperationResponse<object> deleteCommentResponse = null;
            if (string.IsNullOrWhiteSpace(commentHandle))
            {
                deleteTopicResponse = await TestUtilities.DeleteTopic(client: client, topicHandle: topicHandle, authorization: auth);
                deleteUserOperationResponse = await TestUtilities.DeleteUser(client: client, authorization: auth);
                Assert.Fail("Comment posting failed, or handle was invalid.");
            }

            string replyString = "Ciao!!!!!!";
            string replyLanguage = "ItalianLanguage";
            PostReplyRequest postReplyRequest = new PostReplyRequest(text: replyString, language: replyLanguage);
            HttpOperationResponse<PostReplyResponse> postReplyResponse = await client.CommentReplies.PostReplyWithHttpMessagesAsync(commentHandle: commentHandle, request: postReplyRequest, authorization: auth);

            string replyString2 = "Guten Tag!!!!!!";
            string replyLanguage2 = "GermanLanguage";
            PostReplyRequest postReplyRequest2 = new PostReplyRequest(text: replyString2, language: replyLanguage2);
            HttpOperationResponse<PostReplyResponse> postReplyResponse2 = await client.CommentReplies.PostReplyWithHttpMessagesAsync(commentHandle: commentHandle, request: postReplyRequest2, authorization: auth);

            string replyString3 = "Yee Ha!!!!!!";
            string replyLanguage3 = "CowboyLanguage";
            PostReplyRequest postReplyRequest3 = new PostReplyRequest(text: replyString3, language: replyLanguage3);
            HttpOperationResponse<PostReplyResponse> postReplyResponse3 = await client.CommentReplies.PostReplyWithHttpMessagesAsync(commentHandle: commentHandle, request: postReplyRequest3, authorization: auth);

            // Get Replies for only 1
            HttpOperationResponse<FeedResponseReplyView> getReplyResponse = await client.CommentReplies.GetRepliesWithHttpMessagesAsync(commentHandle: commentHandle, cursor: null, limit: 1, authorization: auth);

            // Get 2 Replies using cursor from first one
            HttpOperationResponse<FeedResponseReplyView> getReplyResponse2 = await client.CommentReplies.GetRepliesWithHttpMessagesAsync(commentHandle: commentHandle, cursor: getReplyResponse.Body.Cursor, limit: 2, authorization: auth);

            // Get all 3 replies
            HttpOperationResponse<FeedResponseReplyView> getReplyResponse3 = await client.CommentReplies.GetRepliesWithHttpMessagesAsync(commentHandle: commentHandle, cursor: null, limit: 10, authorization: auth);

            // Get comment afterwards
            HttpOperationResponse<CommentView> getCommentResponse = await client.Comments.GetCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: auth);

            // Clean up first before verifying
            // delete replies - the cleanup method checks replyHandle for null
            HttpOperationResponse<object> deleteReplyResponse = await TestUtilities.DeleteReply(client: client, postReplyResponse: postReplyResponse, authorization: auth);
            HttpOperationResponse<object> deleteReplyResponse2 = await TestUtilities.DeleteReply(client: client, postReplyResponse: postReplyResponse2, authorization: auth);
            HttpOperationResponse<object> deleteReplyResponse3 = await TestUtilities.DeleteReply(client: client, postReplyResponse: postReplyResponse3, authorization: auth);

            deleteCommentResponse = await TestUtilities.DeleteComment(client: client, commentHandle: commentHandle, authorization: auth);
            deleteTopicResponse = await TestUtilities.DeleteTopic(client: client, topicHandle: topicHandle, authorization: auth);
            deleteUserOperationResponse = await TestUtilities.DeleteUser(client: client, authorization: auth);

            // Verify after cleaning up so don't leave junk behind if fails

            // Double check Comment
            Assert.AreEqual(getCommentResponse.Body.Liked, false);
            Assert.AreEqual(getCommentResponse.Body.TopicHandle, topicHandle);
            Assert.AreEqual(getCommentResponse.Body.TotalReplies, 3);
            Assert.AreEqual(getCommentResponse.Body.TotalLikes, 0);

            // Verify Reply info from first Reply
            Assert.AreEqual(getReplyResponse.Body.Data[0].Liked, false);
            Assert.AreEqual(getReplyResponse.Body.Data[0].CommentHandle, commentHandle);
            Assert.AreEqual(getReplyResponse.Body.Data[0].TotalLikes, 0);
            Assert.AreEqual(getReplyResponse.Body.Data[0].ContentStatus, ContentStatus.Active);

            Assert.AreEqual(getReplyResponse.Body.Data[0].Language, replyLanguage3);
            Assert.AreEqual(getReplyResponse.Body.Data[0].Text, replyString3);
            Assert.AreEqual(getReplyResponse.Body.Data[0].TopicHandle, topicHandle);
            Assert.AreEqual(getReplyResponse.Body.Data[0].User.UserHandle, user.UserHandle);
            Assert.AreEqual(getReplyResponse.Body.Data[0].User.Visibility, Visibility.Public);

            // Verify Reply from the Second Get (using cursor from first one)
            Assert.AreEqual(getReplyResponse2.Body.Data[0].CommentHandle, commentHandle);
            Assert.AreEqual(getReplyResponse2.Body.Data[0].Language, replyLanguage2);
            Assert.AreEqual(getReplyResponse2.Body.Data[0].Text, replyString2);
            Assert.AreEqual(getReplyResponse2.Body.Data[0].TopicHandle, topicHandle);
            Assert.AreEqual(getReplyResponse2.Body.Data[1].CommentHandle, commentHandle);
            Assert.AreEqual(getReplyResponse2.Body.Data[1].Language, replyLanguage);
            Assert.AreEqual(getReplyResponse2.Body.Data[1].Text, replyString);
            Assert.AreEqual(getReplyResponse2.Body.Data[1].TopicHandle, topicHandle);

            // Verify Reply from Third Get (getting all comments)
            Assert.AreEqual(getReplyResponse3.Body.Data[0].CommentHandle, commentHandle);
            Assert.AreEqual(getReplyResponse3.Body.Data[0].Language, replyLanguage3);
            Assert.AreEqual(getReplyResponse3.Body.Data[0].Text, replyString3);
            Assert.AreEqual(getReplyResponse3.Body.Data[0].TopicHandle, topicHandle);
            Assert.AreEqual(getReplyResponse3.Body.Data[1].CommentHandle, commentHandle);
            Assert.AreEqual(getReplyResponse3.Body.Data[1].Language, replyLanguage2);
            Assert.AreEqual(getReplyResponse3.Body.Data[1].Text, replyString2);
            Assert.AreEqual(getReplyResponse3.Body.Data[1].TopicHandle, topicHandle);
            Assert.AreEqual(getReplyResponse3.Body.Data[2].CommentHandle, commentHandle);
            Assert.AreEqual(getReplyResponse3.Body.Data[2].Language, replyLanguage);
            Assert.AreEqual(getReplyResponse3.Body.Data[2].Text, replyString);
            Assert.AreEqual(getReplyResponse3.Body.Data[2].TopicHandle, topicHandle);

            // Verify deletion
            Assert.IsTrue(deleteReplyResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteReplyResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteReplyResponse3.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteCommentResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// This test performs the following operations in order:
        /// PostUser
        /// PostTopic
        /// PostComment
        /// PostReply
        /// GetComment
        /// GetReplies
        /// DeleteReply
        /// GetComment
        /// GetReplies
        /// DeleteComment
        /// DeleteTopic
        /// DeleteUser
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task DeleteRepliesTest()
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user = await TestUtilities.PostGenericUser(client);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);
            HttpOperationResponse<object> deleteUserOperationResponse = null;

            // create topic, cleanup and throw an error if null
            var postTopicResponse = await TestUtilities.PostGenericTopic(client, auth);
            string topicHandle = postTopicResponse.TopicHandle;
            HttpOperationResponse<object> deleteTopicResponse = null;
            if (string.IsNullOrWhiteSpace(topicHandle))
            {
                deleteUserOperationResponse = await TestUtilities.DeleteUser(client, auth);
                Assert.Fail("Topic posting failed, or handle was invalid.");
            }

            // Post a comment to Topic
            var postCommentResponse = await TestUtilities.PostGenericComment(client, auth, topicHandle);
            string commentHandle = postCommentResponse.CommentHandle;
            HttpOperationResponse<object> deleteCommentResponse = null;
            if (string.IsNullOrWhiteSpace(commentHandle))
            {
                deleteTopicResponse = await TestUtilities.DeleteTopic(client: client, topicHandle: topicHandle, authorization: auth);
                deleteUserOperationResponse = await TestUtilities.DeleteUser(client: client, authorization: auth);
                Assert.Fail("Comment posting failed, or handle was invalid.");
            }

            // Post reply
            string replyString = "May the force be with!";
            string replyLanguage = "YodaSpeak1000";
            HttpOperationResponse<PostReplyResponse> postReplyResponse = null;
            HttpOperationResponse<FeedResponseCommentView> getCommentsResponse = null;
            HttpOperationResponse<FeedResponseReplyView> getReplyResponse = null;

            PostReplyRequest postReplyRequest = new PostReplyRequest(text: replyString, language: replyLanguage);

            postReplyResponse = await client.CommentReplies.PostReplyWithHttpMessagesAsync(commentHandle: commentHandle, request: postReplyRequest, authorization: auth);

            // Get Comment after reply
            getCommentsResponse = await client.TopicComments.GetTopicCommentsWithHttpMessagesAsync(topicHandle: topicHandle, cursor: null, limit: 5, authorization: auth);

            // Get Replies
            getReplyResponse = await client.CommentReplies.GetRepliesWithHttpMessagesAsync(commentHandle: commentHandle, cursor: null, limit: 2, authorization: auth);

            // Delete Reply - Cleanup method checks validity of the handle
            HttpOperationResponse<object> deleteReplyResponse = await TestUtilities.DeleteReply(client: client, postReplyResponse: postReplyResponse, authorization: auth);

            // get the Comment and double check info showing up in comment
            HttpOperationResponse<FeedResponseCommentView> getCommentsResponse2 = await client.TopicComments.GetTopicCommentsWithHttpMessagesAsync(topicHandle: topicHandle, cursor: null, limit: 5, authorization: auth);

            // Get Replies after delete
            HttpOperationResponse<FeedResponseReplyView> getReplyResponse2 = await client.CommentReplies.GetRepliesWithHttpMessagesAsync(commentHandle: commentHandle, cursor: null, limit: 2, authorization: auth);

            // Clean up first before verifying
            deleteCommentResponse = await TestUtilities.DeleteComment(client: client, commentHandle: commentHandle, authorization: auth);
            deleteTopicResponse = await TestUtilities.DeleteTopic(client: client, topicHandle: topicHandle, authorization: auth);
            deleteUserOperationResponse = await TestUtilities.DeleteUser(client: client, authorization: auth);

            // Verify after cleaning up so don't leave junk behind if fails

            // Verify comment count from GetTopic ... before and after delete
            Assert.AreEqual(getCommentsResponse.Body.Data.Count, 1);
            Assert.AreEqual(getCommentsResponse.Body.Data[0].TotalReplies, 1);
            Assert.AreEqual(getCommentsResponse2.Body.Data[0].TotalReplies, 0);

            // Also verify the GetReplies part
            Assert.AreEqual(getReplyResponse.Body.Data.Count, 1);
            Assert.AreEqual(getReplyResponse2.Body.Data.Count, 0);  // after the delete

            // Verify deletion
            Assert.IsTrue(deleteReplyResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteCommentResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);
        }
    }
}
