// <copyright file="ILikeFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// Like feed entity interface
    /// </summary>
    public interface ILikeFeedEntity
    {
        /// <summary>
        /// Gets or sets like handle
        /// </summary>
        string LikeHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        string UserHandle { get; set; }
    }
}
