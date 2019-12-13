// <copyright file="UserRelationshipsStore.cs" company="Microsoft">
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
    /// User relationships store class
    /// </summary>
    public class UserRelationshipsStore : IUserRelationshipsStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRelationshipsStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public UserRelationshipsStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Update follower relationship.
        /// Follower user : someone who follows me.
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="relationshipHandle">Relationship handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userRelationshipStatus">User relationship status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="readUserRelationshipLookupEntity">Read user relationship lookup entity</param>
        /// <returns>Update follower relationship task</returns>
        public async Task UpdateFollowerRelationship(
            StorageConsistencyMode storageConsistencyMode,
            string relationshipHandle,
            string userHandle,
            string relationshipUserHandle,
            string appHandle,
            UserRelationshipStatus userRelationshipStatus,
            DateTime lastUpdatedTime,
            IUserRelationshipLookupEntity readUserRelationshipLookupEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserFollowers);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowers, TableIdentifier.FollowersLookup) as ObjectTable;
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowers, TableIdentifier.FollowersFeed) as FeedTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowers, TableIdentifier.FollowersCount) as CountTable;

            await this.UpdateUserRelationship(
                storageConsistencyMode,
                relationshipHandle,
                userHandle,
                relationshipUserHandle,
                appHandle,
                userRelationshipStatus,
                lastUpdatedTime,
                readUserRelationshipLookupEntity,
                store,
                lookupTable,
                feedTable,
                countTable);
        }

        /// <summary>
        /// Update following relationship.
        /// Following user : someone who i am following.
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="relationshipHandle">Relationship handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userRelationshipStatus">User relationship status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="readUserRelationshipLookupEntity">Read user relationship lookup entity</param>
        /// <returns>Update following relationship task</returns>
        public async Task UpdateFollowingRelationship(
            StorageConsistencyMode storageConsistencyMode,
            string relationshipHandle,
            string userHandle,
            string relationshipUserHandle,
            string appHandle,
            UserRelationshipStatus userRelationshipStatus,
            DateTime lastUpdatedTime,
            IUserRelationshipLookupEntity readUserRelationshipLookupEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserFollowing);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowing, TableIdentifier.FollowingLookup) as ObjectTable;
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowing, TableIdentifier.FollowingFeed) as FeedTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowing, TableIdentifier.FollowingCount) as CountTable;

            await this.UpdateUserRelationship(
                storageConsistencyMode,
                relationshipHandle,
                userHandle,
                relationshipUserHandle,
                appHandle,
                userRelationshipStatus,
                lastUpdatedTime,
                readUserRelationshipLookupEntity,
                store,
                lookupTable,
                feedTable,
                countTable);
        }

        /// <summary>
        /// Query follower relationship.
        /// Follower user : someone who follows me.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipUserHandle">Relationship handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User relationship lookup entity</returns>
        public async Task<IUserRelationshipLookupEntity> QueryFollowerRelationship(
            string userHandle,
            string relationshipUserHandle,
            string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserFollowers);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowers, TableIdentifier.FollowersLookup) as ObjectTable;
            string objectKey = this.GetObjectKey(appHandle, relationshipUserHandle);
            return await store.QueryObjectAsync<UserRelationshipLookupEntity>(lookupTable, userHandle, objectKey);
        }

        /// <summary>
        /// Query following relationship.
        /// Following user : someone who i am following.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipUserHandle">Relationship handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User relationship lookup entity</returns>
        public async Task<IUserRelationshipLookupEntity> QueryFollowingRelationship(
            string userHandle,
            string relationshipUserHandle,
            string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserFollowing);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowing, TableIdentifier.FollowingLookup) as ObjectTable;
            string objectKey = this.GetObjectKey(appHandle, relationshipUserHandle);
            return await store.QueryObjectAsync<UserRelationshipLookupEntity>(lookupTable, userHandle, objectKey);
        }

        /// <summary>
        /// Query followers for a user in an app.
        /// Follower users : people who follow me.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        public async Task<IList<IUserRelationshipFeedEntity>> QueryFollowers(string userHandle, string appHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserFollowers);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowers, TableIdentifier.FollowersFeed) as FeedTable;
            string feedKey = this.GetFeedKey(appHandle, UserRelationshipStatus.Follow);
            var result = await store.QueryFeedAsync<UserRelationshipFeedEntity>(feedTable, userHandle, feedKey, cursor, limit);
            return result.ToList<IUserRelationshipFeedEntity>();
        }

        /// <summary>
        /// Query count of followers for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Followers count for a user handle and app handle</returns>
        public async Task<long?> QueryFollowersCount(string userHandle, string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserFollowers);
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowers, TableIdentifier.FollowersCount) as CountTable;
            string countKey = this.GetCountKey(appHandle, UserRelationshipStatus.Follow);
            CountEntity countEntity = await store.QueryCountAsync(countTable, userHandle, countKey);
            if (countEntity == null)
            {
                return null;
            }

            return (long)countEntity.Count;
        }

        /// <summary>
        /// Query following for a user in an app.
        /// Following users : people who i am following.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        public async Task<IList<IUserRelationshipFeedEntity>> QueryFollowing(string userHandle, string appHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserFollowing);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowing, TableIdentifier.FollowingFeed) as FeedTable;
            string feedKey = this.GetFeedKey(appHandle, UserRelationshipStatus.Follow);
            var result = await store.QueryFeedAsync<UserRelationshipFeedEntity>(feedTable, userHandle, feedKey, cursor, limit);
            return result.ToList<IUserRelationshipFeedEntity>();
        }

        /// <summary>
        /// Query count of following for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Following count for a user handle and app handle</returns>
        public async Task<long?> QueryFollowingCount(string userHandle, string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserFollowing);
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowing, TableIdentifier.FollowingCount) as CountTable;
            string countKey = this.GetCountKey(appHandle, UserRelationshipStatus.Follow);
            CountEntity countEntity = await store.QueryCountAsync(countTable, userHandle, countKey);
            if (countEntity == null)
            {
                return null;
            }

            return (long)countEntity.Count;
        }

        /// <summary>
        /// Query pending users for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        public async Task<IList<IUserRelationshipFeedEntity>> QueryPendingUsers(string userHandle, string appHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserFollowers);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowers, TableIdentifier.FollowersFeed) as FeedTable;
            string feedKey = this.GetFeedKey(appHandle, UserRelationshipStatus.Pending);
            var result = await store.QueryFeedAsync<UserRelationshipFeedEntity>(feedTable, userHandle, feedKey, cursor, limit);
            return result.ToList<IUserRelationshipFeedEntity>();
        }

        /// <summary>
        /// Query count of pending users for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Pending users count for a user handle and app handle</returns>
        public async Task<long?> QueryPendingUsersCount(string userHandle, string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserFollowers);
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowers, TableIdentifier.FollowersCount) as CountTable;
            string countKey = this.GetCountKey(appHandle, UserRelationshipStatus.Pending);
            CountEntity countEntity = await store.QueryCountAsync(countTable, userHandle, countKey);
            if (countEntity == null)
            {
                return null;
            }

            return (long)countEntity.Count;
        }

        /// <summary>
        /// Query blocked users for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        public async Task<IList<IUserRelationshipFeedEntity>> QueryBlockedUsers(string userHandle, string appHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserFollowers);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowers, TableIdentifier.FollowersFeed) as FeedTable;
            string feedKey = this.GetFeedKey(appHandle, UserRelationshipStatus.Blocked);
            var result = await store.QueryFeedAsync<UserRelationshipFeedEntity>(feedTable, userHandle, feedKey, cursor, limit);
            return result.ToList<IUserRelationshipFeedEntity>();
        }

        /// <summary>
        /// Query count of blocked users for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Blocked users count for a user handle and app handle</returns>
        public async Task<long?> QueryBlockedUsersCount(string userHandle, string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserFollowers);
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserFollowers, TableIdentifier.FollowersCount) as CountTable;
            string countKey = this.GetCountKey(appHandle, UserRelationshipStatus.Blocked);
            CountEntity countEntity = await store.QueryCountAsync(countTable, userHandle, countKey);
            if (countEntity == null)
            {
                return null;
            }

            return (long)countEntity.Count;
        }

        /// <summary>
        /// Update user relationship
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="relationshipHandle">Relationship handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userRelationshipStatus">Relationship status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="readUserRelationshipLookupEntity">Read user relationship lookup entity</param>
        /// <param name="store">store object</param>
        /// <param name="lookupTable">lookup table</param>
        /// <param name="feedTable">feed table</param>
        /// <param name="countTable">count table</param>
        /// <returns>Update follower relationship task</returns>
        private async Task UpdateUserRelationship(
            StorageConsistencyMode storageConsistencyMode,
            string relationshipHandle,
            string userHandle,
            string relationshipUserHandle,
            string appHandle,
            UserRelationshipStatus userRelationshipStatus,
            DateTime lastUpdatedTime,
            IUserRelationshipLookupEntity readUserRelationshipLookupEntity,
            CTStore store,
            ObjectTable lookupTable,
            FeedTable feedTable,
            CountTable countTable)
        {
            string objectKey = this.GetObjectKey(appHandle, relationshipUserHandle);
            string feedKey = this.GetFeedKey(appHandle, userRelationshipStatus);
            string countKey = this.GetCountKey(appHandle, userRelationshipStatus);

            Transaction transaction = new Transaction();

            UserRelationshipFeedEntity relationshipFeedEntity = new UserRelationshipFeedEntity()
            {
                RelationshipHandle = relationshipHandle,
                UserHandle = relationshipUserHandle
            };

            if (readUserRelationshipLookupEntity == null)
            {
                UserRelationshipLookupEntity newRelationshipLookupEntity = new UserRelationshipLookupEntity()
                {
                    RelationshipHandle = relationshipHandle,
                    LastUpdatedTime = lastUpdatedTime,
                    UserRelationshipStatus = userRelationshipStatus,
                };

                transaction.Add(Operation.Insert(lookupTable, userHandle, objectKey, newRelationshipLookupEntity));

                if (userRelationshipStatus != UserRelationshipStatus.None)
                {
                    transaction.Add(Operation.Insert(feedTable, userHandle, feedKey, relationshipHandle, relationshipFeedEntity));
                    transaction.Add(Operation.InsertOrIncrement(countTable, userHandle, countKey));
                }
            }
            else
            {
                UserRelationshipStatus oldUserRelationshipStatus = readUserRelationshipLookupEntity.UserRelationshipStatus;
                string oldRelationshipHandle = readUserRelationshipLookupEntity.RelationshipHandle;

                readUserRelationshipLookupEntity.RelationshipHandle = relationshipHandle;
                readUserRelationshipLookupEntity.UserRelationshipStatus = userRelationshipStatus;
                readUserRelationshipLookupEntity.LastUpdatedTime = lastUpdatedTime;

                string oldFeedKey = this.GetFeedKey(appHandle, oldUserRelationshipStatus);
                string oldCountKey = this.GetCountKey(appHandle, oldUserRelationshipStatus);

                transaction.Add(Operation.Replace(lookupTable, userHandle, objectKey, readUserRelationshipLookupEntity as UserRelationshipLookupEntity));

                if (userRelationshipStatus == oldUserRelationshipStatus)
                {
                    if (userRelationshipStatus != UserRelationshipStatus.None && relationshipHandle != oldRelationshipHandle)
                    {
                        transaction.Add(Operation.Delete(feedTable, userHandle, oldFeedKey, oldRelationshipHandle));
                        transaction.Add(Operation.Insert(feedTable, userHandle, feedKey, relationshipHandle, relationshipFeedEntity));
                    }
                }
                else
                {
                    if (userRelationshipStatus != UserRelationshipStatus.None)
                    {
                        transaction.Add(Operation.Insert(feedTable, userHandle, feedKey, relationshipHandle, relationshipFeedEntity));
                        transaction.Add(Operation.InsertOrIncrement(countTable, userHandle, countKey));
                    }

                    if (oldUserRelationshipStatus != UserRelationshipStatus.None)
                    {
                        transaction.Add(Operation.Delete(feedTable, userHandle, oldFeedKey, oldRelationshipHandle));
                        transaction.Add(Operation.Increment(countTable, userHandle, oldCountKey, -1.0));
                    }
                }
            }

            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Get object key from app handle and user handle
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Object key</returns>
        private string GetObjectKey(string appHandle, string userHandle)
        {
            return string.Join("+", appHandle, userHandle);
        }

        /// <summary>
        /// Get feed key from app handle and relationship status
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="relationshipStatus">Relationship status</param>
        /// <returns>Feed key</returns>
        private string GetFeedKey(string appHandle, UserRelationshipStatus relationshipStatus)
        {
            return string.Join("+", appHandle, relationshipStatus.ToString());
        }

        /// <summary>
        /// Get count key from app handle and relationship status
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="relationshipStatus">Relationship status</param>
        /// <returns>Count key</returns>
        private string GetCountKey(string appHandle, UserRelationshipStatus relationshipStatus)
        {
            return string.Join("+", appHandle, relationshipStatus.ToString());
        }
    }
}
