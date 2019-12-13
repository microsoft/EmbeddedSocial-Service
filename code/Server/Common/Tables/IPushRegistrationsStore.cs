// <copyright file="IPushRegistrationsStore.cs" company="Microsoft">
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
    /// Push Registrations store interface
    /// </summary>
    public interface IPushRegistrationsStore
    {
        /// <summary>
        /// Insert push registrations
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="platformType">Platform type</param>
        /// <param name="registrationId">OS registration id</param>
        /// <param name="hubRegistrationId">Hub registration id</param>
        /// <param name="language">Client language</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <returns>Insert push registration task</returns>
        Task InsertPushRegistration(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            PlatformType platformType,
            string registrationId,
            string hubRegistrationId,
            string language,
            DateTime lastUpdatedTime);

        /// <summary>
        /// Delete push registrations
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="registrationId">OS registration id</param>
        /// <returns>Delete push registration task</returns>
        Task DeletePushRegistration(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string registrationId);

        /// <summary>
        /// Query push registrations
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Push registration entities</returns>
        Task<IList<IPushRegistrationFeedEntity>> QueryPushRegistrations(
            string userHandle,
            string appHandle);

        /// <summary>
        /// Query push registration
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="registrationId">OS registration id</param>
        /// <returns>Push registration entities</returns>
        Task<IPushRegistrationFeedEntity> QueryPushRegistration(
            string userHandle,
            string appHandle,
            string registrationId);
    }
}
