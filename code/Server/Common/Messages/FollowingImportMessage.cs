// <copyright file="FollowingImportMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Following import message class
    /// </summary>
    public class FollowingImportMessage : QueueMessage, IFollowingImportMessage
    {
        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets following user handle
        /// </summary>
        public string FollowingUserHandle { get; set; }
    }
}
