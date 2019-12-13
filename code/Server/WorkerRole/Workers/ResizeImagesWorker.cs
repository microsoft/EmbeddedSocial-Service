// <copyright file="ResizeImagesWorker.cs" company="Microsoft">
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
    /// Resize images worker
    /// </summary>
    public class ResizeImagesWorker : QueueWorker
    {
        /// <summary>
        /// Blobs manager
        /// </summary>
        private IBlobsManager blobsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResizeImagesWorker"/> class
        /// </summary>
        /// <param name="log">log</param>
        /// <param name="resizeImagesQueue">Resize images queue</param>
        /// <param name="blobsManager">Blobs manager</param>
        public ResizeImagesWorker(ILog log, IResizeImagesQueue resizeImagesQueue, IBlobsManager blobsManager)
            : base(log)
        {
            this.Queue = resizeImagesQueue;
            this.blobsManager = blobsManager;
        }

        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Process message task</returns>
        protected override async Task Process(IMessage message)
        {
            if (message is ResizeImageMessage)
            {
                ResizeImageMessage resizeImageMessage = message as ResizeImageMessage;
                if (resizeImageMessage.DequeueCount == 1)
                {
                    await this.blobsManager.CreateImageResizes(ProcessType.Backend, resizeImageMessage.BlobHandle, resizeImageMessage.ImageType);
                }
                else
                {
                    await this.blobsManager.CreateImageResizes(ProcessType.BackendRetry, resizeImageMessage.BlobHandle, resizeImageMessage.ImageType);
                }
            }
        }
    }
}
