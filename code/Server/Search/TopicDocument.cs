// <copyright file="TopicDocument.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Search
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// a topic "document" that will be inserted into the Azure Search index
    /// </summary>
    public class TopicDocument : Document
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TopicDocument"/> class.
        /// </summary>
        /// <param name="topicHandle">uniquely identifies the topic</param>
        /// <param name="topicTitle">a free text title that the user has given to this topic</param>
        /// <param name="topicText">contents of the topic text</param>
        /// <param name="topicTags">a list of <c>hashtags</c> mentioned in the topic</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="userHandle">uniquely identifies the user that created this topic</param>
        /// <param name="searchWeight">the search weight for ranking this topic</param>
        /// <param name="topicLastModifiedTime">date that this topic was created on</param>
        public TopicDocument(string topicHandle, string topicTitle, string topicText, List<string> topicTags, string appHandle, string userHandle, int searchWeight, DateTime topicLastModifiedTime)
        {
            this.TopicHandle = topicHandle;
            this.TopicTitle = topicTitle;
            this.TopicText = topicText;
            this.TopicTags = topicTags;
            this.AppHandle = appHandle;
            this.UserHandle = userHandle;
            this.SearchWeight = searchWeight;
            this.TopicLastModifiedTime = topicLastModifiedTime;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the topic
        /// </summary>
        public string TopicHandle { get; set; }

        /// <summary>
        /// Gets or sets a free text title that the user has given to this topic
        /// </summary>
        public string TopicTitle { get; set; }

        /// <summary>
        /// Gets or sets the contents of the topic text
        /// </summary>
        public string TopicText { get; set; }

        /// <summary>
        /// Gets or sets a list of <c>hashtags</c> mentioned in the topic
        /// </summary>
        public List<string> TopicTags { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the app
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets the user that created this topic
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets a weight that is used to float up some topics in search results; larger value means increased chance of floating up.
        /// </summary>
        public int SearchWeight { get; set; }

        /// <summary>
        /// Gets or sets the date that this topic was created on or last modified on
        /// </summary>
        public DateTime TopicLastModifiedTime { get; set; }
    }
}
