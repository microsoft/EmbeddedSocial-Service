// <copyright file="IRepliesManager.cs" company="Microsoft">
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
    /// Replies manager interface
    /// </summary>
    public interface IRepliesManager
    {
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
        Task CreateReply(
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
            string requestId);

        /// <summary>
        /// Delete reply
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Remove reply task</returns>
        Task DeleteReply(
            ProcessType processType,
            string replyHandle,
            string commentHandle);

        /// <summary>
        /// Read reply
        /// </summary>
        /// <param name="replyHandle">Reply handle</param>
        /// <returns>Reply entity</returns>
        Task<IReplyEntity> ReadReply(string replyHandle);

        /// <summary>
        /// Read replies for a comment sorted by time
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of reply feed entities</returns>
        Task<IList<IReplyFeedEntity>> ReadCommentReplies(string commentHandle, string cursor, int limit);

        /// <summary>
        /// Read count of replies for a comment
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <returns>Number of replies for a comment</returns>
        Task<long?> ReadCommentRepliesCount(string commentHandle);

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
        Task UpdateReply(
            ProcessType processType,
            string replyHandle,
            string text,
            string language,
            ReviewStatus reviewStatus,
            DateTime lastUpdatedTime,
            IReplyEntity replyEntity);
    }
}
