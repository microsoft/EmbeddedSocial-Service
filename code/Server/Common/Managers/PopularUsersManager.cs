// <copyright file="PopularUsersManager.cs" company="Microsoft">
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
    /// Popular users manager class
    /// </summary>
    public class PopularUsersManager : IPopularUsersManager
    {
        /// <summary>
        /// Users store
        /// </summary>
        private IUsersStore usersStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="PopularUsersManager"/> class
        /// </summary>
        /// <param name="usersStore">Users store</param>
        public PopularUsersManager(IUsersStore usersStore)
        {
            this.usersStore = usersStore;
        }

        /// <summary>
        /// Update popular user
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="followersCount">Followers count</param>
        /// <returns>Update popular user task</returns>
        public async Task UpdatePopularUser(
            ProcessType processType,
            string userHandle,
            string appHandle,
            long followersCount)
        {
            await this.usersStore.InsertPopularUser(StorageConsistencyMode.Strong, userHandle, appHandle, followersCount);
        }

        /// <summary>
        /// Delete popular user
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete popular user task</returns>
        public async Task DeletePopularUser(
            ProcessType processType,
            string userHandle,
            string appHandle)
        {
            await this.usersStore.DeletePopularUser(StorageConsistencyMode.Strong, userHandle, appHandle);
        }

        /// <summary>
        /// Read popular users
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user feed entities</returns>
        public async Task<IList<IUserFeedEntity>> ReadPopularUsers(string appHandle, int cursor, int limit)
        {
            return await this.usersStore.QueryPopularUsers(appHandle, cursor, limit);
        }
    }
}
