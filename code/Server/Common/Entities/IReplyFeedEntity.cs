// <copyright file="IReplyFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// Reply feed entity interface
    /// </summary>
    public interface IReplyFeedEntity
    {
        /// <summary>
        /// Gets or sets reply handle
        /// </summary>
        string ReplyHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle of the owner of the reply
        /// </summary>
        string UserHandle { get; set; }
    }
}
