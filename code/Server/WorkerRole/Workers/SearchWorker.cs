// <copyright file="SearchWorker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Workers
{
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Queues;

    /// <summary>
    /// Search worker
    /// </summary>
    public class SearchWorker : QueueWorker
    {
        /// <summary>
        /// Search manager
        /// </summary>
        private ISearchManager searchManager;

        /// <summary>
        /// Topics manager
        /// </summary>
        private ITopicsManager topicsManager;

        /// <summary>
        /// Users manager
        /// </summary>
        private IUsersManager usersManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchWorker"/> class.
        /// </summary>
        /// <param name="log">log</param>
        /// <param name="queue">Search queue</param>
        /// <param name="searchManager">Search manager</param>
        /// <param name="topicsManager">Topics manager</param>
        /// <param name="usersManager">Users manager</param>
        public SearchWorker(ILog log, ISearchQueue queue, ISearchManager searchManager, ITopicsManager topicsManager, IUsersManager usersManager)
            : base(log)
        {
            this.Queue = queue;
            this.searchManager = searchManager;
            this.topicsManager = topicsManager;
            this.usersManager = usersManager;
        }

        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Process message task</returns>
        protected override async Task Process(IMessage message)
        {
            // add a new topic to the search index
            if (message is SearchIndexTopicMessage)
            {
                SearchIndexTopicMessage searchIndexTopicMessage = message as SearchIndexTopicMessage;
                ITopicEntity topicEntity = await this.topicsManager.ReadTopic(searchIndexTopicMessage.TopicHandle);

                // the topic may have been deleted before the search index topic message is processed
                if (topicEntity == null)
                {
                    this.Log.LogInformation("Could not find topic " + searchIndexTopicMessage.TopicHandle);
                    return;
                }

                // the topic may have been updated before the search index topic message is processed
                if (topicEntity.LastUpdatedTime > searchIndexTopicMessage.Timestamp)
                {
                    this.Log.LogInformation("Topic " + searchIndexTopicMessage.TopicHandle + " is newer than the queue message.");
                    return;
                }

                await this.searchManager.IndexTopic(
                    searchIndexTopicMessage.TopicHandle,
                    topicEntity.Title,
                    topicEntity.Text,
                    topicEntity.UserHandle,
                    topicEntity.AppHandle,
                    topicEntity.LastUpdatedTime);
            }

            // remove a topic from the search index
            else if (message is SearchRemoveTopicMessage)
            {
                SearchRemoveTopicMessage searchRemoveTopicMessage = message as SearchRemoveTopicMessage;
                await this.searchManager.RemoveTopic(searchRemoveTopicMessage.TopicHandle);
            }

            // add a new user to the search index
            else if (message is SearchIndexUserMessage)
            {
                SearchIndexUserMessage searchIndexUserMessage = message as SearchIndexUserMessage;
                IUserProfileEntity userProfileEntity = await this.usersManager.ReadUserProfile(searchIndexUserMessage.UserHandle, searchIndexUserMessage.AppHandle);

                // the user may have been deleted before the search index user message is processed
                if (userProfileEntity == null)
                {
                    this.Log.LogInformation("Could not find user " + searchIndexUserMessage.UserHandle + " for app " + searchIndexUserMessage.AppHandle);
                    return;
                }

                // the user may have been updated before the search index user message is processed
                if (userProfileEntity.LastUpdatedTime > searchIndexUserMessage.Timestamp)
                {
                    this.Log.LogInformation("User " + searchIndexUserMessage.UserHandle + " in app " + searchIndexUserMessage.AppHandle + " is newer than the queue message.");
                    return;
                }

                await this.searchManager.IndexUser(
                    searchIndexUserMessage.UserHandle,
                    userProfileEntity.FirstName,
                    userProfileEntity.LastName,
                    searchIndexUserMessage.AppHandle);
            }

            // remove a user from the search index
            else if (message is SearchRemoveUserMessage)
            {
                SearchRemoveUserMessage searchRemoveUserMessage = message as SearchRemoveUserMessage;
                await this.searchManager.RemoveUser(searchRemoveUserMessage.UserHandle, searchRemoveUserMessage.AppHandle);
            }

            // bad message
            else
            {
                this.Log.LogError("received message of unknown type " + message.ToString());
            }
        }
    }
}
