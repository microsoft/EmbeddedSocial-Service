// <copyright file="ContentReportsStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// Content reports store
    /// </summary>
    public class ContentReportsStore : IContentReportsStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReportsStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public ContentReportsStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Insert a new user-generated report of content into the store
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="reportHandle">uniquely identifies this report</param>
        /// <param name="contentType">the type of content being reported</param>
        /// <param name="contentHandle">uniquely identifies the content</param>
        /// <param name="contentUserHandle">uniquely identifies the user who authored the content</param>
        /// <param name="reportingUserHandle">uniquely identifies the user doing the reporting</param>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="reason">the complaint against the content</param>
        /// <param name="createdTime">when the report was received</param>
        /// <param name="hasComplainedBefore">has the reporting user complained about this content before?</param>
        /// <returns>a task that inserts the report into the store</returns>
        public async Task InsertContentReport(
            StorageConsistencyMode storageConsistencyMode,
            string reportHandle,
            ContentType contentType,
            string contentHandle,
            string contentUserHandle,
            string reportingUserHandle,
            string appHandle,
            ReportReason reason,
            DateTime createdTime,
            bool hasComplainedBefore)
        {
            // get all the table interfaces
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.ContentReports);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsLookup) as ObjectTable;
            ObjectTable lookupUniquenessTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsLookupUniquenessByReportingUser) as ObjectTable;
            FeedTable feedByAppTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsRecentFeedByApp) as FeedTable;
            FeedTable feedByContentTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsRecentFeedByContent) as FeedTable;
            FeedTable feedByContentUserTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsRecentFeedByContentUser) as FeedTable;
            FeedTable feedByReportingUserTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsRecentFeedByReportingUser) as FeedTable;
            CountTable countByContentTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsCountByContent) as CountTable;
            CountTable countByContentUserTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsCountByContentUser) as CountTable;
            CountTable countByReportingUserTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsCountByReportingUser) as CountTable;

            // create the two entities that will be inserted into the tables
            ContentReportEntity contentReportEntity = new ContentReportEntity()
            {
                ContentType = contentType,
                ContentHandle = contentHandle,
                ContentUserHandle = contentUserHandle,
                ReportingUserHandle = reportingUserHandle,
                AppHandle = appHandle,
                Reason = reason,
                CreatedTime = createdTime
            };

            ContentReportFeedEntity contentReportFeedEntity = new ContentReportFeedEntity()
            {
                ReportHandle = reportHandle,
                ContentType = contentType,
                ContentHandle = contentHandle,
                ContentUserHandle = contentUserHandle,
                ReportingUserHandle = reportingUserHandle,
                AppHandle = appHandle
            };

            // do the inserts and increments as a transaction
            Transaction transaction = new Transaction();

            // the partition key is app handle for all tables so that a transaction can be achieved
            transaction.Add(Operation.Insert(lookupTable, appHandle, reportHandle, contentReportEntity));
            transaction.Add(Operation.Insert(feedByAppTable, appHandle, appHandle, reportHandle, contentReportFeedEntity));
            transaction.Add(Operation.Insert(feedByContentTable, appHandle, contentHandle, reportHandle, contentReportFeedEntity));

            // only add to the feedByContentUserTable if this is user-generated content
            if (!string.IsNullOrWhiteSpace(contentUserHandle))
            {
                transaction.Add(Operation.Insert(feedByContentUserTable, appHandle, contentUserHandle, reportHandle, contentReportFeedEntity));
            }

            transaction.Add(Operation.Insert(feedByReportingUserTable, appHandle, reportingUserHandle, reportHandle, contentReportFeedEntity));

            // if the reporting user has not previously reported this content, then increment counts
            if (!hasComplainedBefore)
            {
                string uniquenessKey = UniquenessObjectKey(contentHandle, reportingUserHandle);
                transaction.Add(Operation.Insert(lookupUniquenessTable, appHandle, uniquenessKey, new ObjectEntity()));
                transaction.Add(Operation.InsertOrIncrement(countByContentTable, appHandle, contentHandle));

                // only increment the countByContentUserTable if this is user-generated content
                if (!string.IsNullOrWhiteSpace(contentUserHandle))
                {
                    transaction.Add(Operation.InsertOrIncrement(countByContentUserTable, appHandle, contentUserHandle));
                }

                transaction.Add(Operation.InsertOrIncrement(countByReportingUserTable, appHandle, reportingUserHandle));
            }

            // execute the transaction
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Look up a particular content report
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="reportHandle">uniquely identifies the report</param>
        /// <returns>a task that returns the content report</returns>
        public async Task<IContentReportEntity> QueryContentReport(string appHandle, string reportHandle)
        {
            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.ContentReports);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsLookup) as ObjectTable;

            // do the lookup & return it
            return await store.QueryObjectAsync<ContentReportEntity>(lookupTable, appHandle, reportHandle);
        }

        /// <summary>
        /// Look up all the reports against a particular content
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="contentHandle">uniquely identifies the content</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of content reports</returns>
        public async Task<IList<IContentReportFeedEntity>> QueryContentReportsByContent(string appHandle, string contentHandle, string cursor, int limit)
        {
            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.ContentReports);
            FeedTable feedByContentTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsRecentFeedByContent) as FeedTable;

            // get the feed & return it
            var result = await store.QueryFeedAsync<ContentReportFeedEntity>(feedByContentTable, appHandle, contentHandle, cursor, limit);
            return result.ToList<IContentReportFeedEntity>();
        }

        /// <summary>
        /// Look up all the reports against content authored by a particular user
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="userHandle">uniquely identifies the user that created content</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of content reports</returns>
        public async Task<IList<IContentReportFeedEntity>> QueryContentReportsByContentUser(string appHandle, string userHandle, string cursor, int limit)
        {
            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.ContentReports);
            FeedTable feedByContentUserTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsRecentFeedByContentUser) as FeedTable;

            // get the feed & return it
            var result = await store.QueryFeedAsync<ContentReportFeedEntity>(feedByContentUserTable, appHandle, userHandle, cursor, limit);
            return result.ToList<IContentReportFeedEntity>();
        }

        /// <summary>
        /// Look up all the reports against content from a particular user who is doing the reporting
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="userHandle">uniquely identifies the user that reported content</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of content reports</returns>
        public async Task<IList<IContentReportFeedEntity>> QueryContentReportsByReportingUser(string appHandle, string userHandle, string cursor, int limit)
        {
            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.ContentReports);
            FeedTable feedByReportingUserTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsRecentFeedByReportingUser) as FeedTable;

            // get the feed & return it
            var result = await store.QueryFeedAsync<ContentReportFeedEntity>(feedByReportingUserTable, appHandle, userHandle, cursor, limit);
            return result.ToList<IContentReportFeedEntity>();
        }

        /// <summary>
        /// Look up all the reports against content from a particular app
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of content reports</returns>
        public async Task<IList<IContentReportFeedEntity>> QueryContentReportsByApp(string appHandle, string cursor, int limit)
        {
            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.ContentReports);
            FeedTable feedByAppTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsRecentFeedByApp) as FeedTable;

            // get the feed & return it
            var result = await store.QueryFeedAsync<ContentReportFeedEntity>(feedByAppTable, appHandle, appHandle, cursor, limit);
            return result.ToList<IContentReportFeedEntity>();
        }

        /// <summary>
        /// Look up the number of unique users reporting against a particular content
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="contentHandle">uniquely identifies the content</param>
        /// <returns>a task that returns the number of reports</returns>
        public async Task<long> QueryContentReportCount(string appHandle, string contentHandle)
        {
            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.ContentReports);
            CountTable countByContentTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsCountByContent) as CountTable;

            // query the count
            CountEntity countEntity = await store.QueryCountAsync(countByContentTable, appHandle, contentHandle);
            if (countEntity == null || countEntity.PartitionKey != appHandle || countEntity.CountKey != contentHandle)
            {
                return 0;
            }

            return Convert.ToInt64(countEntity.Count);
        }

        /// <summary>
        /// Has the reporting user already complained about this content?
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="contentHandle">uniquely identifies the content</param>
        /// <param name="reportingUserHandle">uniquely identifies the user doing the reporting</param>
        /// <returns>true if this user has previously complained about this content</returns>
        public async Task<bool> HasReportingUserReportedContentBefore(string appHandle, string contentHandle, string reportingUserHandle)
        {
            // check the inputs
            if (string.IsNullOrWhiteSpace(appHandle))
            {
                throw new ArgumentNullException("appHandle");
            }
            else if (string.IsNullOrWhiteSpace(contentHandle))
            {
                throw new ArgumentNullException("contentHandle");
            }
            else if (string.IsNullOrWhiteSpace(reportingUserHandle))
            {
                throw new ArgumentNullException("reportingUserHandle");
            }

            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.ContentReports);
            ObjectTable lookupUniquenessTable = this.tableStoreManager.GetTable(ContainerIdentifier.ContentReports, TableIdentifier.ContentReportsLookupUniquenessByReportingUser) as ObjectTable;

            // check if the reporting user has previously reported this content
            string uniquenessKey = UniquenessObjectKey(contentHandle, reportingUserHandle);
            ObjectEntity uniquenessObject = await store.QueryObjectAsync<ObjectEntity>(lookupUniquenessTable, appHandle, uniquenessKey);
            if (uniquenessObject == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Construct key for looking up the uniqueness table
        /// </summary>
        /// <param name="contentHandle">Content being reported</param>
        /// <param name="reportingUserHandle">User doing the reporting</param>
        /// <returns>Object key</returns>
        private static string UniquenessObjectKey(string contentHandle, string reportingUserHandle)
        {
            return string.Join("+", contentHandle, reportingUserHandle);
        }
    }
}
