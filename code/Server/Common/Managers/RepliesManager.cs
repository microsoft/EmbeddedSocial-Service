// <copyright file="RepliesManager.cs" company="Microsoft">
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
    /// Replies manager class
    /// </summary>
    public class RepliesManager : IRepliesManager
    {
        /// <summary>
        /// Replies store
        /// </summary>
        private IRepliesStore repliesStore;

        /// <summary>
        /// <c>Fanout</c> activities queue
        /// </summary>
        private IFanoutActivitiesQueue fanoutActivitiesQueue;

        /// <summary>
        /// Notifications manager
        /// </summary>
        private INotificationsManager notificationsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepliesManager"/> class
        /// </summary>
        /// <param name="repliesStore">Comments store</param>
        /// <param name="fanoutActivitiesQueue"><c>Fanout</c> activities queue</param>
        /// <param name="notificationsManager">Notifications manager</param>
        public RepliesManager(
            IRepliesStore repliesStore,
            IFanoutActivitiesQueue fanoutActivitiesQueue,
            INotificationsManager notificationsManager)
        {
            this.repliesStore = repliesStore;
            this.fanoutActivitiesQueue = fanoutActivitiesQueue;
            this.notificationsManager = notificationsManager;
        }

        /// <summary>
        /// Create reply
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="text">Reply text</param>
        /// <param name="language">Comment language</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="commentUserHandle">User handle of comment publisher</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="requestId">Request id associated with the create request</param>
        /// <returns>Create comment task</returns>
        public async Task CreateReply(
            ProcessType processType,
            string replyHandle,
            string text,
            string language,
            string userHandle,
            string commentHandle,
            string commentUserHandle,
            string topicHandle,
            DateTime createdTime,
            ReviewStatus reviewStatus,
            string appHandle,
            string requestId)
        {
            await this.repliesStore.InsertReply(
                StorageConsistencyMode.Strong,
                replyHandle,
                commentHandle,
                topicHandle,
                text,
                language,
                userHandle,
                createdTime,
                reviewStatus,
                appHandle,
                requestId);

            await this.repliesStore.InsertCommentReply(
                StorageConsistencyMode.Strong,
                commentHandle,
                replyHandle,
                userHandle);

            if (userHandle != commentUserHandle)
            {
                await this.notificationsManager.CreateNotification(
                    processType,
                    commentUserHandle,
                    appHandle,
                    replyHandle,
                    ActivityType.Reply,
                    userHandle,
                    commentUserHandle,
                    ContentType.Comment,
                    commentHandle,
                    createdTime);
            }

            await this.fanoutActivitiesQueue.SendFanoutActivityMessage(
                userHandle,
                appHandle,
                replyHandle,
                ActivityType.Reply,
                userHandle,
                commentUserHandle,
                ContentType.Comment,
                commentHandle,
                createdTime);

            await this.fanoutActivitiesQueue.SendFanoutTopicActivityMessage(
                topicHandle,
                appHandle,
                replyHandle,
                ActivityType.Reply,
                userHandle,
                commentUserHandle,
                ContentType.Comment,
                commentHandle,
                createdTime);
        }

        /// <summary>
        /// Delete reply
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Remove reply task</returns>
        public async Task DeleteReply(
            ProcessType processType,
            string replyHandle,
            string commentHandle)
        {
            await this.repliesStore.DeleteCommentReply(
                StorageConsistencyMode.Strong,
                commentHandle,
                replyHandle);

            await this.repliesStore.DeleteReply(
                StorageConsistencyMode.Strong,
                replyHandle);
        }

        /// <summary>
        /// Read reply
        /// </summary>
        /// <param name="replyHandle">Reply handle</param>
        /// <returns>Reply entity</returns>
        public async Task<IReplyEntity> ReadReply(string replyHandle)
        {
            return await this.repliesStore.QueryReply(replyHandle);
        }

        /// <summary>
        /// Read replies for a comment sorted by time
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of reply feed entities</returns>
        public async Task<IList<IReplyFeedEntity>> ReadCommentReplies(string commentHandle, string cursor, int limit)
        {
            return await this.repliesStore.QueryCommentReplies(commentHandle, cursor, limit);
        }

        /// <summary>
        /// Read count of replies for a comment
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Number of replies for a comment</returns>
        public async Task<long?> ReadCommentRepliesCount(string commentHandle)
        {
            return await this.repliesStore.QueryCommentRepliesCount(commentHandle);
        }

        /// <summary>
        /// Update a reply
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="text">Reply text</param>
        /// <param name="language">Comment language</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="replyEntity">Reply entity</param>
        /// <returns>Update reply task</returns>
        public async Task UpdateReply(
            ProcessType processType,
            string replyHandle,
            string text,
            string language,
            ReviewStatus reviewStatus,
            DateTime lastUpdatedTime,
            IReplyEntity replyEntity)
        {
            replyEntity.Text = text;
            replyEntity.Language = language;
            replyEntity.ReviewStatus = reviewStatus;
            replyEntity.LastUpdatedTime = lastUpdatedTime;

            await this.repliesStore.UpdateReply(StorageConsistencyMode.Strong, replyHandle, replyEntity);
        }
    }
}
