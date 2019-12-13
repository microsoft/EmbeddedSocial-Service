// <copyright file="ICommentsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Comments manager interface
    /// </summary>
    public interface ICommentsManager
    {
        /// <summary>
        /// Create a comment
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
        Task CreateComment(
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
            string requestId);

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
        Task UpdateComment(
            ProcessType processType,
            string commentHandle,
            string text,
            BlobType blobType,
            string blobHandle,
            ReviewStatus reviewStatus,
            DateTime lastUpdatedTime,
            ICommentEntity commentEntity);

        /// <summary>
        /// Delete comment
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Remove comment task</returns>
        Task DeleteComment(
            ProcessType processType,
            string commentHandle,
            string topicHandle);

        /// <summary>
        /// Read comment
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Comment entity</returns>
        Task<ICommentEntity> ReadComment(string commentHandle);

        /// <summary>
        /// Read comments for a topics sorted by time
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of comment feed entities</returns>
        Task<IList<ICommentFeedEntity>> ReadTopicComments(string topicHandle, string cursor, int limit);

        /// <summary>
        /// Read count of comments for a topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Number of comments for a topic</returns>
        Task<long?> ReadTopicCommentsCount(string topicHandle);
    }
}
