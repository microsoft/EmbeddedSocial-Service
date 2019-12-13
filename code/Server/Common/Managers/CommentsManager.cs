// <copyright file="CommentsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Comments manager class
    /// </summary>
    public class CommentsManager : ICommentsManager
    {
        /// <summary>
        /// Comments store
        /// </summary>
        private ICommentsStore commentsStore;

        /// <summary>
        /// <c>Fanout</c> activities queue
        /// </summary>
        private IFanoutActivitiesQueue fanoutActivitiesQueue;

        /// <summary>
        /// Notifications manager
        /// </summary>
        private INotificationsManager notificationsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentsManager"/> class
        /// </summary>
        /// <param name="commentsStore">Comments store</param>
        /// <param name="fanoutActivitiesQueue"><c>Fanout</c> activities queue</param>
        /// <param name="notificationsManager">Notifications manager</param>
        public CommentsManager(
            ICommentsStore commentsStore,
            IFanoutActivitiesQueue fanoutActivitiesQueue,
            INotificationsManager notificationsManager)
        {
            this.commentsStore = commentsStore;
            this.fanoutActivitiesQueue = fanoutActivitiesQueue;
            this.notificationsManager = notificationsManager;
        }

        /// <summary>
        /// Create comment
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="text">Comment text</param>
        /// <param name="blobType">Blob type</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="language">Comment language</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicPublisherType">Topic publisher type</param>
        /// <param name="topicUserHandle">User handle of topic publisher</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="requestId">Request id associated with the create request</param>
        /// <returns>Create comment task</returns>
        public async Task CreateComment(
            ProcessType processType,
            string commentHandle,
            string text,
            BlobType blobType,
            string blobHandle,
            string language,
            string userHandle,
            string topicHandle,
            PublisherType topicPublisherType,
            string topicUserHandle,
            DateTime createdTime,
            ReviewStatus reviewStatus,
            string appHandle,
            string requestId)
        {
            await this.commentsStore.InsertComment(
                StorageConsistencyMode.Strong,
                commentHandle,
                topicHandle,
                text,
                blobType,
                blobHandle,
                language,
                userHandle,
                createdTime,
                reviewStatus,
                appHandle,
                requestId);

            await this.commentsStore.InsertTopicComment(
                StorageConsistencyMode.Strong,
                topicHandle,
                commentHandle,
                userHandle);

            if (topicPublisherType == PublisherType.User && userHandle != topicUserHandle)
            {
                await this.notificationsManager.CreateNotification(
                    processType,
                    topicUserHandle,
                    appHandle,
                    commentHandle,
                    ActivityType.Comment,
                    userHandle,
                    topicUserHandle,
                    ContentType.Topic,
                    topicHandle,
                    createdTime);
            }

            await this.fanoutActivitiesQueue.SendFanoutActivityMessage(
                userHandle,
                appHandle,
                commentHandle,
                ActivityType.Comment,
                userHandle,
                topicUserHandle,
                ContentType.Topic,
                topicHandle,
                createdTime);

            await this.fanoutActivitiesQueue.SendFanoutTopicActivityMessage(
                topicHandle,
                appHandle,
                commentHandle,
                ActivityType.Comment,
                userHandle,
                topicUserHandle,
                ContentType.Topic,
                topicHandle,
                createdTime);
        }

        /// <summary>
        /// Update a comment
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="text">Comment text</param>
        /// <param name="blobType">Blob type</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="commentEntity">Comment entity</param>
        /// <returns>Update comment task</returns>
        public async Task UpdateComment(
            ProcessType processType,
            string commentHandle,
            string text,
            BlobType blobType,
            string blobHandle,
            ReviewStatus reviewStatus,
            DateTime lastUpdatedTime,
            ICommentEntity commentEntity)
        {
            commentEntity.Text = text;
            commentEntity.BlobType = blobType;
            commentEntity.BlobHandle = blobHandle;
            commentEntity.ReviewStatus = reviewStatus;
            commentEntity.LastUpdatedTime = lastUpdatedTime;

            await this.commentsStore.UpdateComment(StorageConsistencyMode.Strong, commentHandle, commentEntity);
        }

        /// <summary>
        /// Delete comment
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Remove comment task</returns>
        public async Task DeleteComment(
            ProcessType processType,
            string commentHandle,
            string topicHandle)
        {
            await this.commentsStore.DeleteTopicComment(
                StorageConsistencyMode.Strong,
                topicHandle,
                commentHandle);

            await this.commentsStore.DeleteComment(
                StorageConsistencyMode.Strong,
                commentHandle);
        }

        /// <summary>
        /// Read comment
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Comment entity</returns>
        public async Task<ICommentEntity> ReadComment(string commentHandle)
        {
            return await this.commentsStore.QueryComment(commentHandle);
        }

        /// <summary>
        /// Read comments for a topics sorted by time
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of comment feed entities</returns>
        public async Task<IList<ICommentFeedEntity>> ReadTopicComments(string topicHandle, string cursor, int limit)
        {
            return await this.commentsStore.QueryTopicComments(topicHandle, cursor, limit);
        }

        /// <summary>
        /// Read count of comments for a topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Number of comments for a topic</returns>
        public async Task<long?> ReadTopicCommentsCount(string topicHandle)
        {
            return await this.commentsStore.QueryTopicCommentsCount(topicHandle);
        }
    }
}
