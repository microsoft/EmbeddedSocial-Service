// <copyright file="IPinsStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Server.Entities;

    /// <summary>
    /// Pins store interface
    /// </summary>
    public interface IPinsStore
    {
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
        Task UpdatePin(
            StorageConsistencyMode storageConsistencyMode,
            string pinHandle,
            string userHandle,
            string appHandle,
            string topicHandle,
            string topicUserHandle,
            bool pinned,
            DateTime lastUpdatedTime,
            IPinLookupEntity readPinLookupEntity);

        /// <summary>
        /// Query pin
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Pin lookup entity</returns>
        Task<IPinLookupEntity> QueryPin(string userHandle, string topicHandle);

        /// <summary>
        /// Query pins for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of pin feed entities</returns>
        Task<IList<IPinFeedEntity>> QueryPins(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Query count of pins for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Pin count for a user handle and app handle</returns>
        Task<long?> QueryPinsCount(string userHandle, string appHandle);
    }
}
