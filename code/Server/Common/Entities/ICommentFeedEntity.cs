// <copyright file="ICommentFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// Comment feed entity interface
    /// </summary>
    public interface ICommentFeedEntity
    {
        /// <summary>
        /// Gets or sets comment handle
        /// </summary>
        string CommentHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle of the owner of the comment
        /// </summary>
        string UserHandle { get; set; }
    }
}
