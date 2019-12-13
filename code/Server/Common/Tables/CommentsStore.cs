// <copyright file="CommentsStore.cs" company="Microsoft">
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
    /// Default comments table store implementation that talks to <c>CTStore</c>
    /// </summary>
    public class CommentsStore : ICommentsStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentsStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public CommentsStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Insert comment
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="text">Comment text</param>
        /// <param name="blobType">Blob type</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="language">Comment language</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="requestId">Request id associated with the create comment request</param>
        /// <returns>Insert comment task</returns>
        public async Task InsertComment(
            StorageConsistencyMode storageConsistencyMode,
            string commentHandle,
            string topicHandle,
            string text,
            BlobType blobType,
            string blobHandle,
            string language,
            string userHandle,
            DateTime createdTime,
            ReviewStatus reviewStatus,
            string appHandle,
            string requestId)
        {
            CommentEntity commentEntity = new CommentEntity()
            {
                TopicHandle = topicHandle,
                Text = text,
                BlobType = blobType,
                BlobHandle = blobHandle,
                Language = language,
                UserHandle = userHandle,
                CreatedTime = createdTime,
                LastUpdatedTime = createdTime,
                AppHandle = appHandle,
                ReviewStatus = reviewStatus,
                RequestId = requestId
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Comments);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Comments, TableIdentifier.CommentsObject) as ObjectTable;
            Operation operation = Operation.Insert(table, commentHandle, commentHandle, commentEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Update a comment
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="commentEntity">Comment entity</param>
        /// <returns>Update comment task</returns>
        public async Task UpdateComment(StorageConsistencyMode storageConsistencyMode, string commentHandle, ICommentEntity commentEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Comments);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Comments, TableIdentifier.CommentsObject) as ObjectTable;
            Operation operation = Operation.Replace(table, commentHandle, commentHandle, commentEntity as CommentEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete comment
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Delete comment task</returns>
        public async Task DeleteComment(
            StorageConsistencyMode storageConsistencyMode,
            string commentHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Comments);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Comments, TableIdentifier.CommentsObject) as ObjectTable;
            Operation operation = Operation.Delete(table, commentHandle, commentHandle);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query comment
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Comment entity</returns>
        public async Task<ICommentEntity> QueryComment(string commentHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Comments);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Comments, TableIdentifier.CommentsObject) as ObjectTable;
            CommentEntity commentEntity = await store.QueryObjectAsync<CommentEntity>(table, commentHandle, commentHandle);
            return commentEntity;
        }

        /// <summary>
        /// Insert topic comment
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="commentUserHandle">Comment user handle</param>
        /// <returns>Insert topic comment task</returns>
        public async Task InsertTopicComment(
            StorageConsistencyMode storageConsistencyMode,
            string topicHandle,
            string commentHandle,
            string commentUserHandle)
        {
            CommentFeedEntity commentFeedEntity = new CommentFeedEntity()
            {
                CommentHandle = commentHandle,
                UserHandle = commentUserHandle
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicComments);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicComments, TableIdentifier.TopicCommentsFeed) as FeedTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicComments, TableIdentifier.TopicCommentsCount) as CountTable;

            Transaction transaction = new Transaction();
            transaction.Add(Operation.Insert(feedTable, topicHandle, this.tableStoreManager.DefaultFeedKey, commentHandle, commentFeedEntity));
            transaction.Add(Operation.InsertOrIncrement(countTable, topicHandle, this.tableStoreManager.DefaultCountKey));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete topic comment
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Delete topic comment task</returns>
        public async Task DeleteTopicComment(
            StorageConsistencyMode storageConsistencyMode,
            string topicHandle,
            string commentHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicComments);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicComments, TableIdentifier.TopicCommentsFeed) as FeedTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicComments, TableIdentifier.TopicCommentsCount) as CountTable;

            Transaction transaction = new Transaction();
            transaction.Add(Operation.Delete(feedTable, topicHandle, this.tableStoreManager.DefaultFeedKey, commentHandle));
            transaction.Add(Operation.Increment(countTable, topicHandle, this.tableStoreManager.DefaultCountKey, -1.0));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query topic comments
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Comment feed entities</returns>
        public async Task<IList<ICommentFeedEntity>> QueryTopicComments(string topicHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicComments);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.TopicComments, TableIdentifier.TopicCommentsFeed) as FeedTable;
            var result = await store.QueryFeedAsync<CommentFeedEntity>(table, topicHandle, this.tableStoreManager.DefaultFeedKey, cursor, limit);
            return result.ToList<ICommentFeedEntity>();
        }

        /// <summary>
        /// Query topic comments count
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Topic comments count</returns>
        public async Task<long?> QueryTopicCommentsCount(string topicHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicComments);
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicComments, TableIdentifier.TopicCommentsCount) as CountTable;
            var result = await store.QueryCountAsync(countTable, topicHandle, this.tableStoreManager.DefaultCountKey);
            if (result == null)
            {
                return null;
            }

            return (long)result.Count;
        }
    }
}
