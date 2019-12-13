// <copyright file="ReplyFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// Reply feed entity class
    /// </summary>
    public class ReplyFeedEntity : FeedEntity, IReplyFeedEntity
    {
        /// <summary>
        /// Gets or sets reply handle
        /// </summary>
        public string ReplyHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle of the owner of the reply
        /// </summary>
        public string UserHandle { get; set; }
    }
}
