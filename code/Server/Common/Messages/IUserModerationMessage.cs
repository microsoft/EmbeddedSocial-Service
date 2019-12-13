// <copyright file="IUserModerationMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    /// <summary>
    /// User moderation message
    /// </summary>
    public interface IUserModerationMessage : IModerationMessage
    {
        /// <summary>
        /// Gets or sets the user handle
        /// </summary>
        string UserHandle { get; set; }
    }
}
