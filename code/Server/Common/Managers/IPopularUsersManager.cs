// <copyright file="IPopularUsersManager.cs" company="Microsoft">
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
    /// Popular users manager interface
    /// </summary>
    public interface IPopularUsersManager
    {
        /// <summary>
        /// Update popular user
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="incrementLikesCount">Increment likes count</param>
        /// <returns>Update popular user task</returns>
        Task UpdatePopularUser(
            ProcessType processType,
            string userHandle,
            string appHandle,
            long incrementLikesCount);

        /// <summary>
        /// Delete popular user
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete popular user task</returns>
        Task DeletePopularUser(
            ProcessType processType,
            string userHandle,
            string appHandle);

        /// <summary>
        /// Read popular users
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user feed entities</returns>
        Task<IList<IUserFeedEntity>> ReadPopularUsers(string appHandle, int cursor, int limit);
    }
}
