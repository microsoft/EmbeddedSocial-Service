// <copyright file="IRepliesStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Replies store interface
    /// </summary>
    public interface IRepliesStore
    {
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
        Task InsertReply(
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
            string requestId);

        /// <summary>
        /// Update reply
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="replyEntity">Reply entity</param>
        /// <returns>Update reply task</returns>
        Task UpdateReply(
            StorageConsistencyMode storageConsistencyMode,
            string replyHandle,
            IReplyEntity replyEntity);

        /// <summary>
        /// Delete reply
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <returns>Delete reply task</returns>
        Task DeleteReply(
            StorageConsistencyMode storageConsistencyMode,
            string replyHandle);

        /// <summary>
        /// Query reply
        /// </summary>
        /// <param name="replyHandle">Reply handle</param>
        /// <returns>Reply entity</returns>
        Task<IReplyEntity> QueryReply(string replyHandle);

        /// <summary>
        /// Insert comment reply
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="replyUserHandle">Reply user handle</param>
        /// <returns>Insert comment reply task</returns>
        Task InsertCommentReply(
            StorageConsistencyMode storageConsistencyMode,
            string commentHandle,
            string replyHandle,
            string replyUserHandle);

        /// <summary>
        /// Delete comment reply
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <returns>Delete comment reply task</returns>
        Task DeleteCommentReply(
            StorageConsistencyMode storageConsistencyMode,
            string commentHandle,
            string replyHandle);

        /// <summary>
        /// Query comment replies
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Reply feed entities</returns>
        Task<IList<IReplyFeedEntity>> QueryCommentReplies(string commentHandle, string cursor, int limit);

        /// <summary>
        /// Query comment replies count
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Comment replies count</returns>
        Task<long?> QueryCommentRepliesCount(string commentHandle);
    }
}
