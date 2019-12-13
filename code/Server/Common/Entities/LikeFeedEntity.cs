// <copyright file="LikeFeedEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// Like feed entity class
    /// </summary>
    public class LikeFeedEntity : FeedEntity, ILikeFeedEntity
    {
        /// <summary>
        /// Gets or sets like handle
        /// </summary>
        public string LikeHandle { get; set; }

        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        public string UserHandle { get; set; }
    }
}
