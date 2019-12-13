// <copyright file="CommentsTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;

    /// <summary>
    /// Basic functional end to end tests for everything to do with Comments
    /// </summary>
    [TestClass]
    public class CommentsTests
    {
        /// <summary>
        /// Create, get, like, delete comments for a topic
        /// </summary>
        /// <returns>Task that will throw an exception if the test fails</returns>
        [TestMethod]
        public async Task PostGetLikeDeleteCommentTest()
        {
            // create 2 users
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            string firstName = "Stan";
            string lastName = "TopicMan";
            string bio = string.Empty;
            PostUserResponse postUserResponse1 = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);

            string firstName2 = "NotPopularNelly";
            string lastName2 = "Von Jerk";
            string bio2 = "Nelly is not very popular. :(";
            PostUserResponse postUserResponse2 = await TestUtilities.DoLogin(client, firstName2, lastName2, bio2);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // create a topic from user 1
            string topicTitle = "My Favorite Topic";
            string topicText = "It is all about sports!";
            BlobType blobType = BlobType.Custom;
            string blobHandle = "http://myBlobHandle/";
            string language = string.Empty;
            string deepLink = string.Empty;
            string categories = string.Empty;
            string friendlyName = string.Empty;
            string group = string.Empty;
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.User, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth1);
            string topicHandle = null;
            if (postTopicOperationResponse != null && postTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
            }

            // post comment 1 with an image from user 2
            HttpOperationResponse<PostImageResponse> postImageOperationResponse = await ImageTests.AddImage(new System.Uri("http://research.microsoft.com/a/i/c/ms-logo.png"), ImageType.ContentBlob, client, auth2);
            string imageHandle = string.Empty;
            PostCommentRequest postCommentRequest1 = null;
            HttpOperationResponse<PostCommentResponse> postCommentOperationResponse1 = null;
            if (!string.IsNullOrWhiteSpace(topicHandle) && postImageOperationResponse != null && postImageOperationResponse.Response.IsSuccessStatusCode)
            {
                // get the image handle
                imageHandle = postImageOperationResponse.Body.BlobHandle;

                // create a comment from user 2
                postCommentRequest1 = new PostCommentRequest(text: "my comment", blobType: BlobType.Image, blobHandle: imageHandle, language: "gibberish");
                postCommentOperationResponse1 = await client.TopicComments.PostCommentWithHttpMessagesAsync(topicHandle: topicHandle, request: postCommentRequest1, authorization: auth2);
            }

            // post comment 2 from user 1
            PostCommentRequest postCommentRequest2 = null;
            HttpOperationResponse<PostCommentResponse> postCommentOperationResponse2 = null;
            if (!string.IsNullOrWhiteSpace(topicHandle))
            {
                // create a comment from user 1
                postCommentRequest2 = new PostCommentRequest(text: "another comment");
                postCommentOperationResponse2 = await client.TopicComments.PostCommentWithHttpMessagesAsync(topicHandle: topicHandle, request: postCommentRequest2, authorization: auth1);
            }

            // get comment handles
            string commentHandle1 = null;
            string commentHandle2 = null;
            if (postCommentOperationResponse1 != null && postCommentOperationResponse1.Response.IsSuccessStatusCode)
            {
                commentHandle1 = postCommentOperationResponse1.Body.CommentHandle;
            }

            if (postCommentOperationResponse2 != null && postCommentOperationResponse2.Response.IsSuccessStatusCode)
            {
                commentHandle2 = postCommentOperationResponse2.Body.CommentHandle;
            }

            // like comment 2 by user 2
            HttpOperationResponse<object> postLikeOperationResponse = null;
            if (!string.IsNullOrWhiteSpace(commentHandle2))
            {
                postLikeOperationResponse = await client.CommentLikes.PostLikeWithHttpMessagesAsync(commentHandle: commentHandle2, authorization: auth2);
            }

            // get topic & comment feed from the server as user 2
            HttpOperationResponse<TopicView> getTopicOperationResponse = null;
            HttpOperationResponse<FeedResponseCommentView> getCommentsOperationResponse1 = null;
            if (!string.IsNullOrWhiteSpace(commentHandle1) && !string.IsNullOrWhiteSpace(commentHandle2))
            {
                getTopicOperationResponse = await client.Topics.GetTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth1);
                getCommentsOperationResponse1 = await client.TopicComments.GetTopicCommentsWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth2);
            }

            // try to delete comment 1 with wrong bearer token
            HttpOperationResponse<object> deleteCommentOperationResponse1 = null;
            if (postCommentOperationResponse1 != null && postCommentOperationResponse1.Response.IsSuccessStatusCode)
            {
                deleteCommentOperationResponse1 = await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle1, authorization: auth1);
            }

            // delete comment 1 with good bearer token
            HttpOperationResponse<object> deleteCommentOperationResponse2 = null;
            if (postCommentOperationResponse1 != null && postCommentOperationResponse1.Response.IsSuccessStatusCode)
            {
                deleteCommentOperationResponse2 = await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle1, authorization: auth2);
            }

            // get comment feed from the server as user 1
            HttpOperationResponse<FeedResponseCommentView> getCommentsOperationResponse2 = null;
            if (!string.IsNullOrWhiteSpace(commentHandle2))
            {
                getCommentsOperationResponse2 = await client.TopicComments.GetTopicCommentsWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth1);
            }

            // delete comment 1 again with good bearer token
            HttpOperationResponse<object> deleteCommentOperationResponse3 = null;
            if (postCommentOperationResponse1 != null && postCommentOperationResponse1.Response.IsSuccessStatusCode)
            {
                deleteCommentOperationResponse3 = await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle1, authorization: auth2);
            }

            // delete comment 2
            HttpOperationResponse<object> deleteCommentOperationResponse4 = null;
            if (postCommentOperationResponse2 != null && postCommentOperationResponse2.Response.IsSuccessStatusCode)
            {
                deleteCommentOperationResponse4 = await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle2, authorization: auth1);
            }

            // no need to delete the image because there is no image delete API

            // delete topic
            HttpOperationResponse<object> deleteTopicOperationResponse = null;
            if (!string.IsNullOrWhiteSpace(topicHandle))
            {
                deleteTopicOperationResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth1);
            }

            // delete users
            HttpOperationResponse<object> deleteUserOperationResponse1 = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse<object> deleteUserOperationResponse2 = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);

            // check failure conditions
            Assert.IsTrue(postTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(postImageOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(postCommentOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(postCommentOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(getTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(getCommentsOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(getCommentsOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(postLikeOperationResponse.Response.IsSuccessStatusCode);

            Assert.IsFalse(deleteCommentOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteCommentOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsFalse(deleteCommentOperationResponse3.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteCommentOperationResponse4.Response.IsSuccessStatusCode);

            // check response values
            Assert.IsFalse(string.IsNullOrWhiteSpace(postUserResponse1.SessionToken));
            Assert.IsFalse(string.IsNullOrWhiteSpace(postUserResponse2.SessionToken));
            Assert.IsFalse(string.IsNullOrWhiteSpace(topicHandle));
            Assert.IsFalse(string.IsNullOrWhiteSpace(imageHandle));
            Assert.IsFalse(string.IsNullOrWhiteSpace(commentHandle1));
            Assert.IsFalse(string.IsNullOrWhiteSpace(commentHandle2));
            Assert.AreEqual(getTopicOperationResponse.Body.TotalComments, 2);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data.Count, 2);
            Assert.AreEqual(getCommentsOperationResponse2.Body.Data.Count, 1);

            // check comment 1 in comment feed 1 (oldest in the feed)
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[1].BlobHandle, postCommentRequest1.BlobHandle);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[1].BlobType, postCommentRequest1.BlobType);
            Assert.IsTrue(getCommentsOperationResponse1.Body.Data[1].BlobUrl.Contains(imageHandle));
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[1].CommentHandle, commentHandle1);
            Assert.IsTrue(getCommentsOperationResponse1.Body.Data[1].ContentStatus == ContentStatus.Active || getCommentsOperationResponse1.Body.Data[1].ContentStatus == ContentStatus.Clean);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[1].Language, postCommentRequest1.Language);
            Assert.IsFalse(getCommentsOperationResponse1.Body.Data[1].Liked);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[1].Text, postCommentRequest1.Text);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[1].TopicHandle, topicHandle);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[1].TotalLikes, 0);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[1].TotalReplies, 0);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[1].User.UserHandle, postUserResponse2.UserHandle);

            // check comment 2 in comment feed 1 (earliest in the feed)
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].BlobHandle, postCommentRequest2.BlobHandle);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].BlobType, BlobType.Unknown);
            Assert.IsTrue(string.IsNullOrEmpty(getCommentsOperationResponse1.Body.Data[0].BlobUrl));
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].CommentHandle, commentHandle2);
            Assert.IsTrue(getCommentsOperationResponse1.Body.Data[0].ContentStatus == ContentStatus.Active || getCommentsOperationResponse1.Body.Data[0].ContentStatus == ContentStatus.Clean);
            Assert.IsTrue(string.IsNullOrEmpty(getCommentsOperationResponse1.Body.Data[0].Language));
            Assert.IsTrue(getCommentsOperationResponse1.Body.Data[0].Liked);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].Text, postCommentRequest2.Text);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].TopicHandle, topicHandle);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].TotalLikes, 1);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].TotalReplies, 0);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].User.UserHandle, postUserResponse1.UserHandle);

            // check comment 2 in comment feed 2 (only one in the feed)
            Assert.AreEqual(getCommentsOperationResponse2.Body.Data[0].BlobHandle, postCommentRequest2.BlobHandle);
            Assert.AreEqual(getCommentsOperationResponse2.Body.Data[0].BlobType, BlobType.Unknown);
            Assert.IsTrue(string.IsNullOrEmpty(getCommentsOperationResponse2.Body.Data[0].BlobUrl));
            Assert.AreEqual(getCommentsOperationResponse2.Body.Data[0].CommentHandle, commentHandle2);
            Assert.IsTrue(getCommentsOperationResponse2.Body.Data[0].ContentStatus == ContentStatus.Active || getCommentsOperationResponse2.Body.Data[0].ContentStatus == ContentStatus.Clean);
            Assert.IsTrue(string.IsNullOrEmpty(getCommentsOperationResponse2.Body.Data[0].Language));
            Assert.IsFalse(getCommentsOperationResponse2.Body.Data[0].Liked);
            Assert.AreEqual(getCommentsOperationResponse2.Body.Data[0].Text, postCommentRequest2.Text);
            Assert.AreEqual(getCommentsOperationResponse2.Body.Data[0].TopicHandle, topicHandle);
            Assert.AreEqual(getCommentsOperationResponse2.Body.Data[0].TotalLikes, 1);
            Assert.AreEqual(getCommentsOperationResponse2.Body.Data[0].TotalReplies, 0);
            Assert.AreEqual(getCommentsOperationResponse2.Body.Data[0].User.UserHandle, postUserResponse1.UserHandle);
        }

        /// <summary>
        /// Test anonymous reads on comments for a topic
        /// </summary>
        /// <returns>Task that will throw an exception if the test fails</returns>
        [TestMethod]
        public async Task AnonymousReadCommentTest()
        {
            // create 2 users
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            string firstName = "Stan";
            string lastName = "TopicMan";
            string bio = string.Empty;
            PostUserResponse postUserResponse1 = await TestUtilities.DoLogin(client, firstName, lastName, bio);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);

            string firstName2 = "NotPopularNelly";
            string lastName2 = "Von Jerk";
            string bio2 = "Nelly is not very popular. :(";
            PostUserResponse postUserResponse2 = await TestUtilities.DoLogin(client, firstName2, lastName2, bio2);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // create a topic from user 1
            string topicTitle = "My Favorite Topic";
            string topicText = "It is all about sports!";
            BlobType blobType = BlobType.Custom;
            string blobHandle = "http://myBlobHandle/";
            string language = string.Empty;
            string deepLink = string.Empty;
            string categories = string.Empty;
            string friendlyName = string.Empty;
            string group = string.Empty;
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.User, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, categories: categories, language: language, deepLink: deepLink, friendlyName: friendlyName, group: group);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth1);
            string topicHandle = null;
            if (postTopicOperationResponse != null && postTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
            }

            // post comment 1 with an image from user 2
            HttpOperationResponse<PostImageResponse> postImageOperationResponse = await ImageTests.AddImage(new System.Uri("http://research.microsoft.com/a/i/c/ms-logo.png"), ImageType.ContentBlob, client, auth2);
            string imageHandle = string.Empty;
            PostCommentRequest postCommentRequest1 = null;
            HttpOperationResponse<PostCommentResponse> postCommentOperationResponse1 = null;
            if (!string.IsNullOrWhiteSpace(topicHandle) && postImageOperationResponse != null && postImageOperationResponse.Response.IsSuccessStatusCode)
            {
                // get the image handle
                imageHandle = postImageOperationResponse.Body.BlobHandle;

                // create a comment from user 2
                postCommentRequest1 = new PostCommentRequest(text: "my comment", blobType: BlobType.Image, blobHandle: imageHandle, language: "gibberish");
                postCommentOperationResponse1 = await client.TopicComments.PostCommentWithHttpMessagesAsync(topicHandle: topicHandle, request: postCommentRequest1, authorization: auth2);
            }

            // get comment handle
            string commentHandle1 = null;
            if (postCommentOperationResponse1 != null && postCommentOperationResponse1.Response.IsSuccessStatusCode)
            {
                commentHandle1 = postCommentOperationResponse1.Body.CommentHandle;
            }

            // get topic & comment feed from the server as an anonymous user
            HttpOperationResponse<TopicView> getTopicOperationResponse = null;
            HttpOperationResponse<FeedResponseCommentView> getCommentsOperationResponse1 = null;
            string anonAuth = TestUtilities.GetAnonAuth();
            if (!string.IsNullOrWhiteSpace(commentHandle1))
            {
                getTopicOperationResponse = await client.Topics.GetTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: anonAuth);
                getCommentsOperationResponse1 = await client.TopicComments.GetTopicCommentsWithHttpMessagesAsync(topicHandle: topicHandle, authorization: anonAuth);
            }

            // delete comment 1 with good bearer token
            HttpOperationResponse<object> deleteCommentOperationResponse2 = null;
            if (postCommentOperationResponse1 != null && postCommentOperationResponse1.Response.IsSuccessStatusCode)
            {
                deleteCommentOperationResponse2 = await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle1, authorization: auth2);
            }

            // no need to delete the image because there is no image delete API

            // delete topic
            HttpOperationResponse<object> deleteTopicOperationResponse = null;
            if (!string.IsNullOrWhiteSpace(topicHandle))
            {
                deleteTopicOperationResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: auth1);
            }

            // delete users
            HttpOperationResponse<object> deleteUserOperationResponse1 = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse<object> deleteUserOperationResponse2 = await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);

            // check failure conditions
            Assert.IsTrue(postTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(postImageOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(postCommentOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(getTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(getCommentsOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteCommentOperationResponse2.Response.IsSuccessStatusCode);

            // check response values
            Assert.IsFalse(string.IsNullOrWhiteSpace(postUserResponse1.SessionToken));
            Assert.IsFalse(string.IsNullOrWhiteSpace(postUserResponse2.SessionToken));
            Assert.IsFalse(string.IsNullOrWhiteSpace(topicHandle));
            Assert.IsFalse(string.IsNullOrWhiteSpace(imageHandle));
            Assert.IsFalse(string.IsNullOrWhiteSpace(commentHandle1));
            Assert.AreEqual(getTopicOperationResponse.Body.TotalComments, 1);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data.Count, 1);

            // check comment 1 in comment feed 1
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].BlobHandle, postCommentRequest1.BlobHandle);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].BlobType, postCommentRequest1.BlobType);
            Assert.IsTrue(getCommentsOperationResponse1.Body.Data[0].BlobUrl.Contains(imageHandle));
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].CommentHandle, commentHandle1);
            Assert.IsTrue(getCommentsOperationResponse1.Body.Data[0].ContentStatus == ContentStatus.Active || getCommentsOperationResponse1.Body.Data[0].ContentStatus == ContentStatus.Clean);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].Language, postCommentRequest1.Language);
            Assert.IsFalse(getCommentsOperationResponse1.Body.Data[0].Liked);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].Text, postCommentRequest1.Text);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].TopicHandle, topicHandle);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].TotalLikes, 0);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].TotalReplies, 0);
            Assert.AreEqual(getCommentsOperationResponse1.Body.Data[0].User.UserHandle, postUserResponse2.UserHandle);
        }

        /// <summary>
        /// Tests issuing 100 likes in parallel using 100 different SocialPlus clients.
        /// </summary>
        /// <returns>Task that will throw an exception if the test fails</returns>
        /// <remarks>Test currently fails; count updates fail when done rapidly due to OCC</remarks>
        [TestMethod]
        public async Task ConcurrentLikeCommentTest()
        {
            var numTests = 100;

            // Initialize all clients
            SocialPlusClient[] client = new SocialPlusClient[numTests];
            var taskArray = new Task<HttpOperationResponse<object>>[numTests];
            for (int i = 0; i < numTests; i += 1)
            {
                client[i] = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            }

            // Create user, topic, comment using client_0
            var user = await TestUtilities.PostGenericUser(client[0]);
            var auth = AuthHelper.CreateSocialPlusAuth(user.SessionToken);
            var topicResponse = await TestUtilities.PostGenericTopic(client[0], auth);
            var commentResponse = await TestUtilities.PostGenericComment(client[0], auth, topicResponse.TopicHandle);

            // Initialize each PostLike call
            var postLikeFuncList = new Func<Task<HttpOperationResponse<object>>>[numTests];
            for (int i = 0; i < numTests; i += 1)
            {
                postLikeFuncList[i] = () => client[i].CommentLikes.PostLikeWithHttpMessagesAsync(commentHandle: commentResponse.CommentHandle, authorization: auth);
            }

            var postLikeOperationResponse = await ConcurrentCalls<HttpOperationResponse<object>>.FireInParallel(postLikeFuncList);

            await TestUtilities.DeleteComment(client[0], commentResponse.CommentHandle, auth);
            await TestUtilities.DeleteTopic(client[0], topicResponse.TopicHandle, auth);
            await TestUtilities.DeleteUser(client[0], auth);

            // Check that post like operation responses were successful
            for (int i = 0; i < numTests; i += 1)
            {
                Assert.IsTrue(
                    postLikeOperationResponse[i].Response.IsSuccessStatusCode,
                    string.Format($"Like with index {i} failed with status code {postLikeOperationResponse[i].Response.StatusCode}"));
            }
        }
    }
}
