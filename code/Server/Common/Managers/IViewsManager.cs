// <copyright file="IViewsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Views manager interface
    /// </summary>
    public interface IViewsManager
    {
        /// <summary>
        /// Get app compact views from app feed entities
        /// </summary>
        /// <param name="appFeedEntities">App feed entities</param>
        /// <returns>App compact views</returns>
        Task<List<AppCompactView>> GetAppCompactViews(IEnumerable<IAppFeedEntity> appFeedEntities);

        /// <summary>
        /// Get app compact view from app handle
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>App compact view</returns>
        Task<AppCompactView> GetAppCompactView(string appHandle);

        /// <summary>
        /// Get user compact view from user principal entities
        /// </summary>
        /// <param name="userPrincipalFeedEntities">User principal feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User compact views</returns>
        Task<List<UserCompactView>> GetUserCompactViews(IEnumerable<IPrincipal> userPrincipalFeedEntities, string appHandle, string queryingUserHandle);

        /// <summary>
        /// Get user compact view from user relationship feed entities
        /// </summary>
        /// <param name="userRelationshipFeedEntities">User relationship feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User compact views</returns>
        Task<List<UserCompactView>> GetUserCompactViews(IEnumerable<IUserRelationshipFeedEntity> userRelationshipFeedEntities, string appHandle, string queryingUserHandle);

        /// <summary>
        /// Get user compact views from like feed entities
        /// </summary>
        /// <param name="likeFeedEntities">Like feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User compact views</returns>
        Task<List<UserCompactView>> GetUserCompactViews(IEnumerable<ILikeFeedEntity> likeFeedEntities, string appHandle, string queryingUserHandle);

        /// <summary>
        /// Get user compact views from user feed entities
        /// </summary>
        /// <param name="userFeedEntities">User feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User compact views</returns>
        Task<List<UserCompactView>> GetUserCompactViews(IEnumerable<IUserFeedEntity> userFeedEntities, string appHandle, string queryingUserHandle);

        /// <summary>
        /// Get user compact views from user handles
        /// </summary>
        /// <param name="userHandles">User handles</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User compact views</returns>
        Task<List<UserCompactView>> GetUserCompactViews(IEnumerable<string> userHandles, string appHandle, string queryingUserHandle);

        /// <summary>
        /// Get user compact view from user handle
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User compact view</returns>
        Task<UserCompactView> GetUserCompactView(string userHandle, string appHandle, string queryingUserHandle);

        /// <summary>
        /// Get user profile views from user feed entities and app handle
        /// </summary>
        /// <param name="userFeedEntities">User feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User profile views</returns>
        Task<List<UserProfileView>> GetUserProfileViews(IEnumerable<IUserFeedEntity> userFeedEntities, string appHandle, string queryingUserHandle);

        /// <summary>
        /// Get user profile view from user handle and app handle
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User profile view</returns>
        Task<UserProfileView> GetUserProfileView(string userHandle, string appHandle, string queryingUserHandle);

        /// <summary>
        /// Get linked account views for a user
        /// </summary>
        /// <param name="linkedAccountFeedEntities">Linked account feed entities</param>
        /// <returns>Linked account views</returns>
        List<LinkedAccountView> GetLinkedAccountViews(IEnumerable<ILinkedAccountFeedEntity> linkedAccountFeedEntities);

        /// <summary>
        /// Get linked account view
        /// </summary>
        /// <param name="linkedAccountFeedEntity">Linked account feed entity</param>
        /// <returns>Linked account view</returns>
        LinkedAccountView GetLinkedAccountView(ILinkedAccountFeedEntity linkedAccountFeedEntity);

        /// <summary>
        /// Get topic views from topic feed entities
        /// </summary>
        /// <param name="topicFeedEntities">Topic feed entities</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic views</returns>
        Task<List<TopicView>> GetTopicViews(IEnumerable<ITopicFeedEntity> topicFeedEntities, string queryingUserHandle, bool checkFollowingRelationship = false);

        /// <summary>
        /// Get topic views from topic relationship feed entities
        /// </summary>
        /// <param name="topicRelationshipFeedEntities">Topic relationship feed entities</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic views</returns>
        Task<List<TopicView>> GetTopicViews(IEnumerable<ITopicRelationshipFeedEntity> topicRelationshipFeedEntities, string queryingUserHandle, bool checkFollowingRelationship = false);

        /// <summary>
        /// Get topic views from pin feed entities
        /// </summary>
        /// <param name="pinFeedEntities">Pin feed entities</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic views</returns>
        Task<List<TopicView>> GetTopicViews(IEnumerable<IPinFeedEntity> pinFeedEntities, string queryingUserHandle, bool checkFollowingRelationship = false);

        /// <summary>
        /// Get topic views from topic handles
        /// </summary>
        /// <param name="topicHandles">Topic handles</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic views</returns>
        Task<List<TopicView>> GetTopicViews(IEnumerable<string> topicHandles, string queryingUserHandle, bool checkFollowingRelationship = false);

        /// <summary>
        /// Get topic view from topic entity
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicEntity">Topic entity</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic view</returns>
        Task<TopicView> GetTopicView(
            string topicHandle,
            ITopicEntity topicEntity,
            string queryingUserHandle,
            bool checkFollowingRelationship = false);

        /// <summary>
        /// Get topic view from topic handle
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic view</returns>
        Task<TopicView> GetTopicView(string topicHandle, string queryingUserHandle, bool checkFollowingRelationship = false);

        /// <summary>
        /// Get topic view from topic handle
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic view</returns>
        Task<TopicView> GetTopicView(string topicHandle, string userHandle, string appHandle, string queryingUserHandle, bool checkFollowingRelationship = false);

        /// <summary>
        /// Get comment views from comment feed entities
        /// </summary>
        /// <param name="commentFeedEntities">Comment feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Comment views</returns>
        Task<List<CommentView>> GetCommentViews(
            IEnumerable<ICommentFeedEntity> commentFeedEntities,
            string appHandle,
            string queryingUserHandle);

        /// <summary>
        /// Get comment view from comment entity
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="commentEntity">Comment entity</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Comment view</returns>
        Task<CommentView> GetCommentView(string commentHandle, ICommentEntity commentEntity, string queryingUserHandle);

        /// <summary>
        /// Get comment view from comment handle
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Comment view</returns>
        Task<CommentView> GetCommentView(string commentHandle, string userHandle, string appHandle, string queryingUserHandle);

        /// <summary>
        /// Get reply views from reply feed entities
        /// </summary>
        /// <param name="replyFeedEntities">Reply feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Reply views</returns>
        Task<List<ReplyView>> GetReplyViews(
            IEnumerable<IReplyFeedEntity> replyFeedEntities,
            string appHandle,
            string queryingUserHandle);

        /// <summary>
        /// Get reply view from reply entity
        /// </summary>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="replyEntity">Reply entity</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Reply view</returns>
        Task<ReplyView> GetReplyView(string replyHandle, IReplyEntity replyEntity, string queryingUserHandle);

        /// <summary>
        /// Get reply view from reply handle
        /// </summary>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Reply view</returns>
        Task<ReplyView> GetReplyView(string replyHandle, string userHandle, string appHandle, string queryingUserHandle);

        /// <summary>
        /// Get activity views from activity feed entities
        /// </summary>
        /// <param name="activityFeedEntities">activity feed entities</param>
        /// <param name="readActivityHandle">Read activity handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Activity views</returns>
        Task<List<ActivityView>> GetActivityViews(
            IEnumerable<IActivityFeedEntity> activityFeedEntities,
            string readActivityHandle,
            string queryingUserHandle);

        /// <summary>
        /// Get activity view
        /// </summary>
        /// <param name="activityFeedEntity">Activity feed entity</param>
        /// <param name="readActivityHandle">Read activity handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Activity view</returns>
        Task<ActivityView> GetActivityView(
            IActivityFeedEntity activityFeedEntity,
            string readActivityHandle,
            string queryingUserHandle);

        /// <summary>
        /// Get activity view
        /// </summary>
        /// <param name="activityHandle">Activity handle</param>
        /// <param name="activityType">Activity type</param>
        /// <param name="actorUserHandle">Actor user handle</param>
        /// <param name="actedOnUserHandle">Acted on user handle</param>
        /// <param name="actedOnContentType">Acted on content type</param>
        /// <param name="actedOnContentHandle">Acted on content handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="readActivityHandle">Read activity handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Activity view</returns>
        Task<ActivityView> GetActivityView(
            string activityHandle,
            ActivityType activityType,
            string actorUserHandle,
            string actedOnUserHandle,
            ContentType actedOnContentType,
            string actedOnContentHandle,
            string appHandle,
            DateTime createdTime,
            string readActivityHandle,
            string queryingUserHandle);
    }
}
