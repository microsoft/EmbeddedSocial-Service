// <copyright file="IResizeImagesQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System.Threading.Tasks;

    using SocialPlus.Models;

    /// <summary>
    /// Resize images queue interface
    /// </summary>
    public interface IResizeImagesQueue : IQueueBase
    {
        /// <summary>
        /// Send resize image message
        /// </summary>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="imageType">Image type</param>
        /// <returns>Send message task</returns>
        Task SendResizeImageMessage(string blobHandle, ImageType imageType);
    }
}
