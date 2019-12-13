// <copyright file="ILikesStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Server.Entities;

    /// <summary>
    /// Likes store interface
    /// </summary>
    public interface ILikesStore
    {
        /// <summary>
        /// Update like
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="likeHandle">Like handle</param>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="liked">Like status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="readLikeLookupEntity">Read like lookup entity</param>
        /// <returns>Update like task</returns>
        Task UpdateLike(
            StorageConsistencyMode storageConsistencyMode,
            string likeHandle,
            string contentHandle,
            string userHandle,
            bool liked,
            DateTime lastUpdatedTime,
            ILikeLookupEntity readLikeLookupEntity);

        /// <summary>
        /// Query like
        /// </summary>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Like lookup entity</returns>
        Task<ILikeLookupEntity> QueryLike(string contentHandle, string userHandle);

        /// <summary>
        /// Query likes for a content
        /// </summary>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of like feed entities</returns>
        Task<IList<ILikeFeedEntity>> QueryLikes(string contentHandle, string cursor, int limit);

        /// <summary>
        /// Query count of likes for a content
        /// </summary>
        /// <param name="contentHandle">Content handle</param>
        /// <returns>Likes count for a content</returns>
        Task<long?> QueryLikesCount(string contentHandle);
    }
}
