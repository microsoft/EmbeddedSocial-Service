// <copyright file="ActivitiesStore.cs" company="Microsoft">
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
    /// Default activities table store implementation that talks to <c>CTStore</c>.
    /// Activity Store manages the the following tables.
    /// 1. FollowingActivitiesFeed - A following activity feed is a feed of activities done by the people a user is following.
    /// It exposes methods to insert activity and read following activity feed.
    /// We don't have method to delete an activity. When a user undoes an activity (e.g. unlike), we filter them on the read path. ViewsManager incorporates this filter logic.
    /// In the future, if we decide to support a feed for "My activities", the methods for MyActivitiesFeed table should be implemented in this file.
    ///
    /// NOTE: The methods this file should be transactions.
    ///
    /// </summary>
    public class ActivitiesStore : IActivitiesStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivitiesStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public ActivitiesStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Insert following activity
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
        public async Task InsertFollowingActivity(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string activityHandle,
            ActivityType activityType,
            string actorUserHandle,
            string actedOnUserHandle,
            ContentType actedOnContentType,
            string actedOnContentHandle,
            DateTime createdTime)
        {
            // Insert an activity into a user's following activities feed
            //
            // PartitionKey: UserHandle
            // FeedKey: AppHandle
            // ItemKey: ActivityHandle (reverse sorted by time). We use LikeHandle, CommentHandle, ReplyHandle, and RelationshipHandle as activity handles
            ActivityFeedEntity activityFeedEntity = new ActivityFeedEntity()
            {
                ActivityHandle = activityHandle,
                AppHandle = appHandle,
                ActivityType = activityType,
                ActorUserHandle = actorUserHandle,
                ActedOnUserHandle = actedOnUserHandle,
                ActedOnContentType = actedOnContentType,
                ActedOnContentHandle = actedOnContentHandle,
                CreatedTime = createdTime
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.FollowingActivities);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.FollowingActivities, TableIdentifier.FollowingActivitiesFeed) as FeedTable;
            Operation operation = Operation.InsertOrReplace(feedTable, userHandle, appHandle, activityHandle, activityFeedEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query following activities
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Activity feed entities</returns>
        public async Task<IList<IActivityFeedEntity>> QueryFollowingActivities(string userHandle, string appHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.FollowingActivities);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.FollowingActivities, TableIdentifier.FollowingActivitiesFeed) as FeedTable;
            var result = await store.QueryFeedAsync<ActivityFeedEntity>(table, userHandle, appHandle, cursor, limit);
            return result.ToList<IActivityFeedEntity>();
        }
    }
}
