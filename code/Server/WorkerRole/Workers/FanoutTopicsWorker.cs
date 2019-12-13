// <copyright file="FanoutTopicsWorker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Workers
{
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Queues;

    /// <summary>
    /// <c>Fanout</c> topics worker
    /// </summary>
    public class FanoutTopicsWorker : QueueWorker
    {
        /// <summary>
        /// Topics manager
        /// </summary>
        private ITopicsManager topicsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="FanoutTopicsWorker"/> class
        /// </summary>
        /// <param name="log">log</param>
        /// <param name="fanoutTopicsQueue"><c>Fanout</c> topics queue</param>
        /// <param name="topicsManager">Topics manager</param>
        public FanoutTopicsWorker(ILog log, IFanoutTopicsQueue fanoutTopicsQueue, ITopicsManager topicsManager)
            : base(log)
        {
            this.Queue = fanoutTopicsQueue;
            this.topicsManager = topicsManager;
        }

        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Process message task</returns>
        protected override async Task Process(IMessage message)
        {
            if (message is FanoutTopicMessage)
            {
                FanoutTopicMessage fanoutTopicMessage = message as FanoutTopicMessage;
                await this.topicsManager.FanoutTopic(
                    fanoutTopicMessage.UserHandle,
                    fanoutTopicMessage.AppHandle,
                    fanoutTopicMessage.TopicHandle);
            }
        }
    }
}
