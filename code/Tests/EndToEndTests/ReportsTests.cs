// <copyright file="ReportsTests.cs" company="Microsoft">
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
    /// Simple end to end tests for the reporting API calls
    /// </summary>
    [TestClass]
    public class ReportsTests
    {
        /// <summary>
        /// Test reporting on a user.
        /// This method tests our reports API.
        /// It does not test the submission of a report to AVERT (that is done later by a worker role).
        /// It does not test the callback from AVERT.
        /// </summary>
        /// <returns>Fail if an unexpected exception is hit</returns>
        [TestMethod]
        public async Task UserReport()
        {
            // create two users
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserResponse postUserResponse1 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            PostUserResponse postUserResponse2 = await TestUtilities.PostGenericUser(client);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // issue a report from user 2 on user 1
            PostReportRequest postReportRequest1 = new PostReportRequest(Reason.ThreatsCyberbullyingHarassment);
            HttpOperationResponse<object> postUserReportOperationResponse1 = await client.UserReports.PostReportWithHttpMessagesAsync(userHandle: postUserResponse1.UserHandle, postReportRequest: postReportRequest1, authorization: auth2);

            // issue another report from user 2
            PostReportRequest postReportRequest2 = new PostReportRequest(Reason.ContentInfringement);
            HttpOperationResponse<object> postUserReportOperationResponse2 = await client.UserReports.PostReportWithHttpMessagesAsync(userHandle: postUserResponse1.UserHandle, postReportRequest: postReportRequest2, authorization: auth2);

            // issue another report from user 1 that should fail
            PostReportRequest postReportRequest3 = new PostReportRequest(Reason.Other);
            HttpOperationResponse<object> postUserReportOperationResponse3 = await client.UserReports.PostReportWithHttpMessagesAsync(userHandle: postUserResponse1.UserHandle, postReportRequest: postReportRequest3, authorization: auth1);

            // delete user 1
            var deleteUserOperationResponse1 = await TestUtilities.DeleteUser(client, auth1);

            // issue another report from user 2 that should fail
            PostReportRequest postReportRequest4 = new PostReportRequest(Reason.OffensiveContent);
            HttpOperationResponse<object> postUserReportOperationResponse4 = await client.UserReports.PostReportWithHttpMessagesAsync(userHandle: postUserResponse1.UserHandle, postReportRequest: postReportRequest4, authorization: auth2);

            // delete user 2
            var deleteUserOperationResponse2 = await TestUtilities.DeleteUser(client, auth2);

            // check failure conditions
            Assert.IsTrue(postUserReportOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(postUserReportOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsFalse(postUserReportOperationResponse3.Response.IsSuccessStatusCode);
            Assert.AreEqual(postUserReportOperationResponse3.Response.StatusCode, System.Net.HttpStatusCode.Unauthorized);
            Assert.IsFalse(postUserReportOperationResponse4.Response.IsSuccessStatusCode);
            Assert.AreEqual(postUserReportOperationResponse4.Response.StatusCode, System.Net.HttpStatusCode.NotFound);
            Assert.IsTrue(deleteUserOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse2.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// Test reporting on a topic.
        /// This method tests our reports API.
        /// It does not test the submission of a report to AVERT (that is done later by a worker role).
        /// It does not test the callback from AVERT.
        /// </summary>
        /// <returns>Fail if an unexpected exception is hit</returns>
        [TestMethod]
        public async Task TopicReport()
        {
            // create two users
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            PostUserResponse postUserResponse1 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            PostUserResponse postUserResponse2 = await TestUtilities.PostGenericUser(client);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // create a topic from user 1
            var postTopicOperationResponse = await TestUtilities.PostGenericTopic(client, auth1);
            var topicHandle = postTopicOperationResponse.TopicHandle;

            // issue a report from user 2
            PostReportRequest postReportRequest1 = new PostReportRequest(Reason.ThreatsCyberbullyingHarassment);
            HttpOperationResponse<object> postTopicReportOperationResponse1 = await client.TopicReports.PostReportWithHttpMessagesAsync(topicHandle: topicHandle, postReportRequest: postReportRequest1, authorization: auth2);

            // issue another report from user 2
            PostReportRequest postReportRequest2 = new PostReportRequest(Reason.ContentInfringement);
            HttpOperationResponse<object> postTopicReportOperationResponse2 = await client.TopicReports.PostReportWithHttpMessagesAsync(topicHandle: topicHandle, postReportRequest: postReportRequest2, authorization: auth2);

            // issue another report from user 1 that should fail
            PostReportRequest postReportRequest3 = new PostReportRequest(Reason.Other);
            HttpOperationResponse<object> postTopicReportOperationResponse3 = await client.TopicReports.PostReportWithHttpMessagesAsync(topicHandle: topicHandle, postReportRequest: postReportRequest3, authorization: auth1);

            // delete topic
            var deleteTopicOperationResponse = await TestUtilities.DeleteTopic(client, topicHandle, auth1);

            // issue another report from user 2 that should fail
            PostReportRequest postReportRequest4 = new PostReportRequest(Reason.OffensiveContent);
            HttpOperationResponse<object> postTopicReportOperationResponse4 = await client.TopicReports.PostReportWithHttpMessagesAsync(topicHandle: topicHandle, postReportRequest: postReportRequest4, authorization: auth2);

            // delete users
            var deleteUserOperationResponse1 = await TestUtilities.DeleteUser(client, auth1);
            var deleteUserOperationResponse2 = await TestUtilities.DeleteUser(client, auth2);

            // check failure conditions
            Assert.IsTrue(postTopicReportOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(postTopicReportOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsFalse(postTopicReportOperationResponse3.Response.IsSuccessStatusCode);
            Assert.AreEqual(postTopicReportOperationResponse3.Response.StatusCode, System.Net.HttpStatusCode.Unauthorized);
            Assert.IsTrue(deleteTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsFalse(postTopicReportOperationResponse4.Response.IsSuccessStatusCode);
            Assert.AreEqual(postTopicReportOperationResponse4.Response.StatusCode, System.Net.HttpStatusCode.NotFound);
            Assert.IsTrue(deleteUserOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse2.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// Test reporting on a comment.
        /// This method tests our reports API.
        /// It does not test the submission of a report to AVERT (that is done later by a worker role).
        /// It does not test the callback from AVERT.
        /// </summary>
        /// <returns>Fail if an unexpected exception is hit</returns>
        [TestMethod]
        public async Task CommentReport()
        {
            // create two users
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            PostUserResponse postUserResponse1 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            PostUserResponse postUserResponse2 = await TestUtilities.PostGenericUser(client);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // create a topic from user 1
            var postTopicOperationResponse = await TestUtilities.PostGenericTopic(client, auth1);
            var topicHandle = postTopicOperationResponse.TopicHandle;

            // create a comment from user 1
            var postCommentOperationResponse = await TestUtilities.PostGenericComment(client, auth1, topicHandle);
            var commentHandle = postCommentOperationResponse.CommentHandle;

            // issue a report from user 2
            PostReportRequest postReportRequest1 = new PostReportRequest(Reason.ChildEndangermentExploitation);
            HttpOperationResponse<object> postCommentReportOperationResponse1 = await client.CommentReports.PostReportWithHttpMessagesAsync(commentHandle: commentHandle, postReportRequest: postReportRequest1, authorization: auth2);

            // issue another report from user 2
            PostReportRequest postReportRequest2 = new PostReportRequest(Reason.Other);
            HttpOperationResponse<object> postCommentReportOperationResponse2 = await client.CommentReports.PostReportWithHttpMessagesAsync(commentHandle: commentHandle, postReportRequest: postReportRequest2, authorization: auth2);

            // delete comment
            var deleteCommentOperationResponse = await TestUtilities.DeleteComment(client, commentHandle, auth1);

            // delete topic
            var deleteTopicOperationResponse = await TestUtilities.DeleteTopic(client, topicHandle, auth1);

            // issue another report from user 2 that should fail
            PostReportRequest postReportRequest3 = new PostReportRequest(Reason.Other);
            HttpOperationResponse<object> postCommentReportOperationResponse3 = await client.CommentReports.PostReportWithHttpMessagesAsync(commentHandle: commentHandle, postReportRequest: postReportRequest3, authorization: auth2);

            // delete users
            var deleteUserOperationResponse1 = await TestUtilities.DeleteUser(client, auth1);
            var deleteUserOperationResponse2 = await TestUtilities.DeleteUser(client, auth2);

            // check failure conditions
            Assert.IsTrue(postCommentReportOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(postCommentReportOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteCommentOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsFalse(postCommentReportOperationResponse3.Response.IsSuccessStatusCode);
            Assert.AreEqual(postCommentReportOperationResponse3.Response.StatusCode, System.Net.HttpStatusCode.NotFound);
            Assert.IsTrue(deleteUserOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse2.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// Test reporting on a reply.
        /// This method tests our reports API.
        /// It does not test the submission of a report to AVERT (that is done later by a worker role).
        /// It does not test the callback from AVERT.
        /// </summary>
        /// <returns>Fail if an unexpected exception is hit</returns>
        [TestMethod]
        public async Task ReplyReport()
        {
            // create two users
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            PostUserResponse postUserResponse1 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            PostUserResponse postUserResponse2 = await TestUtilities.PostGenericUser(client);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // create a topic from user 1
            var postTopicOperationResponse = await TestUtilities.PostGenericTopic(client, auth1);
            var topicHandle = postTopicOperationResponse.TopicHandle;

            // create a comment from user 2
            var postCommentOperationResponse = await TestUtilities.PostGenericComment(client, auth2, topicHandle);
            var commentHandle = postCommentOperationResponse.CommentHandle;

            // create a reply from user 1
            var postReplyOperationResponse = await TestUtilities.PostGenericReply(client, auth1, commentHandle);
            var replyHandle = postReplyOperationResponse.ReplyHandle;

            // issue a report from user 2
            PostReportRequest postReportRequest1 = new PostReportRequest(Reason.OffensiveContent);
            HttpOperationResponse<object> postReplyReportOperationResponse1 = await client.ReplyReports.PostReportWithHttpMessagesAsync(replyHandle: replyHandle, postReportRequest: postReportRequest1, authorization: auth2);

            // issue another report from user 2
            PostReportRequest postReportRequest2 = new PostReportRequest(Reason.ThreatsCyberbullyingHarassment);
            HttpOperationResponse<object> postReplyReportOperationResponse2 = await client.ReplyReports.PostReportWithHttpMessagesAsync(replyHandle: replyHandle, postReportRequest: postReportRequest2, authorization: auth2);

            // delete reply
            var deleteReplyOperationResponse = await TestUtilities.DeleteReply(client, replyHandle, auth1);

            // delete comment
            var deleteCommentOperationResponse = await TestUtilities.DeleteComment(client, commentHandle, auth2);

            // delete topic
            var deleteTopicOperationResponse = await TestUtilities.DeleteTopic(client, topicHandle, auth1);

            // issue another report from user 2 that should fail
            PostReportRequest postReportRequest3 = new PostReportRequest(Reason.ContentInfringement);
            HttpOperationResponse<object> postReplyReportOperationResponse3 = await client.ReplyReports.PostReportWithHttpMessagesAsync(replyHandle: replyHandle, postReportRequest: postReportRequest3, authorization: auth2);

            // delete users
            var deleteUserOperationResponse1 = await TestUtilities.DeleteUser(client, auth1);
            var deleteUserOperationResponse2 = await TestUtilities.DeleteUser(client, auth2);

            // check failure conditions
            Assert.IsTrue(postReplyReportOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(postReplyReportOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteReplyOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteCommentOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsFalse(postReplyReportOperationResponse3.Response.IsSuccessStatusCode);
            Assert.AreEqual(postReplyReportOperationResponse3.Response.StatusCode, System.Net.HttpStatusCode.NotFound);
            Assert.IsTrue(deleteUserOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse2.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// Issue several reports for manual testing with the AVERT team.
        /// Do not use this in regular tests because it purposely does not clean up state.
        /// This lack of cleanup is needed because of long latencies in getting an AVERT callback.
        /// AVERT is no longer calling us back with results from automated scans and is instead calling us back
        /// only after a human has reviewed the request.
        /// AVERT's SLA to us is 1 day M-F and up to 3 days over the weekend.
        /// Callbacks will only be generated if the service under test is talking to production AVERT
        /// and the callbacks will be processed only if the service under test is checking for the production
        /// AVERT certificate thumbprint.
        /// </summary>
        /// <returns>Fail if an unexpected exception is hit</returns>
        [TestMethod]
        public async Task ManualReportTesting()
        {
            // WARNING: do not run this test unless you are doing a test where you can tolerate 1-3 days of latency
            // and can manually verify the result by inspecting Azure Tables
            Assert.IsTrue(false);

            // create two users with benign profiles
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            PostUserResponse postUserResponse1 = await TestUtilities.PostGenericUser(client);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse1.SessionToken);
            PostUserResponse postUserResponse2 = await TestUtilities.PostGenericUser(client);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);

            // issue a Threats / Cyberbullying / Harassment report from user 2 on user 1
            PostReportRequest postReportRequest1 = new PostReportRequest(Reason.ThreatsCyberbullyingHarassment);
            HttpOperationResponse<object> postUserReportOperationResponse1 = await client.UserReports.PostReportWithHttpMessagesAsync(userHandle: postUserResponse1.UserHandle, postReportRequest: postReportRequest1, authorization: auth2);

            // issue a Content Infringment report from user 2
            PostReportRequest postReportRequest2 = new PostReportRequest(Reason.ContentInfringement);
            HttpOperationResponse<object> postUserReportOperationResponse2 = await client.UserReports.PostReportWithHttpMessagesAsync(userHandle: postUserResponse1.UserHandle, postReportRequest: postReportRequest2, authorization: auth2);

            // check failure conditions
            Assert.IsTrue(postUserReportOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(postUserReportOperationResponse2.Response.IsSuccessStatusCode);

            // create a threatening topic from user 1
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.User, text: "I am going to beat you up.", title: "You're in big trouble.", blobType: BlobType.Custom, blobHandle: null, categories: null, language: null, deepLink: null, friendlyName: null, group: null);
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth1);
            string topicHandle = null;
            if (postTopicOperationResponse != null && postTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                topicHandle = postTopicOperationResponse.Body.TopicHandle;
            }

            // issue a Threats / Cyberbullying / Harassment report from user 2
            PostReportRequest postTopicReportRequest1 = new PostReportRequest(Reason.ThreatsCyberbullyingHarassment);
            HttpOperationResponse<object> postTopicReportOperationResponse1 = await client.TopicReports.PostReportWithHttpMessagesAsync(topicHandle: topicHandle, postReportRequest: postTopicReportRequest1, authorization: auth2);

            // check failure conditions
            Assert.IsTrue(postTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(postTopicReportOperationResponse1.Response.IsSuccessStatusCode);

            // create a benign comment from user 1
            var postCommentOperationResponse = await TestUtilities.PostGenericComment(client, auth1, topicHandle);
            var commentHandle = postCommentOperationResponse.CommentHandle;

            // issue a Child Endangerment / Exploitation report from user 2
            PostReportRequest postCommentReportRequest1 = new PostReportRequest(Reason.ChildEndangermentExploitation);
            HttpOperationResponse<object> postCommentReportOperationResponse1 = await client.CommentReports.PostReportWithHttpMessagesAsync(commentHandle: commentHandle, postReportRequest: postCommentReportRequest1, authorization: auth2);

            // check failure conditions
            Assert.IsTrue(postCommentReportOperationResponse1.Response.IsSuccessStatusCode);

            // create a profanity laden reply from user 1
            PostReplyRequest postReplyRequest = new PostReplyRequest(text: "fuck. shit.");
            HttpOperationResponse<PostReplyResponse> postReplyOperationResponse = await client.CommentReplies.PostReplyWithHttpMessagesAsync(commentHandle: commentHandle, request: postReplyRequest, authorization: auth1);
            string replyHandle = null;
            if (postReplyOperationResponse != null && postReplyOperationResponse.Response.IsSuccessStatusCode)
            {
                replyHandle = postReplyOperationResponse.Body.ReplyHandle;
            }

            // issue an Offensive Content report from user 2
            PostReportRequest postReplyReportRequest1 = new PostReportRequest(Reason.OffensiveContent);
            HttpOperationResponse<object> postReplyReportOperationResponse1 = await client.ReplyReports.PostReportWithHttpMessagesAsync(replyHandle: replyHandle, postReportRequest: postReplyReportRequest1, authorization: auth2);

            // check failure conditions
            Assert.IsTrue(postReplyOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(postReplyReportOperationResponse1.Response.IsSuccessStatusCode);

            // do NOT clean up the users after the test ends
        }
    }
}
