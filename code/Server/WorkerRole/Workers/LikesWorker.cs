// <copyright file="LikesWorker.cs" company="Microsoft">
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
    /// Likes worker
    /// </summary>
    public class LikesWorker : QueueWorker
    {
        /// <summary>
        /// Likes manager
        /// </summary>
        private ILikesManager likesManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LikesWorker"/> class
        /// </summary>
        /// <param name="log">log</param>
        /// <param name="likesQueue">Likes queue</param>
        /// <param name="likesManager">Likes manager</param>
        public LikesWorker(ILog log, ILikesQueue likesQueue, ILikesManager likesManager)
            : base(log)
        {
            this.Queue = likesQueue;
            this.likesManager = likesManager;
        }

        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Process message task</returns>
        protected override async Task Process(IMessage message)
        {
            if (message is LikeMessage)
            {
                LikeMessage likeMessage = message as LikeMessage;
                ProcessType processType = ProcessType.Backend;
                if (likeMessage.DequeueCount > 1)
                {
                    processType = ProcessType.BackendRetry;
                }

                var likeLookupEntity = await this.likesManager.ReadLike(likeMessage.ContentHandle, likeMessage.UserHandle);
                if (likeLookupEntity != null && likeLookupEntity.LastUpdatedTime > likeMessage.LastUpdatedTime)
                {
                    return;
                }

                await this.likesManager.UpdateLike(
                    processType,
                    likeMessage.LikeHandle,
                    likeMessage.ContentType,
                    likeMessage.ContentHandle,
                    likeMessage.UserHandle,
                    likeMessage.Liked,
                    likeMessage.ContentPublisherType,
                    likeMessage.ContentUserHandle,
                    likeMessage.ContentCreatedTime,
                    likeMessage.AppHandle,
                    likeMessage.LastUpdatedTime,
                    likeLookupEntity);
            }
        }
    }
}
