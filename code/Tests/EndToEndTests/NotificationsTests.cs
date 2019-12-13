// <copyright file="NotificationsTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;

    /// <summary>
    /// Tests related to Notifications
    /// </summary>
    [TestClass]
    public class NotificationsTests
    {
        /// <summary>
        /// Several types of notication test with Get Put and Count
        /// </summary>
        /// <returns>Fail if an exception is hit</returns>
        [TestMethod]
        public async Task GetPutCountNotificationTest()
        {
            await this.GetPutCountNotificationTestHelper(false, null);
        }

        /// <summary>
        /// Several types of notication test with Get Put and Count
        /// </summary>
        /// <param name="appPublished">flag to indicate if topics are app published</param>
        /// <param name="appHandle">app handle</param>
        /// <returns>Fail if an exception is hit</returns>
        public async Task GetPutCountNotificationTestHelper(bool appPublished, string appHandle)
        {
            SocialPlusClient client1 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            SocialPlusClient client2 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);
            SocialPlusClient client3 = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserResponse postUserResponse = await TestUtilities.DoLogin(client1, "Stan", "TopicMan", string.Empty);
            string auth1 = AuthHelper.CreateSocialPlusAuth(postUserResponse.SessionToken);

            if (appPublished)
            {
                // add user1 as admin
                bool added = ManageAppsUtils.AddAdmin(TestConstants.EnvironmentName, appHandle, postUserResponse.UserHandle);
                if (!added)
                {
                    // delete the user and fail the test
                    await client1.Users.DeleteUserAsync(auth1);
                    Assert.Fail("Failed to set user as administrator");
                }
            }

            PostUserResponse postUserResponse2 = await TestUtilities.DoLogin(client2, "Emily", "Johnson", string.Empty);
            string auth2 = AuthHelper.CreateSocialPlusAuth(postUserResponse2.SessionToken);
            PostUserResponse postUserResponse3 = await TestUtilities.DoLogin(client3, "Johnny", "OnTheSpot", string.Empty);
            string auth3 = AuthHelper.CreateSocialPlusAuth(postUserResponse3.SessionToken);

            PublisherType publisherType = appPublished ? PublisherType.App : PublisherType.User;
            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: publisherType, text: "Text", title: "Title", blobHandle: "BlobHandle", language: "en-US", deepLink: "link", categories: "categories", friendlyName: "friendlyName", group: "group");
            PostTopicResponse postTopicResponse = await client1.Topics.PostTopicAsync(postTopicRequest, auth1);

            // all three users like the topic that was created
            await client1.TopicLikes.PostLikeAsync(postTopicResponse.TopicHandle, auth1);
            await client2.TopicLikes.PostLikeAsync(postTopicResponse.TopicHandle, auth2);
            await client3.TopicLikes.PostLikeAsync(postTopicResponse.TopicHandle, auth3);

            if (appPublished)
            {
                // for an app published topic, the topic creator (the content owner) does not receive notifications
                // when the topic is liked.
                await Task.Delay(TestConstants.ServiceBusMediumDelay);
                FeedResponseActivityView notifications1 = await client1.MyNotifications.GetNotificationsAsync(auth1);
                notifications1 = await client1.MyNotifications.GetNotificationsAsync(auth1, null, 10);
                CountResponse count1 = await client1.MyNotifications.GetNotificationsCountAsync(auth1);

                // Clean up state from the test
                await client1.Topics.DeleteTopicAsync(postTopicResponse.TopicHandle, auth1);
                ManageAppsUtils.DeleteAdmin(TestConstants.EnvironmentName, appHandle, postUserResponse.UserHandle);
                await client1.Users.DeleteUserAsync(auth1);
                await client2.Users.DeleteUserAsync(auth2);
                await client3.Users.DeleteUserAsync(auth3);

                // check that no notifications are delivered
                Assert.AreEqual(0, notifications1.Data.Count);
                Assert.AreEqual(0, count1.Count);
            }
            else
            {
                // for a user published topic, user1 (the content owner) should receive two notifications:
                // one when user2 likes the topic, and one when user3 likes the topic
                FeedResponseActivityView notifications1 = null;
                FeedResponseActivityView notifications2 = null;
                FeedResponseActivityView notifications3 = null;
                CountResponse count1 = null;
                await TestUtilities.AutoRetryServiceBusHelper(
                    async () =>
                    {
                        // get the first notification
                        notifications1 = await client1.MyNotifications.GetNotificationsAsync(auth1, null, 1);

                        // get the second notification using the first one as the cursor
                        notifications2 = await client1.MyNotifications.GetNotificationsAsync(auth1, notifications1.Cursor, 1);

                        // get up to 10 notifications
                        notifications3 = await client1.MyNotifications.GetNotificationsAsync(auth1, null, 10);

                        // get the count of unread notifications
                        count1 = await client1.MyNotifications.GetNotificationsCountAsync(auth1);
                    },
                    () =>
                    {
                        // verify
                        Assert.AreEqual(1, notifications1.Data.Count);
                        Assert.AreEqual(ActivityType.Like, notifications1.Data[0].ActivityType);
                        Assert.AreEqual(1, notifications1.Data[0].ActorUsers.Count);
                        Assert.AreEqual(1, notifications1.Data[0].TotalActions);
                        Assert.AreEqual(postTopicResponse.TopicHandle, notifications1.Data[0].ActedOnContent.ContentHandle);
                        Assert.AreEqual(BlobType.Unknown, notifications1.Data[0].ActedOnContent.BlobType);
                        Assert.AreEqual(ContentType.Topic, notifications1.Data[0].ActedOnContent.ContentType);

                        Assert.AreEqual(1, notifications2.Data.Count);
                        Assert.AreEqual(ActivityType.Like, notifications2.Data[0].ActivityType);
                        Assert.AreEqual(1, notifications2.Data[0].ActorUsers.Count);
                        Assert.AreEqual(1, notifications2.Data[0].TotalActions);
                        Assert.AreEqual(postTopicResponse.TopicHandle, notifications2.Data[0].ActedOnContent.ContentHandle);
                        Assert.AreEqual(BlobType.Unknown, notifications2.Data[0].ActedOnContent.BlobType);
                        Assert.AreEqual(ContentType.Topic, notifications2.Data[0].ActedOnContent.ContentType);

                        Assert.AreEqual(2, notifications3.Data.Count);
                        Assert.AreEqual(ActivityType.Like, notifications3.Data[0].ActivityType);
                        Assert.AreEqual(1, notifications3.Data[0].ActorUsers.Count);
                        Assert.AreEqual(1, notifications3.Data[0].TotalActions);
                        Assert.AreEqual(postTopicResponse.TopicHandle, notifications3.Data[0].ActedOnContent.ContentHandle);
                        Assert.AreEqual(BlobType.Unknown, notifications3.Data[0].ActedOnContent.BlobType);
                        Assert.AreEqual(ContentType.Topic, notifications3.Data[0].ActedOnContent.ContentType);

                        Assert.AreEqual(2, count1.Count);
                    });

                // Update which is the most recent notification the user has read
                PutNotificationsStatusRequest putNotificationStatusRequest = new PutNotificationsStatusRequest(notifications3.Data.First().ActivityHandle);
                await client1.MyNotifications.PutNotificationsStatusAsync(putNotificationStatusRequest, auth1);

                // Get Notification
                FeedResponseActivityView notifications4 = await client1.MyNotifications.GetNotificationsAsync(auth1, null, 10);
                var count2 = await client1.MyNotifications.GetNotificationsCountAsync(auth1);

                // User3 creates another like on the topic.  Even though this doesn't change
                // the like status because the user has already liked this topic, it does generate
                // another notification.
                await client3.TopicLikes.PostLikeAsync(postTopicResponse.TopicHandle, auth3);

                FeedResponseActivityView notifications5 = null;
                CountResponse count3 = null;
                await TestUtilities.AutoRetryServiceBusHelper(
                    async () =>
                    {
                        // Get new notifications and the count of unread notifications
                        notifications5 = await client1.MyNotifications.GetNotificationsAsync(auth1, null, 10);
                        count3 = await client1.MyNotifications.GetNotificationsCountAsync(auth1);
                    }, () =>
                    {
                        // verify
                        Assert.AreEqual(2, notifications5.Data.Count);
                        Assert.AreEqual(ActivityType.Like, notifications5.Data[0].ActivityType);
                        Assert.AreEqual(1, notifications5.Data[0].ActorUsers.Count);
                        Assert.AreEqual(1, notifications5.Data[0].TotalActions);
                        Assert.AreEqual(postTopicResponse.TopicHandle, notifications5.Data[0].ActedOnContent.ContentHandle);
                        Assert.AreEqual(BlobType.Unknown, notifications5.Data[0].ActedOnContent.BlobType);
                        Assert.AreEqual(ContentType.Topic, notifications5.Data[0].ActedOnContent.ContentType);

                        Assert.AreEqual(1, count3.Count);
                    });

                // User2 deletes their like on the topic.  This generates a notification
                await client2.TopicLikes.DeleteLikeAsync(postTopicResponse.TopicHandle, auth2);

                FeedResponseActivityView notifications6 = null;
                CountResponse count4 = null;
                await TestUtilities.AutoRetryServiceBusHelper(
                    async () =>
                    {
                        // Get new notifications and the count of unread notifications
                        notifications6 = await client1.MyNotifications.GetNotificationsAsync(auth1, null, 10);
                        count4 = await client1.MyNotifications.GetNotificationsCountAsync(auth1);
                    }, () =>
                    {
                        // verify
                        Assert.AreEqual(1, notifications6.Data.Count);
                        Assert.AreEqual(ActivityType.Like, notifications6.Data[0].ActivityType);
                        Assert.AreEqual(1, notifications6.Data[0].ActorUsers.Count);
                        Assert.AreEqual(1, notifications6.Data[0].TotalActions);
                        Assert.AreEqual(postTopicResponse.TopicHandle, notifications6.Data[0].ActedOnContent.ContentHandle);
                        Assert.AreEqual(BlobType.Unknown, notifications6.Data[0].ActedOnContent.BlobType);
                        Assert.AreEqual(ContentType.Topic, notifications6.Data[0].ActedOnContent.ContentType);

                        Assert.AreEqual(1, count4.Count);
                    });

                // User2 once again likes the topic and generates a notification
                await client2.TopicLikes.PostLikeAsync(postTopicResponse.TopicHandle, auth2);

                FeedResponseActivityView notifications7 = null;
                CountResponse count5 = null;

                await TestUtilities.AutoRetryServiceBusHelper(
                    async () =>
                    {
                        // Get new notifications and the count of unread notifications
                        notifications7 = await client1.MyNotifications.GetNotificationsAsync(auth1, null, 10);
                        count5 = await client1.MyNotifications.GetNotificationsCountAsync(auth1);
                    }, () =>
                    {
                        // verify
                        Assert.AreEqual(2, notifications7.Data.Count);
                        Assert.AreEqual(ActivityType.Like, notifications7.Data[0].ActivityType);
                        Assert.AreEqual(1, notifications7.Data[0].ActorUsers.Count);
                        Assert.AreEqual(1, notifications7.Data[0].TotalActions);
                        Assert.AreEqual(postTopicResponse.TopicHandle, notifications7.Data[0].ActedOnContent.ContentHandle);
                        Assert.AreEqual(BlobType.Unknown, notifications7.Data[0].ActedOnContent.BlobType);
                        Assert.AreEqual(ContentType.Topic, notifications7.Data[0].ActedOnContent.ContentType);

                        Assert.AreEqual(2, count5.Count);
                    });

                // Update the most recent notification read
                putNotificationStatusRequest = new PutNotificationsStatusRequest(notifications7.Data.First().ActivityHandle);
                await client1.MyNotifications.PutNotificationsStatusAsync(putNotificationStatusRequest, auth1);

                // Get new notifications and the count of unread notifications
                var notifications8 = await client1.MyNotifications.GetNotificationsAsync(auth1, null, 10);
                var count6 = await client1.MyNotifications.GetNotificationsCountAsync(auth1);

                // Clean up state from the test
                await client1.Topics.DeleteTopicAsync(postTopicResponse.TopicHandle, auth1);
                await client1.Users.DeleteUserAsync(auth1);
                await client2.Users.DeleteUserAsync(auth2);
                await client3.Users.DeleteUserAsync(auth3);

                // Validate everything
                Assert.AreEqual(1, notifications1.Data.Count);
                Assert.AreEqual(ActivityType.Like, notifications1.Data[0].ActivityType);
                Assert.AreEqual(1, notifications1.Data[0].ActorUsers.Count);
                Assert.AreEqual(1, notifications1.Data[0].TotalActions);
                Assert.AreEqual(postTopicResponse.TopicHandle, notifications1.Data[0].ActedOnContent.ContentHandle);
                Assert.AreEqual(BlobType.Unknown, notifications1.Data[0].ActedOnContent.BlobType);
                Assert.AreEqual(ContentType.Topic, notifications1.Data[0].ActedOnContent.ContentType);

                Assert.AreEqual(1, notifications2.Data.Count);
                Assert.AreEqual(ActivityType.Like, notifications2.Data[0].ActivityType);
                Assert.AreEqual(1, notifications2.Data[0].ActorUsers.Count);
                Assert.AreEqual(1, notifications2.Data[0].TotalActions);
                Assert.AreEqual(postTopicResponse.TopicHandle, notifications2.Data[0].ActedOnContent.ContentHandle);
                Assert.AreEqual(BlobType.Unknown, notifications2.Data[0].ActedOnContent.BlobType);
                Assert.AreEqual(ContentType.Topic, notifications2.Data[0].ActedOnContent.ContentType);

                Assert.AreEqual(2, notifications3.Data.Count);
                Assert.AreEqual(ActivityType.Like, notifications3.Data[0].ActivityType);
                Assert.AreEqual(1, notifications3.Data[0].ActorUsers.Count);
                Assert.AreEqual(1, notifications3.Data[0].TotalActions);
                Assert.AreEqual(postTopicResponse.TopicHandle, notifications3.Data[0].ActedOnContent.ContentHandle);
                Assert.AreEqual(BlobType.Unknown, notifications3.Data[0].ActedOnContent.BlobType);
                Assert.AreEqual(ContentType.Topic, notifications3.Data[0].ActedOnContent.ContentType);

                Assert.AreEqual(2, notifications4.Data.Count);
                Assert.AreEqual(ActivityType.Like, notifications4.Data[0].ActivityType);
                Assert.AreEqual(1, notifications4.Data[0].ActorUsers.Count);
                Assert.AreEqual(1, notifications4.Data[0].TotalActions);
                Assert.AreEqual(postTopicResponse.TopicHandle, notifications4.Data[0].ActedOnContent.ContentHandle);
                Assert.AreEqual(BlobType.Unknown, notifications4.Data[0].ActedOnContent.BlobType);
                Assert.AreEqual(ContentType.Topic, notifications4.Data[0].ActedOnContent.ContentType);

                Assert.AreEqual(2, notifications5.Data.Count);
                Assert.AreEqual(ActivityType.Like, notifications5.Data[0].ActivityType);
                Assert.AreEqual(1, notifications5.Data[0].ActorUsers.Count);
                Assert.AreEqual(1, notifications5.Data[0].TotalActions);
                Assert.AreEqual(postTopicResponse.TopicHandle, notifications5.Data[0].ActedOnContent.ContentHandle);
                Assert.AreEqual(BlobType.Unknown, notifications5.Data[0].ActedOnContent.BlobType);
                Assert.AreEqual(ContentType.Topic, notifications5.Data[0].ActedOnContent.ContentType);

                Assert.AreEqual(1, notifications6.Data.Count);
                Assert.AreEqual(ActivityType.Like, notifications6.Data[0].ActivityType);
                Assert.AreEqual(1, notifications6.Data[0].ActorUsers.Count);
                Assert.AreEqual(1, notifications6.Data[0].TotalActions);
                Assert.AreEqual(postTopicResponse.TopicHandle, notifications6.Data[0].ActedOnContent.ContentHandle);
                Assert.AreEqual(BlobType.Unknown, notifications6.Data[0].ActedOnContent.BlobType);
                Assert.AreEqual(ContentType.Topic, notifications6.Data[0].ActedOnContent.ContentType);

                Assert.AreEqual(2, notifications7.Data.Count);
                Assert.AreEqual(ActivityType.Like, notifications7.Data[0].ActivityType);
                Assert.AreEqual(1, notifications7.Data[0].ActorUsers.Count);
                Assert.AreEqual(1, notifications7.Data[0].TotalActions);
                Assert.AreEqual(postTopicResponse.TopicHandle, notifications7.Data[0].ActedOnContent.ContentHandle);
                Assert.AreEqual(BlobType.Unknown, notifications7.Data[0].ActedOnContent.BlobType);
                Assert.AreEqual(ContentType.Topic, notifications7.Data[0].ActedOnContent.ContentType);

                Assert.AreEqual(2, notifications8.Data.Count);
                Assert.AreEqual(ActivityType.Like, notifications8.Data[0].ActivityType);
                Assert.AreEqual(1, notifications8.Data[0].ActorUsers.Count);
                Assert.AreEqual(1, notifications8.Data[0].TotalActions);
                Assert.AreEqual(postTopicResponse.TopicHandle, notifications8.Data[0].ActedOnContent.ContentHandle);
                Assert.AreEqual(BlobType.Unknown, notifications8.Data[0].ActedOnContent.BlobType);
                Assert.AreEqual(ContentType.Topic, notifications8.Data[0].ActedOnContent.ContentType);

                Assert.AreEqual(2, count1.Count);
                Assert.AreEqual(0, count2.Count);
                Assert.AreEqual(1, count3.Count);
                Assert.AreEqual(1, count4.Count);
                Assert.AreEqual(2, count5.Count);
                Assert.AreEqual(0, count6.Count);
            }
        }
    }
}
