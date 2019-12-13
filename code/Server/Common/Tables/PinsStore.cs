// <copyright file="PinsStore.cs" company="Microsoft">
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
    /// Pins store class
    /// </summary>
    public class PinsStore : IPinsStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PinsStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public PinsStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Update pin
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="pinHandle">Pin handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicUserHandle">User handle of the topic owner</param>
        /// <param name="pinned">Pin status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="readPinLookupEntity">Read pin lookup entity</param>
        /// <returns>Update pin task</returns>
        public async Task UpdatePin(
            StorageConsistencyMode storageConsistencyMode,
            string pinHandle,
            string userHandle,
            string appHandle,
            string topicHandle,
            string topicUserHandle,
            bool pinned,
            DateTime lastUpdatedTime,
            IPinLookupEntity readPinLookupEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Pins);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.Pins, TableIdentifier.PinsLookup) as ObjectTable;
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.Pins, TableIdentifier.PinsFeed) as FeedTable;
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.Pins, TableIdentifier.PinsCount) as CountTable;
            Transaction transaction = new Transaction();

            PinFeedEntity pinFeedEntity = new PinFeedEntity()
            {
                PinHandle = pinHandle,
                TopicHandle = topicHandle,
                TopicUserHandle = topicUserHandle,
                AppHandle = appHandle
            };

            if (readPinLookupEntity == null)
            {
                PinLookupEntity newPinLookupEntity = new PinLookupEntity()
                {
                    PinHandle = pinHandle,
                    LastUpdatedTime = lastUpdatedTime,
                    Pinned = pinned
                };

                transaction.Add(Operation.Insert(lookupTable, userHandle, topicHandle, newPinLookupEntity));

                if (pinned)
                {
                    transaction.Add(Operation.Insert(feedTable, userHandle, appHandle, pinHandle, pinFeedEntity));
                    transaction.Add(Operation.Insert(feedTable, userHandle, MasterApp.AppHandle, pinHandle, pinFeedEntity));
                    transaction.Add(Operation.InsertOrIncrement(countTable, userHandle, appHandle));
                    transaction.Add(Operation.InsertOrIncrement(countTable, userHandle, MasterApp.AppHandle));
                }
            }
            else
            {
                bool oldPinned = readPinLookupEntity.Pinned;
                string oldPinHandle = readPinLookupEntity.PinHandle;

                readPinLookupEntity.PinHandle = pinHandle;
                readPinLookupEntity.Pinned = pinned;
                readPinLookupEntity.LastUpdatedTime = lastUpdatedTime;

                transaction.Add(Operation.Replace(lookupTable, userHandle, topicHandle, readPinLookupEntity as PinLookupEntity));

                if (pinned == oldPinned)
                {
                    if (pinned && pinHandle != oldPinHandle)
                    {
                        transaction.Add(Operation.Delete(feedTable, userHandle, appHandle, oldPinHandle));
                        transaction.Add(Operation.Delete(feedTable, userHandle, MasterApp.AppHandle, oldPinHandle));
                        transaction.Add(Operation.Insert(feedTable, userHandle, appHandle, pinHandle, pinFeedEntity));
                        transaction.Add(Operation.Insert(feedTable, userHandle, MasterApp.AppHandle, pinHandle, pinFeedEntity));
                    }
                }
                else
                {
                    if (pinned)
                    {
                        transaction.Add(Operation.Insert(feedTable, userHandle, appHandle, pinHandle, pinFeedEntity));
                        transaction.Add(Operation.Insert(feedTable, userHandle, MasterApp.AppHandle, pinHandle, pinFeedEntity));
                        transaction.Add(Operation.Increment(countTable, userHandle, appHandle, 1));
                        transaction.Add(Operation.Increment(countTable, userHandle, MasterApp.AppHandle, 1));
                    }
                    else
                    {
                        transaction.Add(Operation.Delete(feedTable, userHandle, appHandle, oldPinHandle));
                        transaction.Add(Operation.Delete(feedTable, userHandle, MasterApp.AppHandle, oldPinHandle));
                        transaction.Add(Operation.Increment(countTable, userHandle, appHandle, -1.0));
                        transaction.Add(Operation.Increment(countTable, userHandle, MasterApp.AppHandle, -1.0));
                    }
                }
            }

            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query pin
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Pin lookup entity</returns>
        public async Task<IPinLookupEntity> QueryPin(string userHandle, string topicHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Pins);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.Pins, TableIdentifier.PinsLookup) as ObjectTable;
            return await store.QueryObjectAsync<PinLookupEntity>(lookupTable, userHandle, topicHandle);
        }

        /// <summary>
        /// Query pins for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of pin feed entities</returns>
        public async Task<IList<IPinFeedEntity>> QueryPins(string userHandle, string appHandle, string cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Pins);
            FeedTable feedTable = this.tableStoreManager.GetTable(ContainerIdentifier.Pins, TableIdentifier.PinsFeed) as FeedTable;
            var result = await store.QueryFeedAsync<PinFeedEntity>(feedTable, userHandle, appHandle, cursor, limit);
            return result.ToList<IPinFeedEntity>();
        }

        /// <summary>
        /// Query count of pins for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Pin count for a user handle and app handle</returns>
        public async Task<long?> QueryPinsCount(string userHandle, string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Pins);
            CountTable countTable = this.tableStoreManager.GetTable(ContainerIdentifier.Pins, TableIdentifier.PinsCount) as CountTable;
            CountEntity countEntity = await store.QueryCountAsync(countTable, userHandle, appHandle);
            if (countEntity == null)
            {
                return null;
            }

            return (long)countEntity.Count;
        }
    }
}
