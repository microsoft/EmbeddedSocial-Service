// <copyright file="LikesStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// Likes store class
    /// </summary>
    public class LikesStore : ILikesStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LikesStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public LikesStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

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
        public async Task UpdateLike(
            StorageConsistencyMode storageConsistencyMode,
            string likeHandle,
            string contentHandle,
            string userHandle,
            bool liked,
            DateTime lastUpdatedTime,
            ILikeLookupEntity readLikeLookupEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Likes);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.Likes, TableIdentifier.LikesLookup) as ObjectTable;
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.Likes, TableIdentifier.LikesFeed) as FeedTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.Likes, TableIdentifier.LikesCount) as CountTable;
            Transaction transaction = new Transaction();

            LikeFeedEntity likeFeedEntity = new LikeFeedEntity()
            {
                LikeHandle = likeHandle,
                UserHandle = userHandle
            };

            if (readLikeLookupEntity == null)
            {
                LikeLookupEntity newLikeLookupEntity = new LikeLookupEntity()
                {
                    LikeHandle = likeHandle,
                    LastUpdatedTime = lastUpdatedTime,
                    Liked = liked
                };

                transaction.Add(Operation.Insert(lookupTable, contentHandle, userHandle, newLikeLookupEntity));

                if (liked)
                {
                    transaction.Add(Operation.Insert(feedTable, contentHandle, this.tableStoreManager.DefaultFeedKey, likeHandle, likeFeedEntity));
                    transaction.Add(Operation.InsertOrIncrement(countTable, contentHandle, this.tableStoreManager.DefaultCountKey));
                }
            }
            else
            {
                bool oldLiked = readLikeLookupEntity.Liked;
                string oldLikeHandle = readLikeLookupEntity.LikeHandle;

                readLikeLookupEntity.LikeHandle = likeHandle;
                readLikeLookupEntity.Liked = liked;
                readLikeLookupEntity.LastUpdatedTime = lastUpdatedTime;

                transaction.Add(Operation.Replace(lookupTable, contentHandle, userHandle, readLikeLookupEntity as LikeLookupEntity));

                if (liked == oldLiked)
                {
                    if (liked && likeHandle != oldLikeHandle)
                    {
                        transaction.Add(Operation.Delete(feedTable, contentHandle, this.tableStoreManager.DefaultFeedKey, oldLikeHandle));
                        transaction.Add(Operation.Insert(feedTable, contentHandle, this.tableStoreManager.DefaultFeedKey, likeHandle, likeFeedEntity));
                    }
                }
                else
                {
                    if (liked)
                    {
                        transaction.Add(Operation.Insert(feedTable, contentHandle, this.tableStoreManager.DefaultFeedKey, likeHandle, likeFeedEntity));
                        transaction.Add(Operation.Increment(countTable, contentHandle, this.tableStoreManager.DefaultCountKey, 1));
                    }
                    else
                    {
                        transaction.Add(Operation.Delete(feedTable, contentHandle, this.tableStoreManager.DefaultFeedKey, oldLikeHandle));
                        transaction.Add(Operation.Increment(countTable, contentHandle, this.tableStoreManager.DefaultCountKey, -1.0));
                    }
                }
            }

            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query like
        /// </summary>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Like lookup entity</returns>
        public async Task<ILikeLookupEntity> QueryLike(string contentHandle, string userHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Likes);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.Likes, TableIdentifier.LikesLookup) as ObjectTable;
            return await store.QueryObjectAsync<LikeLookupEntity>(lookupTable, contentHandle, userHandle);
        }

        /// <summary>
        /// Query likes for a content
        /// </summary>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of like feed entities</returns>
        public async Task<IList<ILikeFeedEntity>> QueryLikes(string contentHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Likes);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.Likes, TableIdentifier.LikesFeed) as FeedTable;
            var result = await store.QueryFeedAsync<LikeFeedEntity>(feedTable, contentHandle, this.tableStoreManager.DefaultFeedKey, cursor, limit);
            return result.ToList<ILikeFeedEntity>();
        }

        /// <summary>
        /// Query count of likes for a content
        /// </summary>
        /// <param name="contentHandle">Content handle</param>
        /// <returns>Likes count for a content</returns>
        public async Task<long?> QueryLikesCount(string contentHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Likes);
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.Likes, TableIdentifier.LikesCount) as CountTable;
            CountEntity countEntity = await store.QueryCountAsync(countTable, contentHandle, this.tableStoreManager.DefaultCountKey);
            if (countEntity == null)
            {
                return null;
            }

            return (long)countEntity.Count;
        }
    }
}
