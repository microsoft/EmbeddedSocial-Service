// <copyright file="RepliesStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// Default replies table store implementation that talks to <c>CTStore</c>
    /// </summary>
    public class RepliesStore : IRepliesStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepliesStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table manager</param>
        public RepliesStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Insert reply
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="text">Reply text</param>
        /// <param name="language">Reply language</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="requestId">Request id associated with the create reply request</param>
        /// <returns>Insert reply task</returns>
        public async Task InsertReply(
            StorageConsistencyMode storageConsistencyMode,
            string replyHandle,
            string commentHandle,
            string topicHandle,
            string text,
            string language,
            string userHandle,
            DateTime createdTime,
            ReviewStatus reviewStatus,
            string appHandle,
            string requestId)
        {
            ReplyEntity replyEntity = new ReplyEntity()
            {
                CommentHandle = commentHandle,
                TopicHandle = topicHandle,
                Text = text,
                Language = language,
                UserHandle = userHandle,
                CreatedTime = createdTime,
                LastUpdatedTime = createdTime,
                AppHandle = appHandle,
                ReviewStatus = reviewStatus,
                RequestId = requestId
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Replies);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Replies, TableIdentifier.RepliesObject) as ObjectTable;
            Operation operation = Operation.Insert(table, replyHandle, replyHandle, replyEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Update reply
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="replyEntity">Reply entity</param>
        /// <returns>Update reply task</returns>
        public async Task UpdateReply(
            StorageConsistencyMode storageConsistencyMode,
            string replyHandle,
            IReplyEntity replyEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Replies);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Replies, TableIdentifier.RepliesObject) as ObjectTable;
            Operation operation = Operation.Replace(table, replyHandle, replyHandle, replyEntity as ReplyEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete reply
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <returns>Delete reply task</returns>
        public async Task DeleteReply(
            StorageConsistencyMode storageConsistencyMode,
            string replyHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Replies);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Replies, TableIdentifier.RepliesObject) as ObjectTable;
            Operation operation = Operation.Delete(table, replyHandle, replyHandle);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query reply
        /// </summary>
        /// <param name="replyHandle">Reply handle</param>
        /// <returns>Reply entity</returns>
        public async Task<IReplyEntity> QueryReply(string replyHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Replies);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Replies, TableIdentifier.RepliesObject) as ObjectTable;
            ReplyEntity replyEntity = await store.QueryObjectAsync<ReplyEntity>(table, replyHandle, replyHandle);
            return replyEntity;
        }

        /// <summary>
        /// Insert comment reply
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="replyUserHandle">Reply user handle</param>
        /// <returns>Insert comment reply task</returns>
        public async Task InsertCommentReply(
            StorageConsistencyMode storageConsistencyMode,
            string commentHandle,
            string replyHandle,
            string replyUserHandle)
        {
            ReplyFeedEntity replyFeedEntity = new ReplyFeedEntity()
            {
                ReplyHandle = replyHandle,
                UserHandle = replyUserHandle
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.CommentReplies);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.CommentReplies, TableIdentifier.CommentRepliesFeed) as FeedTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.CommentReplies, TableIdentifier.CommentRepliesCount) as CountTable;

            Transaction transaction = new Transaction();
            transaction.Add(Operation.Insert(feedTable, commentHandle, this.tableStoreManager.DefaultFeedKey, replyHandle, replyFeedEntity));
            transaction.Add(Operation.InsertOrIncrement(countTable, commentHandle, this.tableStoreManager.DefaultCountKey));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete comment reply
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <returns>Delete comment reply task</returns>
        public async Task DeleteCommentReply(
            StorageConsistencyMode storageConsistencyMode,
            string commentHandle,
            string replyHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.CommentReplies);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.CommentReplies, TableIdentifier.CommentRepliesFeed) as FeedTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.CommentReplies, TableIdentifier.CommentRepliesCount) as CountTable;

            Transaction transaction = new Transaction();
            transaction.Add(Operation.Delete(feedTable, commentHandle, this.tableStoreManager.DefaultFeedKey, replyHandle));
            transaction.Add(Operation.Increment(countTable, commentHandle, this.tableStoreManager.DefaultCountKey, -1.0));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query comment replies
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Reply feed entities</returns>
        public async Task<IList<IReplyFeedEntity>> QueryCommentReplies(string commentHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.CommentReplies);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.CommentReplies, TableIdentifier.CommentRepliesFeed) as FeedTable;
            var result = await store.QueryFeedAsync<ReplyFeedEntity>(table, commentHandle, this.tableStoreManager.DefaultFeedKey, cursor, limit);
            return result.ToList<IReplyFeedEntity>();
        }

        /// <summary>
        /// Query comment replies count
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Comment replies count</returns>
        public async Task<long?> QueryCommentRepliesCount(string commentHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.CommentReplies);
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.CommentReplies, TableIdentifier.CommentRepliesCount) as CountTable;
            var result = await store.QueryCountAsync(countTable, commentHandle, this.tableStoreManager.DefaultCountKey);
            if (result == null)
            {
                return null;
            }

            return (long)result.Count;
        }
    }
}
