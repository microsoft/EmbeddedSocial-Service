// <copyright file="FollowingImportsWorker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Workers
{
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Queues;

    /// <summary>
    /// Following imports worker
    /// </summary>
    public class FollowingImportsWorker : QueueWorker
    {
        /// <summary>
        /// Topics manager
        /// </summary>
        private ITopicsManager topicsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="FollowingImportsWorker"/> class
        /// </summary>
        /// <param name="log">log</param>
        /// <param name="followingImportsQueue">Following imports queue</param>
        /// <param name="topicsManager">Topics manager</param>
        public FollowingImportsWorker(ILog log, IFollowingImportsQueue followingImportsQueue, ITopicsManager topicsManager)
            : base(log)
        {
            this.Queue = followingImportsQueue;
            this.topicsManager = topicsManager;
        }

        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Process message task</returns>
        protected override async Task Process(IMessage message)
        {
            if (message is FollowingImportMessage)
            {
                FollowingImportMessage followingImportMessage = message as FollowingImportMessage;
                await this.topicsManager.ImportTopics(
                    followingImportMessage.UserHandle,
                    followingImportMessage.AppHandle,
                    followingImportMessage.FollowingUserHandle);
            }
        }
    }
}
