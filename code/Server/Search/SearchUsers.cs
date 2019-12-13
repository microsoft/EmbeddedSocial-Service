// <copyright file="SearchUsers.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Search
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using RedDog.Search.Model;
    using SocialPlus.Logging;

    /// <summary>
    /// Search API for users
    /// </summary>
    public class SearchUsers : SearchBase
    {
        /// <summary>
        /// Name of the search index
        /// </summary>
        private const string IndexName = "users";

        /// <summary>
        /// Name of the index column of the search index
        /// </summary>
        private const string DocumentHandleName = "key";

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchUsers"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="serviceName">name of the search service</param>
        /// <param name="serviceAdminKey">key to access the search service</param>
        public SearchUsers(ILog log, string serviceName, string serviceAdminKey)
            : base(log, serviceName, serviceAdminKey, IndexName, DocumentHandleName)
        {
        }

        /// <summary>
        /// Find users that match a string, and optionally skip first n records and return optionally some records.
        /// Supports the following operators: suffix: "foo*", and: "foo+bar", or: "foo|bar", not: "-foo", phrase: ""foo bar"", precedence: <c>"foo+(bar|baz)"</c>.
        /// You need to escape * if it is at the end of a word, and - if it is at the start of a word.
        /// Default is to use and, so if you use whitespace to separate words, "foo bar", that is equivalent to "foo+bar".
        /// </summary>
        /// <param name="name">what string to search for</param>
        /// <param name="appHandle">optional -- what app handle to search within</param>
        /// <param name="skipRecords">optional -- skip these many first records (max allowed is 100000)</param>
        /// <param name="numRecords">optional -- return only these many records (max allowed is 1000)</param>
        /// <returns>list of user search results; empty list if no results</returns>
        public async Task<List<UserDocumentResult>> Search(string name, string appHandle = null, long skipRecords = 0, long numRecords = 50)
        {
            this.Log.LogInformation("searching for user string: " + name + ", in app: " + appHandle);

            // check parameter ranges
            if (skipRecords < 0 || skipRecords > MaxSkipRecords || numRecords < 1 || numRecords > MaxRecords)
            {
                this.Log.LogException("got bad parameters");
            }

            // form the query
            SearchQuery query = new SearchQuery(name)
                                .Mode(SearchMode.All)
                                .Skip(skipRecords)
                                .Top(numRecords)
                                .Count(false);
            if (!string.IsNullOrWhiteSpace(appHandle))
            {
                query.Filter = "appHandle eq '" + appHandle + "'";
            }

            // issue it
            List<Dictionary<string, object>> results = await this.GetSearchResults(query);

            // parse the results
            List<UserDocumentResult> response = new List<UserDocumentResult>();
            if (results == null || results.Count == 0)
            {
                return response;
            }

            foreach (Dictionary<string, object> result in results)
            {
                if (result.ContainsKey("appHandle") && result.ContainsKey("userHandle"))
                {
                    response.Add(new UserDocumentResult(result["appHandle"].ToString(), result["userHandle"].ToString()));
                }
            }

            return response;
        }

        /// <summary>
        /// Create the user index.
        /// </summary>
        /// <returns>create index task</returns>
        public async Task CreateIndex()
        {
            // form the index
            Index index = new Index(IndexName)
                        .WithStringField("key",        f => f.IsKey(true).IsRetrievable(true).IsSearchable(false).IsFilterable(false).SupportSuggestions(false).IsSortable(false).IsFacetable(false))
                        .WithStringField("firstName",  f => f.IsKey(false).IsRetrievable(false).IsSearchable(true).IsFilterable(false).SupportSuggestions(false).IsSortable(false).IsFacetable(false))
                        .WithStringField("lastName",   f => f.IsKey(false).IsRetrievable(false).IsSearchable(true).IsFilterable(false).SupportSuggestions(false).IsSortable(false).IsFacetable(false))
                        .WithStringField("appHandle",  f => f.IsKey(false).IsRetrievable(true).IsSearchable(false).IsFilterable(true).SupportSuggestions(false).IsSortable(false).IsFacetable(false))
                        .WithStringField("userHandle", f => f.IsKey(false).IsRetrievable(true).IsSearchable(false).IsFilterable(false).SupportSuggestions(false).IsSortable(false).IsFacetable(false));

            // submit the create task
            await this.CreateIndex(index);
        }

        /// <summary>
        /// adds a User to the search index
        /// </summary>
        /// <param name="user">User to add</param>
        /// <returns>add user task</returns>
        public async Task AddUser(UserDocument user)
        {
            await this.AddDocument(user, this.CreateDocumentInsertOperation);
        }

        /// <summary>
        /// adds users to the search index
        /// </summary>
        /// <param name="users">users to add</param>
        /// <returns>add users task</returns>
        public async Task AddUsers(List<UserDocument> users)
        {
            List<Document> documents = users.Cast<Document>().ToList();
            await this.AddDocuments(documents, this.CreateDocumentInsertOperation);
        }

        /// <summary>
        /// deletes a user from the search index
        /// </summary>
        /// <param name="userHandle">user handle to delete</param>
        /// <param name="appHandle">which app this user handle is to be deleted from</param>
        /// <returns>delete user task</returns>
        public async Task DeleteUser(string userHandle, string appHandle)
        {
            // check if important values are null
            if (string.IsNullOrWhiteSpace(userHandle) || string.IsNullOrWhiteSpace(appHandle))
            {
                this.Log.LogException("got bad parameters");
            }

            string key = userHandle + appHandle;
            await this.DeleteDocument(userHandle);
        }

        /// <summary>
        /// Creates an insert operation for the supplied user
        /// </summary>
        /// <param name="document">the user to create an operation for</param>
        /// <returns>insert operation</returns>
        private IndexOperation CreateDocumentInsertOperation(Document document)
        {
            this.Log.LogInformation("creating user insert");

            UserDocument user = null;
            if (document is UserDocument)
            {
                user = (UserDocument)document;
            }
            else
            {
                this.Log.LogException("got document that is not a user document");
            }

            // check if important values are null
            if (user == null || string.IsNullOrWhiteSpace(user.Key) || string.IsNullOrWhiteSpace(user.UserHandle) || string.IsNullOrWhiteSpace(user.AppHandle))
            {
                this.Log.LogException("got bad parameters");
            }

            IndexOperation operation = new IndexOperation(IndexOperationType.Upload, "key", user.Key)
                                                              .WithProperty("firstName", user.FirstName)
                                                              .WithProperty("lastName", user.LastName)
                                                              .WithProperty("appHandle", user.AppHandle)
                                                              .WithProperty("userHandle", user.UserHandle);
            return operation;
        }
    }
}
