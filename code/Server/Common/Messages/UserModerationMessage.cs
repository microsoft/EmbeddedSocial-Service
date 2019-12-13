// <copyright file="UserModerationMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    /// <summary>
    /// User moderation message
    /// </summary>
    public class UserModerationMessage : ModerationMessage, IUserModerationMessage
    {
        /// <summary>
        /// Gets or sets the user handle
        /// </summary>
        public string UserHandle { get; set; }
    }
}
