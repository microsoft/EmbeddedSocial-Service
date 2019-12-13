// <copyright file="SearchBase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using RedDog.Search;
    using RedDog.Search.Http;
    using RedDog.Search.Model;
    using SocialPlus.Logging;

    /// <notes>
    /// Azure Search API : http://msdn.microsoft.com/en-us/library/dn798918.aspx
    /// RedDog Search DLL : https://github.com/reddog-io/RedDog.Search
    /// Make sure your NuGet package for NewtonSoft is the latest. Runtime will fail if the version is too old.
    /// </notes>
    /// <summary>
    /// Base class for search
    /// </summary>
    public abstract class SearchBase
    {
        /// <summary>
        /// Maximum number of records that can be skipped in a search query
        /// </summary>
        protected static readonly int MaxSkipRecords = 100000;

        /// <summary>
        /// Maximum number of records that can be requested in a search query
        /// </summary>
        protected static readonly int MaxRecords = 1000;

        /// <summary>
        /// Name of the Azure Search index this instance deals with
        /// </summary>
        private string indexName;

        /// <summary>
        /// Document handle name that is the unique key
        /// </summary>
        private string documentHandleName;

        /// <summary>
        /// Connection to the Azure Search instance
        /// </summary>
        private ApiConnection connection;

        /// <summary>
        /// Client for managing the Azure Search index
        /// </summary>
        private IndexManagementClient managementClient;

        /// <summary>
        /// Client for querying the Azure Search index
        /// </summary>
        private IndexQueryClient queryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchBase"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="serviceName">name of the search service</param>
        /// <param name="serviceAdminKey">key to access the search service</param>
        /// <param name="indexName">which index to talk to</param>
        /// <param name="documentHandleName">the key into the index</param>
        protected SearchBase(ILog log, string serviceName, string serviceAdminKey, string indexName, string documentHandleName)
        {
            this.Log = log;

            // check service credentials
            if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(serviceAdminKey))
            {
                this.Log.LogException("got bad search credentials");
            }

            // create connection and clients and index
            this.Log.LogInformation("creating connection and index clients to Azure Search");
            this.connection = ApiConnection.Create(serviceName, serviceAdminKey);
            this.managementClient = new IndexManagementClient(this.connection);
            this.queryClient = new IndexQueryClient(this.connection);

            this.indexName = indexName;
            this.documentHandleName = documentHandleName;
        }

        /// <summary>
        /// Gets log
        /// </summary>
        protected ILog Log { get; private set; }

        /// <summary>
        /// Deletes the index.
        /// Use only for debugging.
        /// Will need to recreate the index after this operation.
        /// </summary>
        /// <returns>task to delete the index</returns>
        public async Task DeleteIndex()
        {
            // does index exist?
            this.Log.LogInformation("searching for existing index");
            IApiResponse<Index> response = await this.managementClient.GetIndexAsync(this.indexName);
            if (response == null || (!response.IsSuccess && response.StatusCode != System.Net.HttpStatusCode.NotFound))
            {
                this.Log.LogException("search for existing index failed: " + ((response == null || response.Error == null) ? string.Empty : response.Error.Message));
            }

            // delete index if it exists
            if (response.Body != null && response.Body.Name != null && response.Body.Name == this.indexName)
            {
                this.Log.LogInformation("deleting index");
                IApiResponse response2 = await this.managementClient.DeleteIndexAsync(this.indexName);
                if (response2 == null || !response2.IsSuccess)
                {
                    this.Log.LogException("delete of existing index failed: " + ((response2 == null || response2.Error == null) ? string.Empty : response2.Error.Message));
                }
            }
        }

        /// <summary>
        /// Gets the number of documents that are in the search index
        /// </summary>
        /// <returns>Number of documents</returns>
        public async Task<long> GetNumberOfDocuments()
        {
            // issue the query
            this.Log.LogInformation("issuing the stats query");
            IApiResponse<IndexStatistics> response = await this.managementClient.GetIndexStatisticsAsync(this.indexName);
            if (response == null || !response.IsSuccess || response.Body == null)
            {
                this.Log.LogException("failed: " + ((response == null || response.Error == null) ? string.Empty : response.Error.Message));
            }

            return response.Body.DocumentCount;
        }

        /// <summary>
        /// Gets the total storage used in the search index
        /// </summary>
        /// <returns>Number of bytes</returns>
        public async Task<long> GetStorageUsage()
        {
            // issue the query
            this.Log.LogInformation("issuing the stats query");
            IApiResponse<IndexStatistics> response = await this.managementClient.GetIndexStatisticsAsync(this.indexName);
            if (response == null || !response.IsSuccess || response.Body == null)
            {
                this.Log.LogException("failed: " + ((response == null || response.Error == null) ? string.Empty : response.Error.Message));
            }

            return response.Body.StorageSize;
        }

        /// <summary>
        /// Will add a document to the Azure Search Index. If the document exists, it will be replaced.
        /// </summary>
        /// <param name="document">document to add</param>
        /// <param name="createDocumentInsertOperation">method that creates the operation</param>
        /// <returns>task to add a document</returns>
        protected async Task AddDocument(Document document, Func<Document, IndexOperation> createDocumentInsertOperation)
        {
            this.Log.LogInformation("adding document to search index");
            List<Document> documents = new List<Document>();
            documents.Add(document);
            await this.AddDocuments(documents, createDocumentInsertOperation);
        }

        /// <summary>
        /// Will batch add documents to the Azure Search Index. If any documents exist, they will be replaced.
        /// </summary>
        /// <param name="documents">list of documents, with a max limit of 1000</param>
        /// <param name="createDocumentInsertOperation">method that creates the operation</param>
        /// <returns>task to add documents</returns>
        protected async Task AddDocuments(List<Document> documents, Func<Document, IndexOperation> createDocumentInsertOperation)
        {
            this.Log.LogInformation("adding documents to search index");

            // skip if null or too large
            if (documents == null || documents.Count == 0 || documents.Count > 1000)
            {
                this.Log.LogException("got bad parameters");
            }

            // generate operations
            List<IndexOperation> operations = new List<IndexOperation>();
            foreach (Document document in documents)
            {
                IndexOperation operation = createDocumentInsertOperation(document);
                if (operation == null)
                {
                    this.Log.LogException("got null operation");
                }

                operations.Add(operation);
            }

            // issue the insert
            IApiResponse<IEnumerable<IndexOperationResult>> response = await this.managementClient.PopulateAsync(this.indexName, operations.ToArray());
            if (response == null || !response.IsSuccess)
            {
                this.Log.LogException("got service failure: " + ((response == null || response.Error == null) ? string.Empty : response.Error.Message));
            }
        }

        /// <summary>
        /// Deletes a document from the search index in Azure Search
        /// </summary>
        /// <param name="documentHandle">string that uniquely identifies the document</param>
        /// <returns>task to delete a document</returns>
        protected async Task DeleteDocument(string documentHandle)
        {
            this.Log.LogInformation("deleting document " + documentHandle + " from search index");

            // check if important values are null
            if (string.IsNullOrWhiteSpace(documentHandle))
            {
                this.Log.LogException("got empty documentHandle");
            }

            IApiResponse<IEnumerable<IndexOperationResult>> response = await this.managementClient.PopulateAsync(this.indexName, new IndexOperation(IndexOperationType.Delete, this.documentHandleName, documentHandle));
            if (response == null || !response.IsSuccess)
            {
                this.Log.LogException("operation failed: " + ((response == null || response.Error == null) ? string.Empty : response.Error.Message));
            }
        }

        /// <summary>
        /// Issues the search query and gets back the results
        /// </summary>
        /// <param name="query">Properly formatted SearchQuery</param>
        /// <returns>list of dictionaries with each dictionary having the retrievable document fields </returns>
        protected async Task<List<Dictionary<string, object>>> GetSearchResults(SearchQuery query)
        {
            // issue the query
            this.Log.LogInformation("issuing the search query");
            Task<IApiResponse<SearchQueryResult>> task = this.queryClient.SearchAsync(this.indexName, query);

            IApiResponse<SearchQueryResult> response = await task;
            if (response == null || !response.IsSuccess)
            {
                this.Log.LogException("query failed: " + ((response == null || response.Error == null) ? string.Empty : response.Error.Message));
            }

            // extract results
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

            // no results
            if (response.Body == null || response.Body.Records == null || response.Body.Records.Count() == 0)
            {
                this.Log.LogInformation("got no results");
                return results;
            }

            foreach (SearchQueryRecord record in response.Body.Records)
            {
                if (record != null && record.Properties != null && record.Properties.Count > 0)
                {
                    results.Add(record.Properties);
                }
            }

            this.Log.LogInformation("got " + results.Count + " results");
            return results;
        }

        /// <summary>
        /// Gets trending search results
        /// </summary>
        /// <param name="query">Properly formatted SearchQuery for trending</param>
        /// <param name="requestedFieldName">Name of the string field that trending will be done on / faceted on</param>
        /// <returns>list of search result, count</returns>
        protected async Task<List<Tuple<string, double>>> GetTrendingSearchResults(SearchQuery query, string requestedFieldName)
        {
            // issue the query
            query.Select(requestedFieldName);
            this.Log.LogInformation("issuing the search query");
            Task<IApiResponse<SearchQueryResult>> task = this.queryClient.SearchAsync(this.indexName, query);

            IApiResponse<SearchQueryResult> response = await task;
            if (response == null || !response.IsSuccess)
            {
                this.Log.LogException("query failed: " + ((response == null || response.Error == null) ? string.Empty : response.Error.Message));
            }

            // extract results
            List<Tuple<string, double>> results = new List<Tuple<string, double>>();

            // no results
            if (response.Body == null || response.Body.Facets == null || response.Body.Facets.Count != 1 ||
                response.Body.Facets.ToList()[0].Key != requestedFieldName ||
                response.Body.Facets.ToList()[0].Value == null || response.Body.Facets.ToList()[0].Value.Count() == 0)
            {
                this.Log.LogInformation("got no results");
                return results;
            }

            foreach (FacetResult result in response.Body.Facets.ToList()[0].Value)
            {
                if (result != null && result.Count != 0 && !string.IsNullOrEmpty(result.Value))
                {
                    results.Add(new Tuple<string, double>(result.Value, result.Count));
                }
            }

            this.Log.LogInformation("got " + results.Count + " results");
            return results;
        }

        /// <summary>
        /// Gets autocomplete results
        /// </summary>
        /// <param name="query">Properly formatted SearchQuery for autocomplete</param>
        /// <param name="requestedFieldName">Name of the string field that will be autocompleted</param>
        /// <returns>list of matching strings</returns>
        protected async Task<List<string>> GetAutoCompleteResults(SuggestionQuery query, string requestedFieldName)
        {
            // issue the query
            query.Select(requestedFieldName);
            query.SearchField(requestedFieldName);
            this.Log.LogInformation("issuing the search query");
            Task<IApiResponse<SuggestionResult>> task = this.queryClient.SuggestAsync(this.indexName, query);

            IApiResponse<SuggestionResult> response = await task;
            if (response == null || !response.IsSuccess)
            {
                this.Log.LogException("query failed: " + ((response == null || response.Error == null) ? string.Empty : response.Error.Message));
            }

            // extract results
            List<string> results = new List<string>();

            // no results
            if (response.Body == null || response.Body.Records == null || response.Body.Records.Count() == 0)
            {
                this.Log.LogInformation("got no results");
                return results;
            }

            foreach (SuggestionResultRecord result in response.Body.Records)
            {
                if (result != null && result.Text != null && result.Text.Length >= 3 && !results.Contains(result.Text))
                {
                    results.Add(result.Text);
                }
            }

            this.Log.LogInformation("got " + results.Count + " unique results");
            return results;
        }

        /// <summary>
        /// Create the given index
        /// </summary>
        /// <param name="index">Properly filled out index</param>
        /// <returns>task to create the index</returns>
        protected async Task CreateIndex(Index index)
        {
            // does index exist?
            this.Log.LogInformation("searching for existing index");
            Task<IApiResponse<Index>> task = this.managementClient.GetIndexAsync(this.indexName);
            IApiResponse<Index> response = await task;
            if (response == null || (!response.IsSuccess && response.StatusCode != System.Net.HttpStatusCode.NotFound))
            {
                this.Log.LogException("get index failed: " + ((response == null || response.Error == null) ? string.Empty : response.Error.Message));
            }

            // create index if it doesn't exist
            if (response == null || response.Body == null || response.Body.Name == null || response.Body.Name != this.indexName)
            {
                this.Log.LogInformation("creating index");

                // submit the create task
                Task<IApiResponse<Index>> task2 = this.managementClient.CreateIndexAsync(index);
                IApiResponse<Index> response2 = await task2;
                if (response2 == null || !response2.IsSuccess)
                {
                    this.Log.LogException("create index failed: " + ((response2 == null || response2.Error == null) ? string.Empty : response2.Error.Message));
                }
            }
        }

        /// <summary>
        /// Useless method to get around a compile/runtime issue
        /// </summary>
        private void DummyTypeReference()
        {
            // The Reddog.Search assembly is used by this file, and that assembly depends on System.Net.Http.Formatting.
            // Because this project does not have any code that explicitly references assembly System.Net.Http.Formatting,
            // we must include the code below to ensure that Visual Studio will copy the System.Net.Http.Formatting assembly
            // to any projects that include SocialPlus.Server.Search.
            var dummyType = typeof(System.Net.Http.Formatting.JsonMediaTypeFormatter);
            Console.WriteLine(dummyType.FullName);
        }
    }
}
