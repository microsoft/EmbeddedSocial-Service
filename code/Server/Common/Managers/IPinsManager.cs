// <copyright file="IPinsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Pins manager interface
    /// </summary>
    public interface IPinsManager
    {
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
        /// <param name="pinLookupEntity">Pin lookup time</param>
        /// <returns>Update pin task</returns>
        Task UpdatePin(
            ProcessType processType,
            string pinHandle,
            string userHandle,
            string topicHandle,
            bool pinned,
            PublisherType topicPublisherType,
            string topicUserHandle,
            string appHandle,
            DateTime lastUpdatedTime,
            IPinLookupEntity pinLookupEntity);

        /// <summary>
        /// Read pin
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Pin lookup entity</returns>
        Task<IPinLookupEntity> ReadPin(string userHandle, string topicHandle);

        /// <summary>
        /// Read pins for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of pin feed entities</returns>
        Task<IList<IPinFeedEntity>> ReadPins(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Read count of pins for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Pins count for a user in an app</returns>
        Task<long?> ReadPinsCount(string userHandle, string appHandle);
    }
}
