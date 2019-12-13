// <copyright file="FanoutActivitiesQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// <c>Fanout</c> activities queue class
    /// </summary>
    public class FanoutActivitiesQueue : QueueBase, IFanoutActivitiesQueue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FanoutActivitiesQueue"/> class
        /// </summary>
        /// <param name="queueManager">queue manager</param>
        public FanoutActivitiesQueue(IQueueManager queueManager)
            : base(queueManager)
        {
            this.QueueIdentifier = QueueIdentifier.FanoutActivities;
        }

        /// <summary>
        /// Send <c>fanout</c> activity message
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="activityHandle">Activity handle</param>
        /// <param name="activityType">Activity type</param>
        /// <param name="actorUserHandle">Actor user handle</param>
        /// <param name="actedOnUserHandle">Acted on user handle</param>
        /// <param name="actedOnContentType">Acted on content type</param>
        /// <param name="actedOnContentHandle">Acted on content handle</param>
        /// <param name="createdTime">Created time</param>
        /// <returns>Send message task</returns>
        public async Task SendFanoutActivityMessage(
            string userHandle,
            string appHandle,
            string activityHandle,
            ActivityType activityType,
            string actorUserHandle,
            string actedOnUserHandle,
            ContentType actedOnContentType,
            string actedOnContentHandle,
            DateTime createdTime)
        {
            FanoutActivityMessage message = new FanoutActivityMessage()
            {
                UserHandle = userHandle,
                AppHandle = appHandle,
                ActivityHandle = activityHandle,
                ActivityType = activityType,
                ActorUserHandle = actorUserHandle,
                ActedOnUserHandle = actedOnUserHandle,
                ActedOnContentType = actedOnContentType,
                ActedOnContentHandle = actedOnContentHandle,
                CreatedTime = createdTime
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }

        /// <summary>
        /// Send <c>fanout</c> topic activity message
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="activityHandle">Activity handle</param>
        /// <param name="activityType">Activity type</param>
        /// <param name="actorUserHandle">Actor user handle</param>
        /// <param name="actedOnUserHandle">Acted on user handle</param>
        /// <param name="actedOnContentType">Acted on content type</param>
        /// <param name="actedOnContentHandle">Acted on content handle</param>
        /// <param name="createdTime">Created time</param>
        /// <returns>Send message task</returns>
        public async Task SendFanoutTopicActivityMessage(
            string topicHandle,
            string appHandle,
            string activityHandle,
            ActivityType activityType,
            string actorUserHandle,
            string actedOnUserHandle,
            ContentType actedOnContentType,
            string actedOnContentHandle,
            DateTime createdTime)
        {
            FanoutTopicActivityMessage message = new FanoutTopicActivityMessage()
            {
                TopicHandle = topicHandle,
                AppHandle = appHandle,
                ActivityHandle = activityHandle,
                ActivityType = activityType,
                ActorUserHandle = actorUserHandle,
                ActedOnUserHandle = actedOnUserHandle,
                ActedOnContentType = actedOnContentType,
                ActedOnContentHandle = actedOnContentHandle,
                CreatedTime = createdTime
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }
    }
}
