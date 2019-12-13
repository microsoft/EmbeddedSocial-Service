// <copyright file="TopicsStore.cs" company="Microsoft">
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
    /// Default topics table store implementation that talks to <c>CTStore</c>
    /// </summary>
    public class TopicsStore : ITopicsStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicsStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public TopicsStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Insert topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="title">Topic title</param>
        /// <param name="text">Topic text</param>
        /// <param name="blobType">Blob type</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="categories">Topic categories</param>
        /// <param name="language">Topic language</param>
        /// <param name="group">Topic group</param>
        /// <param name="deepLink">Topic deep link</param>
        /// <param name="friendlyName">Topic friendly name</param>
        /// <param name="publisherType">Publisher type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="requestId">Request id associated with the create topic request</param>
        /// <returns>Insert topic task</returns>
        public async Task InsertTopic(
            StorageConsistencyMode storageConsistencyMode,
            string topicHandle,
            string title,
            string text,
            BlobType blobType,
            string blobHandle,
            string categories,
            string language,
            string group,
            string deepLink,
            string friendlyName,
            PublisherType publisherType,
            string userHandle,
            DateTime createdTime,
            ReviewStatus reviewStatus,
            string appHandle,
            string requestId)
        {
            TopicEntity topicEntity = new TopicEntity()
            {
                Title = title,
                Text = text,
                BlobType = blobType,
                BlobHandle = blobHandle,
                Categories = categories,
                Language = language,
                Group = group,
                DeepLink = deepLink,
                FriendlyName = friendlyName,
                PublisherType = publisherType,
                UserHandle = userHandle,
                CreatedTime = createdTime,
                LastUpdatedTime = createdTime,
                AppHandle = appHandle,
                ReviewStatus = reviewStatus,
                RequestId = requestId
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Topics);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Topics, TableIdentifier.TopicsObject) as ObjectTable;
            Operation operation = Operation.Insert(table, topicHandle, topicHandle, topicEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Update topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicEntity">Topic entity</param>
        /// <returns>Update topic task</returns>
        public async Task UpdateTopic(
            StorageConsistencyMode storageConsistencyMode,
            string topicHandle,
            ITopicEntity topicEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Topics);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Topics, TableIdentifier.TopicsObject) as ObjectTable;
            Operation operation = Operation.Replace(table, topicHandle, topicHandle, topicEntity as TopicEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Delete topic task</returns>
        public async Task DeleteTopic(
            StorageConsistencyMode storageConsistencyMode,
            string topicHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Topics);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Topics, TableIdentifier.TopicsObject) as ObjectTable;
            Operation operation = Operation.Delete(table, topicHandle, topicHandle);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Topic entity</returns>
        public async Task<ITopicEntity> QueryTopic(string topicHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Topics);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Topics, TableIdentifier.TopicsObject) as ObjectTable;
            TopicEntity topicEntity = await store.QueryObjectAsync<TopicEntity>(table, topicHandle, topicHandle);
            return topicEntity;
        }

        /// <summary>
        /// Insert recent topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">User handle</param>
        /// <returns>Insert recent topic task</returns>
        public async Task InsertRecentTopic(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicHandle,
            string topicUserHandle)
        {
            TopicFeedEntity topicFeedEntity = new TopicFeedEntity()
            {
                AppHandle = appHandle,
                TopicHandle = topicHandle,
                UserHandle = topicUserHandle
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.RecentTopics);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.RecentTopics, TableIdentifier.RecentTopicsFeed) as FeedTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.Insert(table, ContainerIdentifier.RecentTopics.ToString(), appHandle, topicHandle, topicFeedEntity));
            transaction.Add(Operation.Insert(table, ContainerIdentifier.RecentTopics.ToString(), MasterApp.AppHandle, topicHandle, topicFeedEntity));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete recent topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Delete recent topic task</returns>
        public async Task DeleteRecentTopic(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.RecentTopics);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.RecentTopics, TableIdentifier.RecentTopicsFeed) as FeedTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.Delete(table, ContainerIdentifier.RecentTopics.ToString(), appHandle, topicHandle));
            transaction.Add(Operation.Delete(table, ContainerIdentifier.RecentTopics.ToString(), MasterApp.AppHandle, topicHandle));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query recent topics
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> QueryRecentTopics(string appHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.RecentTopics);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.RecentTopics, TableIdentifier.RecentTopicsFeed) as FeedTable;
            var result = await store.QueryFeedAsync<TopicFeedEntity>(table, ContainerIdentifier.RecentTopics.ToString(), appHandle, cursor, limit);
            return result.ToList<ITopicFeedEntity>();
        }

        /// <summary>
        /// Insert popular topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="timeRange">Time range</param>
        /// <param name="hostAppHandle">Hosting app handle: Master app or client app</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">User handle</param>
        /// <param name="score">Entity value</param>
        /// <param name="expiration">Topic expiration</param>
        /// <returns>Insert popular topic task</returns>
        public async Task InsertPopularTopic(
            StorageConsistencyMode storageConsistencyMode,
            TimeRange timeRange,
            string hostAppHandle,
            string appHandle,
            string topicHandle,
            string topicUserHandle,
            long score,
            DateTime expiration)
        {
            TopicRankFeedEntity topicRankFeedEntity = new TopicRankFeedEntity()
            {
                AppHandle = appHandle,
                TopicHandle = topicHandle,
                UserHandle = topicUserHandle
            };
            var serializedEntity = StoreSerializers.MinimalTopicRankFeedEntitySerialize(topicRankFeedEntity);

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PopularTopics);
            Transaction transaction = new Transaction();
            RankFeedTable topicsTable = this.tableStoreManager.GetTable(ContainerIdentifier.PopularTopics, TableIdentifier.PopularTopicsFeed) as RankFeedTable;
            string feedKey = this.GetPopularTopicsFeedKey(timeRange, hostAppHandle);
            transaction.Add(Operation.InsertOrReplace(topicsTable, ContainerIdentifier.PopularTopics.ToString(), feedKey, serializedEntity, score));

            if (timeRange != TimeRange.AllTime)
            {
                RankFeedTable expirationsTable = this.tableStoreManager.GetTable(ContainerIdentifier.PopularTopics, TableIdentifier.PopularTopicsExpirationsFeed) as RankFeedTable;
                transaction.Add(Operation.InsertOrReplace(expirationsTable, ContainerIdentifier.PopularTopics.ToString(), feedKey, serializedEntity, expiration.ToFileTime()));
            }

            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete popular topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="timeRange">Time range</param>
        /// <param name="hostAppHandle">Hosting app handle: Master app or client app</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">User handle</param>
        /// <returns>Delete popular topic task</returns>
        public async Task DeletePopularTopic(
            StorageConsistencyMode storageConsistencyMode,
            TimeRange timeRange,
            string hostAppHandle,
            string appHandle,
            string topicHandle,
            string topicUserHandle)
        {
            TopicRankFeedEntity topicRankFeedEntity = new TopicRankFeedEntity()
            {
                AppHandle = appHandle,
                TopicHandle = topicHandle,
                UserHandle = topicUserHandle
            };
            var serializedEntity = StoreSerializers.MinimalTopicRankFeedEntitySerialize(topicRankFeedEntity);

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PopularTopics);
            Transaction transaction = new Transaction();
            RankFeedTable topicsTable = this.tableStoreManager.GetTable(ContainerIdentifier.PopularTopics, TableIdentifier.PopularTopicsFeed) as RankFeedTable;
            string feedKey = this.GetPopularTopicsFeedKey(timeRange, hostAppHandle);
            transaction.Add(Operation.DeleteIfExists(topicsTable, ContainerIdentifier.PopularTopics.ToString(), feedKey, serializedEntity));

            if (timeRange != TimeRange.AllTime)
            {
                RankFeedTable expirationsTable = this.tableStoreManager.GetTable(ContainerIdentifier.PopularTopics, TableIdentifier.PopularTopicsExpirationsFeed) as RankFeedTable;
                transaction.Add(Operation.DeleteIfExists(expirationsTable, ContainerIdentifier.PopularTopics.ToString(), feedKey, serializedEntity));
            }

            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query popular topics today
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <param name="hostAppHandle">Hosting app handle: Master app or client app</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> QueryPopularTopics(TimeRange timeRange, string hostAppHandle, int cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PopularTopics);
            RankFeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PopularTopics, TableIdentifier.PopularTopicsFeed) as RankFeedTable;
            string feedKey = this.GetPopularTopicsFeedKey(timeRange, hostAppHandle);

            // convert our current API cursor (an int) to a CTStore cursor (a string).  Note that CTStore cursors
            // are offset by 1 from API cursors.  This hack will be removed when we change our API to use string-based
            // cursors.
            string ctCursor;
            if (cursor == 0)
            {
                ctCursor = null;
            }
            else
            {
                ctCursor = (cursor - 1).ToString();
            }

            var result = await store.QueryRankFeedReverseAsync(table, ContainerIdentifier.PopularTopics.ToString(), feedKey, ctCursor, limit);
            var convertedResults = result.Select(item => StoreSerializers.MinimalTopicRankFeedEntityDeserialize(item.ItemKey));
            return convertedResults.ToList<ITopicFeedEntity>();
        }

        /// <summary>
        /// Query popular topics min score
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <param name="hostAppHandle">Hosting app handle: Master app or client app</param>
        /// <returns>Score of the last entity in popular topics feed</returns>
        public async Task<long?> QueryPopularTopicsMinScore(TimeRange timeRange, string hostAppHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PopularTopics);
            RankFeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PopularTopics, TableIdentifier.PopularTopicsFeed) as RankFeedTable;
            string feedKey = this.GetPopularTopicsFeedKey(timeRange, hostAppHandle);

            // querying the rank feed in reverse order with a null cursor and a limit of 1 extracts the min entry in the feed
            var result = await store.QueryRankFeedReverseAsync(table, ContainerIdentifier.PopularTopics.ToString(), feedKey, null, 1);
            if (result.Count > 0)
            {
                return (long)result.First().Score;
            }

            return null;
        }

        /// <summary>
        /// Query popular topics expirations
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <param name="hostAppHandle">Host app handle</param>
        /// <param name="expiration">Expiration time</param>
        /// <returns>Topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> QueryPopularTopicsExpirations(TimeRange timeRange, string hostAppHandle, DateTime expiration)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PopularTopics);
            RankFeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PopularTopics, TableIdentifier.PopularTopicsExpirationsFeed) as RankFeedTable;
            string feedKey = this.GetPopularTopicsFeedKey(timeRange, hostAppHandle);
            var result = await store.QueryRankFeedByScoreAsync(table, ContainerIdentifier.PopularTopics.ToString(), feedKey, 0, expiration.ToFileTime());
            var convertedResults = result.Select(item => StoreSerializers.MinimalTopicRankFeedEntityDeserialize(item.ItemKey));
            return convertedResults.ToList<ITopicFeedEntity>();
        }

        /// <summary>
        /// Query popular topics count
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <param name="hostAppHandle">Hosting app handle: Master app or client app</param>
        /// <returns>Popular topics feed length</returns>
        public async Task<long> QueryPopularTopicsCount(TimeRange timeRange, string hostAppHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PopularTopics);
            RankFeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PopularTopics, TableIdentifier.PopularTopicsFeed) as RankFeedTable;
            string feedKey = this.GetPopularTopicsFeedKey(timeRange, hostAppHandle);
            return await store.QueryRankFeedLengthAsync(table, ContainerIdentifier.PopularTopics.ToString(), feedKey);
        }

        /// <summary>
        /// Query popular topics maximum count
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <returns>Maximum count of popular topics feeds</returns>
        public async Task<long> QueryPopularTopicsMaxCount(TimeRange timeRange)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PopularTopics);
            RankFeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PopularTopics, TableIdentifier.PopularTopicsFeed) as RankFeedTable;
            return table.MaxFeedSizeInCache;
        }

        /// <summary>
        /// Insert user topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Insert user topic task</returns>
        public async Task InsertUserTopic(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string topicHandle)
        {
            TopicFeedEntity topicFeedEntity = new TopicFeedEntity()
            {
                AppHandle = appHandle,
                TopicHandle = topicHandle,
                UserHandle = userHandle
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserTopics);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserTopics, TableIdentifier.UserTopicsFeed) as FeedTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserTopics, TableIdentifier.UserTopicsCount) as CountTable;

            Transaction transaction = new Transaction();
            transaction.Add(Operation.Insert(feedTable, userHandle, appHandle, topicHandle, topicFeedEntity));
            transaction.Add(Operation.Insert(feedTable, userHandle, MasterApp.AppHandle, topicHandle, topicFeedEntity));
            transaction.Add(Operation.InsertOrIncrement(countTable, userHandle, appHandle));
            transaction.Add(Operation.InsertOrIncrement(countTable, userHandle, MasterApp.AppHandle));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete user topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Delete user topic task</returns>
        public async Task DeleteUserTopic(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string topicHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserTopics);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserTopics, TableIdentifier.UserTopicsFeed) as FeedTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserTopics, TableIdentifier.UserTopicsCount) as CountTable;

            Transaction transaction = new Transaction();
            transaction.Add(Operation.Delete(feedTable, userHandle, appHandle, topicHandle));
            transaction.Add(Operation.Delete(feedTable, userHandle, MasterApp.AppHandle, topicHandle));
            transaction.Add(Operation.Increment(countTable, userHandle, appHandle, -1.0));
            transaction.Add(Operation.Increment(countTable, userHandle, MasterApp.AppHandle, -1.0));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query user topics
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> QueryUserTopics(string userHandle, string appHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserTopics);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.UserTopics, TableIdentifier.UserTopicsFeed) as FeedTable;
            var result = await store.QueryFeedAsync<TopicFeedEntity>(table, userHandle, appHandle, cursor, limit);
            return result.ToList<ITopicFeedEntity>();
        }

        /// <summary>
        /// Query user topics count
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User topics count</returns>
        public async Task<long?> QueryUserTopicsCount(string userHandle, string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserTopics);
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserTopics, TableIdentifier.UserTopicsCount) as CountTable;
            var result = await store.QueryCountAsync(countTable, userHandle, appHandle);
            if (result == null)
            {
                return null;
            }

            return (long)result.Count;
        }

        /// <summary>
        /// Insert popular user topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="score">Entity value</param>
        /// <returns>Insert popular user topic task</returns>
        public async Task InsertPopularUserTopic(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string topicHandle,
            long score)
        {
            TopicRankFeedEntity topicRankFeedEntity = new TopicRankFeedEntity()
            {
                AppHandle = appHandle,
                TopicHandle = topicHandle,
                UserHandle = userHandle
            };
            var serializedEntity = StoreSerializers.MinimalTopicRankFeedEntitySerialize(topicRankFeedEntity);

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PopularUserTopics);
            RankFeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PopularUserTopics, TableIdentifier.PopularUserTopicsFeed) as RankFeedTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.InsertOrReplace(table, userHandle, appHandle, serializedEntity, score));
            transaction.Add(Operation.InsertOrReplace(table, userHandle, MasterApp.AppHandle, serializedEntity, score));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete popular user topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Delete popular user topic task</returns>
        public async Task DeletePopularUserTopic(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string topicHandle)
        {
            TopicRankFeedEntity topicRankFeedEntity = new TopicRankFeedEntity()
            {
                AppHandle = appHandle,
                TopicHandle = topicHandle,
                UserHandle = userHandle
            };
            var serializedEntity = StoreSerializers.MinimalTopicRankFeedEntitySerialize(topicRankFeedEntity);

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PopularUserTopics);
            RankFeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PopularUserTopics, TableIdentifier.PopularUserTopicsFeed) as RankFeedTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.DeleteIfExists(table, userHandle, appHandle, serializedEntity));
            transaction.Add(Operation.DeleteIfExists(table, userHandle, MasterApp.AppHandle, serializedEntity));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query popular user topics
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> QueryPopularUserTopics(string userHandle, string appHandle, int cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PopularTopics);
            RankFeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PopularUserTopics, TableIdentifier.PopularUserTopicsFeed) as RankFeedTable;
            var result = await store.QueryRankFeedAsync(table, userHandle, appHandle, cursor.ToString(), limit);
            var convertedResults = result.Select(item => StoreSerializers.MinimalTopicRankFeedEntityDeserialize(item.ItemKey));
            return convertedResults.ToList<ITopicFeedEntity>();
        }

        /// <summary>
        /// Insert following topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">User handle of topic owner</param>
        /// <returns>Insert following topic task</returns>
        public async Task InsertFollowingTopic(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string topicHandle,
            string topicUserHandle)
        {
            TopicFeedEntity topicFeedEntity = new TopicFeedEntity()
            {
                AppHandle = appHandle,
                TopicHandle = topicHandle,
                UserHandle = topicUserHandle
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.FollowingTopics);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.FollowingTopics, TableIdentifier.FollowingTopicsFeed) as FeedTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.InsertOrReplace(table, userHandle, appHandle, topicHandle, topicFeedEntity));
            transaction.Add(Operation.InsertOrReplace(table, userHandle, MasterApp.AppHandle, topicHandle, topicFeedEntity));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete following topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Delete following topic task</returns>
        public async Task DeleteFollowingTopic(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string topicHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.FollowingTopics);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.FollowingTopics, TableIdentifier.FollowingTopicsFeed) as FeedTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.Delete(table, userHandle, appHandle, topicHandle));
            transaction.Add(Operation.Delete(table, userHandle, MasterApp.AppHandle, topicHandle));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query following topics
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> QueryFollowingTopics(string userHandle, string appHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.FollowingTopics);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.FollowingTopics, TableIdentifier.FollowingTopicsFeed) as FeedTable;
            var result = await store.QueryFeedAsync<TopicFeedEntity>(table, userHandle, appHandle, cursor, limit);
            return result.ToList<ITopicFeedEntity>();
        }

        /// <summary>
        /// Insert featured topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">User handle</param>
        /// <returns>Insert featured topic task</returns>
        public async Task InsertFeaturedTopic(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicHandle,
            string topicUserHandle)
        {
            TopicFeedEntity topicFeedEntity = new TopicFeedEntity()
            {
                AppHandle = appHandle,
                TopicHandle = topicHandle,
                UserHandle = topicUserHandle
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.FeaturedTopics);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.FeaturedTopics, TableIdentifier.FeaturedTopicsFeed) as FeedTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.Insert(table, ContainerIdentifier.FeaturedTopics.ToString(), appHandle, topicHandle, topicFeedEntity));
            transaction.Add(Operation.Insert(table, ContainerIdentifier.FeaturedTopics.ToString(), MasterApp.AppHandle, topicHandle, topicFeedEntity));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete featured topic
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Delete featured topic task</returns>
        public async Task DeleteFeaturedTopic(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string topicHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.FeaturedTopics);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.FeaturedTopics, TableIdentifier.FeaturedTopicsFeed) as FeedTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.Delete(table, ContainerIdentifier.FeaturedTopics.ToString(), appHandle, topicHandle));
            transaction.Add(Operation.Delete(table, ContainerIdentifier.FeaturedTopics.ToString(), MasterApp.AppHandle, topicHandle));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query featured topics
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> QueryFeaturedTopics(string appHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.FeaturedTopics);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.FeaturedTopics, TableIdentifier.FeaturedTopicsFeed) as FeedTable;
            var result = await store.QueryFeedAsync<TopicFeedEntity>(table, ContainerIdentifier.FeaturedTopics.ToString(), appHandle, cursor, limit);
            return result.ToList<ITopicFeedEntity>();
        }

        /// <summary>
        /// Get feed key for popular topics table
        /// </summary>
        /// <param name="timeRange">Time range</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Feed key for popular </returns>
        private string GetPopularTopicsFeedKey(TimeRange timeRange, string appHandle)
        {
            return string.Join("+", timeRange.ToString(), appHandle);
        }
    }
}
