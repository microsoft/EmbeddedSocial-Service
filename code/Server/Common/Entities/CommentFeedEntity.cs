// <copyright file="CommentFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// Comment feed entity class
    /// </summary>
    public class CommentFeedEntity : FeedEntity, ICommentFeedEntity
    {
        /// <summary>
        /// Gets or sets comment handle
        /// </summary>
        public string CommentHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle of the owner of the comment
        /// </summary>
        public string UserHandle { get; set; }
    }
}
