// <copyright file="TopicDocumentResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Search
{
    /// <summary>
    /// results from searching for a topic
    /// </summary>
    public class TopicDocumentResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TopicDocumentResult"/> class
        /// </summary>
        /// <param name="topicHandle">uniquely identifies the topic</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="userHandle">uniquely identifies the user that created this topic</param>
        public TopicDocumentResult(string topicHandle, string appHandle, string userHandle)
        {
            this.TopicHandle = topicHandle;
            this.AppHandle = appHandle;
            this.UserHandle = userHandle;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the topic
        /// </summary>
        public string TopicHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the app
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets the user that created this topic
        /// </summary>
        public string UserHandle { get; set; }
    }
}
