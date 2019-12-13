// <copyright file="ResizeImagesQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Resize images queue class
    /// </summary>
    public class ResizeImagesQueue : QueueBase, IResizeImagesQueue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResizeImagesQueue"/> class
        /// </summary>
        /// <param name="queueManager">queue manager</param>
        public ResizeImagesQueue(IQueueManager queueManager)
            : base(queueManager)
        {
            this.QueueIdentifier = QueueIdentifier.ResizeImages;
        }

        /// <summary>
        /// Send resize image message
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="imageType">Image type</param>
        /// <returns>Send message task</returns>
        public async Task SendResizeImageMessage(string blobHandle, ImageType imageType)
        {
            ResizeImageMessage message = new ResizeImageMessage()
            {
                BlobHandle = blobHandle,
                ImageType = imageType
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }
    }
}
