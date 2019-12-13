// <copyright file="UserReportsStore.cs" company="Microsoft">
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
    public class UserReportsStore : IUserReportsStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserReportsStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public UserReportsStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Insert a new user-generated report of another user into the store
        /// </summary>
        /// <param name="storageConsistencyMode">consistency to use</param>
        /// <param name="reportHandle">uniquely identifies this report</param>
        /// <param name="reportedUserHandle">uniquely identifies the user who is being reported</param>
        /// <param name="reportingUserHandle">uniquely identifies the user doing the reporting</param>
        /// <param name="appHandle">uniquely identifies the app that the user is in</param>
        /// <param name="reason">the complaint against the content</param>
        /// <param name="lastUpdatedTime">when the report was received</param>
        /// <param name="hasComplainedBefore">has the reporting user complained about this user before?</param>
        /// <returns>a task that inserts the report into the store</returns>
        public async Task InsertUserReport(
            StorageConsistencyMode storageConsistencyMode,
            string reportHandle,
            string reportedUserHandle,
            string reportingUserHandle,
            string appHandle,
            ReportReason reason,
            DateTime lastUpdatedTime,
            bool hasComplainedBefore)
        {
            // get all the table interfaces
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserReports);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserReports, TableIdentifier.UserReportsLookup) as ObjectTable;
            ObjectTable lookupUniquenessTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserReports, TableIdentifier.UserReportsLookupUniquenessByReportingUser) as ObjectTable;
            FeedTable feedByAppTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserReports, TableIdentifier.UserReportsRecentFeedByApp) as FeedTable;
            FeedTable feedByReportedUserTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserReports, TableIdentifier.UserReportsRecentFeedByReportedUser) as FeedTable;
            FeedTable feedByReportingUserTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserReports, TableIdentifier.UserReportsRecentFeedByReportingUser) as FeedTable;
            CountTable countByReportedUserTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserReports, TableIdentifier.UserReportsCountByReportedUser) as CountTable;
            CountTable countByReportingUserTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserReports, TableIdentifier.UserReportsCountByReportingUser) as CountTable;

            // create the two entities that will be inserted into the tables
            UserReportEntity userReportEntity = new UserReportEntity()
            {
                ReportedUserHandle = reportedUserHandle,
                ReportingUserHandle = reportingUserHandle,
                AppHandle = appHandle,
                Reason = reason,
                CreatedTime = lastUpdatedTime
            };

            UserReportFeedEntity userReportFeedEntity = new UserReportFeedEntity()
            {
                ReportHandle = reportHandle,
                ReportedUserHandle = reportedUserHandle,
                ReportingUserHandle = reportingUserHandle,
                AppHandle = appHandle
            };

            // do the inserts and increments as a transaction
            Transaction transaction = new Transaction();

            // the partition key is app handle for all tables so that a transaction can be achieved
            transaction.Add(Operation.Insert(lookupTable, appHandle, reportHandle, userReportEntity));
            transaction.Add(Operation.Insert(feedByAppTable, appHandle, appHandle, reportHandle, userReportFeedEntity));
            transaction.Add(Operation.Insert(feedByReportedUserTable, appHandle, reportedUserHandle, reportHandle, userReportFeedEntity));
            transaction.Add(Operation.Insert(feedByReportingUserTable, appHandle, reportingUserHandle, reportHandle, userReportFeedEntity));

            // if the reporting user has not previously reported this user, then increment counts
            if (!hasComplainedBefore)
            {
                string uniquenessKey = UniquenessObjectKey(reportedUserHandle, reportingUserHandle);
                transaction.Add(Operation.Insert(lookupUniquenessTable, appHandle, uniquenessKey, new ObjectEntity()));
                transaction.Add(Operation.InsertOrIncrement(countByReportedUserTable, appHandle, reportedUserHandle));
                transaction.Add(Operation.InsertOrIncrement(countByReportingUserTable, appHandle, reportingUserHandle));
            }

            // execute the transaction
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Look up a particular user report
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the user is in</param>
        /// <param name="reportHandle">uniquely identifies the report</param>
        /// <returns>a task that returns the user report</returns>
        public async Task<IUserReportEntity> QueryUserReport(string appHandle, string reportHandle)
        {
            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserReports);
            ObjectTable lookupTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserReports, TableIdentifier.UserReportsLookup) as ObjectTable;

            // do the lookup & return it
            return await store.QueryObjectAsync<UserReportEntity>(lookupTable, appHandle, reportHandle);
        }

        /// <summary>
        /// Look up all the reports against a particular user
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the user is in</param>
        /// <param name="reportedUserHandle">uniquely identifies the user who is being reported</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of user reports</returns>
        public async Task<IList<IUserReportFeedEntity>> QueryUserReportsByReportedUser(string appHandle, string reportedUserHandle, string cursor, int limit)
        {
            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserReports);
            FeedTable feedByReportedUserTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserReports, TableIdentifier.UserReportsRecentFeedByReportedUser) as FeedTable;

            // get the feed & return it
            var result = await store.QueryFeedAsync<UserReportFeedEntity>(feedByReportedUserTable, appHandle, reportedUserHandle, cursor, limit);
            return result.ToList<IUserReportFeedEntity>();
        }

        /// <summary>
        /// Look up all the user reports from a particular user who is doing the reporting
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the user is in</param>
        /// <param name="reportingUserHandle">uniquely identifies the user</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of user reports</returns>
        public async Task<IList<IUserReportFeedEntity>> QueryUserReportsByReportingUser(string appHandle, string reportingUserHandle, string cursor, int limit)
        {
            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserReports);
            FeedTable feedByReportingUserTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserReports, TableIdentifier.UserReportsRecentFeedByReportingUser) as FeedTable;

            // get the feed & return it
            var result = await store.QueryFeedAsync<UserReportFeedEntity>(feedByReportingUserTable, appHandle, reportingUserHandle, cursor, limit);
            return result.ToList<IUserReportFeedEntity>();
        }

        /// <summary>
        /// Look up all the reports against users from a particular app
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of user reports</returns>
        public async Task<IList<IUserReportFeedEntity>> QueryUserReportsByApp(string appHandle, string cursor, int limit)
        {
            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserReports);
            FeedTable feedByAppTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserReports, TableIdentifier.UserReportsRecentFeedByApp) as FeedTable;

            // get the feed & return it
            var result = await store.QueryFeedAsync<UserReportFeedEntity>(feedByAppTable, appHandle, appHandle, cursor, limit);
            return result.ToList<IUserReportFeedEntity>();
        }

        /// <summary>
        /// Look up the number of unique users reporting against a particular user
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the user is in</param>
        /// <param name="reportedUserHandle">uniquely identifies the user who is being reported</param>
        /// <returns>a task that returns the number of reports</returns>
        public async Task<long> QueryUserReportCount(string appHandle, string reportedUserHandle)
        {
            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserReports);
            CountTable countByReportedUserTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserReports, TableIdentifier.UserReportsCountByReportedUser) as CountTable;

            // query the count
            CountEntity countEntity = await store.QueryCountAsync(countByReportedUserTable, appHandle, reportedUserHandle);
            if (countEntity == null || countEntity.PartitionKey != appHandle || countEntity.CountKey != reportedUserHandle)
            {
                return 0;
            }

            return Convert.ToInt64(countEntity.Count);
        }

        /// <summary>
        /// Has the reporting user already complained about this user?
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the users are in</param>
        /// <param name="reportedUserHandle">uniquely identifies the user who is being reported</param>
        /// <param name="reportingUserHandle">uniquely identifies the user doing the reporting</param>
        /// <returns>true if this user has previously complained about this user</returns>
        public async Task<bool> HasReportingUserReportedUserBefore(string appHandle, string reportedUserHandle, string reportingUserHandle)
        {
            // check the inputs
            if (string.IsNullOrWhiteSpace(appHandle))
            {
                throw new ArgumentNullException("appHandle");
            }
            else if (string.IsNullOrWhiteSpace(reportedUserHandle))
            {
                throw new ArgumentNullException("reportedUserHandle");
            }
            else if (string.IsNullOrWhiteSpace(reportingUserHandle))
            {
                throw new ArgumentNullException("reportingUserHandle");
            }

            // get the table interface
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.UserReports);
            ObjectTable lookupUniquenessTable = this.tableStoreManager.GetTable(ContainerIdentifier.UserReports, TableIdentifier.UserReportsLookupUniquenessByReportingUser) as ObjectTable;

            // check if the reporting user has previously reported this user
            string uniquenessKey = UniquenessObjectKey(reportedUserHandle, reportingUserHandle);
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
        /// <param name="reportedUserHandle">User being reported</param>
        /// <param name="reportingUserHandle">User doing the reporting</param>
        /// <returns>Object key</returns>
        private static string UniquenessObjectKey(string reportedUserHandle, string reportingUserHandle)
        {
            return string.Join("+", reportedUserHandle, reportingUserHandle);
        }
    }
}
