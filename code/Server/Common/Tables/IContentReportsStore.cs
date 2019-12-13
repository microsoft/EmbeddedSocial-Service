// <copyright file="IContentReportsStore.cs" company="Microsoft">
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
    /// Interface to the store that contains user-generated reporting of content
    /// </summary>
    public interface IContentReportsStore
    {
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
        /// <param name="lastUpdatedTime">when the report was received</param>
        /// <param name="hasComplainedBefore">has the reporting user complained about this content before?</param>
        /// <returns>a task that inserts the report into the store</returns>
        Task InsertContentReport(
            StorageConsistencyMode storageConsistencyMode,
            string reportHandle,
            ContentType contentType,
            string contentHandle,
            string contentUserHandle,
            string reportingUserHandle,
            string appHandle,
            ReportReason reason,
            DateTime lastUpdatedTime,
            bool hasComplainedBefore);

        /// <summary>
        /// Look up a particular content report
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="reportHandle">uniquely identifies the report</param>
        /// <returns>a task that returns the content report</returns>
        Task<IContentReportEntity> QueryContentReport(string appHandle, string reportHandle);

        /// <summary>
        /// Look up all the reports against a particular content
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="contentHandle">uniquely identifies the content</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of content reports</returns>
        Task<IList<IContentReportFeedEntity>> QueryContentReportsByContent(string appHandle, string contentHandle, string cursor, int limit);

        /// <summary>
        /// Look up all the reports against content authored by a particular user
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="userHandle">uniquely identifies the user that created content</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of content reports</returns>
        Task<IList<IContentReportFeedEntity>> QueryContentReportsByContentUser(string appHandle, string userHandle, string cursor, int limit);

        /// <summary>
        /// Look up all the reports against content from a particular user who is doing the reporting
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="userHandle">uniquely identifies the user that reported content</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of content reports</returns>
        Task<IList<IContentReportFeedEntity>> QueryContentReportsByReportingUser(string appHandle, string userHandle, string cursor, int limit);

        /// <summary>
        /// Look up all the reports against content from a particular app
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>a task that returns a list of content reports</returns>
        Task<IList<IContentReportFeedEntity>> QueryContentReportsByApp(string appHandle, string cursor, int limit);

        /// <summary>
        /// Look up the number of unique users reporting against a particular content
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="contentHandle">uniquely identifies the content</param>
        /// <returns>a task that returns the number of reports</returns>
        Task<long> QueryContentReportCount(string appHandle, string contentHandle);

        /// <summary>
        /// Has the reporting user already complained about this content?
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="contentHandle">uniquely identifies the content</param>
        /// <param name="reportingUserHandle">uniquely identifies the user doing the reporting</param>
        /// <returns>true if this user has previously complained about this content</returns>
        Task<bool> HasReportingUserReportedContentBefore(string appHandle, string contentHandle, string reportingUserHandle);
    }
}
