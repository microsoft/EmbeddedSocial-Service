// <copyright file="IFanoutTopicsQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System.Threading.Tasks;

    /// <summary>
    /// <c>Fanout</c> topics queue interface
    /// </summary>
    public interface IFanoutTopicsQueue : IQueueBase
    {
        /// <summary>
        /// Send <c>fanout</c> topic message
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Send message task</returns>
        Task SendFanoutTopicMessage(string userHandle, string appHandle, string topicHandle);
    }
}
