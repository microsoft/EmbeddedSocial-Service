// <copyright file="TopicRelationshipsStore.cs" company="Microsoft">
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
    /// Topic relationships store class.
    ///
    /// Unlike user relationships, topic relationships are not symettric.
    /// For user relationships, both the UserFollowers feed and the UserFollowing feed are lists of users.
    /// For topic relationships:
    ///   The TopicFollowers feed is a list of user handles who are following a topic, indexed by the topic handle.
    ///   The TopicFollowing feed is a list of topic handles that a user is following, indexed by the user handle.
    /// </summary>
    public class TopicRelationshipsStore : ITopicRelationshipsStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicRelationshipsStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public TopicRelationshipsStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Update topic follower relationship.
        /// Follower user : someone who follows the topic.
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="relationshipHandle">Relationship handle</param>
        /// <param name="topicHandle">topic handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicRelationshipStatus">Topic relationship status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="readTopicRelationshipLookupEntity">Read topic relationship lookup entity</param>
        /// <returns>Update follower relationship task</returns>
        public async Task UpdateTopicFollowerRelationship(
            StorageConsistencyMode storageConsistencyMode,
            string relationshipHandle,
            string topicHandle,
            string relationshipUserHandle,
            string appHandle,
            TopicRelationshipStatus topicRelationshipStatus,
            DateTime lastUpdatedTime,
            ITopicRelationshipLookupEntity readTopicRelationshipLookupEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicFollowers);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicFollowers, TableIdentifier.TopicFollowersLookup) as ObjectTable;
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicFollowers, TableIdentifier.TopicFollowersFeed) as FeedTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicFollowers, TableIdentifier.TopicFollowersCount) as CountTable;

            string objectKey = this.GetObjectKey(appHandle, relationshipUserHandle);
            string feedKey = this.GetFeedKey(appHandle, topicRelationshipStatus);
            string countKey = this.GetCountKey(appHandle, topicRelationshipStatus);

            Transaction transaction = new Transaction();

            UserRelationshipFeedEntity relationshipFeedEntity = new UserRelationshipFeedEntity()
            {
                RelationshipHandle = relationshipHandle,
                UserHandle = relationshipUserHandle
            };

            if (readTopicRelationshipLookupEntity == null)
            {
                // if readTopicRelationshipLookupEntity is null, then we are inserting a new relationship
                TopicRelationshipLookupEntity newRelationshipLookupEntity = new TopicRelationshipLookupEntity()
                {
                    RelationshipHandle = relationshipHandle,
                    LastUpdatedTime = lastUpdatedTime,
                    TopicRelationshipStatus = topicRelationshipStatus,
                };

                transaction.Add(Operation.Insert(lookupTable, topicHandle, objectKey, newRelationshipLookupEntity));

                if (topicRelationshipStatus != TopicRelationshipStatus.None)
                {
                    transaction.Add(Operation.Insert(feedTable, topicHandle, feedKey, relationshipHandle, relationshipFeedEntity));
                    transaction.Add(Operation.InsertOrIncrement(countTable, topicHandle, countKey));
                }
            }
            else
            {
                // otherwise, we are updating an existing relationship
                TopicRelationshipStatus oldTopicRelationshipStatus = readTopicRelationshipLookupEntity.TopicRelationshipStatus;
                string oldRelationshipHandle = readTopicRelationshipLookupEntity.RelationshipHandle;
                string oldFeedKey = this.GetFeedKey(appHandle, oldTopicRelationshipStatus);
                string oldCountKey = this.GetCountKey(appHandle, oldTopicRelationshipStatus);

                readTopicRelationshipLookupEntity.RelationshipHandle = relationshipHandle;
                readTopicRelationshipLookupEntity.TopicRelationshipStatus = topicRelationshipStatus;
                readTopicRelationshipLookupEntity.LastUpdatedTime = lastUpdatedTime;

                transaction.Add(Operation.Replace(lookupTable, topicHandle, objectKey, readTopicRelationshipLookupEntity as TopicRelationshipLookupEntity));

                if (topicRelationshipStatus == oldTopicRelationshipStatus)
                {
                    if (topicRelationshipStatus != TopicRelationshipStatus.None && relationshipHandle != oldRelationshipHandle)
                    {
                        transaction.Add(Operation.Delete(feedTable, topicHandle, oldFeedKey, oldRelationshipHandle));
                        transaction.Add(Operation.Insert(feedTable, topicHandle, feedKey, relationshipHandle, relationshipFeedEntity));
                    }
                }
                else
                {
                    if (topicRelationshipStatus != TopicRelationshipStatus.None)
                    {
                        transaction.Add(Operation.Insert(feedTable, topicHandle, feedKey, relationshipHandle, relationshipFeedEntity));
                        transaction.Add(Operation.Increment(countTable, topicHandle, countKey));
                    }

                    if (oldTopicRelationshipStatus != TopicRelationshipStatus.None)
                    {
                        transaction.Add(Operation.Delete(feedTable, topicHandle, oldFeedKey, oldRelationshipHandle));
                        transaction.Add(Operation.Increment(countTable, topicHandle, oldCountKey, -1.0));
                    }
                }
            }

            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Update topic following relationship.
        /// Following topic : a topic that i am following.
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="relationshipHandle">Relationship handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipTopicHandle">Relationship topic handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicRelationshipStatus">Topic relationship status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="readTopicRelationshipLookupEntity">Read topic relationship lookup entity</param>
        /// <returns>Update following relationship task</returns>
        public async Task UpdateTopicFollowingRelationship(
            StorageConsistencyMode storageConsistencyMode,
            string relationshipHandle,
            string userHandle,
            string relationshipTopicHandle,
            string appHandle,
            TopicRelationshipStatus topicRelationshipStatus,
            DateTime lastUpdatedTime,
            ITopicRelationshipLookupEntity readTopicRelationshipLookupEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicFollowing);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicFollowing, TableIdentifier.TopicFollowingLookup) as ObjectTable;
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicFollowing, TableIdentifier.TopicFollowingFeed) as FeedTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicFollowing, TableIdentifier.TopicFollowingCount) as CountTable;

            string objectKey = this.GetObjectKey(appHandle, relationshipTopicHandle);
            string feedKey = this.GetFeedKey(appHandle, topicRelationshipStatus);
            string countKey = this.GetCountKey(appHandle, topicRelationshipStatus);

            Transaction transaction = new Transaction();

            TopicRelationshipFeedEntity relationshipFeedEntity = new TopicRelationshipFeedEntity()
            {
                RelationshipHandle = relationshipHandle,
                TopicHandle = relationshipTopicHandle
            };

            if (readTopicRelationshipLookupEntity == null)
            {
                // if readTopicRelationshipLookupEntity is null, then we are inserting a new relationship
                TopicRelationshipLookupEntity newRelationshipLookupEntity = new TopicRelationshipLookupEntity()
                {
                    RelationshipHandle = relationshipHandle,
                    LastUpdatedTime = lastUpdatedTime,
                    TopicRelationshipStatus = topicRelationshipStatus,
                };

                transaction.Add(Operation.Insert(lookupTable, userHandle, objectKey, newRelationshipLookupEntity));

                if (topicRelationshipStatus != TopicRelationshipStatus.None)
                {
                    transaction.Add(Operation.Insert(feedTable, userHandle, feedKey, relationshipHandle, relationshipFeedEntity));
                    transaction.Add(Operation.InsertOrIncrement(countTable, userHandle, countKey));
                }
            }
            else
            {
                // otherwise, we are updating an existing relationship
                TopicRelationshipStatus oldTopicRelationshipStatus = readTopicRelationshipLookupEntity.TopicRelationshipStatus;
                string oldRelationshipHandle = readTopicRelationshipLookupEntity.RelationshipHandle;
                string oldFeedKey = this.GetFeedKey(appHandle, oldTopicRelationshipStatus);
                string oldCountKey = this.GetCountKey(appHandle, oldTopicRelationshipStatus);

                readTopicRelationshipLookupEntity.RelationshipHandle = relationshipHandle;
                readTopicRelationshipLookupEntity.TopicRelationshipStatus = topicRelationshipStatus;
                readTopicRelationshipLookupEntity.LastUpdatedTime = lastUpdatedTime;

                transaction.Add(Operation.Replace(lookupTable, userHandle, objectKey, readTopicRelationshipLookupEntity as TopicRelationshipLookupEntity));

                if (topicRelationshipStatus == oldTopicRelationshipStatus)
                {
                    if (topicRelationshipStatus != TopicRelationshipStatus.None && relationshipHandle != oldRelationshipHandle)
                    {
                        transaction.Add(Operation.Delete(feedTable, userHandle, oldFeedKey, oldRelationshipHandle));
                        transaction.Add(Operation.Insert(feedTable, userHandle, feedKey, relationshipHandle, relationshipFeedEntity));
                    }
                }
                else
                {
                    if (topicRelationshipStatus != TopicRelationshipStatus.None)
                    {
                        transaction.Add(Operation.Insert(feedTable, userHandle, feedKey, relationshipHandle, relationshipFeedEntity));
                        transaction.Add(Operation.Increment(countTable, userHandle, countKey));
                    }

                    if (oldTopicRelationshipStatus != TopicRelationshipStatus.None)
                    {
                        transaction.Add(Operation.Delete(feedTable, userHandle, oldFeedKey, oldRelationshipHandle));
                        transaction.Add(Operation.Increment(countTable, userHandle, oldCountKey, -1.0));
                    }
                }
            }

            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query topic follower relationship
        /// Follower user : someone who follows the topic.
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User relationship lookup entity</returns>
        public async Task<ITopicRelationshipLookupEntity> QueryTopicFollowerRelationship(
            string topicHandle,
            string relationshipUserHandle,
            string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicFollowers);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicFollowers, TableIdentifier.TopicFollowersLookup) as ObjectTable;
            string objectKey = this.GetObjectKey(appHandle, relationshipUserHandle);
            return await store.QueryObjectAsync<TopicRelationshipLookupEntity>(lookupTable, topicHandle, objectKey);
        }

        /// <summary>
        /// Query topic following relationship.
        /// Following topic : a topic that i am following.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipTopicHandle">Relationship topic handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User relationship lookup entity</returns>
        public async Task<ITopicRelationshipLookupEntity> QueryTopicFollowingRelationship(
            string userHandle,
            string relationshipTopicHandle,
            string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicFollowing);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicFollowing, TableIdentifier.TopicFollowingLookup) as ObjectTable;
            string objectKey = this.GetObjectKey(appHandle, relationshipTopicHandle);
            return await store.QueryObjectAsync<TopicRelationshipLookupEntity>(lookupTable, userHandle, objectKey);
        }

        /// <summary>
        /// Query topic followers in an app.
        /// Follower users : users who follow the topic.
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        public async Task<IList<IUserRelationshipFeedEntity>> QueryTopicFollowers(string topicHandle, string appHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicFollowers);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicFollowers, TableIdentifier.TopicFollowersFeed) as FeedTable;
            string feedKey = this.GetFeedKey(appHandle, TopicRelationshipStatus.Follow);
            var result = await store.QueryFeedAsync<UserRelationshipFeedEntity>(feedTable, topicHandle, feedKey, cursor, limit);
            return result.ToList<IUserRelationshipFeedEntity>();
        }

        /// <summary>
        /// Query topic following in an app.
        /// Following topics : topics that i am following.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of topic relationship feed entities</returns>
        public async Task<IList<ITopicRelationshipFeedEntity>> QueryTopicFollowing(string userHandle, string appHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicFollowing);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicFollowing, TableIdentifier.TopicFollowingFeed) as FeedTable;
            string feedKey = this.GetFeedKey(appHandle, TopicRelationshipStatus.Follow);
            var result = await store.QueryFeedAsync<TopicRelationshipFeedEntity>(feedTable, userHandle, feedKey, cursor, limit);
            return result.ToList<ITopicRelationshipFeedEntity>();
        }

        /// <summary>
        /// Query count of topic followers in an app.
        /// This is the number of users who follow a given topic in an app.
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Topic followers count</returns>
        public async Task<long?> QueryTopicFollowersCount(string topicHandle, string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicFollowers);
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicFollowers, TableIdentifier.TopicFollowersCount) as CountTable;
            string countKey = this.GetCountKey(appHandle, TopicRelationshipStatus.Follow);
            CountEntity countEntity = await store.QueryCountAsync(countTable, topicHandle, countKey);
            if (countEntity == null)
            {
                return null;
            }

            return (long)countEntity.Count;
        }

        /// <summary>
        /// Query count of topic following in an app.
        /// This is the number of topics a user is following in an app.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Topic following count</returns>
        public async Task<long?> QueryTopicFollowingCount(string userHandle, string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.TopicFollowing);
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.TopicFollowing, TableIdentifier.TopicFollowingCount) as CountTable;
            string countKey = this.GetCountKey(appHandle, TopicRelationshipStatus.Follow);
            CountEntity countEntity = await store.QueryCountAsync(countTable, userHandle, countKey);
            if (countEntity == null)
            {
                return null;
            }

            return (long)countEntity.Count;
        }

        /// <summary>
        /// Get object key from app handle and lookup handle.
        ///
        /// The lookup handle may be either a topic handle or a user handle.
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="lookupHandle">Lookup handle</param>
        /// <returns>Object key</returns>
        private string GetObjectKey(string appHandle, string lookupHandle)
        {
            return string.Join("+", appHandle, lookupHandle);
        }

        /// <summary>
        /// Get feed key from app handle and relationship status
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="relationshipStatus">Relationship status</param>
        /// <returns>Feed key</returns>
        private string GetFeedKey(string appHandle, TopicRelationshipStatus relationshipStatus)
        {
            return string.Join("+", appHandle, relationshipStatus.ToString());
        }

        /// <summary>
        /// Get count key from app handle and relationship status
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="relationshipStatus">Relationship status</param>
        /// <returns>Count key</returns>
        private string GetCountKey(string appHandle, TopicRelationshipStatus relationshipStatus)
        {
            return string.Join("+", appHandle, relationshipStatus.ToString());
        }
    }
}
