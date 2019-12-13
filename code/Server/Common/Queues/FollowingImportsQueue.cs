// <copyright file="FollowingImportsQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System.Threading.Tasks;

    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Following imports queue class
    /// </summary>
    public class FollowingImportsQueue : QueueBase, IFollowingImportsQueue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FollowingImportsQueue"/> class
        /// </summary>
        /// <param name="queueManager">queue manager</param>
        public FollowingImportsQueue(IQueueManager queueManager)
            : base(queueManager)
        {
            this.QueueIdentifier = QueueIdentifier.FollowingImports;
        }

        /// <summary>
        /// Send following import message
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="followingUserHandle">Following user handle</param>
        /// <returns>Send message task</returns>
        public async Task SendFollowingImportMessage(string userHandle, string appHandle, string followingUserHandle)
        {
            FollowingImportMessage message = new FollowingImportMessage()
            {
                UserHandle = userHandle,
                AppHandle = appHandle,
                FollowingUserHandle = followingUserHandle
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }
    }
}
