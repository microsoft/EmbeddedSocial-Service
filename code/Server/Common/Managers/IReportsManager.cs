// <copyright file="IReportsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Reports manager interface
    /// </summary>
    public interface IReportsManager
    {
        /// <summary>
        /// Record a new content report
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="reportHandle">Report handle</param>
        /// <param name="contentType">Content type</param>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="contentUserHandle">User handle for the creator of the content</param>
        /// <param name="reportingUserHandle">User handle for the user who is doing the reporting</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="reason">Report reason</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Update content report task</returns>
        Task CreateContentReport(
            ProcessType processType,
            string reportHandle,
            ContentType contentType,
            string contentHandle,
            string contentUserHandle,
            string reportingUserHandle,
            string appHandle,
            ReportReason reason,
            DateTime lastUpdatedTime,
            Uri callbackUri);

        /// <summary>
        /// Do further processing on a content report and ask a human to review it
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportHandle">Report handle</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Process content report task</returns>
        Task SubmitContentReportForReview(ProcessType processType, string appHandle, string reportHandle, Uri callbackUri);

        /// <summary>
        /// Read content report
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportHandle">Report handle</param>
        /// <returns>Content report</returns>
        Task<IContentReportEntity> ReadContentReport(string appHandle, string reportHandle);

        /// <summary>
        /// Read content reports for a content
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of content report feed entities</returns>
        Task<IList<IContentReportFeedEntity>> ReadContentReportsForContent(string appHandle, string contentHandle, string cursor, int limit);

        /// <summary>
        /// Read content reports for content created by a user
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="contentUserHandle">User handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of content report feed entities</returns>
        Task<IList<IContentReportFeedEntity>> ReadContentReportsForUser(string appHandle, string contentUserHandle, string cursor, int limit);

        /// <summary>
        /// Read content reports for content reported by a user
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportingUserHandle">User handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of content report feed entities</returns>
        Task<IList<IContentReportFeedEntity>> ReadContentReportsFromUser(string appHandle, string reportingUserHandle, string cursor, int limit);

        /// <summary>
        /// Read content reports for content posted from an app
        /// </summary>
        /// <param name="appHandle">Appp handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of content report feed entities</returns>
        Task<IList<IContentReportFeedEntity>> ReadContentReportsForApp(string appHandle, string cursor, int limit);

        /// <summary>
        /// Read count of reports for a content
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="contentHandle">Content handle</param>
        /// <returns>Reports count for content</returns>
        Task<long> ReadContentReportsCount(string appHandle, string contentHandle);

        /// <summary>
        /// Record a new user report
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="reportHandle">Report handle</param>
        /// <param name="reportedUserHandle">User handle for the user being reported on</param>
        /// <param name="reportingUserHandle">User handle for the user who is doing the reporting</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="reason">Report reason</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Update content report task</returns>
        Task CreateUserReport(
            ProcessType processType,
            string reportHandle,
            string reportedUserHandle,
            string reportingUserHandle,
            string appHandle,
            ReportReason reason,
            DateTime lastUpdatedTime,
            Uri callbackUri);

        /// <summary>
        /// Do further processing on a user report and ask a human to review it
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportHandle">Report handle</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Process user report task</returns>
        Task SubmitUserReportForReview(ProcessType processType, string appHandle, string reportHandle, Uri callbackUri);

        /// <summary>
        /// Read user report
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportHandle">Report handle</param>
        /// <returns>User report</returns>
        Task<IUserReportEntity> ReadUserReport(string appHandle, string reportHandle);

        /// <summary>
        /// Read user reports against a user
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportedUserHandle">User handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user report feed entities</returns>
        Task<IList<IUserReportFeedEntity>> ReadUserReportsForUser(string appHandle, string reportedUserHandle, string cursor, int limit);

        /// <summary>
        /// Read user reports reported by a user
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportingUserHandle">User handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user report feed entities</returns>
        Task<IList<IUserReportFeedEntity>> ReadUserReportsFromUser(string appHandle, string reportingUserHandle, string cursor, int limit);

        /// <summary>
        /// Read user reports posted from an app
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user report feed entities</returns>
        Task<IList<IUserReportFeedEntity>> ReadUserReportsForApp(string appHandle, string cursor, int limit);

        /// <summary>
        /// Get count of reports against a user
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportedUserHandle">User being reported against</param>
        /// <returns>Reports count against a user</returns>
        Task<long> ReadUserReportsCount(string appHandle, string reportedUserHandle);

        /// <summary>
        /// Process the response from a human review of a report
        /// </summary>
        /// <param name="reportHandle">Uniquely identifies the report</param>
        /// <param name="review">Results of the review</param>
        /// <returns>Task that processes the result</returns>
        Task ProcessReportResult(string reportHandle, string review);
    }
}
