// <copyright file="SearchManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Search;

    /// <summary>
    /// Search manager class
    /// </summary>
    public class SearchManager : ISearchManager
    {
        /// <summary>
        /// Connection string provider
        /// </summary>
        private readonly IConnectionStringProvider connectionStringProvider;

        /// <summary>
        /// Internal lock. Together with the flag below, its role is to ensure the method <c>Init</c> has completed before
        /// we attempt to perform the real work done by the public methods in this class.
        /// </summary>
        private readonly object locker = new object();

        /// <summary>
        /// Internal flag. Its role is to provide a barrier so that no work gets done until <c>Init</c> is done.
        /// </summary>
        private readonly ManualResetEvent initDone = new ManualResetEvent(false);

        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Internal flag. Together with the flag below, its role is to ensure the method <c>Init</c> has completed before
        /// we attempt to perform the real work done by the public methods in this class.
        /// </summary>
        private bool initStarted = false;

        /// <summary>
        /// private instance of search users class
        /// </summary>
        private SearchUsers searchUsers;

        /// <summary>
        /// private instance of the search topics class
        /// </summary>
        private SearchTopics searchTopics;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchManager"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="connectionStringProvider">connection string provider</param>
        public SearchManager(ILog log, IConnectionStringProvider connectionStringProvider)
        {
            this.log = log;
            this.connectionStringProvider = connectionStringProvider;
        }

        /// <summary>
        /// Index topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="title">Topic title</param>
        /// <param name="text">Topic text</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="lastModifiedTime">When this topic was created or modified</param>
        /// <returns>Index topic task</returns>
        public async Task IndexTopic(
            string topicHandle,
            string title,
            string text,
            string userHandle,
            string appHandle,
            DateTime lastModifiedTime)
        {
            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init(SearchInstanceType.Default);
            }

            // If init not done, wait
            this.initDone.WaitOne();

            var hashtags = this.ExtractUniqueHashtags(string.Join(" ", title, text));
            await this.searchTopics.AddTopic(new TopicDocument(topicHandle, title, text, hashtags, appHandle, userHandle, 0, lastModifiedTime));
        }

        /// <summary>
        /// Remove topic from index
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>Remove topic from index task</returns>
        public async Task RemoveTopic(string topicHandle)
        {
            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init(SearchInstanceType.Default);
            }

            // If init not done, wait
            this.initDone.WaitOne();

            await this.searchTopics.DeleteTopic(topicHandle);
        }

        /// <summary>
        /// Index user
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Index user task</returns>
        public async Task IndexUser(string userHandle, string firstName, string lastName, string appHandle)
        {
            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init(SearchInstanceType.Default);
            }

            // If init not done, wait
            this.initDone.WaitOne();

            await this.searchUsers.AddUser(new UserDocument(firstName, lastName, appHandle, userHandle));
        }

        /// <summary>
        /// Remove user from index
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Remove user from index task</returns>
        public async Task RemoveUser(string userHandle, string appHandle)
        {
            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init(SearchInstanceType.Default);
            }

            // If init not done, wait
            this.initDone.WaitOne();

            await this.searchUsers.DeleteUser(userHandle, appHandle);
        }

        /// <summary>
        /// Search topics
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>Topic feed entities</returns>
        public async Task<IList<ITopicFeedEntity>> GetTopics(string query, string appHandle, int cursor, int limit)
        {
            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init(SearchInstanceType.Default);
            }

            // If init not done, wait
            this.initDone.WaitOne();

            // Decide the scope of the search
            bool searchTopicTags = true;
            bool searchTopicTitle = true;
            bool searchTopicText = true;
            if (this.ContainsOnlyHashtags(query))
            {
                // since the user is querying for one or more hashtags and no words without hashtags, search only hashtags
                searchTopicTitle = false;
                searchTopicText = false;
            }

            var topicDocuments = await this.searchTopics.Search(searchString: query, appHandle: appHandle, searchTopicTags: searchTopicTags, searchTopicTitle: searchTopicTitle, searchTopicText: searchTopicText, skipRecords: cursor, numRecords: limit);
            IEnumerable<TopicSearchFeedEntity> topicFeedEntities =
                from topicDocument in topicDocuments
                select new TopicSearchFeedEntity()
                {
                    TopicHandle = topicDocument.TopicHandle,
                    UserHandle = topicDocument.UserHandle,
                    AppHandle = topicDocument.AppHandle
                };

            return topicFeedEntities.ToList<ITopicFeedEntity>();
        }

        /// <summary>
        /// Search users
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>User feed entities</returns>
        public async Task<IList<IUserFeedEntity>> GetUsers(string query, string appHandle, int cursor, int limit)
        {
            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init(SearchInstanceType.Default);
            }

            // If init not done, wait
            this.initDone.WaitOne();

            var userDocuments = await this.searchUsers.Search(query, appHandle, skipRecords: cursor, numRecords: limit);
            IEnumerable<UserSearchFeedEntity> topicFeedEntities =
                from userDocument in userDocuments
                select new UserSearchFeedEntity()
                {
                    UserHandle = userDocument.UserHandle,
                    AppHandle = userDocument.AppHandle
                };

            return topicFeedEntities.ToList<IUserFeedEntity>();
        }

        /// <summary>
        /// Get trending <c>hashtags</c>
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>List of <c>hashtags</c></returns>
        public async Task<IList<string>> GetTrendingHashtags(string appHandle)
        {
            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init(SearchInstanceType.Default);
            }

            // If init not done, wait
            this.initDone.WaitOne();

            // have to change
            var result = await this.searchTopics.GetTrendingTopicTags(TimeSpan.FromDays(100), appHandle);
            return result.Select(r => r.Item1).ToList();
        }

        /// <summary>
        /// Get autocompleted <c>hashtags</c>
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>List of <c>hashtags</c></returns>
        public async Task<IList<string>> GetAutocompletedHashtags(string query, string appHandle)
        {
            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init(SearchInstanceType.Default);
            }

            // If init not done, wait
            this.initDone.WaitOne();

            return await this.searchTopics.AutoCompleteTopicTag(query, appHandle);
        }

        /// <summary>
        /// Extract unique <c>hashtags</c> from text
        /// </summary>
        /// <param name="text">Text to extract <c>hashtags</c> from</param>
        /// <returns>List of unique <c>hashtags</c></returns>
        private List<string> ExtractUniqueHashtags(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<string>();
            }

            HashSet<string> uniqueHashtags = new HashSet<string>();
            string[] words = text.Split();
            foreach (string word in words)
            {
                if (word.Length > 2)
                {
                    if (word.StartsWith("#"))
                    {
                        if (!uniqueHashtags.Contains(word))
                        {
                            uniqueHashtags.Add(word);
                        }
                    }
                }
            }

            return uniqueHashtags.ToList();
        }

        /// <summary>
        /// Determines whether the text consists of only hashtags
        /// </summary>
        /// <param name="text">input text, such as a search query</param>
        /// <returns>true if the text is 1 or more hashtags, false otherwise</returns>
        private bool ContainsOnlyHashtags(string text)
        {
            // false if empty or whitespace
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            // check each word
            bool onlyHashtags = true;
            string[] words = text.Split();
            foreach (string word in words)
            {
                if (!string.IsNullOrWhiteSpace(word) && (!word.StartsWith("#") || word.Length == 1))
                {
                    onlyHashtags = false;
                    break;
                }
            }

            return onlyHashtags;
        }

        /// <summary>
        /// Initializes the search user and topics
        /// </summary>
        /// <param name="searchInstanceType">type of search instance</param>
        /// <returns>empty</returns>
        private async Task Init(SearchInstanceType searchInstanceType)
        {
            // Guard that ensures Init is executed once only
            lock (this.locker)
            {
                if (this.initStarted == true)
                {
                    return;
                }

                this.initStarted = true;
            }

            // get the connection strings
            string searchServiceName = await this.connectionStringProvider.GetSearchServiceName(searchInstanceType);
            string searchServiceAdminKey = await this.connectionStringProvider.GetSearchServiceAdminKey(searchInstanceType);

            // instantiate the two search interfaces
            this.searchUsers = new SearchUsers(this.log, searchServiceName, searchServiceAdminKey);
            this.searchTopics = new SearchTopics(this.log, searchServiceName, searchServiceAdminKey);

            // Init done
            this.initDone.Set();
        }
    }
}
