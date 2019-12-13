// <copyright file="IFollowingImportMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    /// <summary>
    /// Following import message interface
    /// </summary>
    public interface IFollowingImportMessage : IMessage
    {
        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets following user handle
        /// </summary>
        string FollowingUserHandle { get; set; }
    }
}
