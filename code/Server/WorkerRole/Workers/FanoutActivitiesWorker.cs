// <copyright file="FanoutActivitiesWorker.cs" company="Microsoft">
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
    /// <c>Fanout</c> activities worker
    /// </summary>
    public class FanoutActivitiesWorker : QueueWorker
    {
        /// <summary>
        /// Activities manager
        /// </summary>
        private IActivitiesManager activitiesManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="FanoutActivitiesWorker"/> class
        /// </summary>
        /// <param name="log">log</param>
        /// <param name="fanoutActivitiesQueue"><c>Fanout</c> activities queue</param>
        /// <param name="activitiesManager">Activities manager</param>
        public FanoutActivitiesWorker(ILog log, IFanoutActivitiesQueue fanoutActivitiesQueue, IActivitiesManager activitiesManager)
            : base(log)
        {
            this.Queue = fanoutActivitiesQueue;
            this.activitiesManager = activitiesManager;
        }

        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Process message task</returns>
        protected override async Task Process(IMessage message)
        {
            if (message is FanoutActivityMessage)
            {
                FanoutActivityMessage fanoutActivityMessage = message as FanoutActivityMessage;
                await this.activitiesManager.FanoutActivity(
                    fanoutActivityMessage.UserHandle,
                    fanoutActivityMessage.AppHandle,
                    fanoutActivityMessage.ActivityHandle,
                    fanoutActivityMessage.ActivityType,
                    fanoutActivityMessage.ActorUserHandle,
                    fanoutActivityMessage.ActedOnUserHandle,
                    fanoutActivityMessage.ActedOnContentType,
                    fanoutActivityMessage.ActedOnContentHandle,
                    fanoutActivityMessage.CreatedTime);
            }
            else if (message is FanoutTopicActivityMessage)
            {
                FanoutTopicActivityMessage fanoutActivityMessage = message as FanoutTopicActivityMessage;
                await this.activitiesManager.FanoutTopicActivity(
                    fanoutActivityMessage.TopicHandle,
                    fanoutActivityMessage.AppHandle,
                    fanoutActivityMessage.ActivityHandle,
                    fanoutActivityMessage.ActivityType,
                    fanoutActivityMessage.ActorUserHandle,
                    fanoutActivityMessage.ActedOnUserHandle,
                    fanoutActivityMessage.ActedOnContentType,
                    fanoutActivityMessage.ActedOnContentHandle,
                    fanoutActivityMessage.CreatedTime);
            }
        }
    }
}
