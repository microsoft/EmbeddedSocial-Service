// <copyright file="IActivitiesStore.cs" company="Microsoft">
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
    /// Activities store interface
    /// </summary>
    public interface IActivitiesStore
    {
        /// <summary>
        /// Insert activity
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="activityHandle">Activity handle</param>
        /// <param name="activityType">Activity type</param>
        /// <param name="actorUserHandle">Actor user handle</param>
        /// <param name="actedOnUserHandle">Acted on user handle</param>
        /// <param name="actedOnContentType">Acted on content type</param>
        /// <param name="actedOnContentHandle">Acted on content handle</param>
        /// <param name="createdTime">Created time</param>
        /// <returns>Insert activity task</returns>
        Task InsertFollowingActivity(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string activityHandle,
            ActivityType activityType,
            string actorUserHandle,
            string actedOnUserHandle,
            ContentType actedOnContentType,
            string actedOnContentHandle,
            DateTime createdTime);

        /// <summary>
        /// Query activities
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Activity feed entities</returns>
        Task<IList<IActivityFeedEntity>> QueryFollowingActivities(string userHandle, string appHandle, string cursor, int limit);
    }
}
