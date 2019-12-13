// <copyright file="LogApplicationMetrics.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Metrics
{
    using System;

    using SocialPlus.Logging;

    /// <summary>
    /// Logger for application metrics
    /// </summary>
    public class LogApplicationMetrics : IApplicationMetrics
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogApplicationMetrics"/> class.
        /// </summary>
        /// <param name="log">Log</param>
        public LogApplicationMetrics(ILog log)
        {
            this.log = log;
        }

        /// <summary>
        /// Logs a new like
        /// </summary>
        /// <param name="processType">Frontend" (executed by the Web role), "Backend", or "BackendRetry" (both executed by the Worker role)</param>
        /// <param name="likeHandle">Handle of the like</param>
        /// <param name="contentType">"Topic" or "Comment" or "Reply"</param>
        /// <param name="contentHandle">Handle of the content (whether topic, comment, or reply)</param>
        /// <param name="userHandle">Handle of the user posting the like event</param>
        /// <param name="liked">"True" (when the call was PostLike), "False" (when the call was DeleteLike)</param>
        /// <param name="contentPublisherType">"User" or "App"</param>
        /// <param name="contentUserHandle">Handle of the user who owns the content</param>
        /// <param name="contentCreatedTime">time when content was created in Utc</param>
        /// <param name="appHandle">Handle of the application (always set to a single value corresponding to Beihai)</param>
        /// <param name="lastUpdatedTime">time the field was last updated</param>
        /// <param name="likeLookupEntityUpdatedTime">time field internal to EmbeddedSocial</param>
        /// <param name="likeLookupEntityLiked">boolean field internal to EmbeddedSocial</param>
        /// <param name="likelookupEntityLikeHandle">handle field internal to EmbeddedSocial</param>
        public void Like(
            string processType,
            string likeHandle,
            string contentType,
            string contentHandle,
            string userHandle,
            string liked,
            string contentPublisherType,
            string contentUserHandle,
            DateTime contentCreatedTime,
            string appHandle,
            DateTime lastUpdatedTime,
            DateTime? likeLookupEntityUpdatedTime,
            string likeLookupEntityLiked,
            string likelookupEntityLikeHandle)
        {
            string logMessage = $"processType = {processType}, likeHandle = {likeHandle}, contentType = {contentType}, contentHandle = {contentHandle}, userHandle = {userHandle}";
            logMessage += $", liked = {liked}, contentPublisherType = {contentPublisherType}, contentUserHandle = {contentUserHandle}, contentCreatedTime = {contentCreatedTime}";
            logMessage += $", appHandle = {appHandle}, lastUpdatedTime = {lastUpdatedTime}, likeLookupEntityUpdatedTime = {likeLookupEntityUpdatedTime}";
            logMessage += $", likeLookupEntityLiked = {likeLookupEntityLiked}, likelookupEntityLikeHandle = {likelookupEntityLikeHandle}";
            this.log.LogInformation(logMessage);
        }

        /// <summary>
        /// Logs a new comment
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="text">Comment text</param>
        /// <param name="blobType">Blob type</param>
        /// <param name="blobHandle">Blob handle</param>
        /// <param name="language">Comment language</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicPublisherType">Topic publisher type</param>
        /// <param name="topicUserHandle">User handle of topic publisher</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="appHandle">App handle</param>
        public void Comment(
            string processType,
            string commentHandle,
            string text,
            string blobType,
            string blobHandle,
            string language,
            string userHandle,
            string topicHandle,
            string topicPublisherType,
            string topicUserHandle,
            DateTime createdTime,
            string reviewStatus,
            string appHandle)
        {
            string logMessage = $"processType = {processType}, commentHandle = {commentHandle}, text = {text}, blobType = {blobType}, blobHandle = {blobHandle}";
            logMessage += $", language = {language}, userHandle = {userHandle}, topicHandle = {topicHandle}, topicPublisherType = {topicPublisherType}";
            logMessage += $", topicUserHandle = {topicUserHandle}, createdTime = {createdTime}, reviewStatus = {reviewStatus}, appHandle = {appHandle}";
            this.log.LogInformation(logMessage);
        }

        /// <summary>
        /// Log the addition of a new user
        /// </summary>
        public void AddUser()
        {
            string logMessage = "Added 1 User";
            this.log.LogInformation(logMessage);
        }

        /// <summary>
        /// Log the deletion of a user
        /// </summary>
        public void DeleteUser()
        {
            string logMessage = "Deleted 1 User";
            this.log.LogInformation(logMessage);
        }

        /// <summary>
        /// Log the addition of a new active user (sign-in)
        /// </summary>
        public void AddActiveUser()
        {
            string logMessage = "Added 1 Active User";
            this.log.LogInformation(logMessage);
        }

        /// <summary>
        /// Log the deletion of an active user (sign-off)
        /// </summary>
        public void DeleteActiveUser()
        {
            string logMessage = "Deleted 1 Active User";
            this.log.LogInformation(logMessage);
        }
    }
}
