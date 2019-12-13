// <copyright file="IUserReportsStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Interface to the store that contains user-generated reporting of users
    /// </summary>
    public interface IUserReportsStore
    {
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
        Task InsertUserReport(
            StorageConsistencyMode storageConsistencyMode,
            string reportHandle,
            string reportedUserHandle,
            string reportingUserHandle,
            string appHandle,
            ReportReason reason,
            DateTime lastUpdatedTime,
            bool hasComplainedBefore);

        /// <summary>
        /// Look up a particular user report
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the user is in</param>
        /// <param name="reportHandle">uniquely identifies the report</param>
        /// <returns>a task that returns the user report</returns>
        Task<IUserReportEntity> QueryUserReport(string appHandle, string reportHandle);

        /// <summary>
        /// Look up all the reports against a particular user
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the user is in</param>
        /// <param name="reportedUserHandle">uniquely identifies the user who is being reported</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of user reports</returns>
        Task<IList<IUserReportFeedEntity>> QueryUserReportsByReportedUser(string appHandle, string reportedUserHandle, string cursor, int limit);

        /// <summary>
        /// Look up all the user reports from a particular user who is doing the reporting
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the user is in</param>
        /// <param name="reportingUserHandle">uniquely identifies the user</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of user reports</returns>
        Task<IList<IUserReportFeedEntity>> QueryUserReportsByReportingUser(string appHandle, string reportingUserHandle, string cursor, int limit);

        /// <summary>
        /// Look up all the reports against users from a particular app
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of user reports</returns>
        Task<IList<IUserReportFeedEntity>> QueryUserReportsByApp(string appHandle, string cursor, int limit);

        /// <summary>
        /// Look up the number of unique users reporting against a particular user
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the user is in</param>
        /// <param name="reportedUserHandle">uniquely identifies the user who is being reported</param>
        /// <returns>a task that returns the number of reports</returns>
        Task<long> QueryUserReportCount(string appHandle, string reportedUserHandle);

        /// <summary>
        /// Has the reporting user already complained about this user?
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the users are in</param>
        /// <param name="reportedUserHandle">uniquely identifies the user who is being reported</param>
        /// <param name="reportingUserHandle">uniquely identifies the user doing the reporting</param>
        /// <returns>true if this user has previously complained about this user</returns>
        Task<bool> HasReportingUserReportedUserBefore(string appHandle, string reportedUserHandle, string reportingUserHandle);
    }
}
