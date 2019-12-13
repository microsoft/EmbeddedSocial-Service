// <copyright file="ICommentsStore.cs" company="Microsoft">
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
    /// Comments store interface
    /// </summary>
    public interface ICommentsStore
    {
        /// <summary>
        /// Insert a comment
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
        Task InsertComment(
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
            string requestId);

        /// <summary>
        /// Update a comment
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="commentEntity">Comment entity</param>
        /// <returns>Update comment task</returns>
        Task UpdateComment(StorageConsistencyMode storageConsistencyMode, string commentHandle, ICommentEntity commentEntity);

        /// <summary>
        /// Delete a comment
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Delete comment task</returns>
        Task DeleteComment(
            StorageConsistencyMode storageConsistencyMode,
            string commentHandle);

        /// <summary>
        /// Query comment
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Comment entity</returns>
        Task<ICommentEntity> QueryComment(string commentHandle);

        /// <summary>
        /// Insert topic comment
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="commentUserHandle">Comment user handle</param>
        /// <returns>Insert topic comment task</returns>
        Task InsertTopicComment(
            StorageConsistencyMode storageConsistencyMode,
            string topicHandle,
            string commentHandle,
            string commentUserHandle);

        /// <summary>
        /// Delete topic comment
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Delete topic comment task</returns>
        Task DeleteTopicComment(
            StorageConsistencyMode storageConsistencyMode,
            string topicHandle,
            string commentHandle);

        /// <summary>
        /// Query topic comments
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Comment feed entities</returns>
        Task<IList<ICommentFeedEntity>> QueryTopicComments(string topicHandle, string cursor, int limit);

        /// <summary>
        /// Query topic comments count
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Topic comments count</returns>
        Task<long?> QueryTopicCommentsCount(string topicHandle);
    }
}
