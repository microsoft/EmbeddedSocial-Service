// <copyright file="IFollowingImportsQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System.Threading.Tasks;

    /// <summary>
    /// Following imports queue interface
    /// </summary>
    public interface IFollowingImportsQueue : IQueueBase
    {
        /// <summary>
        /// Send following import message
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="followingUserHandle">Following user handle</param>
        /// <returns>Send message task</returns>
        Task SendFollowingImportMessage(string userHandle, string appHandle, string followingUserHandle);
    }
}
