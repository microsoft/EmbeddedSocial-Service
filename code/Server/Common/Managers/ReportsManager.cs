// <copyright file="ReportsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.AVERT;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Reports manager class
    /// </summary>
    public class ReportsManager : IReportsManager
    {
        /// <summary>
        /// Default reporting threshold to be used if the app developer has not specified one.
        /// Each time the report count increments by this amount, a review will be requested.
        /// If set to 1, a review will be requested on each report.
        /// </summary>
        private const long DefaultReportThreshold = 1;

        /// <summary>
        /// Content reports store
        /// </summary>
        private readonly IContentReportsStore contentReportsStore;

        /// <summary>
        /// User reports store
        /// </summary>
        private readonly IUserReportsStore userReportsStore;

        /// <summary>
        /// App configuration store
        /// </summary>
        private readonly IAppsStore appsStore;

        /// <summary>
        /// Users store
        /// </summary>
        private readonly IUsersStore usersStore;

        /// <summary>
        /// AVERT store
        /// </summary>
        private readonly IAVERTStore avertStore;

        /// <summary>
        /// Queue to send reports to the worker role
        /// </summary>
        private readonly IReportsQueue reportsQueue;

        /// <summary>
        /// Blobs manager
        /// </summary>
        private readonly IBlobsManager blobsManager;

        /// <summary>
        /// Topics manager
        /// </summary>
        private readonly ITopicsManager topicsManager;

        /// <summary>
        /// Comments manager
        /// </summary>
        private readonly ICommentsManager commentsManager;

        /// <summary>
        /// Replies Manager
        /// </summary>
        private readonly IRepliesManager repliesManager;

        /// <summary>
        /// Connection string provider
        /// </summary>
        private readonly IConnectionStringProvider connectionStringProvider;

        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

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
        /// Internal flag. Together with the flag above, its role is to ensure the method <c>Init</c> has completed before
        /// we attempt to perform the real work done by the public methods in this class.
        /// </summary>
        private bool initStarted = false;

        /// <summary>
        /// Interface to the report abuse service
        /// </summary>
        private ReportAbuse reportAbuseClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsManager"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="contentReportsStore">Content Reports Store</param>
        /// <param name="userReportsStore">User Reports Store</param>
        /// <param name="appsStore">Apps Store</param>
        /// <param name="usersStore">Users store</param>
        /// <param name="avertStore">AVERT store</param>
        /// <param name="reportsQueue">Reports Queue</param>
        /// <param name="blobsManager">Blobs Manager</param>
        /// <param name="topicsManager">Topics Manager</param>
        /// <param name="commentsManager">Comments Manager</param>
        /// <param name="repliesManager">Replies Manager</param>
        /// <param name="connectionStringProvider">connection string provider</param>
        public ReportsManager(
            ILog log,
            IContentReportsStore contentReportsStore,
            IUserReportsStore userReportsStore,
            IAppsStore appsStore,
            IUsersStore usersStore,
            IAVERTStore avertStore,
            IReportsQueue reportsQueue,
            IBlobsManager blobsManager,
            ITopicsManager topicsManager,
            ICommentsManager commentsManager,
            IRepliesManager repliesManager,
            IConnectionStringProvider connectionStringProvider)
        {
            this.log = log;
            this.contentReportsStore = contentReportsStore;
            this.userReportsStore = userReportsStore;
            this.appsStore = appsStore;
            this.usersStore = usersStore;
            this.avertStore = avertStore;
            this.reportsQueue = reportsQueue;
            this.blobsManager = blobsManager;
            this.topicsManager = topicsManager;
            this.commentsManager = commentsManager;
            this.repliesManager = repliesManager;
            this.connectionStringProvider = connectionStringProvider;
        }

        /// <summary>
        /// Record a content report
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
        public async Task CreateContentReport(
            ProcessType processType,
            string reportHandle,
            ContentType contentType,
            string contentHandle,
            string contentUserHandle,
            string reportingUserHandle,
            string appHandle,
            ReportReason reason,
            DateTime lastUpdatedTime,
            Uri callbackUri)
        {
            // check whether similar complaint has been made before
            bool hasComplainedBefore = await this.contentReportsStore.HasReportingUserReportedContentBefore(appHandle, contentHandle, reportingUserHandle);

            // have the store record this request
            await this.contentReportsStore.InsertContentReport(
                StorageConsistencyMode.Strong,
                reportHandle,
                contentType,
                contentHandle,
                contentUserHandle,
                reportingUserHandle,
                appHandle,
                reason,
                lastUpdatedTime,
                hasComplainedBefore);

            // do we need to submit a job to the worker role to do more processing?
            if (await this.IsContentReviewRequired(reportHandle, appHandle, contentHandle))
            {
                await this.reportsQueue.SendContentReportMessage(appHandle, reportHandle, callbackUri);
            }
        }

        /// <summary>
        /// Do further processing on a content report and ask a human to review it
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportHandle">Report handle</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Process content report task</returns>
        public async Task SubmitContentReportForReview(ProcessType processType, string appHandle, string reportHandle, Uri callbackUri)
        {
            // get the report
            IContentReportEntity contentReport = await this.ReadContentReport(appHandle, reportHandle);
            if (contentReport == null)
            {
                // this should never happen; throw an exception
                this.log.LogException("Got null content report for appHandle " + appHandle + " and reportHandle " + reportHandle);
            }

            // get the content
            string text1 = string.Empty;
            string text2 = string.Empty;
            Uri imageUri = null;
            switch (contentReport.ContentType)
            {
                // pull information out of a topic
                case ContentType.Topic:
                    ITopicEntity topicEntity = await this.topicsManager.ReadTopic(contentReport.ContentHandle);
                    if (topicEntity == null)
                    {
                        this.log.LogInformation("Topic has already been deleted for appHandle " + appHandle + " and reportHandle " + reportHandle);
                        return;
                    }
                    else if (topicEntity.ReviewStatus == ReviewStatus.Banned)
                    {
                        this.log.LogInformation("Topic has already been banned for appHandle " + appHandle + " and reportHandle " + reportHandle);
                        return;
                    }

                    text1 = topicEntity.Title;
                    text2 = topicEntity.Text;
                    if (topicEntity.BlobType == BlobType.Image && !string.IsNullOrWhiteSpace(topicEntity.BlobHandle))
                    {
                        imageUri = await this.blobsManager.ReadImageCdnUrl(topicEntity.BlobHandle);
                    }

                    break;

                // pull information out of a comment
                case ContentType.Comment:
                    ICommentEntity commentEntity = await this.commentsManager.ReadComment(contentReport.ContentHandle);
                    if (commentEntity == null)
                    {
                        this.log.LogInformation("Comment has already been deleted for appHandle " + appHandle + " and reportHandle " + reportHandle);
                        return;
                    }
                    else if (commentEntity.ReviewStatus == ReviewStatus.Banned)
                    {
                        this.log.LogInformation("Comment has already been banned for appHandle " + appHandle + " and reportHandle " + reportHandle);
                        return;
                    }

                    text1 = commentEntity.Text;
                    if (commentEntity.BlobType == BlobType.Image && !string.IsNullOrWhiteSpace(commentEntity.BlobHandle))
                    {
                        imageUri = await this.blobsManager.ReadImageCdnUrl(commentEntity.BlobHandle);
                    }

                    break;

                // pull information out of a reply
                case ContentType.Reply:
                    IReplyEntity replyEntity = await this.repliesManager.ReadReply(contentReport.ContentHandle);
                    if (replyEntity == null)
                    {
                        this.log.LogInformation("Reply has already been deleted for appHandle " + appHandle + " and reportHandle " + reportHandle);
                        return;
                    }
                    else if (replyEntity.ReviewStatus == ReviewStatus.Banned)
                    {
                        this.log.LogInformation("Reply has already been banned for appHandle " + appHandle + " and reportHandle " + reportHandle);
                        return;
                    }

                    text1 = replyEntity.Text;
                    break;

                default:
                    // this should never happen; throw an exception
                    this.log.LogException("Got an unexpected content type: " + contentReport.ContentType + " for appHandle " + appHandle + " and reportHandle " + reportHandle);
                    break;
            }

            // skip if there's nothing to be submitted for review
            if (string.IsNullOrWhiteSpace(text1) && string.IsNullOrWhiteSpace(text2) && imageUri == null)
            {
                // nothing we can do here since there is no content
                this.log.LogInformation("The content is empty for appHandle " + appHandle + " and reportHandle " + reportHandle);
                return;
            }

            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init();
            }

            // If init not done, wait
            this.initDone.WaitOne();

            // submit the request to AVERT
            string reviewResponse = await this.reportAbuseClient.SubmitReviewRequest(contentReport.Reason, contentReport.CreatedTime, callbackUri, text1, text2, null, imageUri);

            // store it in AVERTStore
            await this.avertStore.InsertSubmission(StorageConsistencyMode.Strong, reportHandle, appHandle, ReportType.Content, DateTime.UtcNow, reviewResponse);
        }

        /// <summary>
        /// Get content report
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportHandle">Report handle</param>
        /// <returns>Content report</returns>
        public async Task<IContentReportEntity> ReadContentReport(string appHandle, string reportHandle)
        {
            return await this.contentReportsStore.QueryContentReport(appHandle, reportHandle);
        }

        /// <summary>
        /// Read content reports for a content
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of content report feed entities</returns>
        public async Task<IList<IContentReportFeedEntity>> ReadContentReportsForContent(string appHandle, string contentHandle, string cursor, int limit)
        {
            return await this.contentReportsStore.QueryContentReportsByContent(appHandle, contentHandle, cursor, limit);
        }

        /// <summary>
        /// Read content reports for content created by a user
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="contentUserHandle">User handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of content report feed entities</returns>
        public async Task<IList<IContentReportFeedEntity>> ReadContentReportsForUser(string appHandle, string contentUserHandle, string cursor, int limit)
        {
            return await this.contentReportsStore.QueryContentReportsByContentUser(appHandle, contentUserHandle, cursor, limit);
        }

        /// <summary>
        /// Read content reports for content reported by a user
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportingUserHandle">User handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of content report feed entities</returns>
        public async Task<IList<IContentReportFeedEntity>> ReadContentReportsFromUser(string appHandle, string reportingUserHandle, string cursor, int limit)
        {
            return await this.contentReportsStore.QueryContentReportsByReportingUser(appHandle, reportingUserHandle, cursor, limit);
        }

        /// <summary>
        /// Read content reports for content posted from an app
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of content report feed entities</returns>
        public async Task<IList<IContentReportFeedEntity>> ReadContentReportsForApp(string appHandle, string cursor, int limit)
        {
            return await this.contentReportsStore.QueryContentReportsByApp(appHandle, cursor, limit);
        }

        /// <summary>
        /// Get count of reports for a content
        /// </summary>
        /// <param name="appHandle">Appp handle</param>
        /// <param name="contentHandle">Content handle</param>
        /// <returns>Reports count for content</returns>
        public async Task<long> ReadContentReportsCount(string appHandle, string contentHandle)
        {
            return await this.contentReportsStore.QueryContentReportCount(appHandle, contentHandle);
        }

        /// <summary>
        /// Record a user report
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
        public async Task CreateUserReport(
            ProcessType processType,
            string reportHandle,
            string reportedUserHandle,
            string reportingUserHandle,
            string appHandle,
            ReportReason reason,
            DateTime lastUpdatedTime,
            Uri callbackUri)
        {
            // check whether similar complaint has been made before
            bool hasComplainedBefore = await this.userReportsStore.HasReportingUserReportedUserBefore(appHandle, reportedUserHandle, reportingUserHandle);

            // have the store record this request
            await this.userReportsStore.InsertUserReport(
                StorageConsistencyMode.Strong,
                reportHandle,
                reportedUserHandle,
                reportingUserHandle,
                appHandle,
                reason,
                lastUpdatedTime,
                hasComplainedBefore);

            // do we need to submit a job to the worker role to do more processing?
            if (await this.IsUserReviewRequired(reportHandle, appHandle, reportedUserHandle))
            {
                await this.reportsQueue.SendUserReportMessage(appHandle, reportHandle, callbackUri);
            }
        }

        /// <summary>
        /// Do further processing on a user report and ask a human to review it
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportHandle">Report handle</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Process user report task</returns>
        public async Task SubmitUserReportForReview(ProcessType processType, string appHandle, string reportHandle, Uri callbackUri)
        {
            // get the report
            IUserReportEntity userReport = await this.ReadUserReport(appHandle, reportHandle);
            if (userReport == null)
            {
                // this should never happen; throw an exception
                this.log.LogException("Got null user report for appHandle " + appHandle + " and reportHandle " + reportHandle);
            }

            // get the user profile content
            string text1 = string.Empty;
            string text2 = string.Empty;
            string text3 = string.Empty;
            Uri imageUri = null;
            IUserProfileEntity userProfileEntity = await this.usersStore.QueryUserProfile(userReport.ReportedUserHandle, appHandle);
            if (userProfileEntity == null)
            {
                this.log.LogInformation("User has already been deleted for appHandle " + appHandle + " and reportHandle " + reportHandle);
                return;
            }
            else if (userProfileEntity.ReviewStatus == ReviewStatus.Banned)
            {
                this.log.LogInformation("User has already been banned for appHandle " + appHandle + " and reportHandle " + reportHandle);
                return;
            }

            text1 = userProfileEntity.FirstName;
            text2 = userProfileEntity.LastName;
            text3 = userProfileEntity.Bio;
            if (!string.IsNullOrWhiteSpace(userProfileEntity.PhotoHandle))
            {
                imageUri = await this.blobsManager.ReadImageCdnUrl(userProfileEntity.PhotoHandle);
            }

            // skip if there's nothing to be submitted for review
            if (string.IsNullOrWhiteSpace(text1) && string.IsNullOrWhiteSpace(text2) && string.IsNullOrWhiteSpace(text3) && imageUri == null)
            {
                // nothing we can do here since there is no content
                this.log.LogInformation("The user profile is empty for appHandle " + appHandle + " and reportHandle " + reportHandle);
                return;
            }

            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init();
            }

            // If init not done, wait
            this.initDone.WaitOne();

            // submit the request to AVERT
            string reviewResponse = await this.reportAbuseClient.SubmitReviewRequest(userReport.Reason, userReport.CreatedTime, callbackUri, text1, text2, text3, imageUri);

            // store it in AVERTStore
            await this.avertStore.InsertSubmission(StorageConsistencyMode.Strong, reportHandle, appHandle, ReportType.User, DateTime.UtcNow, reviewResponse);
        }

        /// <summary>
        /// Read user report
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportHandle">Report handle</param>
        /// <returns>User report</returns>
        public async Task<IUserReportEntity> ReadUserReport(string appHandle, string reportHandle)
        {
            return await this.userReportsStore.QueryUserReport(appHandle, reportHandle);
        }

        /// <summary>
        /// Read user reports against a user
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportedUserHandle">User handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user report feed entities</returns>
        public async Task<IList<IUserReportFeedEntity>> ReadUserReportsForUser(string appHandle, string reportedUserHandle, string cursor, int limit)
        {
            return await this.userReportsStore.QueryUserReportsByReportedUser(appHandle, reportedUserHandle, cursor, limit);
        }

        /// <summary>
        /// Read user reports reported by a user
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportingUserHandle">User handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user report feed entities</returns>
        public async Task<IList<IUserReportFeedEntity>> ReadUserReportsFromUser(string appHandle, string reportingUserHandle, string cursor, int limit)
        {
            return await this.userReportsStore.QueryUserReportsByReportingUser(appHandle, reportingUserHandle, cursor, limit);
        }

        /// <summary>
        /// Read user reports posted from an app
        /// </summary>
        /// <param name="appHandle">Appp handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user report feed entities</returns>
        public async Task<IList<IUserReportFeedEntity>> ReadUserReportsForApp(string appHandle, string cursor, int limit)
        {
            return await this.userReportsStore.QueryUserReportsByApp(appHandle, cursor, limit);
        }

        /// <summary>
        /// Get count of reports against a user
        /// </summary>
        /// <param name="appHandle">Appp handle</param>
        /// <param name="reportedUserHandle">User being reported against</param>
        /// <returns>Reports count against a user</returns>
        public async Task<long> ReadUserReportsCount(string appHandle, string reportedUserHandle)
        {
            return await this.userReportsStore.QueryUserReportCount(appHandle, reportedUserHandle);
        }

        /// <summary>
        /// Process the response from a human review of a report
        /// </summary>
        /// <param name="reportHandle">Uniquely identifies the report</param>
        /// <param name="review">Results of the review</param>
        /// <returns>Task that processes the result</returns>
        public async Task ProcessReportResult(string reportHandle, string review)
        {
            // is the report handle valid?
            if (string.IsNullOrWhiteSpace(reportHandle))
            {
                this.log.LogException(new ArgumentNullException("reportHandle"));
            }

            IAVERTTransactionEntity transactionEntity = await this.avertStore.QueryTransaction(reportHandle);
            if (transactionEntity == null)
            {
                this.log.LogException(new KeyNotFoundException());
            }
            else if (transactionEntity.ReportHandle != reportHandle)
            {
                this.log.LogException("Got back reportHandle that did not match the expected: " + reportHandle);
            }

            // record this response
            await this.avertStore.InsertResponse(StorageConsistencyMode.Strong, reportHandle, DateTime.UtcNow, review, transactionEntity);

            // Note: We do not check if there was already a previous response for this report.
            // It's ok for AVERT to call us back multiple times - if any of those responses indicate
            // bad content, then we take down the content and we're done.

            // Note: there is no expectation of simultaneous updates here, so to reduce overhead,
            // this read-modify-replace is not done with a lock.

            // Note: We do not check if the content changed between the report and the response.
            // We may ban even updated content if the original content was deemed bad.
            // This prevents a user from frequently making minor updates to their content
            // (e.g. add a space to text) to increase the visible lifetime of their offensive
            // content.

            // do we need to delete content?
            if (await this.ShouldContentBeDeleted(transactionEntity.AppHandle, review))
            {
                if (transactionEntity.ReportType == ReportType.User)
                {
                    await this.BanUserProfileContent(transactionEntity);
                }
                else if (transactionEntity.ReportType == ReportType.Content)
                {
                    await this.BanContent(transactionEntity);
                }
                else
                {
                    this.log.LogException("ReportType is of unknown value: " + transactionEntity.ReportType);
                }
            }

            // do we need to tag the content as mature?
            else if (await this.ShouldContentBeTaggedMature(review))
            {
                if (transactionEntity.ReportType == ReportType.User)
                {
                    await this.TagUserProfileAsMature(transactionEntity);
                }
                else if (transactionEntity.ReportType == ReportType.Content)
                {
                    await this.TagContentAsMature(transactionEntity);
                }
                else
                {
                    this.log.LogException("ReportType is of unknown value: " + transactionEntity.ReportType);
                }
            }

            // tag the content as clean
            else
            {
                if (transactionEntity.ReportType == ReportType.User)
                {
                    await this.TagUserProfileAsClean(transactionEntity);
                }
                else if (transactionEntity.ReportType == ReportType.Content)
                {
                    await this.TagContentAsClean(transactionEntity);
                }
                else
                {
                    this.log.LogException("ReportType is of unknown value: " + transactionEntity.ReportType);
                }
            }
        }

        /// <summary>
        /// Ban user profile content that was reviewed and found to be in violation
        /// </summary>
        /// <param name="transactionEntity">details of the review</param>
        /// <returns>task that bans user profile content</returns>
        private async Task BanUserProfileContent(IAVERTTransactionEntity transactionEntity)
        {
            // get the relevant report
            IUserReportEntity userReport = await this.ReadUserReport(transactionEntity.AppHandle, transactionEntity.ReportHandle);
            if (userReport == null)
            {
                this.log.LogException("did not find relevant user report for report handle: " + transactionEntity.ReportHandle);
            }

            if (string.IsNullOrWhiteSpace(userReport.ReportedUserHandle))
            {
                this.log.LogException("got empty user handle for report handle: " + transactionEntity.ReportHandle);
            }

            // get the relevant user profile
            IUserProfileEntity userProfileEntity = await this.usersStore.QueryUserProfile(userReport.ReportedUserHandle, userReport.AppHandle);
            if (userProfileEntity == null)
            {
                this.log.LogInformation("User profile is already deleted for report handle: " + transactionEntity.ReportHandle);
                return;
            }
            else if (userProfileEntity.ReviewStatus == ReviewStatus.Banned)
            {
                this.log.LogInformation("User profile is already banned for report handle: " + transactionEntity.ReportHandle);
                return;
            }

            // delete the photo if it is not null; photo must be deleted as per MS rules
            this.log.LogInformation("Banning user profile content for user handle: " + userReport.ReportedUserHandle);
            if (!string.IsNullOrWhiteSpace(userProfileEntity.PhotoHandle))
            {
                await this.blobsManager.DeleteImage(transactionEntity.AppHandle, userReport.ReportedUserHandle, userProfileEntity.PhotoHandle, ImageType.UserPhoto);
            }

            // wipe out the photo handle and set the status to banned
            userProfileEntity.PhotoHandle = string.Empty;
            userProfileEntity.ReviewStatus = ReviewStatus.Banned;
            await this.usersStore.UpdateUserProfile(StorageConsistencyMode.Strong, userReport.ReportedUserHandle, userReport.AppHandle, userProfileEntity);
        }

        /// <summary>
        /// Tag user profile that was reviewed and found to have mature content
        /// </summary>
        /// <param name="transactionEntity">details of the review</param>
        /// <returns>task that tags user profile content as mature</returns>
        private async Task TagUserProfileAsMature(IAVERTTransactionEntity transactionEntity)
        {
            // get the relevant report
            IUserReportEntity userReport = await this.ReadUserReport(transactionEntity.AppHandle, transactionEntity.ReportHandle);
            if (userReport == null)
            {
                this.log.LogException("did not find relevant user report for report handle: " + transactionEntity.ReportHandle);
            }

            if (string.IsNullOrWhiteSpace(userReport.ReportedUserHandle))
            {
                this.log.LogException("got empty user handle for report handle: " + transactionEntity.ReportHandle);
            }

            // get the relevant user profile
            IUserProfileEntity userProfileEntity = await this.usersStore.QueryUserProfile(userReport.ReportedUserHandle, userReport.AppHandle);
            if (userProfileEntity == null)
            {
                this.log.LogInformation("User profile is already deleted for report handle: " + transactionEntity.ReportHandle);
                return;
            }
            else if (userProfileEntity.ReviewStatus == ReviewStatus.Banned)
            {
                this.log.LogInformation("User profile is already banned for report handle: " + transactionEntity.ReportHandle);
                return;
            }
            else if (userProfileEntity.ReviewStatus == ReviewStatus.Mature)
            {
                this.log.LogInformation("User profile is already tagged for report handle: " + transactionEntity.ReportHandle);
                return;
            }

            // set the status to mature
            this.log.LogInformation("Setting user profile to mature for user handle: " + userReport.ReportedUserHandle);
            userProfileEntity.ReviewStatus = ReviewStatus.Mature;
            await this.usersStore.UpdateUserProfile(StorageConsistencyMode.Strong, userReport.ReportedUserHandle, userReport.AppHandle, userProfileEntity);
        }

        /// <summary>
        /// Tag user profile that was reviewed and found to have no violations.
        /// </summary>
        /// <remarks>
        /// A user profile will only be tagged as clean if the review status is "Active".
        /// That means the user profile has not previously been reviewed by AVERT or CVS.
        /// If a user profile was previously tagged clean, then it will remain clean.
        /// If a user profile was previously tagged as banned, then it will remain banned.
        /// If a user profile was previously tagged as mature, then it will remain mature.
        /// </remarks>
        /// <param name="transactionEntity">details of the review</param>
        /// <returns>task that tags user profile content as clean</returns>
        private async Task TagUserProfileAsClean(IAVERTTransactionEntity transactionEntity)
        {
            // get the relevant report
            IUserReportEntity userReport = await this.ReadUserReport(transactionEntity.AppHandle, transactionEntity.ReportHandle);
            if (userReport == null)
            {
                this.log.LogException("did not find relevant user report for report handle: " + transactionEntity.ReportHandle);
            }

            if (string.IsNullOrWhiteSpace(userReport.ReportedUserHandle))
            {
                this.log.LogException("got empty user handle for report handle: " + transactionEntity.ReportHandle);
            }

            // get the relevant user profile
            IUserProfileEntity userProfileEntity = await this.usersStore.QueryUserProfile(userReport.ReportedUserHandle, userReport.AppHandle);
            if (userProfileEntity == null)
            {
                this.log.LogInformation("User profile is already deleted for report handle: " + transactionEntity.ReportHandle);
                return;
            }
            else if (userProfileEntity.ReviewStatus != ReviewStatus.Active)
            {
                this.log.LogInformation("User profile is tagged as not active for report handle: " + transactionEntity.ReportHandle);
                return;
            }

            // set the status to clean
            this.log.LogInformation("Setting user profile to clean for user handle: " + userReport.ReportedUserHandle);
            userProfileEntity.ReviewStatus = ReviewStatus.Clean;
            await this.usersStore.UpdateUserProfile(StorageConsistencyMode.Strong, userReport.ReportedUserHandle, userReport.AppHandle, userProfileEntity);
        }

        /// <summary>
        /// Ban content that was reviewed and found to be in violation
        /// </summary>
        /// <param name="transactionEntity">details of the review</param>
        /// <returns>task that bans content</returns>
        private async Task BanContent(IAVERTTransactionEntity transactionEntity)
        {
            // get the relevant report
            IContentReportEntity contentReport = await this.ReadContentReport(transactionEntity.AppHandle, transactionEntity.ReportHandle);
            if (contentReport == null)
            {
                this.log.LogException("did not find relevant content report for report handle: " + transactionEntity.ReportHandle);
            }

            if (string.IsNullOrWhiteSpace(contentReport.ContentHandle))
            {
                this.log.LogException("got empty content handle for report handle: " + transactionEntity.ReportHandle);
            }

            // get the relevant topic & ban it
            if (contentReport.ContentType == ContentType.Topic)
            {
                ITopicEntity topicEntity = await this.topicsManager.ReadTopic(contentReport.ContentHandle);
                if (topicEntity == null)
                {
                    this.log.LogInformation("Topic is already deleted for report handle: " + transactionEntity.ReportHandle);
                    return;
                }
                else if (topicEntity.ReviewStatus == ReviewStatus.Banned)
                {
                    this.log.LogInformation("Topic is already banned for report handle: " + transactionEntity.ReportHandle);
                    return;
                }

                // delete the image if it is not null; image must be deleted as per MS rules
                this.log.LogInformation("Banning content for topic handle: " + contentReport.ContentHandle);
                if (!string.IsNullOrWhiteSpace(topicEntity.BlobHandle) && topicEntity.BlobType == BlobType.Image)
                {
                    await this.blobsManager.DeleteImage(transactionEntity.AppHandle, contentReport.ContentHandle, topicEntity.BlobHandle, ImageType.ContentBlob);
                }

                // update the status to banned and remove link to the image
                await this.topicsManager.UpdateTopic(
                    ProcessType.Frontend,
                    contentReport.ContentHandle,
                    topicEntity.Title,
                    topicEntity.Text,
                    BlobType.Unknown,
                    string.Empty,
                    topicEntity.Categories,
                    ReviewStatus.Banned,
                    DateTime.UtcNow,
                    topicEntity);
                return;
            }

            // get the relevant comment & ban it
            if (contentReport.ContentType == ContentType.Comment)
            {
                ICommentEntity commentEntity = await this.commentsManager.ReadComment(contentReport.ContentHandle);
                if (commentEntity == null)
                {
                    this.log.LogInformation("Comment is already deleted for report handle: " + transactionEntity.ReportHandle);
                    return;
                }
                else if (commentEntity.ReviewStatus == ReviewStatus.Banned)
                {
                    this.log.LogInformation("Comment is already banned for report handle: " + transactionEntity.ReportHandle);
                    return;
                }

                // delete the image if it is not null; image must be deleted as per MS rules
                this.log.LogInformation("Banning content for comment handle: " + contentReport.ContentHandle);
                if (!string.IsNullOrWhiteSpace(commentEntity.BlobHandle) && commentEntity.BlobType == BlobType.Image)
                {
                    await this.blobsManager.DeleteImage(transactionEntity.AppHandle, contentReport.ContentHandle, commentEntity.BlobHandle, ImageType.ContentBlob);
                }

                // update the status to banned and remove link to the image
                await this.commentsManager.UpdateComment(
                    ProcessType.Frontend,
                    contentReport.ContentHandle,
                    commentEntity.Text,
                    BlobType.Unknown,
                    string.Empty,
                    ReviewStatus.Banned,
                    DateTime.UtcNow,
                    commentEntity);
                return;
            }

            // get the relevant reply & ban it
            if (contentReport.ContentType == ContentType.Reply)
            {
                IReplyEntity replyEntity = await this.repliesManager.ReadReply(contentReport.ContentHandle);
                if (replyEntity == null)
                {
                    this.log.LogInformation("Reply is already deleted for report handle: " + transactionEntity.ReportHandle);
                    return;
                }
                else if (replyEntity.ReviewStatus == ReviewStatus.Banned)
                {
                    this.log.LogInformation("Reply is already banned for report handle: " + transactionEntity.ReportHandle);
                    return;
                }

                // update the status to banned
                this.log.LogInformation("Banning content for reply handle: " + contentReport.ContentHandle);
                await this.repliesManager.UpdateReply(
                    ProcessType.Frontend,
                    contentReport.ContentHandle,
                    replyEntity.Text,
                    replyEntity.Language,
                    ReviewStatus.Banned,
                    DateTime.UtcNow,
                    replyEntity);
                return;
            }
        }

        /// <summary>
        /// Tag content that was reviewed and found to be for mature audiences
        /// </summary>
        /// <param name="transactionEntity">details of the review</param>
        /// <returns>task that tags content as mature</returns>
        private async Task TagContentAsMature(IAVERTTransactionEntity transactionEntity)
        {
            // get the relevant report
            IContentReportEntity contentReport = await this.ReadContentReport(transactionEntity.AppHandle, transactionEntity.ReportHandle);
            if (contentReport == null)
            {
                this.log.LogException("did not find relevant content report for report handle: " + transactionEntity.ReportHandle);
            }

            if (string.IsNullOrWhiteSpace(contentReport.ContentHandle))
            {
                this.log.LogException("got empty content handle for report handle: " + transactionEntity.ReportHandle);
            }

            // get the relevant topic & tag it
            if (contentReport.ContentType == ContentType.Topic)
            {
                ITopicEntity topicEntity = await this.topicsManager.ReadTopic(contentReport.ContentHandle);
                if (topicEntity == null)
                {
                    this.log.LogInformation("Topic is already deleted for report handle: " + transactionEntity.ReportHandle);
                    return;
                }
                else if (topicEntity.ReviewStatus == ReviewStatus.Banned)
                {
                    this.log.LogInformation("Topic is already banned for report handle: " + transactionEntity.ReportHandle);
                    return;
                }
                else if (topicEntity.ReviewStatus == ReviewStatus.Mature)
                {
                    this.log.LogInformation("Topic is already tagged as mature for report handle: " + transactionEntity.ReportHandle);
                    return;
                }

                // update the status to mature
                this.log.LogInformation("Tagging content as mature for topic handle: " + contentReport.ContentHandle);
                await this.topicsManager.UpdateTopic(
                    ProcessType.Frontend,
                    contentReport.ContentHandle,
                    topicEntity.Title,
                    topicEntity.Text,
                    topicEntity.BlobType,
                    topicEntity.BlobHandle,
                    topicEntity.Categories,
                    ReviewStatus.Mature,
                    DateTime.UtcNow,
                    topicEntity);
                return;
            }

            // get the relevant comment & ban it
            if (contentReport.ContentType == ContentType.Comment)
            {
                ICommentEntity commentEntity = await this.commentsManager.ReadComment(contentReport.ContentHandle);
                if (commentEntity == null)
                {
                    this.log.LogInformation("Comment is already deleted for report handle: " + transactionEntity.ReportHandle);
                    return;
                }
                else if (commentEntity.ReviewStatus == ReviewStatus.Banned)
                {
                    this.log.LogInformation("Comment is already banned for report handle: " + transactionEntity.ReportHandle);
                    return;
                }
                else if (commentEntity.ReviewStatus == ReviewStatus.Mature)
                {
                    this.log.LogInformation("Comment is already tagged as mature for report handle: " + transactionEntity.ReportHandle);
                    return;
                }

                // update the status to mature
                this.log.LogInformation("Tagging content as mature for comment handle: " + contentReport.ContentHandle);
                await this.commentsManager.UpdateComment(
                    ProcessType.Frontend,
                    contentReport.ContentHandle,
                    commentEntity.Text,
                    commentEntity.BlobType,
                    commentEntity.BlobHandle,
                    ReviewStatus.Mature,
                    DateTime.UtcNow,
                    commentEntity);
                return;
            }

            // get the relevant reply & ban it
            if (contentReport.ContentType == ContentType.Reply)
            {
                IReplyEntity replyEntity = await this.repliesManager.ReadReply(contentReport.ContentHandle);
                if (replyEntity == null)
                {
                    this.log.LogInformation("Reply is already deleted for report handle: " + transactionEntity.ReportHandle);
                    return;
                }
                else if (replyEntity.ReviewStatus == ReviewStatus.Banned)
                {
                    this.log.LogInformation("Reply is already banned for report handle: " + transactionEntity.ReportHandle);
                    return;
                }
                else if (replyEntity.ReviewStatus == ReviewStatus.Mature)
                {
                    this.log.LogInformation("Reply is already tagged as mature for report handle: " + transactionEntity.ReportHandle);
                    return;
                }

                // update the status to mature
                this.log.LogInformation("Tagging content as mature for reply handle: " + contentReport.ContentHandle);
                await this.repliesManager.UpdateReply(
                    ProcessType.Frontend,
                    contentReport.ContentHandle,
                    replyEntity.Text,
                    replyEntity.Language,
                    ReviewStatus.Mature,
                    DateTime.UtcNow,
                    replyEntity);
                return;
            }
        }

        /// <summary>
        /// Tag content that was reviewed and found to have no violations
        /// </summary>
        /// <param name="transactionEntity">details of the review</param>
        /// <returns>task that tags content as clean</returns>
        private async Task TagContentAsClean(IAVERTTransactionEntity transactionEntity)
        {
            // get the relevant report
            IContentReportEntity contentReport = await this.ReadContentReport(transactionEntity.AppHandle, transactionEntity.ReportHandle);
            if (contentReport == null)
            {
                this.log.LogException("did not find relevant content report for report handle: " + transactionEntity.ReportHandle);
            }

            if (string.IsNullOrWhiteSpace(contentReport.ContentHandle))
            {
                this.log.LogException("got empty content handle for report handle: " + transactionEntity.ReportHandle);
            }

            // get the relevant topic & tag it
            if (contentReport.ContentType == ContentType.Topic)
            {
                ITopicEntity topicEntity = await this.topicsManager.ReadTopic(contentReport.ContentHandle);
                if (topicEntity == null)
                {
                    this.log.LogInformation("Topic is already deleted for report handle: " + transactionEntity.ReportHandle);
                    return;
                }
                else if (topicEntity.ReviewStatus != ReviewStatus.Active)
                {
                    this.log.LogInformation("Topic is not tagged as active for report handle: " + transactionEntity.ReportHandle);
                    return;
                }

                // update the status to clean
                this.log.LogInformation("Tagging content as clean for topic handle: " + contentReport.ContentHandle);
                await this.topicsManager.UpdateTopic(
                    ProcessType.Frontend,
                    contentReport.ContentHandle,
                    topicEntity.Title,
                    topicEntity.Text,
                    topicEntity.BlobType,
                    topicEntity.BlobHandle,
                    topicEntity.Categories,
                    ReviewStatus.Clean,
                    DateTime.UtcNow,
                    topicEntity);
                return;
            }

            // get the relevant comment & ban it
            if (contentReport.ContentType == ContentType.Comment)
            {
                ICommentEntity commentEntity = await this.commentsManager.ReadComment(contentReport.ContentHandle);
                if (commentEntity == null)
                {
                    this.log.LogInformation("Comment is already deleted for report handle: " + transactionEntity.ReportHandle);
                    return;
                }
                else if (commentEntity.ReviewStatus != ReviewStatus.Active)
                {
                    this.log.LogInformation("Comment is not tagged as active for report handle: " + transactionEntity.ReportHandle);
                    return;
                }

                // update the status to clean
                this.log.LogInformation("Tagging content as clean for comment handle: " + contentReport.ContentHandle);
                await this.commentsManager.UpdateComment(
                    ProcessType.Frontend,
                    contentReport.ContentHandle,
                    commentEntity.Text,
                    commentEntity.BlobType,
                    commentEntity.BlobHandle,
                    ReviewStatus.Clean,
                    DateTime.UtcNow,
                    commentEntity);
                return;
            }

            // get the relevant reply & ban it
            if (contentReport.ContentType == ContentType.Reply)
            {
                IReplyEntity replyEntity = await this.repliesManager.ReadReply(contentReport.ContentHandle);
                if (replyEntity == null)
                {
                    this.log.LogInformation("Reply is already deleted for report handle: " + transactionEntity.ReportHandle);
                    return;
                }
                else if (replyEntity.ReviewStatus != ReviewStatus.Active)
                {
                    this.log.LogInformation("Reply is not tagged as active for report handle: " + transactionEntity.ReportHandle);
                    return;
                }

                // update the status to clean
                this.log.LogInformation("Tagging content as mature for reply handle: " + contentReport.ContentHandle);
                await this.repliesManager.UpdateReply(
                    ProcessType.Frontend,
                    contentReport.ContentHandle,
                    replyEntity.Text,
                    replyEntity.Language,
                    ReviewStatus.Clean,
                    DateTime.UtcNow,
                    replyEntity);
                return;
            }
        }

        /// <summary>
        /// Implements the policy of deciding whether a piece of content should be deleted
        /// </summary>
        /// <remarks>
        /// Child abuse and spam are never allowed and this method will return true for them.
        /// Mature content, if not allowed by the app, will also result in a true return value.
        /// </remarks>
        /// <param name="appHandle">uniquely identifies the app that the content came from</param>
        /// <param name="review">results of an AVERT review</param>
        /// <returns>true if the content should be deleted</returns>
        private async Task<bool> ShouldContentBeDeleted(string appHandle, string review)
        {
            // get the app-specific policy on mature content
            bool allowMatureContent = true;
            IValidationConfigurationEntity validationConfiguration = await this.appsStore.QueryValidationConfiguration(appHandle);
            if (validationConfiguration != null)
            {
                allowMatureContent = validationConfiguration.AllowMatureContent;
            }

            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init();
            }

            // If init not done, wait
            this.initDone.WaitOne();

            // process the policy codes in the response
            bool banContent = false;
            if (this.reportAbuseClient.IsContentNotAllowed(review))
            {
                banContent = true;
            }
            else if (!allowMatureContent && this.reportAbuseClient.IsContentMature(review))
            {
                banContent = true;
            }

            return banContent;
        }

        /// <summary>
        /// Implements the policy of deciding whether a piece of content should be tagged as mature
        /// </summary>
        /// <param name="review">results of an AVERT review</param>
        /// <returns>true if the content should be marked as mature</returns>
        private async Task<bool> ShouldContentBeTaggedMature(string review)
        {
            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init();
            }

            // If init not done, wait
            this.initDone.WaitOne();

            // process the policy codes in the response
            return this.reportAbuseClient.IsContentMature(review);
        }

        /// <summary>
        /// Implements the policy of deciding whether a piece of content requires review upon receiving a new report
        /// </summary>
        /// <param name="reportHandle">report handle</param>
        /// <param name="appHandle">app that the content was created in</param>
        /// <param name="contentHandle">uniquely identifies the content</param>
        /// <returns>true if the content needs to be reviewed, false otherwise</returns>
        private async Task<bool> IsContentReviewRequired(string reportHandle, string appHandle, string contentHandle)
        {
            // if this report has been submitted before to AVERT, then no additional review is required
            IAVERTTransactionEntity transactionEntity = await this.avertStore.QueryTransaction(reportHandle);
            if (transactionEntity != null)
            {
                return false;

                // Technically there is a race condition here. A user could submit two reports on the same content
                // and both reports could arrive here at the same time and both could find that there has been no
                // previous transaction submitted to AVERT. As a result, two review requests will go to AVERT.
                // The chances of that happening are slim, and the downside is minor (additional load on AVERT),
                // so I have chosen to not do an expensive lock operation here.
            }

            long threshold = DefaultReportThreshold;

            // get the reporting threshold for the app
            IValidationConfigurationEntity validationConfiguration = await this.appsStore.QueryValidationConfiguration(appHandle);
            if (validationConfiguration != null && validationConfiguration.ContentReportThreshold > 0)
            {
                threshold = validationConfiguration.ContentReportThreshold;
            }

            // get the report count for the content
            long reportCount = await this.ReadContentReportsCount(appHandle, contentHandle);

            // Each time the reportCount on a piece of content increments by this threshold, a review will be requested.
            // Some design considerations to keep in mind:
            // (1) this threshold is a per-app setting; the intent is that for apps that generate a lot of spurious reports,
            //     this value will be configured to >1
            // (2) if the AVERT team complains about excessive reports from us, we can bump up this threshold for the
            //     apps that cause the most reports to be generated
            // (3) AVERT's AI and human review processes are not perfect - the AI algorithms are constantly improving, and
            //     different humans will apply different subjective criteria; hence we should allow content to be reviewed
            //     again even if it has passed a previous review
            // (4) if a content or user profile is banned, then that content or user will no longer be visible, and hence will
            //     not generate any more reports
            return reportCount % threshold == 0;
        }

        /// <summary>
        /// Implements the policy of deciding whether a user requires review upon receiving a new report
        /// </summary>
        /// <param name="reportHandle">report handle</param>
        /// <param name="appHandle">app that the user lives in</param>
        /// <param name="reportedUserHandle">uniquely identifies the user being reported on</param>
        /// <returns>true if the content needs to be reviewed, false otherwise</returns>
        private async Task<bool> IsUserReviewRequired(string reportHandle, string appHandle, string reportedUserHandle)
        {
            // if this report has been submitted before to AVERT, then no additional review is required
            IAVERTTransactionEntity transactionEntity = await this.avertStore.QueryTransaction(reportHandle);
            if (transactionEntity != null)
            {
                return false;

                // Technically there is a race condition here. A user could submit two reports on the same user
                // and both reports could arrive here at the same time and both could find that there has been no
                // previous transaction submitted to AVERT. As a result, two review requests will go to AVERT.
                // The chances of that happening are slim, and the downside is minor (additional load on AVERT),
                // so I have chosen to not do an expensive lock operation here.
            }

            long threshold = DefaultReportThreshold;

            // get the reporting threshold for the app
            IValidationConfigurationEntity validationConfiguration = await this.appsStore.QueryValidationConfiguration(appHandle);
            if (validationConfiguration != null && validationConfiguration.UserReportThreshold > 0)
            {
                threshold = validationConfiguration.UserReportThreshold;
            }

            // get the report count for the user
            long reportCount = await this.ReadUserReportsCount(appHandle, reportedUserHandle);

            // Each time the reportCount on a user increments by this threshold, a review will be requested.
            // Some design considerations to keep in mind:
            // (1) this threshold is a per-app setting; the intent is that for apps that generate a lot of spurious reports,
            //     this value will be configured to >1
            // (2) if the AVERT team complains about excessive reports from us, we can bump up this threshold for the
            //     apps that cause the most reports to be generated
            // (3) AVERT's AI and human review processes are not perfect - the AI algorithms are constantly improving, and
            //     different humans will apply different subjective criteria; hence we should allow content to be reviewed
            //     again even if it has passed a previous review
            // (4) if a content or user profile is banned, then that content or user will no longer be visible, and hence will
            //     not generate any more reports
            return reportCount % threshold == 0;
        }

        /// <summary>
        /// Initializes the report abuse client
        /// </summary>
        /// <returns>task that initializes the report abuse client</returns>
        private async Task Init()
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

            // initialize the client
            Uri avertUri = new Uri(await this.connectionStringProvider.GetAVERTUrl(AVERTInstanceType.Default));
            string avertKey = await this.connectionStringProvider.GetAVERTKey(AVERTInstanceType.Default);
            this.reportAbuseClient = new ReportAbuse(avertUri, avertKey);

            // Init done
            this.initDone.Set();
        }
    }
}
