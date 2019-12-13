// <copyright file="PinsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Pins manager class
    /// </summary>
    public class PinsManager : IPinsManager
    {
        /// <summary>
        /// Pins store
        /// </summary>
        private IPinsStore pinsStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="PinsManager"/> class
        /// </summary>
        /// <param name="pinsStore">Pins store</param>
        public PinsManager(IPinsStore pinsStore)
        {
            this.pinsStore = pinsStore;
        }

        /// <summary>
        /// Update pin
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="pinHandle">Pin handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="pinned">Pin status</param>
        /// <param name="topicPublisherType">Topic publisher type</param>
        /// <param name="topicUserHandle">Topic user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="pinLookupEntity">Pin lookup entity</param>
        /// <returns>Update pin task</returns>
        public async Task UpdatePin(
            ProcessType processType,
            string pinHandle,
            string userHandle,
            string topicHandle,
            bool pinned,
            PublisherType topicPublisherType,
            string topicUserHandle,
            string appHandle,
            DateTime lastUpdatedTime,
            IPinLookupEntity pinLookupEntity)
        {
            await this.pinsStore.UpdatePin(
                StorageConsistencyMode.Strong,
                pinHandle,
                userHandle,
                appHandle,
                topicHandle,
                topicUserHandle,
                pinned,
                lastUpdatedTime,
                pinLookupEntity);
        }

        /// <summary>
        /// Read pin
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Pin lookup entity</returns>
        public async Task<IPinLookupEntity> ReadPin(string userHandle, string topicHandle)
        {
            return await this.pinsStore.QueryPin(userHandle, topicHandle);
        }

        /// <summary>
        /// Read pins for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of pin feed entities</returns>
        public async Task<IList<IPinFeedEntity>> ReadPins(string userHandle, string appHandle, string cursor, int limit)
        {
            return await this.pinsStore.QueryPins(userHandle, appHandle, cursor, limit);
        }

        /// <summary>
        /// Read count of pins for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Pins count for a user in an app</returns>
        public async Task<long?> ReadPinsCount(string userHandle, string appHandle)
        {
            return await this.pinsStore.QueryPinsCount(userHandle, appHandle);
        }
    }
}
