// <copyright file="SearchTopics.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using RedDog.Search.Model;
    using SocialPlus.Logging;

    /// <summary>
    /// Search API for Topics
    /// </summary>
    public class SearchTopics : SearchBase
    {
        /// <summary>
        /// Name of the search index
        /// </summary>
        private const string IndexName = "topics";

        /// <summary>
        /// Name of the index column of the search index
        /// </summary>
        private const string DocumentHandleName = "topicHandle";

        /// <summary>
        /// Name of the scoring profile
        /// </summary>
        private const string ScoringProfileName = "weightedTopicScore";

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTopics"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="serviceName">name of the search service</param>
        /// <param name="serviceAdminKey">key to access the search service</param>
        public SearchTopics(ILog log, string serviceName, string serviceAdminKey)
            : base(log, serviceName, serviceAdminKey, IndexName, DocumentHandleName)
        {
        }

        /// <summary>
        /// Find topicHandles that match a topicTag, and optionally an appHandle, and optionally a userHandle,
        /// and optionally skip first n records and return optionally some records.
        /// Supports the following operators: suffix: "foo*", and: "foo+bar", or: "foo|bar", not: "-foo", phrase: ""foo bar"", precedence: <c>"foo+(bar|baz)"</c>.
        /// You need to escape * if it is at the end of a word, and - if it is at the start of a word.
        /// Default is to use and, so if you use whitespace to separate words, "foo bar", that is equivalent to "foo+bar".
        /// </summary>
        /// <param name="searchString">what string to search for</param>
        /// <param name="appHandle">optional -- only results from this app will be included</param>
        /// <param name="userHandle">optional -- only results from this user will be included</param>
        /// <param name="searchTopicTags">optional -- if true, will search hash tag field</param>
        /// <param name="searchTopicTitle">optional -- if true, will search topic title field</param>
        /// <param name="searchTopicText">optional -- if true, will search topic text field</param>
        /// <param name="skipRecords">optional -- skip these many first records (max allowed is 100000)</param>
        /// <param name="numRecords">optional -- return only these many records (max allowed is 1000)</param>
        /// <returns>list of SearchTopicDocumentResults; empty list if no results</returns>
        public async Task<List<TopicDocumentResult>> Search(
                                                      string searchString,
                                                      string appHandle = null,
                                                      string userHandle = null,
                                                      bool searchTopicTags = true,
                                                      bool searchTopicTitle = false,
                                                      bool searchTopicText = false,
                                                      long skipRecords = 0,
                                                      long numRecords = 50)
        {
            // check parameter ranges
            if (skipRecords < 0 || skipRecords > MaxSkipRecords || numRecords < 1 || numRecords > MaxRecords)
            {
                this.Log.LogException("bad parameters: " + skipRecords + "," + numRecords);
            }

            if (searchTopicTags == false && searchTopicTitle == false && searchTopicText == false)
            {
                this.Log.LogException("bad parameters, all search fields are false");
            }

            // do we need to filter by app handle?
            string filter = string.Empty;
            if (!string.IsNullOrWhiteSpace(appHandle))
            {
                filter = "appHandle eq '" + appHandle + "'";
            }

            // do we need to filter by user handle?
            if (!string.IsNullOrWhiteSpace(userHandle))
            {
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filter = filter + " and ";
                }

                filter = filter + "userHandle eq '" + userHandle + "'";
            }

            // what fields do we need to search?
            string searchField = string.Empty;
            if (searchTopicTags)
            {
                searchField = "topicTags,";
            }

            if (searchTopicTitle)
            {
                searchField = searchField + "topicTitle,";
            }

            if (searchTopicText)
            {
                searchField = searchField + "topicText,";
            }

            searchField = searchField.TrimEnd(',');

            // form the query
            SearchQuery query = new SearchQuery(searchString)
                                    .Mode(SearchMode.All)
                                    .SearchField(searchField)
                                    .Skip(skipRecords)
                                    .Top(numRecords)
                                    .Count(false)
                                    .Filter(filter)
                                    .ScoringProfile(ScoringProfileName);

            // issue it
            this.Log.LogInformation("issuing the search query: " + searchString);
            List<Dictionary<string, object>> results = await this.GetSearchResults(query);

            // parse the results
            List<TopicDocumentResult> response = new List<TopicDocumentResult>();
            if (results == null || results.Count == 0)
            {
                return response;
            }

            foreach (Dictionary<string, object> result in results)
            {
                if (result.ContainsKey("appHandle") && result.ContainsKey("userHandle") && result.ContainsKey("topicHandle"))
                {
                    string resultUserHandle = result["userHandle"] == null ? null : result["userHandle"].ToString();
                    response.Add(new TopicDocumentResult(result["topicHandle"].ToString(), result["appHandle"].ToString(), resultUserHandle));
                }
            }

            return response;
        }

        /// <summary>
        /// Create the topic index.
        /// </summary>
        /// <returns>create index task</returns>
        public async Task CreateIndex()
        {
            // form the index
            Index index = new Index(IndexName)
                .WithStringField("topicHandle",             f => f.IsKey(true).IsRetrievable(true).IsSearchable(false).IsFilterable(false).SupportSuggestions(false).IsSortable(false).IsFacetable(false))
                .WithStringField("topicTitle",              f => f.IsKey(false).IsRetrievable(false).IsSearchable(true).IsFilterable(false).SupportSuggestions(false).IsSortable(false).IsFacetable(false))
                .WithStringField("topicText",               f => f.IsKey(false).IsRetrievable(false).IsSearchable(true).IsFilterable(false).SupportSuggestions(false).IsSortable(false).IsFacetable(false))
                .WithStringCollectionField("topicTags",     f => f.IsKey(false).IsRetrievable(true).IsSearchable(true).IsFilterable(false).SupportSuggestions(true).IsSortable(false).IsFacetable(true))
                .WithStringField("appHandle",               f => f.IsKey(false).IsRetrievable(true).IsSearchable(false).IsFilterable(true).SupportSuggestions(false).IsSortable(false).IsFacetable(false))
                .WithStringField("userHandle",              f => f.IsKey(false).IsRetrievable(true).IsSearchable(false).IsFilterable(true).SupportSuggestions(false).IsSortable(false).IsFacetable(false))
                .WithIntegerField("searchWeight",           f => f.IsKey(false).IsRetrievable(false).IsSearchable(false).IsFilterable(true).SupportSuggestions(false).IsSortable(true).IsFacetable(false))
                .WithDateTimeField("topicLastModifiedTime", f => f.IsKey(false).IsRetrievable(false).IsSearchable(false).IsFilterable(true).SupportSuggestions(false).IsSortable(true).IsFacetable(false));

            // create custom scoring profile
            ScoringProfile weightedScore = new ScoringProfile();
            weightedScore.Name = ScoringProfileName;

            // function for scoring searchWeight
            ScoringProfileFunction weightedFunction1 = new ScoringProfileFunction();
            weightedFunction1.Type = ScoringProfileFunctionType.Magnitude;
            weightedFunction1.Boost = 10;
            weightedFunction1.FieldName = "searchWeight";
            weightedFunction1.Interpolation = InterpolationType.Quadratic;
            weightedFunction1.Magnitude = new ScoringProfileFunctionMagnitude();
            weightedFunction1.Magnitude.BoostingRangeStart = 1;
            weightedFunction1.Magnitude.BoostingRangeEnd = 100;
            weightedFunction1.Magnitude.ConstantBoostBeyondRange = true;
            weightedScore.Functions.Add(weightedFunction1);

            // function for scoring freshness
            ScoringProfileFunction weightedFunction2 = new ScoringProfileFunction();
            weightedFunction2.Type = ScoringProfileFunctionType.Freshness;
            weightedFunction2.Boost = 10;
            weightedFunction2.FieldName = "topicLastModifiedTime";
            weightedFunction2.Interpolation = InterpolationType.Linear;
            weightedFunction2.Freshness = new ScoringProfileFunctionFreshness();
            weightedFunction2.Freshness.BoostingDuration = new TimeSpan(2, 0, 0, 0);
            weightedScore.Functions.Add(weightedFunction2);

            weightedScore.FunctionAggregation = FunctionAggregation.Sum;
            index.ScoringProfiles.Add(weightedScore);
            index.DefaultScoringProfile = ScoringProfileName;

            // submit the create task
            await this.CreateIndex(index);
        }

        /// <summary>
        /// Fetch trending hash tags.
        /// </summary>
        /// <param name="duration">only topics created within this timespan are considered</param>
        /// <param name="appHandle">optional -- will provide results only for a particular app</param>
        /// <param name="count">optional -- how many top results to return (default is 50)</param>
        /// <returns>a sorted list of "topic tags, number of topics", empty list on no results</returns>
        public async Task<List<Tuple<string, double>>> GetTrendingTopicTags(TimeSpan duration, string appHandle = null, uint count = 20)
        {
            // check parameter ranges
            if (duration == null || count == 0)
            {
                this.Log.LogException("bad parameters");
            }

            // filter by time duration
            string filter = "topicLastModifiedTime ge " + DateTime.UtcNow.Subtract(duration).ToString("yyyy-MM-ddTHH:mm:ssZ");

            // do we need to filter by app handle?
            if (!string.IsNullOrWhiteSpace(appHandle))
            {
                filter = filter + " and appHandle eq '" + appHandle + "'";
            }

            // form the query
            SearchQuery query = new SearchQuery(string.Empty)
                                     .Mode(SearchMode.Any)
                                     .SearchField(string.Empty)
                                     .Count(false)
                                     .Filter(filter)
                                     .Count(true)
                                     .Facet("topicTags", "count:" + count);

            this.Log.LogInformation("issuing the trending topics query");
            return await this.GetTrendingSearchResults(query, "topicTags");
        }

        /// <summary>
        /// autocompletes the given topic tag
        /// </summary>
        /// <param name="partialTopic">minimum length of 3, maximum length of 25</param>
        /// <param name="appHandle">optional -- will provide results only for a particular app</param>
        /// <param name="fuzzy">optional -- if true will allow up to 1 character spelling error</param>
        /// <returns>a list of topic tags, null on failure, empty list on no results</returns>
        public async Task<List<string>> AutoCompleteTopicTag(string partialTopic, string appHandle = null, bool fuzzy = false)
        {
            uint count = 10;

            // test the input parameters
            if (string.IsNullOrWhiteSpace(partialTopic))
            {
                throw new ArgumentNullException("partialTopic");
            }

            if (partialTopic.Length < 3 || partialTopic.Length > 25)
            {
                throw new ArgumentException("partialTopic must have a minimum length of 3 and a maximum length of 25", "partialTopic");
            }

            if (count < 1 || count > 10)
            {
                throw new ArgumentOutOfRangeException("count", "count must be between 1 and 10");
            }

            // do we need to filter by app handle?
            string filter = string.Empty;
            if (!string.IsNullOrWhiteSpace(appHandle))
            {
                filter = "appHandle eq '" + appHandle + "'";
            }

            // issue the query
            this.Log.LogInformation("issuing the autocomplete query: " + partialTopic);
            SuggestionQuery query = new SuggestionQuery(partialTopic)
                                    .Fuzzy(fuzzy)
                                    .Filter(filter)
                                    .Top(count);
            return await this.GetAutoCompleteResults(query, "topicTags");
        }

        /// <summary>
        /// adds a topic to the search index
        /// </summary>
        /// <param name="topic">topic to add</param>
        /// <returns>add topic task</returns>
        public async Task AddTopic(TopicDocument topic)
        {
            await this.AddDocument(topic, this.CreateDocumentInsertOperation);
        }

        /// <summary>
        /// adds topics to the search index
        /// </summary>
        /// <param name="topics">topics to add</param>
        /// <returns>add topics task</returns>
        public async Task AddTopics(List<TopicDocument> topics)
        {
            List<Document> documents = topics.Cast<Document>().ToList();
            await this.AddDocuments(documents, this.CreateDocumentInsertOperation);
        }

        /// <summary>
        /// deletes a topic from the search index
        /// </summary>
        /// <param name="topicHandle">topic handle to delete</param>
        /// <returns>delete topic task</returns>
        public async Task DeleteTopic(string topicHandle)
        {
            // check if important values are null
            if (string.IsNullOrWhiteSpace(topicHandle))
            {
                this.Log.LogException("got bad parameters");
            }

            await this.DeleteDocument(topicHandle);
        }

        /// <summary>
        /// Creates an insert operation for the supplied topic
        /// </summary>
        /// <param name="document">the topic to create an operation for</param>
        /// <returns>insert operation</returns>
        private IndexOperation CreateDocumentInsertOperation(Document document)
        {
            this.Log.LogInformation("creating topic insert");

            TopicDocument topic = null;
            if (document is TopicDocument)
            {
                topic = (TopicDocument)document;
            }
            else
            {
                this.Log.LogException("got document that is not a topic document");
            }

            // check if important values are null
            if (topic == null || string.IsNullOrWhiteSpace(topic.TopicHandle))
            {
                this.Log.LogException("got bad parameters");
            }

            if (topic.TopicTags == null)
            {
                topic.TopicTags = new List<string>();
            }

            IndexOperation operation = new IndexOperation(IndexOperationType.Upload, "topicHandle", topic.TopicHandle)
                                                          .WithProperty("topicTitle", topic.TopicTitle)
                                                          .WithProperty("topicText", topic.TopicText)
                                                          .WithProperty("topicTags", topic.TopicTags)
                                                          .WithProperty("appHandle", topic.AppHandle)
                                                          .WithProperty("userHandle", topic.UserHandle)
                                                          .WithProperty("searchWeight", topic.SearchWeight)
                                                          .WithProperty("topicLastModifiedTime", new DateTimeOffset(topic.TopicLastModifiedTime));
            return operation;
        }
    }
}
