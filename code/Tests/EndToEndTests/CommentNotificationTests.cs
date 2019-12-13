// <copyright file="CommentNotificationTests.cs" company="Microsoft">
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
    /// Tests to verify that comments generate notifications
    /// </summary>
    [TestClass]
    public class CommentNotificationTests
    {
        /// <summary>
        /// Tests whether a notification is generated for a new comment
        /// </summary>
        /// <returns>task that runs the test</returns>
        [TestMethod]
        public async Task CommentNotification()
        {
            // Setup two users
            SocialPlusClient client1 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            SocialPlusClient client2 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            var user1 = await TestUtilities.PostGenericUser(client1);
            var user2 = await TestUtilities.PostGenericUser(client2);
            var auth1 = AuthHelper.CreateSocialPlusAuth(user1.SessionToken);
            var auth2 = AuthHelper.CreateSocialPlusAuth(user2.SessionToken);

            // create a topic by user 1
            var postTopicResponse = await TestUtilities.PostGenericTopic(client1, auth1);

            // get the notification feed
            HttpOperationResponse<FeedResponseActivityView> getNotificationsOperationResponse1 = await client1.MyNotifications.GetNotificationsWithHttpMessagesAsync(authorization: auth1);

            // create a comment by user 2
            var postCommentOperationResponse = await TestUtilities.PostGenericComment(client2, auth2, postTopicResponse.TopicHandle);

            // wait for notifications to fan out
            await Task.Delay(TestConstants.ServiceBusLongDelay);

            // get the notification feed
            HttpOperationResponse<FeedResponseActivityView> getNotificationsOperationResponse2 = await client1.MyNotifications.GetNotificationsWithHttpMessagesAsync(authorization: auth1);

            // clean up
            HttpOperationResponse<object> deleteCommentOperationResponse = await client2.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: postCommentOperationResponse.CommentHandle, authorization: auth2);

            HttpOperationResponse<object> deleteTopicOperationResponse = await client1.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: postTopicResponse.TopicHandle, authorization: auth1);

            HttpOperationResponse<object> deleteUserOperationResponse1 = await client1.Users.DeleteUserWithHttpMessagesAsync(authorization: auth1);
            HttpOperationResponse<object> deleteUserOperationResponse2 = await client2.Users.DeleteUserWithHttpMessagesAsync(authorization: auth2);

            // check everything went well
            Assert.IsTrue(getNotificationsOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(getNotificationsOperationResponse2.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteCommentOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteTopicOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse1.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse2.Response.IsSuccessStatusCode);

            // check the notification feed contents
            Assert.AreEqual(getNotificationsOperationResponse1.Body.Data.Count, 0);
            Assert.AreEqual(getNotificationsOperationResponse2.Body.Data.Count, 1);
            Assert.AreEqual(getNotificationsOperationResponse2.Body.Data[0].ActivityType, ActivityType.Comment);
            Assert.IsTrue(getNotificationsOperationResponse2.Body.Data[0].Unread);
            Assert.AreEqual(getNotificationsOperationResponse2.Body.Data[0].ActedOnContent.ContentHandle, postTopicResponse.TopicHandle);
            Assert.AreEqual(getNotificationsOperationResponse2.Body.Data[0].ActorUsers.Count, 1);
            Assert.AreEqual(getNotificationsOperationResponse2.Body.Data[0].ActorUsers[0].UserHandle, user2.UserHandle);
            Assert.AreEqual(getNotificationsOperationResponse2.Body.Data[0].TotalActions, 1);
        }
    }
}