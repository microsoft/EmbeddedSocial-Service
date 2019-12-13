// <copyright file="ISearchRemoveTopicMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    /// <summary>
    /// Search remove topic message interface
    /// </summary>
    public interface ISearchRemoveTopicMessage : IMessage
    {
        /// <summary>
        /// Gets or sets TopicHandle of the topic to remove
        /// </summary>
        string TopicHandle { get; set; }
    }
}
