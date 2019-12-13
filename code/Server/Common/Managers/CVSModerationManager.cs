// <copyright file="CVSModerationManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;
    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Blobs;
    using SocialPlus.Server.CVS;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// The CVS moderation manager class uses CVS to moderate text and image content
    /// </summary>
    /// <remarks>
    /// The Content Validation Service (CVS) is a content moderation provider.
    /// The API is documented at http://cvs-docs.azurewebsites.net/
    /// </remarks>
    public class CVSModerationManager : IModerationManager
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Users manager
        /// </summary>
        private readonly IUsersManager usersManager;

        /// <summary>
        /// Blobs manager
        /// </summary>
        private readonly IBlobsManager blobsManager;

        /// <summary>
        /// CVS transaction store
        /// </summary>
        private readonly ICVSTransactionStore cvsTransactionStore;

        /// <summary>
        /// Content moderation store
        /// </summary>
        private readonly IModerationStore moderationStore;

        /// <summary>
        /// Moderation queue
        /// </summary>
        private readonly IModerationQueue moderationQueue;

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
        /// Apps Manager
        /// </summary>
        private readonly IAppsManager appsManager;

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
        /// Internal flag. Together with the flag above, its role is to ensure the method <c>Init</c> has completed before
        /// we attempt to perform the real work done by the public methods in this class.
        /// </summary>
        private bool initStarted = false;

        /// <summary>
        /// Url to contact the CVS service
        /// </summary>
        private string cvsUrl;

        /// <summary>
        /// Key required to connect to the CVS service
        /// </summary>
        private string cvsKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="CVSModerationManager"/> class.
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="cvsTransactionStore">CVS transaction store</param>
        /// <param name="moderationStore">Moderation store</param>
        /// <param name="moderationQueue">Moderation queue</param>
        /// <param name="blobsManager">Blobs Manager</param>
        /// <param name="topicsManager">Topics Manager</param>
        /// <param name="commentsManager">Comments Manager</param>
        /// <param name="repliesManager">Replies Manager</param>
        /// <param name="appsManager">Apps Manager</param>
        /// <param name="connectionStringProvider">connection string provider</param>
        public CVSModerationManager(
            ILog log,
            IUsersManager usersManager,
            ICVSTransactionStore cvsTransactionStore,
            IModerationStore moderationStore,
            IModerationQueue moderationQueue,
            IBlobsManager blobsManager,
            ITopicsManager topicsManager,
            ICommentsManager commentsManager,
            IRepliesManager repliesManager,
            IAppsManager appsManager,
            IConnectionStringProvider connectionStringProvider)
        {
            this.log = log;
            this.usersManager = usersManager;
            this.cvsTransactionStore = cvsTransactionStore;
            this.moderationStore = moderationStore;
            this.moderationQueue = moderationQueue;
            this.blobsManager = blobsManager;
            this.topicsManager = topicsManager;
            this.commentsManager = commentsManager;
            this.repliesManager = repliesManager;
            this.appsManager = appsManager;
            this.connectionStringProvider = connectionStringProvider;
        }

        /// <summary>
        /// Creates a content moderation request
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="contentType">Content type</param>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Create content moderation request task</returns>
        public async Task CreateContentModerationRequest(
            ProcessType processType,
            string appHandle,
            string moderationHandle,
            ContentType contentType,
            string contentHandle,
            Uri callbackUri)
        {
            // check input
            if (string.IsNullOrWhiteSpace(appHandle))
            {
                throw new ArgumentNullException("appHandle");
            }

            if (string.IsNullOrWhiteSpace(moderationHandle))
            {
                throw new ArgumentNullException("moderationHandle");
            }

            if (string.IsNullOrWhiteSpace(contentHandle))
            {
                throw new ArgumentNullException("contentHandle");
            }

            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            // queue up the message
            await this.moderationQueue.SendContentModerationMessage(appHandle, moderationHandle, contentType, contentHandle, callbackUri);
        }

        /// <summary>
        /// Creates a image moderation request
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="userHandle">User who uploaded the image</param>
        /// <param name="imageType">Image type</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Create content moderation request task</returns>
        public async Task CreateImageModerationRequest(
            ProcessType processType,
            string appHandle,
            string moderationHandle,
            string blobHandle,
            string userHandle,
            ImageType imageType,
            Uri callbackUri)
        {
            // check input
            if (string.IsNullOrWhiteSpace(appHandle))
            {
                throw new ArgumentNullException("appHandle");
            }

            if (string.IsNullOrWhiteSpace(moderationHandle))
            {
                throw new ArgumentNullException("moderationHandle");
            }

            if (string.IsNullOrWhiteSpace(blobHandle))
            {
                throw new ArgumentNullException("blobHandle");
            }

            if (string.IsNullOrWhiteSpace(userHandle))
            {
                throw new ArgumentNullException("userHandle");
            }

            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            // queue up the message
            await this.moderationQueue.SendImageModerationMessage(appHandle, moderationHandle, blobHandle, userHandle, imageType, callbackUri);
        }

        /// <summary>
        /// Creates a user moderation request
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="userHandle">Unique identitifier of the user being moderated</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Create user moderation request task</returns>
        public async Task CreateUserModerationRequest(
            ProcessType processType,
            string appHandle,
            string moderationHandle,
            string userHandle,
            Uri callbackUri)
        {
            // check input
            if (string.IsNullOrWhiteSpace(appHandle))
            {
                throw new ArgumentNullException("appHandle");
            }

            if (string.IsNullOrWhiteSpace(moderationHandle))
            {
                throw new ArgumentNullException("moderationHandle");
            }

            if (string.IsNullOrWhiteSpace(userHandle))
            {
                throw new ArgumentNullException("userHandle");
            }

            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            // queue up the message
            await this.moderationQueue.SendUserModerationMessage(appHandle, moderationHandle, userHandle, callbackUri);
        }

        /// <summary>
        /// Submit a moderation request for the content to the moderation provider
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="contentType">Content type</param>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Submit content for moderation task</returns>
        public async Task SubmitContentForModeration(
            ProcessType processType,
            string appHandle,
            string moderationHandle,
            ContentType contentType,
            string contentHandle,
            Uri callbackUri)
        {
            // if init has not yet been called, do it now.
            if (this.initStarted == false)
            {
                await this.Init();
            }

            // if init is not yet done, wait for it to finish.
            this.initDone.WaitOne();

            // check input
            if (string.IsNullOrWhiteSpace(appHandle))
            {
                throw new ArgumentNullException("appHandle");
            }

            if (string.IsNullOrWhiteSpace(moderationHandle))
            {
                throw new ArgumentNullException("moderationHandle");
            }

            if (string.IsNullOrWhiteSpace(contentHandle))
            {
                throw new ArgumentNullException("contentHandle");
            }

            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            // extract the content to be submitted for moderation
            string userHandle = string.Empty;
            string text1 = string.Empty;
            string text2 = string.Empty;
            string imageHandle = null;

            switch (contentType)
            {
                case ContentType.Topic:
                    ITopicEntity topicEntity = await this.topicsManager.ReadTopic(contentHandle);
                    if (topicEntity == null)
                    {
                        // nothing more to do here; it has already been deleted
                        this.log.LogInformation("Topic has already been deleted for appHandle " + appHandle + " and moderationHandle " + moderationHandle);
                        return;
                    }
                    else if (topicEntity.ReviewStatus == ReviewStatus.Banned)
                    {
                        // nothing more to do here; it has already been banned
                        this.log.LogInformation("Topic has already been banned for appHandle " + appHandle + " and moderationHandle " + moderationHandle);
                        return;
                    }

                    text1 = topicEntity.Title;
                    text2 = topicEntity.Text;
                    if (topicEntity.BlobType == BlobType.Image && !string.IsNullOrWhiteSpace(topicEntity.BlobHandle))
                    {
                        imageHandle = topicEntity.BlobHandle;
                    }

                    userHandle = topicEntity.UserHandle;
                    break;

                case ContentType.Comment:
                    ICommentEntity commentEntity = await this.commentsManager.ReadComment(contentHandle);
                    if (commentEntity == null)
                    {
                        // nothing more to do here; it has already been deleted
                        this.log.LogInformation("Comment has already been deleted for appHandle " + appHandle + " and moderationHandle " + moderationHandle);
                        return;
                    }
                    else if (commentEntity.ReviewStatus == ReviewStatus.Banned)
                    {
                        // nothing more to do here; it has already been banned
                        this.log.LogInformation("Comment has already been banned for appHandle " + appHandle + " and moderationHandle " + moderationHandle);
                        return;
                    }

                    text1 = commentEntity.Text;
                    if (commentEntity.BlobType == BlobType.Image && !string.IsNullOrWhiteSpace(commentEntity.BlobHandle))
                    {
                        imageHandle = commentEntity.BlobHandle;
                    }

                    userHandle = commentEntity.UserHandle;
                    break;

                case ContentType.Reply:
                    IReplyEntity replyEntity = await this.repliesManager.ReadReply(contentHandle);
                    if (replyEntity == null)
                    {
                        // nothing more to do here; it has already been deleted
                        this.log.LogInformation("Reply has already been deleted for appHandle " + appHandle + " and moderationHandle " + moderationHandle);
                        return;
                    }
                    else if (replyEntity.ReviewStatus == ReviewStatus.Banned)
                    {
                        // nothing more to do here; it has already been banned
                        this.log.LogInformation("Reply has already been banned for appHandle " + appHandle + " and moderationHandle " + moderationHandle);
                        return;
                    }

                    text1 = replyEntity.Text;
                    userHandle = replyEntity.UserHandle;
                    break;

                default:
                    // this should never happen; throw an exception
                    this.log.LogException("Got an unexpected content type: " + contentType + " for appHandle " + appHandle + " and moderationHandle " + moderationHandle);
                    break;
            }

            // skip if there's nothing to be submitted for review
            if (string.IsNullOrWhiteSpace(text1) && string.IsNullOrWhiteSpace(text2) && imageHandle == null)
            {
                // nothing we can do here since there is no content
                this.log.LogInformation("The content is empty for appHandle " + appHandle + " and moderationHandle " + moderationHandle);
                return;
            }

            CVSRequest cvsRequest = new CVSRequest(new Uri(this.cvsUrl), this.cvsKey);
            CVSContent cvsContent = new CVSContent();

            if (!string.IsNullOrWhiteSpace(text1))
            {
                cvsContent.AddText(text1);
            }

            if (!string.IsNullOrWhiteSpace(text2))
            {
                cvsContent.AddText(text2);
            }

            if (imageHandle != null)
            {
                // check to see that the image exists in our blob storage
                if (await this.blobsManager.ImageExists(imageHandle))
                {
                    // enforce that the version of the image we submit to cvs meets
                    // certain format requirements
                    var newImageHandle = await this.EnforceCVSImageFormat(imageHandle);
                    Uri imageUri = await this.blobsManager.ReadImageCdnUrl(newImageHandle);
                    if (imageUri != null)
                    {
                        cvsContent.AddImageUri(imageUri);
                    }
                }
            }

            // create asynchronous job
            var jobId = await cvsRequest.SubmitAsyncJob(cvsContent, callbackUri);

            var submissionTime = DateTime.UtcNow;

            // store cvs transaction
            await this.cvsTransactionStore.InsertTransaction(
                StorageConsistencyMode.Strong,
                moderationHandle,
                appHandle,
                ReportType.Content,
                submissionTime,
                cvsContent.GetContentItemsAsJson(),
                jobId,
                callbackUri.ToString());

            // add moderation to content moderation store.
            // this is necessary as it records the content lookup information, which
            // would be required when CVS posts results back with a moderation handle
            await this.moderationStore.InsertModeration(
                StorageConsistencyMode.Strong,
                moderationHandle,
                imageHandle,
                contentType,
                contentHandle,
                userHandle,
                appHandle,
                ImageType.ContentBlob,
                ModerationStatus.Pending,
                submissionTime);
        }

        /// <summary>
        /// Submit a moderation request for the content to the moderation provider
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="userHandle">User who owns the image</param>
        /// <param name="imageType">Image type</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Submit content for moderation task</returns>
        public async Task SubmitImageForModeration(
            ProcessType processType,
            string appHandle,
            string moderationHandle,
            string blobHandle,
            string userHandle,
            ImageType imageType,
            Uri callbackUri)
        {
            // if init has not yet been called, do it now.
            if (this.initStarted == false)
            {
                await this.Init();
            }

            // if init is not yet done, wait for it to finish.
            this.initDone.WaitOne();

            // check input
            if (string.IsNullOrWhiteSpace(appHandle))
            {
                throw new ArgumentNullException("appHandle");
            }

            if (string.IsNullOrWhiteSpace(moderationHandle))
            {
                throw new ArgumentNullException("moderationHandle");
            }

            if (string.IsNullOrWhiteSpace(blobHandle))
            {
                throw new ArgumentNullException("blobHandle");
            }

            if (string.IsNullOrWhiteSpace(userHandle))
            {
                throw new ArgumentNullException("userHandle");
            }

            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            // check to see that the image exists in our blob storage
            if (!await this.blobsManager.ImageExists(blobHandle))
            {
                return;
            }

            // enforce that the version of the image we submit to cvs meets
            // certain format requirements
            var newImageHandle = await this.EnforceCVSImageFormat(blobHandle);
            var imageUri = await this.blobsManager.ReadImageCdnUrl(newImageHandle);
            if (imageUri == null)
            {
                this.log.LogInformation(string.Format("There is no image url to validate for blob {0}.", blobHandle));
                return;
            }

            CVSRequest cvsRequest = new CVSRequest(new Uri(this.cvsUrl), this.cvsKey);
            CVSContent cvsContent = new CVSContent();

            cvsContent.AddImageUri(imageUri);

            // submit items to CVS
            var jobId = await cvsRequest.SubmitAsyncJob(cvsContent, callbackUri);
            var submissionTime = DateTime.UtcNow;

            // store cvs transaction
            await this.cvsTransactionStore.InsertTransaction(
                StorageConsistencyMode.Strong,
                moderationHandle,
                appHandle,
                ReportType.Image,
                submissionTime,
                cvsContent.GetContentItemsAsJson(),
                jobId,
                callbackUri.ToString());

            // add image moderation to the moderation store.
            // this is necessary as it records the lookup information, which
            // would be required when CVS posts results back with a moderation handle
            await this.moderationStore.InsertModeration(
                StorageConsistencyMode.Strong,
                moderationHandle,
                blobHandle,
                ContentType.Unknown,
                null,
                userHandle,
                appHandle,
                imageType,
                ModerationStatus.Pending,
                submissionTime);
        }

        /// <summary>
        /// Submit a moderation request for the user profile data to the moderation provider
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="userHandle">Unique identitifier of the user being moderated</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Submit user for moderation task</returns>
        public async Task SubmitUserForModeration(
            ProcessType processType,
            string appHandle,
            string moderationHandle,
            string userHandle,
            Uri callbackUri)
        {
            // if init has not yet been called, do it now.
            if (this.initStarted == false)
            {
                await this.Init();
            }

            // if init is not yet done, wait for it to finish.
            this.initDone.WaitOne();

            // check input
            if (string.IsNullOrWhiteSpace(appHandle))
            {
                throw new ArgumentNullException("appHandle");
            }

            if (string.IsNullOrWhiteSpace(moderationHandle))
            {
                throw new ArgumentNullException("moderationHandle");
            }

            if (string.IsNullOrWhiteSpace(userHandle))
            {
                throw new ArgumentNullException("userHandle");
            }

            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            // get the user profile content
            IUserProfileEntity userProfileEntity = await this.usersManager.ReadUserProfile(userHandle, appHandle);
            if (userProfileEntity == null || userProfileEntity.ReviewStatus == ReviewStatus.Banned)
            {
                // nothing more to do here; it has already been deleted or banned
                this.log.LogInformation("User has already been deleted or banned for appHandle " + appHandle + " and moderationHandle " + moderationHandle);
                return;
            }

            string text1 = userProfileEntity.FirstName;
            string text2 = userProfileEntity.LastName;
            string text3 = userProfileEntity.Bio;
            var imageHandle = userProfileEntity.PhotoHandle;
            Uri imageUri = null;

            // check to see if the image exists in our blob storage
            if (!string.IsNullOrWhiteSpace(imageHandle) && await this.blobsManager.ImageExists(imageHandle))
            {
                // enforce that the version of the image we submit to cvs meets
                // certain format requirements
                var newImageHandle = await this.EnforceCVSImageFormat(imageHandle);
                imageUri = await this.blobsManager.ReadImageCdnUrl(newImageHandle);
                if (imageUri == null)
                {
                    this.log.LogInformation(string.Format("There is no image url to validate for image {0}.", newImageHandle));
                    return;
                }
            }

            // skip if there's nothing to be submitted for review
            if (string.IsNullOrWhiteSpace(text1) && string.IsNullOrWhiteSpace(text2) && string.IsNullOrWhiteSpace(text3) && imageUri == null)
            {
                // nothing we can do here since there is no content
                this.log.LogInformation("The user profile is empty for appHandle " + appHandle + " and moderationHandle " + moderationHandle);
                return;
            }

            // Build CVS request
            CVSRequest cvsRequest = new CVSRequest(new Uri(this.cvsUrl), this.cvsKey);
            CVSContent cvsContent = new CVSContent();

            if (!string.IsNullOrWhiteSpace(text1))
            {
                cvsContent.AddText(text1);
            }

            if (!string.IsNullOrWhiteSpace(text2))
            {
                cvsContent.AddText(text2);
            }

            if (!string.IsNullOrWhiteSpace(text3))
            {
                cvsContent.AddText(text3);
            }

            if (imageUri != null)
            {
                cvsContent.AddImageUri(imageUri);
            }

            // submit to CVS
            var jobId = await cvsRequest.SubmitAsyncJob(cvsContent, callbackUri);

            var submissionTime = DateTime.UtcNow;

            // store cvs transaction
            await this.cvsTransactionStore.InsertTransaction(
                StorageConsistencyMode.Strong,
                moderationHandle,
                appHandle,
                ReportType.User,
                submissionTime,
                cvsContent.GetContentItemsAsJson(),
                jobId,
                callbackUri.ToString());

            // add user moderation to the moderation store
            // this is necessary as it records the user lookup information, which
            // would be required when CVS posts results back with a moderation handle
            await this.moderationStore.InsertModeration(
                StorageConsistencyMode.Strong,
                moderationHandle,
                imageHandle,
                ContentType.Unknown,
                null,
                userHandle,
                appHandle,
                ImageType.UserPhoto,
                ModerationStatus.Pending,
                submissionTime);
        }

        /// <summary>
        /// Process the results from the moderation provider
        /// </summary>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="results">Results of moderation review</param>
        /// <returns>Task that updates the moderation record</returns>
        public async Task ProcessModerationResults(string moderationHandle, JToken results)
        {
            // validate parameters
            if (string.IsNullOrWhiteSpace(moderationHandle))
            {
                throw new ArgumentNullException("moderationHandle");
            }

            if (results == null)
            {
                throw new ArgumentNullException("results");
            }

            // fetch the CVS transaction from the store
            ICVSTransactionEntity cvsTransaction = await this.cvsTransactionStore.QueryTransaction(moderationHandle);
            if (cvsTransaction == null || !cvsTransaction.ModerationHandle.Equals(moderationHandle))
            {
                throw new KeyNotFoundException();
            }

            // update the CVS transaction with the response
            CVSResponse cvsResponse = new CVSResponse(results);
            await this.cvsTransactionStore.UpdateTransactionWithResponse(
                StorageConsistencyMode.Strong,
                moderationHandle,
                DateTime.UtcNow,
                results.ToString(),
                cvsTransaction);

            // fetch the moderation record
            var moderationEntity = await this.moderationStore.QueryModeration(cvsTransaction.AppHandle, moderationHandle);

            // if the CVS review failed, then update the moderation status and return.
            if (cvsResponse.HasFailed)
            {
                this.log.LogError($"CVS review failed for moderation handle {moderationHandle}.");
                await this.moderationStore.UpdateModerationStatus(StorageConsistencyMode.Strong, moderationHandle, ModerationStatus.Failed, moderationEntity);
                return;
            }

            // extract the review result from the CVS response
            ReviewStatus reviewStatus = ReviewStatus.Active;
            try
            {
                reviewStatus = cvsResponse.GetReviewStatus();
            }
            catch (Exception e)
            {
                this.log.LogError("ProcessModerationResults: Exception occurred processing CVS review status in CVS response", e);
            }

            switch (cvsTransaction.ReportType)
            {
                // If this is a content moderation transaction, then process the content moderation review result
                case ReportType.Content:
                    await this.ProcessContentModerationReviewResult(moderationHandle, moderationEntity, reviewStatus);
                    break;

                // If this is a user moderation transaction, then process the user moderation review result
                case ReportType.User:
                    await this.ProcessUserModerationReviewResult(moderationHandle, moderationEntity, reviewStatus);
                    break;

                // If this is a image moderation transaction, then process the image moderation review result
                case ReportType.Image:
                    await this.ProcessImageModerationReviewResult(moderationHandle, moderationEntity, reviewStatus);
                    break;
            }

            if (reviewStatus != ReviewStatus.Banned && reviewStatus != ReviewStatus.Clean && reviewStatus != ReviewStatus.Mature)
            {
                this.log.LogError($"Inconclusive moderation result = {reviewStatus} for moderation handle = {moderationHandle} returned from CVS.");
                await this.moderationStore.UpdateModerationStatus(StorageConsistencyMode.Strong, moderationHandle, ModerationStatus.Failed, moderationEntity);
            }
            else
            {
                await this.moderationStore.UpdateModerationStatus(StorageConsistencyMode.Strong, moderationHandle, ModerationStatus.Completed, moderationEntity);
            }
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

            this.cvsUrl = await this.connectionStringProvider.GetCVSUrl(CVSInstanceType.Default);
            this.cvsKey = await this.connectionStringProvider.GetCVSKey(CVSInstanceType.Default);

            // Init done
            this.initDone.Set();
        }

        /// <summary>
        /// Process the content moderation review results from the moderation provider
        /// </summary>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="moderationEntity">Moderation entity</param>
        /// <param name="reviewStatus">Review status from CVS</param>
        /// <returns>Task that processes the moderation review result</returns>
        private async Task ProcessContentModerationReviewResult(string moderationHandle, IModerationEntity moderationEntity, ReviewStatus reviewStatus)
        {
            // this method implements the following policy:
            // - if the CVS review status is banned, then we ban the content
            // - if the CVS review status is mature and the app policy does not allow mature content, then we ban the content
            // - if the CVS review status is mature and the app policy does allow mature content, then we allow the content
            // - if the CVS review status is clean, then we allow the content
            var matureContentAllowed = await this.appsManager.IsMatureContentAllowed(moderationEntity.AppHandle);
            if (reviewStatus == ReviewStatus.Banned || (reviewStatus == ReviewStatus.Mature && !matureContentAllowed))
            {
                await this.BanContent(moderationEntity, reviewStatus);
            }
            else if (reviewStatus == ReviewStatus.Clean || reviewStatus == ReviewStatus.Mature)
            {
                await this.UpdateContentReviewStatus(moderationEntity, reviewStatus);
            }
        }

        /// <summary>
        /// Process the user moderation review results from the moderation provider
        /// </summary>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="moderationEntity">Moderation entity</param>
        /// <param name="reviewStatus">Review status from CVS</param>
        /// <returns>Task that processes the moderation review result</returns>
        private async Task ProcessUserModerationReviewResult(string moderationHandle, IModerationEntity moderationEntity, ReviewStatus reviewStatus)
        {
            // this method implements the following policy:
            // - if the CVS review status is banned, then we ban the user
            // - if the CVS review status is mature and the app policy does not allow mature content, then we ban the user
            // - if the CVS review status is mature and the app policy does allow mature content, then we allow the user
            // - if the CVS review status is clean, then we allow the user
            var matureContentAllowed = await this.appsManager.IsMatureContentAllowed(moderationEntity.AppHandle);
            if (reviewStatus == ReviewStatus.Banned || (reviewStatus == ReviewStatus.Mature && !matureContentAllowed))
            {
                await this.BanUserProfile(moderationEntity, reviewStatus);
            }
            else if (reviewStatus == ReviewStatus.Clean || reviewStatus == ReviewStatus.Mature)
            {
                await this.UpdateUserProfileReviewStatus(moderationEntity, reviewStatus);
            }
        }

        /// <summary>
        /// Process the image moderation review results from the moderation provider
        /// </summary>
        /// <param name="moderationHandle">Moderation handle</param>
        /// <param name="moderationEntity">Moderation entity</param>
        /// <param name="reviewStatus">Review status from CVS</param>
        /// <returns>Task that processes the moderation review result</returns>
        private async Task ProcessImageModerationReviewResult(string moderationHandle, IModerationEntity moderationEntity, ReviewStatus reviewStatus)
        {
            // this method implements the following policy:
            // - if the CVS review status is banned, then we ban the image
            // - if the CVS review status is mature and the app policy does not allow mature content, then we ban the image
            // - if the CVS review status is mature and the app policy does allow mature content, then we allow the image
            // - if the CVS review status is clean, then we allow the image
            var matureContentAllowed = await this.appsManager.IsMatureContentAllowed(moderationEntity.AppHandle);
            if (reviewStatus == ReviewStatus.Banned || (reviewStatus == ReviewStatus.Mature && !matureContentAllowed))
            {
                await this.BanImage(moderationEntity, reviewStatus);
            }
            else if (reviewStatus == ReviewStatus.Clean || reviewStatus == ReviewStatus.Mature)
            {
                await this.UpdateImageReviewStatus(moderationEntity, reviewStatus);
            }
        }

        /// <summary>
        /// Ensures that the image is of the correct size and type for CVS
        /// </summary>
        /// <param name="blobHandle">original blob handle of image</param>
        /// <returns>blob handle for image to send to CVS</returns>
        private async Task<string> EnforceCVSImageFormat(string blobHandle)
        {
            // Before we send an image to CVS for review, it must meet the CVS image requirements.
            // 1. Supported image formats: JPEG, PNG, GIF, BMP.
            // 2. Image file size should be less than 4MB.
            // 3. Image must be at least 50 pixels in width and height.
            // Following code retrieves the image blob to ensure the above
            // criteria met before submitting content to CVS.
            IBlobItem imageBlob = await this.blobsManager.ReadImage(blobHandle);
            if (imageBlob.Stream == null || imageBlob.Stream.Length <= 0)
            {
                this.log.LogException("Could not retrieve image " + blobHandle);
            }

            // If the image stream length is greater than 4MB do not submit the image
            if (imageBlob.Stream.Length > 4 * 1024 * 1024)
            {
                var originalImageBlob = imageBlob;
                string newImageName = blobHandle + ImageSizesConfiguration.Huge.Id;

                this.log.LogInformation($"The original image {blobHandle} exceeds the maximum size allowed by CVS");

                // instead, attempt to read the huge image
                imageBlob = await this.blobsManager.ReadImage(newImageName);
                if (imageBlob.Stream == null || imageBlob.Stream.Length <= 0)
                {
                    this.log.LogInformation($"A resized image of {blobHandle} does not already exist in blob storage. Creating a new resized image.");

                    // if the huge image does not exist, then we resize
                    Image sourceImage = Image.FromStream(originalImageBlob.Stream);
                    ImageSize newSize = ImageSizesConfiguration.Huge;
                    await this.blobsManager.CreateResizedImage(newImageName, sourceImage, newSize);
                }

                return newImageName;
            }

            // For images that are too small for CVS, there is no benefit to resizing.
            // In this case, we simply return null so that nothing will be submitted to CVS.
            Image originalImage = Image.FromStream(imageBlob.Stream);
            if (originalImage.Height < 50 || originalImage.Width < 50)
            {
                this.log.LogError(string.Format("Image {0} height or width is less than the minimum allowed value of 50 pixels in CVS.", blobHandle));
                return null;
            }

            return blobHandle;
        }

        /// <summary>
        /// Bans content
        /// </summary>
        /// <param name="moderationEntity">moderation entity</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Ban content task</returns>
        private async Task BanContent(IModerationEntity moderationEntity, ReviewStatus reviewStatus)
        {
            switch (moderationEntity.ContentType)
            {
                case ContentType.Topic:
                    await this.BanTopic(moderationEntity, reviewStatus);
                    break;

                case ContentType.Comment:
                    await this.BanComment(moderationEntity, reviewStatus);
                    break;

                case ContentType.Reply:
                    await this.BanReply(moderationEntity, reviewStatus);
                    break;
            }
        }

        /// <summary>
        /// Bans a topic
        /// </summary>
        /// <remarks>
        /// To ban a topic, we update the topic review status and we delete any image associated with the topic.
        /// </remarks>
        /// <param name="moderationEntity">Moderation entity</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Ban topic task</returns>
        private async Task BanTopic(IModerationEntity moderationEntity, ReviewStatus reviewStatus)
        {
            var appHandle = moderationEntity.AppHandle;
            var topicHandle = moderationEntity.ContentHandle;

            ITopicEntity topicEntity = await this.topicsManager.ReadTopic(topicHandle);
            if (topicEntity == null)
            {
                this.log.LogInformation("Topic is already deleted or not found");
                return;
            }

            // delete the image if it is not null; image must be deleted as per MS rules
            this.log.LogInformation("Banning content for topic handle: " + topicHandle);
            if (!string.IsNullOrWhiteSpace(topicEntity.BlobHandle) && topicEntity.BlobType == BlobType.Image)
            {
                await this.BanImage(moderationEntity, reviewStatus);
            }

            // update the status to banned and remove link to the image
            await this.topicsManager.UpdateTopic(
                ProcessType.Backend,
                topicHandle,
                topicEntity.Title,
                topicEntity.Text,
                BlobType.Unknown,
                string.Empty,
                topicEntity.Categories,
                reviewStatus,
                DateTime.UtcNow,
                topicEntity);
        }

        /// <summary>
        /// Bans a comment
        /// </summary>
        /// <param name="moderationEntity">Moderation entity</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Ban comment task</returns>
        private async Task BanComment(IModerationEntity moderationEntity, ReviewStatus reviewStatus)
        {
            var appHandle = moderationEntity.AppHandle;
            var commentHandle = moderationEntity.ContentHandle;

            ICommentEntity commentEntity = await this.commentsManager.ReadComment(commentHandle);
            if (commentEntity == null)
            {
                this.log.LogInformation("Comment is already deleted or not found");
                return;
            }

            // delete the image if it is not null; image must be deleted as per MS rules
            this.log.LogInformation("Banning content for comment handle: " + commentHandle);
            if (!string.IsNullOrWhiteSpace(commentEntity.BlobHandle) && commentEntity.BlobType == BlobType.Image)
            {
                await this.BanImage(moderationEntity, reviewStatus);
            }

            // update the status to banned and remove link to the image
            await this.commentsManager.UpdateComment(
                    ProcessType.Backend,
                    commentHandle,
                    commentEntity.Text,
                    BlobType.Unknown,
                    string.Empty,
                    reviewStatus,
                    DateTime.UtcNow,
                    commentEntity);
        }

        /// <summary>
        /// Bans a reply
        /// </summary>
        /// <param name="moderationEntity">Moderation entity</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Ban reply task</returns>
        private async Task BanReply(IModerationEntity moderationEntity, ReviewStatus reviewStatus)
        {
            var appHandle = moderationEntity.AppHandle;
            var replyHandle = moderationEntity.ContentHandle;

            IReplyEntity replyEntity = await this.repliesManager.ReadReply(replyHandle);
            if (replyEntity == null)
            {
                this.log.LogInformation("Reply is already deleted or not found");
                return;
            }

            // update the status to banned
            this.log.LogInformation("Banning content for reply handle: " + replyHandle);
            await this.repliesManager.UpdateReply(
                ProcessType.Backend,
                replyHandle,
                replyEntity.Text,
                replyEntity.Language,
                reviewStatus,
                DateTime.UtcNow,
                replyEntity);
        }

        /// <summary>
        /// Bans user profile content
        /// </summary>
        /// <param name="moderationEntity">Moderation entity</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Ban user profile content task</returns>
        private async Task BanUserProfile(IModerationEntity moderationEntity, ReviewStatus reviewStatus)
        {
            var appHandle = moderationEntity.AppHandle;
            var userHandle = moderationEntity.UserHandle;

            IUserProfileEntity userProfileEntity = await this.usersManager.ReadUserProfile(userHandle, appHandle);
            if (userProfileEntity == null)
            {
                this.log.LogException("User profile is already deleted or not found");
                return;
            }

            this.log.LogInformation("Banning user profile content for user handle: " + userHandle);

            if (!string.IsNullOrWhiteSpace(userProfileEntity.PhotoHandle))
            {
                await this.BanImage(moderationEntity, reviewStatus);
            }

            // wipe out the photo handle and set the status to banned
            userProfileEntity.PhotoHandle = string.Empty;
            await this.usersManager.UpdateUserReviewStatus(userHandle, appHandle, reviewStatus, userProfileEntity);
        }

        /// <summary>
        /// Bans an image
        /// </summary>
        /// <param name="moderationEntity">Moderation entity</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Ban image task</returns>
        private async Task BanImage(IModerationEntity moderationEntity, ReviewStatus reviewStatus)
        {
            var appHandle = moderationEntity.AppHandle;
            var userHandle = moderationEntity.UserHandle;
            var imageHandle = moderationEntity.ImageHandle;
            var imageType = moderationEntity.ImageType;

            // check that the image is hosted in our blob storage
            if (await this.blobsManager.ImageExists(imageHandle))
            {
                // first, update the image metadata to indicate that the image was banned
                await this.UpdateImageReviewStatus(moderationEntity, reviewStatus);

                // then, delete the image
                await this.blobsManager.DeleteImage(appHandle, userHandle, imageHandle, imageType);
            }
        }

        /// <summary>
        /// Updates content review status
        /// </summary>
        /// <param name="contentModerationEntity">Content moderation entity</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Update content review status task</returns>
        private async Task UpdateContentReviewStatus(IModerationEntity contentModerationEntity, ReviewStatus reviewStatus)
        {
            switch (contentModerationEntity.ContentType)
            {
                case ContentType.Topic:
                    await this.UpdateTopicReviewStatus(contentModerationEntity.AppHandle, contentModerationEntity.ContentHandle, reviewStatus);
                    break;

                case ContentType.Comment:
                    await this.UpdateCommentReviewStatus(contentModerationEntity.AppHandle, contentModerationEntity.ContentHandle, reviewStatus);
                    break;

                case ContentType.Reply:
                    await this.UpdateReplyReviewStatus(contentModerationEntity.AppHandle, contentModerationEntity.ContentHandle, reviewStatus);
                    break;
            }
        }

        /// <summary>
        /// Updates topic review status
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Update topic review status task</returns>
        private async Task UpdateTopicReviewStatus(string appHandle, string topicHandle, ReviewStatus reviewStatus)
        {
            ITopicEntity topicEntity = await this.topicsManager.ReadTopic(topicHandle);
            if (topicEntity == null)
            {
                this.log.LogError("Topic is deleted");
                return;
            }

            if (!this.ReviewStatusUpdateAllowed(topicEntity.ReviewStatus, reviewStatus))
            {
                this.log.LogInformation("Cannot update topic review status. Topic is tagged at a higher moderation threshold.");
                return;
            }

            // update topic
            await this.topicsManager.UpdateTopic(
                    ProcessType.Backend,
                    topicHandle,
                    topicEntity.Title,
                    topicEntity.Text,
                    topicEntity.BlobType,
                    topicEntity.BlobHandle,
                    topicEntity.Categories,
                    reviewStatus,
                    DateTime.UtcNow,
                    topicEntity);
        }

        /// <summary>
        /// Updates comment review status
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Update comment review status task</returns>
        private async Task UpdateCommentReviewStatus(string appHandle, string commentHandle, ReviewStatus reviewStatus)
        {
            ICommentEntity commentEntity = await this.commentsManager.ReadComment(commentHandle);
            if (commentEntity == null)
            {
                this.log.LogError("Comment is deleted or not found");
                return;
            }

            if (!this.ReviewStatusUpdateAllowed(commentEntity.ReviewStatus, reviewStatus))
            {
                this.log.LogInformation("Cannot update comment review status. Comment is tagged at a higher moderation threshold.");
                return;
            }

            // update the status
            await this.commentsManager.UpdateComment(
                ProcessType.Backend,
                commentHandle,
                commentEntity.Text,
                commentEntity.BlobType,
                commentEntity.BlobHandle,
                reviewStatus,
                DateTime.UtcNow,
                commentEntity);
        }

        /// <summary>
        /// Updates reply review status
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Update reply review status task</returns>
        private async Task UpdateReplyReviewStatus(string appHandle, string replyHandle, ReviewStatus reviewStatus)
        {
            IReplyEntity replyEntity = await this.repliesManager.ReadReply(replyHandle);
            if (replyEntity == null)
            {
                this.log.LogError("Reply is deleted or not found.");
                return;
            }

            if (!this.ReviewStatusUpdateAllowed(replyEntity.ReviewStatus, reviewStatus))
            {
                this.log.LogInformation("Cannot update reply review status. Reply is tagged at a higher moderation threshold.");
                return;
            }

            // update the status
            await this.repliesManager.UpdateReply(
                ProcessType.Backend,
                replyHandle,
                replyEntity.Text,
                replyEntity.Language,
                reviewStatus,
                DateTime.UtcNow,
                replyEntity);
        }

        /// <summary>
        /// Updates user profile review status
        /// </summary>
        /// <param name="moderationEntity">Moderation entity</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Update user profile review status task</returns>
        private async Task UpdateUserProfileReviewStatus(IModerationEntity moderationEntity, ReviewStatus reviewStatus)
        {
            var appHandle = moderationEntity.AppHandle;
            var userHandle = moderationEntity.UserHandle;

            this.log.LogInformation($"UpdateUserProfileReviewStatus: appHandle = {appHandle}, userHandle = {userHandle}, reviewStatus = {reviewStatus}");

            IUserProfileEntity userProfileEntity = await this.usersManager.ReadUserProfile(userHandle, appHandle);
            if (userProfileEntity == null)
            {
                this.log.LogError("User profile is deleted or not found");
                return;
            }

            if (!this.ReviewStatusUpdateAllowed(userProfileEntity.ReviewStatus, reviewStatus))
            {
                this.log.LogInformation("Cannot update user profile review status. User is tagged at a higher moderation threshold.");
                return;
            }

            await this.usersManager.UpdateUserReviewStatus(userHandle, appHandle, reviewStatus, userProfileEntity);
        }

        /// <summary>
        /// Updates image review status and bans image if needed
        /// </summary>
        /// <param name="moderationEntity">Moderation entity</param>
        /// <param name="reviewStatus">Review status</param>
        /// <returns>Update image review status task</returns>
        private async Task UpdateImageReviewStatus(IModerationEntity moderationEntity, ReviewStatus reviewStatus)
        {
            var appHandle = moderationEntity.AppHandle;
            var userHandle = moderationEntity.UserHandle;
            var imageHandle = moderationEntity.ImageHandle;

            IImageMetadataEntity imageMetadataEntity = await this.blobsManager.ReadImageMetadata(appHandle, userHandle, imageHandle);
            if (imageMetadataEntity == null)
            {
                this.log.LogError("Cannot update review status. Image is deleted or not found");
                return;
            }

            if (!this.ReviewStatusUpdateAllowed(imageMetadataEntity.ReviewStatus, reviewStatus))
            {
                this.log.LogInformation("Cannot update review status. Image is tagged at a higher moderation threshold.");
                return;
            }

            await this.blobsManager.UpdateImageReviewStatus(appHandle, userHandle, imageHandle, reviewStatus, imageMetadataEntity);
        }

        /// <summary>
        /// Ensures that certain transitions of review status are not allowed
        /// </summary>
        /// <remarks>
        /// We only allow updates to review status from: Clean => {Mature or Banned}; Mature => Banned.
        /// This ensures that if we happen to receive multiple review results from CVS for the
        /// same moderation handle, we will never discard the more "severe" review result.
        /// </remarks>
        /// <param name="fromReviewStatus">From review status</param>
        /// <param name="toReviewStatus">To review status</param>
        /// <returns>Bool indicating if the update is allowed</returns>
        private bool ReviewStatusUpdateAllowed(ReviewStatus fromReviewStatus, ReviewStatus toReviewStatus)
        {
            // If the status is set at a threshold higher than Clean then do not allow update
            if (toReviewStatus.Equals(ReviewStatus.Clean) &&
                (fromReviewStatus.Equals(ReviewStatus.Mature) || fromReviewStatus.Equals(ReviewStatus.Banned)))
            {
                return false;
            }

            // If the status is set at a threshold higher than Mature then do not allow update
            else if (toReviewStatus.Equals(ReviewStatus.Mature) && fromReviewStatus.Equals(ReviewStatus.Banned))
            {
                return false;
            }

            return true;
        }
    }
}
