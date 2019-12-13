// <copyright file="UserLookupEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// User lookup entity class
    /// </summary>
    public class UserLookupEntity : ObjectEntity, IUserLookupEntity
    {
        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        public string UserHandle { get; set; }
    }
}
