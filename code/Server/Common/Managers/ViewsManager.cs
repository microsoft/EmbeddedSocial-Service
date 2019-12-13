// <copyright file="ViewsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Blobs;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Principal;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Views manager class
    /// </summary>
    public class ViewsManager : IViewsManager
    {
        // ViewsManager needs pointers to only the Store objects rather than the managers.
        // Whenever a manager requires only read operations from another manager, directly include pointer to the store.
        // This helps us eliminate circular dependencies between managers. Stores are leaf nodes, so we are safe.

        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Apps store
        /// </summary>
        private IAppsStore appsStore;

        /// <summary>
        /// Users store
        /// </summary>
        private IUsersStore usersStore;

        /// <summary>
        /// Topic relationships store
        /// </summary>
        private ITopicRelationshipsStore topicRelationshipsStore;

        /// <summary>
        /// User relationships store
        /// </summary>
        private IUserRelationshipsStore userRelationshipsStore;

        /// <summary>
        /// Topics store
        /// </summary>
        private ITopicsStore topicsStore;

        /// <summary>
        /// Comments store
        /// </summary>
        private ICommentsStore commentsStore;

        /// <summary>
        /// Replies store
        /// </summary>
        private IRepliesStore repliesStore;

        /// <summary>
        /// Likes store
        /// </summary>
        private ILikesStore likesStore;

        /// <summary>
        /// Pins store
        /// </summary>
        private IPinsStore pinsStore;

        /// <summary>
        /// Blobs store
        /// </summary>
        private IBlobsStore blobsStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewsManager"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="appsStore">Apps store</param>
        /// <param name="usersStore">Users store</param>
        /// <param name="userRelationshipsStore">User relationships store</param>
        /// <param name="topicsStore">Topics store</param>
        /// <param name="topicRelationshipsStore">Topic relationships store</param>
        /// <param name="commentsStore">Comments store</param>
        /// <param name="repliesStore">Replies store</param>
        /// <param name="likesStore">Likes store</param>
        /// <param name="pinsStore">Pins store</param>
        /// <param name="blobsStore">Blobs store</param>
        public ViewsManager(
            ILog log,
            IAppsStore appsStore,
            IUsersStore usersStore,
            IUserRelationshipsStore userRelationshipsStore,
            ITopicsStore topicsStore,
            ITopicRelationshipsStore topicRelationshipsStore,
            ICommentsStore commentsStore,
            IRepliesStore repliesStore,
            ILikesStore likesStore,
            IPinsStore pinsStore,
            IBlobsStore blobsStore)
        {
            this.log = log;
            this.appsStore = appsStore;
            this.usersStore = usersStore;
            this.userRelationshipsStore = userRelationshipsStore;
            this.topicsStore = topicsStore;
            this.topicRelationshipsStore = topicRelationshipsStore;
            this.commentsStore = commentsStore;
            this.repliesStore = repliesStore;
            this.likesStore = likesStore;
            this.pinsStore = pinsStore;
            this.blobsStore = blobsStore;
        }

        /// <summary>
        /// Get app compact views from app feed entities
        /// </summary>
        /// <param name="appFeedEntities">App feed entities</param>
        /// <returns>App compact views</returns>
        public async Task<List<AppCompactView>> GetAppCompactViews(IEnumerable<IAppFeedEntity> appFeedEntities)
        {
            IEnumerable<Task<AppCompactView>> tasks = from entity in appFeedEntities select this.GetAppCompactView(entity.AppHandle);
            AppCompactView[] views = await Task.WhenAll(tasks.ToArray());
            List<AppCompactView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get app compact view from app handle
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>App compact view</returns>
        public async Task<AppCompactView> GetAppCompactView(string appHandle)
        {
            IAppProfileEntity appProfileEntity = await this.appsStore.QueryAppProfile(appHandle);
            if (appProfileEntity == null)
            {
                return null;
            }

            if (appProfileEntity.AppStatus == AppStatus.Banned)
            {
                return null;
            }

            AppCompactView view = new AppCompactView()
            {
                Name = appProfileEntity.Name,
                PlatformType = appProfileEntity.PlatformType,
                IconHandle = appProfileEntity.IconHandle,
                IconUrl = await this.GetCdnUrl(BlobType.Image, appProfileEntity.IconHandle),
                DeepLink = appProfileEntity.DeepLink,
                StoreLink = appProfileEntity.StoreLink
            };

            return view;
        }

        /// <summary>
        /// Get user compact view from user principal entities
        /// </summary>
        /// <param name="userPrincipalFeedEntities">User principal feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User compact views</returns>
        public async Task<List<UserCompactView>> GetUserCompactViews(IEnumerable<IPrincipal> userPrincipalFeedEntities, string appHandle, string queryingUserHandle)
        {
            // This code assumes that all objects in the IEnumerable are user principals.
            IEnumerable<Task<UserCompactView>> tasks = from entity in userPrincipalFeedEntities select this.GetUserCompactView(((UserPrincipal)entity).UserHandle, appHandle, queryingUserHandle);
            UserCompactView[] views = await Task.WhenAll(tasks.ToArray());
            List<UserCompactView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get user compact view from user relationship feed entities
        /// </summary>
        /// <param name="userRelationshipFeedEntities">User relationship feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User compact views</returns>
        public async Task<List<UserCompactView>> GetUserCompactViews(
            IEnumerable<IUserRelationshipFeedEntity> userRelationshipFeedEntities,
            string appHandle,
            string queryingUserHandle)
        {
            IEnumerable<Task<UserCompactView>> tasks = from entity in userRelationshipFeedEntities select this.GetUserCompactView(entity.UserHandle, appHandle, queryingUserHandle);
            UserCompactView[] views = await Task.WhenAll(tasks.ToArray());
            List<UserCompactView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get user compact views from like feed entities
        /// </summary>
        /// <param name="likeFeedEntities">Like feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User compact views</returns>
        public async Task<List<UserCompactView>> GetUserCompactViews(
            IEnumerable<ILikeFeedEntity> likeFeedEntities,
            string appHandle,
            string queryingUserHandle)
        {
            IEnumerable<Task<UserCompactView>> tasks = from entity in likeFeedEntities select this.GetUserCompactView(entity.UserHandle, appHandle, queryingUserHandle);
            UserCompactView[] views = await Task.WhenAll(tasks.ToArray());
            List<UserCompactView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get user compact views from user feed entities
        /// </summary>
        /// <param name="userFeedEntities">User feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User compact views</returns>
        public async Task<List<UserCompactView>> GetUserCompactViews(
            IEnumerable<IUserFeedEntity> userFeedEntities,
            string appHandle,
            string queryingUserHandle)
        {
            IEnumerable<Task<UserCompactView>> tasks = from entity in userFeedEntities select this.GetUserCompactView(entity.UserHandle, appHandle, queryingUserHandle);
            UserCompactView[] views = await Task.WhenAll(tasks.ToArray());
            List<UserCompactView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get user compact views from user handles
        /// </summary>
        /// <param name="userHandles">User handles</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User compact views</returns>
        public async Task<List<UserCompactView>> GetUserCompactViews(
            IEnumerable<string> userHandles,
            string appHandle,
            string queryingUserHandle)
        {
            IEnumerable<Task<UserCompactView>> tasks = from userHandle in userHandles select this.GetUserCompactView(userHandle, appHandle, queryingUserHandle);
            UserCompactView[] views = await Task.WhenAll(tasks.ToArray());
            List<UserCompactView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get user compact view from user handle
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User compact view</returns>
        public async Task<UserCompactView> GetUserCompactView(string userHandle, string appHandle, string queryingUserHandle)
        {
            var taskUserProfile = this.usersStore.QueryUserProfile(userHandle, appHandle);
            Task<IUserRelationshipLookupEntity> taskFollowerLookup = null;
            if (queryingUserHandle != null && userHandle != queryingUserHandle)
            {
                taskFollowerLookup = this.userRelationshipsStore.QueryFollowerRelationship(userHandle, queryingUserHandle, appHandle);
            }

            var userProfileEntity = await taskUserProfile;
            if (userProfileEntity == null)
            {
                return null;
            }

            var allowed = await this.ReviewStatusIsAllowed(userProfileEntity.ReviewStatus, appHandle);

            // do not return the profile of a user if the review status prohibits it
            if (!allowed)
            {
                return null;
            }

            string photoUrl = await this.GetCdnUrl(BlobType.Image, userProfileEntity.PhotoHandle);
            UserCompactView view = new UserCompactView()
            {
                UserHandle = userHandle,
                FirstName = userProfileEntity.FirstName,
                LastName = userProfileEntity.LastName,
                PhotoHandle = userProfileEntity.PhotoHandle,
                PhotoUrl = photoUrl,
                Visibility = userProfileEntity.Visibility,
                FollowerStatus = UserRelationshipStatus.None
            };

            if (taskFollowerLookup != null)
            {
                var followerLookupEntity = await taskFollowerLookup;
                if (followerLookupEntity != null)
                {
                    view.FollowerStatus = followerLookupEntity.UserRelationshipStatus;
                }
            }

            return view;
        }

        /// <summary>
        /// Get user profile views from user feed entities and app handle
        /// </summary>
        /// <param name="userFeedEntities">User feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User profile views</returns>
        public async Task<List<UserProfileView>> GetUserProfileViews(
            IEnumerable<IUserFeedEntity> userFeedEntities,
            string appHandle,
            string queryingUserHandle)
        {
            IEnumerable<Task<UserProfileView>> tasks = from entity in userFeedEntities select this.GetUserProfileView(entity.UserHandle, appHandle, queryingUserHandle);
            UserProfileView[] views = await Task.WhenAll(tasks.ToArray());
            List<UserProfileView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get user profile view from user handle and app handle
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>User profile view</returns>
        public async Task<UserProfileView> GetUserProfileView(
            string userHandle,
            string appHandle,
            string queryingUserHandle)
        {
            var taskUserProfile = this.usersStore.QueryUserProfile(userHandle, appHandle);
            var taskFollowerCount = this.userRelationshipsStore.QueryFollowersCount(userHandle, appHandle);
            var taskFollowingCount = this.userRelationshipsStore.QueryFollowingCount(userHandle, appHandle);

            Task<IUserRelationshipLookupEntity> taskFollowerLookup = null;
            Task<IUserRelationshipLookupEntity> taskFollowingLookup = null;
            if (queryingUserHandle != null && userHandle != queryingUserHandle)
            {
                taskFollowerLookup = this.userRelationshipsStore.QueryFollowerRelationship(userHandle, queryingUserHandle, appHandle);
                taskFollowingLookup = this.userRelationshipsStore.QueryFollowingRelationship(userHandle, queryingUserHandle, appHandle);
            }

            var taskTopicCount = this.topicsStore.QueryUserTopicsCount(userHandle, appHandle);

            var userProfileEntity = await taskUserProfile;
            if (userProfileEntity == null)
            {
                return null;
            }

            var allowed = await this.ReviewStatusIsAllowed(userProfileEntity.ReviewStatus, appHandle);

            // do not return the profile of a user if the review status prohibits it
            if (!allowed)
            {
                return null;
            }

            string photoUrl = await this.GetCdnUrl(BlobType.Image, userProfileEntity.PhotoHandle);
            UserProfileView view = new UserProfileView()
            {
                UserHandle = userHandle,
                FirstName = userProfileEntity.FirstName,
                LastName = userProfileEntity.LastName,
                Bio = userProfileEntity.Bio,
                PhotoHandle = userProfileEntity.PhotoHandle,
                PhotoUrl = photoUrl,
                Visibility = userProfileEntity.Visibility,
                TotalTopics = 0,                           // this will be filled in below
                TotalFollowers = 0,                        // this will be filled in below
                TotalFollowing = 0,                        // this will be filled in below
                FollowerStatus = UserRelationshipStatus.None,  // this may be filled in below
                FollowingStatus = UserRelationshipStatus.None, // this may be filled in below
                ProfileStatus = userProfileEntity.ReviewStatus
            };

            long? followerCount = await taskFollowerCount;
            view.TotalFollowers = followerCount ?? 0;

            long? followingCount = await taskFollowingCount;
            view.TotalFollowing = followingCount ?? 0;

            if (taskFollowerLookup != null)
            {
                var followerLookupEntity = await taskFollowerLookup;
                if (followerLookupEntity != null)
                {
                    view.FollowerStatus = followerLookupEntity.UserRelationshipStatus;
                }
            }

            if (taskFollowingLookup != null)
            {
                var followingLookupEntity = await taskFollowingLookup;
                if (followingLookupEntity != null)
                {
                    view.FollowingStatus = followingLookupEntity.UserRelationshipStatus;
                }
            }

            long? topicsCount = await taskTopicCount;
            view.TotalTopics = topicsCount ?? 0;

            return view;
        }

        /// <summary>
        /// Get linked account views for a user
        /// </summary>
        /// <param name="linkedAccountFeedEntities">Linked account feed entities</param>
        /// <returns>Linked account views</returns>
        public List<LinkedAccountView> GetLinkedAccountViews(
            IEnumerable<ILinkedAccountFeedEntity> linkedAccountFeedEntities)
        {
            IEnumerable<LinkedAccountView> views = from entity in linkedAccountFeedEntities select this.GetLinkedAccountView(entity);
            return views.ToList();
        }

        /// <summary>
        /// Get linked account view
        /// </summary>
        /// <param name="linkedAccountFeedEntity">Linked account feed entity</param>
        /// <returns>Linked account view</returns>
        public LinkedAccountView GetLinkedAccountView(
            ILinkedAccountFeedEntity linkedAccountFeedEntity)
        {
            LinkedAccountView view = new LinkedAccountView()
            {
                IdentityProvider = linkedAccountFeedEntity.IdentityProviderType,
                AccountId = linkedAccountFeedEntity.AccountId
            };

            return view;
        }

        /// <summary>
        /// Get topic views from topic feed entities
        /// </summary>
        /// <param name="topicFeedEntities">Topic feed entities</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic views</returns>
        public async Task<List<TopicView>> GetTopicViews(
            IEnumerable<ITopicFeedEntity> topicFeedEntities,
            string queryingUserHandle,
            bool checkFollowingRelationship = false)
        {
            IEnumerable<Task<TopicView>> tasks =
                from entity in topicFeedEntities
                select this.GetTopicView(
                entity.TopicHandle,
                entity.UserHandle,
                entity.AppHandle,
                queryingUserHandle,
                checkFollowingRelationship);

            TopicView[] views = await Task.WhenAll(tasks.ToArray());
            List<TopicView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get topic views from topic relationship feed entities
        /// </summary>
        /// <param name="topicRelationshipFeedEntities">Topic relationship feed entities</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic views</returns>
        public async Task<List<TopicView>> GetTopicViews(
            IEnumerable<ITopicRelationshipFeedEntity> topicRelationshipFeedEntities,
            string queryingUserHandle,
            bool checkFollowingRelationship = false)
        {
            IEnumerable<Task<TopicView>> tasks =
                from entity in topicRelationshipFeedEntities
                select this.GetTopicView(
                entity.TopicHandle,
                queryingUserHandle,
                checkFollowingRelationship);

            TopicView[] views = await Task.WhenAll(tasks.ToArray());
            List<TopicView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get topic views from pin feed entities
        /// </summary>
        /// <param name="pinFeedEntities">Pin feed entities</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic views</returns>
        public async Task<List<TopicView>> GetTopicViews(
            IEnumerable<IPinFeedEntity> pinFeedEntities,
            string queryingUserHandle,
            bool checkFollowingRelationship = false)
        {
            IEnumerable<Task<TopicView>> tasks = from entity in pinFeedEntities
                                                 select this.GetTopicView(
                                                     entity.TopicHandle,
                                                     entity.TopicUserHandle,
                                                     entity.AppHandle,
                                                     queryingUserHandle,
                                                     checkFollowingRelationship);
            TopicView[] views = await Task.WhenAll(tasks.ToArray());
            List<TopicView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get topic views from topic handles
        /// </summary>
        /// <param name="topicHandles">Topic handles</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic views</returns>
        public async Task<List<TopicView>> GetTopicViews(
            IEnumerable<string> topicHandles,
            string queryingUserHandle,
            bool checkFollowingRelationship = false)
        {
            IEnumerable<Task<TopicView>> tasks = from topicHandle in topicHandles
                                                 select this.GetTopicView(
                                                     topicHandle,
                                                     queryingUserHandle,
                                                     checkFollowingRelationship);
            TopicView[] views = await Task.WhenAll(tasks.ToArray());
            List<TopicView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get topic view from topic entity
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicEntity">Topic entity</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic view</returns>
        public async Task<TopicView> GetTopicView(
            string topicHandle,
            ITopicEntity topicEntity,
            string queryingUserHandle,
            bool checkFollowingRelationship = false)
        {
            return await this.GetTopicView(topicHandle, topicEntity, topicEntity.UserHandle, topicEntity.AppHandle, queryingUserHandle, checkFollowingRelationship);
        }

        /// <summary>
        /// Get topic view from topic handle
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic view</returns>
        public async Task<TopicView> GetTopicView(
            string topicHandle,
            string queryingUserHandle,
            bool checkFollowingRelationship = false)
        {
            var topicEntity = await this.topicsStore.QueryTopic(topicHandle);
            if (topicEntity == null)
            {
                return null;
            }

            return await this.GetTopicView(topicHandle, topicEntity, topicEntity.UserHandle, topicEntity.AppHandle, queryingUserHandle, checkFollowingRelationship);
        }

        /// <summary>
        /// Get topic view from topic handle
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic view</returns>
        public async Task<TopicView> GetTopicView(
            string topicHandle,
            string userHandle,
            string appHandle,
            string queryingUserHandle,
            bool checkFollowingRelationship = false)
        {
            return await this.GetTopicView(topicHandle, null, userHandle, appHandle, queryingUserHandle, checkFollowingRelationship);
        }

        /// <summary>
        /// Get comment views from comment feed entities
        /// </summary>
        /// <param name="commentFeedEntities">Comment feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Comment views</returns>
        public async Task<List<CommentView>> GetCommentViews(
            IEnumerable<ICommentFeedEntity> commentFeedEntities,
            string appHandle,
            string queryingUserHandle)
        {
            IEnumerable<Task<CommentView>> tasks = from commentFeedEntity in commentFeedEntities
                                                   select this.GetCommentView(
                                                       commentFeedEntity.CommentHandle,
                                                       commentFeedEntity.UserHandle,
                                                       appHandle,
                                                       queryingUserHandle);
            CommentView[] views = await Task.WhenAll(tasks.ToArray());
            List<CommentView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get comment view from comment entity
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="commentEntity">Comment entity</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Comment view</returns>
        public async Task<CommentView> GetCommentView(string commentHandle, ICommentEntity commentEntity, string queryingUserHandle)
        {
            return await this.GetCommentView(
                commentHandle,
                commentEntity,
                commentEntity.UserHandle,
                commentEntity.AppHandle,
                queryingUserHandle);
        }

        /// <summary>
        /// Get comment view from comment handle
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Comment view</returns>
        public async Task<CommentView> GetCommentView(string commentHandle, string userHandle, string appHandle, string queryingUserHandle)
        {
            return await this.GetCommentView(commentHandle, null, userHandle, appHandle, queryingUserHandle);
        }

        /// <summary>
        /// Get reply views from reply feed entities
        /// </summary>
        /// <param name="replyFeedEntities">Reply feed entities</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Reply views</returns>
        public async Task<List<ReplyView>> GetReplyViews(
            IEnumerable<IReplyFeedEntity> replyFeedEntities,
            string appHandle,
            string queryingUserHandle)
        {
            IEnumerable<Task<ReplyView>> tasks = from replyFeedEntity in replyFeedEntities
                                                 select this.GetReplyView(
                                                     replyFeedEntity.ReplyHandle,
                                                     replyFeedEntity.UserHandle,
                                                     appHandle,
                                                     queryingUserHandle);
            ReplyView[] views = await Task.WhenAll(tasks.ToArray());
            List<ReplyView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get reply view from reply entity
        /// </summary>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="replyEntity">Reply entity</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Reply view</returns>
        public async Task<ReplyView> GetReplyView(string replyHandle, IReplyEntity replyEntity, string queryingUserHandle)
        {
            return await this.GetReplyView(replyHandle, replyEntity, replyEntity.UserHandle, replyEntity.AppHandle, queryingUserHandle);
        }

        /// <summary>
        /// Get reply view from reply handle
        /// </summary>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Reply view</returns>
        public async Task<ReplyView> GetReplyView(string replyHandle, string userHandle, string appHandle, string queryingUserHandle)
        {
            return await this.GetReplyView(replyHandle, null, userHandle, appHandle, queryingUserHandle);
        }

        /// <summary>
        /// Get activity views from activity feed entities
        /// </summary>
        /// <param name="activityFeedEntities">activity feed entities</param>
        /// <param name="readActivityHandle">Read activity handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Activity views</returns>
        public async Task<List<ActivityView>> GetActivityViews(
            IEnumerable<IActivityFeedEntity> activityFeedEntities,
            string readActivityHandle,
            string queryingUserHandle)
        {
            IEnumerable<Task<ActivityView>> tasks = from activityFeedEntity in activityFeedEntities
                                                    select this.GetActivityView(
                                                        activityFeedEntity,
                                                        readActivityHandle,
                                                        queryingUserHandle);
            ActivityView[] views = await Task.WhenAll(tasks.ToArray());
            List<ActivityView> filteredViews = views.Where(v => v != null).Select(v => v).ToList();
            return filteredViews;
        }

        /// <summary>
        /// Get activity view
        /// </summary>
        /// <param name="activityFeedEntity">Activity feed entity</param>
        /// <param name="readActivityHandle">Read activity handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Activity view</returns>
        public async Task<ActivityView> GetActivityView(
            IActivityFeedEntity activityFeedEntity,
            string readActivityHandle,
            string queryingUserHandle)
        {
            return await this.GetActivityView(
                activityFeedEntity.ActivityHandle,
                activityFeedEntity.ActivityType,
                activityFeedEntity.ActorUserHandle,
                activityFeedEntity.ActedOnUserHandle,
                activityFeedEntity.ActedOnContentType,
                activityFeedEntity.ActedOnContentHandle,
                activityFeedEntity.AppHandle,
                activityFeedEntity.CreatedTime,
                readActivityHandle,
                queryingUserHandle);
        }

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
        public async Task<ActivityView> GetActivityView(
            string activityHandle,
            ActivityType activityType,
            string actorUserHandle,
            string actedOnUserHandle,
            ContentType actedOnContentType,
            string actedOnContentHandle,
            string appHandle,
            DateTime createdTime,
            string readActivityHandle,
            string queryingUserHandle)
        {
            // check key input values
            if (string.IsNullOrWhiteSpace(activityHandle))
            {
                this.log.LogException("activityHandle is required");
            }

            // form the activity view
            ActivityView view = new ActivityView()
            {
                ActivityHandle = activityHandle,
                CreatedTime = createdTime,
                ActivityType = activityType,
                ActorUsers = new List<UserCompactView>(),
                ActedOnUser = null, // may be filled in below
                ActedOnContent = null, // may be filled in below
                TotalActions = 1,
                Unread = readActivityHandle == null ? true : string.CompareOrdinal(readActivityHandle, activityHandle) > 0,
                App = null // may be filled in below
            };

            // get the actor user; this can be null for "my activity"
            Task<UserCompactView> taskActorUser = null;
            if (!string.IsNullOrWhiteSpace(actorUserHandle))
            {
                taskActorUser = this.GetUserCompactView(actorUserHandle, appHandle, queryingUserHandle);
            }

            // get the acted on user; this can be null sometimes to save on a table lookup
            Task<UserCompactView> taskActedOnUser = null;
            if (!string.IsNullOrWhiteSpace(actedOnUserHandle))
            {
                taskActedOnUser = this.GetUserCompactView(actedOnUserHandle, appHandle, queryingUserHandle);
            }

            // get the acted on content; this can be null for follower/following activities
            Task<ContentCompactView> taskActedOnContent = null;
            if (actedOnContentHandle != null)
            {
                taskActedOnContent = this.GetContentCompactView(actedOnContentType, actedOnContentHandle, queryingUserHandle);
            }

            Task<ContentCompactView> taskContent = null;
            if (activityType == ActivityType.Comment)
            {
                taskContent = this.GetContentCompactView(ContentType.Comment, activityHandle, queryingUserHandle);
            }
            else if (activityType == ActivityType.Reply)
            {
                taskContent = this.GetContentCompactView(ContentType.Reply, activityHandle, queryingUserHandle);
            }

            Task<AppCompactView> taskApp = null;
            if (appHandle != null)
            {
                taskApp = this.GetAppCompactView(appHandle);
            }

            Task<ILikeLookupEntity> taskLikeLookup = null;
            if (activityType == ActivityType.Like)
            {
                taskLikeLookup = this.likesStore.QueryLike(actedOnContentHandle, actorUserHandle);
            }

            Task<IUserRelationshipLookupEntity> taskFollowerLookup = null;
            if (actedOnUserHandle != null && (activityType == ActivityType.Following || activityType == ActivityType.FollowRequest))
            {
                taskFollowerLookup = this.userRelationshipsStore.QueryFollowerRelationship(actedOnUserHandle, actorUserHandle, appHandle);
            }

            if (taskActorUser != null)
            {
                var actorUser = await taskActorUser;
                if (actorUser == null)
                {
                    // this can happen when the user has been deleted. Don't generate an activity in this situation.
                    return null;
                }

                view.ActorUsers.Add(actorUser);
            }

            if (taskActedOnContent != null)
            {
                var actedOnContent = await taskActedOnContent;
                if (actedOnContent == null)
                {
                    return null;
                }

                view.ActedOnContent = actedOnContent;
            }

            if (taskContent != null)
            {
                var content = await taskContent;
                if (content == null)
                {
                    return null;
                }
            }

            if (taskActedOnUser != null)
            {
                view.ActedOnUser = await taskActedOnUser;
                if (view.ActedOnUser == null)
                {
                    // this can happen when the user has been deleted. Don't generate an activity in this situation.
                    return null;
                }
            }

            if (taskApp != null)
            {
                view.App = await taskApp;
                if (view.App == null)
                {
                    return null;
                }
            }

            if (taskLikeLookup != null)
            {
                var likeLookupEntity = await taskLikeLookup;
                if (likeLookupEntity == null || !likeLookupEntity.Liked || likeLookupEntity.LikeHandle != activityHandle)
                {
                    return null;
                }
            }

            if (taskFollowerLookup != null)
            {
                var followerLookupEntity = await taskFollowerLookup;
                if (activityType == ActivityType.Following)
                {
                    if (followerLookupEntity == null || followerLookupEntity.UserRelationshipStatus != UserRelationshipStatus.Follow)
                    {
                        return null;
                    }
                }

                if (activityType == ActivityType.FollowRequest)
                {
                    if (followerLookupEntity == null || followerLookupEntity.UserRelationshipStatus != UserRelationshipStatus.Pending)
                    {
                        return null;
                    }
                }
            }

            return view;
        }

        /// <summary>
        /// Check if the user attempting to view a topic is a follower of the topic author
        /// </summary>
        /// <param name="topicAuthor">user compact view of the topic author</param>
        /// <returns>true if </returns>
        private static bool IsTopicAuthorFollower(UserCompactView topicAuthor)
        {
            if (topicAuthor != null && topicAuthor.FollowerStatus == UserRelationshipStatus.Follow)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the user attempting to view a topic is a follower of the topic
        /// </summary>
        /// <param name="topicRelationship">topic relationship</param>
        /// <returns>true if </returns>
        private static bool IsTopicFollower(ITopicRelationshipLookupEntity topicRelationship)
        {
            if (topicRelationship != null && topicRelationship.TopicRelationshipStatus == TopicRelationshipStatus.Follow)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get content compact view.
        /// Will return a null compact view if the content has been deleted,
        /// or (is banned and the querying user is not the author).
        /// </summary>
        /// <param name="contentType">Content type</param>
        /// <param name="contentHandle">Content handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Content compact view</returns>
        private async Task<ContentCompactView> GetContentCompactView(ContentType contentType, string contentHandle, string queryingUserHandle)
        {
            if (contentType == ContentType.Topic)
            {
                var topicEntity = await this.topicsStore.QueryTopic(contentHandle);
                var allowed = await this.ReviewStatusIsAllowed(topicEntity.ReviewStatus, topicEntity.AppHandle);
                if (topicEntity != null && allowed)
                {
                    string blobUrl = await this.GetCdnUrl(topicEntity.BlobType, topicEntity.BlobHandle);
                    return new ContentCompactView()
                    {
                        ContentType = contentType,
                        ContentHandle = contentHandle,
                        Text = topicEntity.Title,
                        BlobType = topicEntity.BlobType,
                        BlobHandle = topicEntity.BlobHandle,
                        BlobUrl = blobUrl
                    };
                }
            }
            else if (contentType == ContentType.Comment)
            {
                var commentEntity = await this.commentsStore.QueryComment(contentHandle);
                var allowed = await this.ReviewStatusIsAllowed(commentEntity.ReviewStatus, commentEntity.AppHandle);
                if (commentEntity != null && allowed)
                {
                    string blobUrl = await this.GetCdnUrl(commentEntity.BlobType, commentEntity.BlobHandle);
                    return new ContentCompactView()
                    {
                        ContentType = contentType,
                        ContentHandle = contentHandle,
                        ParentHandle = commentEntity.TopicHandle,
                        Text = commentEntity.Text,
                        BlobType = commentEntity.BlobType,
                        BlobHandle = commentEntity.BlobHandle,
                        BlobUrl = blobUrl
                    };
                }
            }
            else if (contentType == ContentType.Reply)
            {
                var replyEntity = await this.repliesStore.QueryReply(contentHandle);
                var allowed = await this.ReviewStatusIsAllowed(replyEntity.ReviewStatus, replyEntity.AppHandle);
                if (replyEntity != null && allowed)
                {
                    return new ContentCompactView()
                    {
                        ContentType = contentType,
                        ContentHandle = contentHandle,
                        ParentHandle = replyEntity.CommentHandle,
                        RootHandle = replyEntity.TopicHandle,
                        Text = replyEntity.Text
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// Get reply view
        /// </summary>
        /// <param name="replyHandle">Reply handle</param>
        /// <param name="replyEntity">Comment entity</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Reply view</returns>
        private async Task<ReplyView> GetReplyView(
            string replyHandle,
            IReplyEntity replyEntity,
            string userHandle,
            string appHandle,
            string queryingUserHandle)
        {
            Task<IReplyEntity> taskReply = null;
            if (replyEntity == null)
            {
                taskReply = this.repliesStore.QueryReply(replyHandle);
            }

            var taskLikesCount = this.likesStore.QueryLikesCount(replyHandle);
            Task<ILikeLookupEntity> taskLikeLookup = null;
            if (queryingUserHandle != null)
            {
                taskLikeLookup = this.likesStore.QueryLike(replyHandle, queryingUserHandle);
            }

            // get the user that created the reply
            UserCompactView replyAuthor = null;
            if (userHandle != null)
            {
                replyAuthor = await this.GetUserCompactView(userHandle, appHandle, queryingUserHandle);
            }

            if (taskReply != null)
            {
                replyEntity = await taskReply;
            }

            if (replyEntity == null)
            {
                return null;
            }

            var allowed = await this.ReviewStatusIsAllowed(replyEntity.ReviewStatus, replyEntity.AppHandle);

            // do not return a reply if the review status prohibits it
            if (!allowed)
            {
                return null;
            }

            if (replyAuthor == null)
            {
                // this can happen when the author has been deleted; do not return a reply view
                return null;
            }

            ReplyView view = new ReplyView()
            {
                ReplyHandle = replyHandle,
                CommentHandle = replyEntity.CommentHandle,
                TopicHandle = replyEntity.TopicHandle,
                CreatedTime = replyEntity.CreatedTime,
                LastUpdatedTime = replyEntity.LastUpdatedTime,
                User = replyAuthor,
                Text = replyEntity.Text,
                Language = replyEntity.Language,
                TotalLikes = 0, // will be filled in below
                Liked = false, // may be filled in below
                ContentStatus = replyEntity.ReviewStatus
            };

            long? likesCount = await taskLikesCount;
            view.TotalLikes = likesCount ?? 0;

            if (taskLikeLookup != null)
            {
                var likeLookup = await taskLikeLookup;
                if (likeLookup != null)
                {
                    view.Liked = likeLookup.Liked;
                }
            }

            return view;
        }

        /// <summary>
        /// Get comment view
        /// </summary>
        /// <param name="commentHandle">Comment handle</param>
        /// <param name="commentEntity">Comment entity</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <returns>Comment view</returns>
        private async Task<CommentView> GetCommentView(
            string commentHandle,
            ICommentEntity commentEntity,
            string userHandle,
            string appHandle,
            string queryingUserHandle)
        {
            Task<ICommentEntity> taskComment = null;
            if (commentEntity == null)
            {
                taskComment = this.commentsStore.QueryComment(commentHandle);
            }

            var taskLikesCount = this.likesStore.QueryLikesCount(commentHandle);
            var taskRepliesCount = this.repliesStore.QueryCommentRepliesCount(commentHandle);
            Task<ILikeLookupEntity> taskLikeLookup = null;
            if (queryingUserHandle != null)
            {
                taskLikeLookup = this.likesStore.QueryLike(commentHandle, queryingUserHandle);
            }

            // get the user that created the comment
            UserCompactView commentAuthor = null;
            if (userHandle != null)
            {
                commentAuthor = await this.GetUserCompactView(userHandle, appHandle, queryingUserHandle);
            }

            if (taskComment != null)
            {
                commentEntity = await taskComment;
            }

            if (commentEntity == null)
            {
                return null;
            }

            var allowed = await this.ReviewStatusIsAllowed(commentEntity.ReviewStatus, commentEntity.AppHandle);

            // do not return a comment if the review status prohibits it
            if (!allowed)
            {
                return null;
            }

            if (commentAuthor == null)
            {
                // this can happen when the comment author has been deleted; don't return a comment view
                return null;
            }

            string blobUrl = await this.GetCdnUrl(commentEntity.BlobType, commentEntity.BlobHandle);

            CommentView view = new CommentView()
            {
                CommentHandle = commentHandle,
                TopicHandle = commentEntity.TopicHandle,
                CreatedTime = commentEntity.CreatedTime,
                LastUpdatedTime = commentEntity.LastUpdatedTime,
                User = commentAuthor,
                Text = commentEntity.Text,
                BlobType = commentEntity.BlobType,
                BlobHandle = commentEntity.BlobHandle,
                BlobUrl = blobUrl,
                Language = commentEntity.Language,
                TotalLikes = 0, // will be filled in below
                TotalReplies = 0, // will be filled in below
                Liked = false, // may be filled in below
                ContentStatus = commentEntity.ReviewStatus
            };

            long? likesCount = await taskLikesCount;
            long? repliesCount = await taskRepliesCount;
            view.TotalLikes = likesCount ?? 0;
            view.TotalReplies = repliesCount ?? 0;

            if (taskLikeLookup != null)
            {
                var likeLookup = await taskLikeLookup;
                if (likeLookup != null)
                {
                    view.Liked = likeLookup.Liked;
                }
            }

            return view;
        }

        /// <summary>
        /// Get a topic view from a topic entity.
        /// If the topic entity parameter is null, use the topic handle parameter to get the topic entity.
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <param name="topicEntity">Topic entity</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="checkFollowingRelationship">Should check following relationship</param>
        /// <returns>Topic view</returns>
        private async Task<TopicView> GetTopicView(
            string topicHandle,
            ITopicEntity topicEntity,
            string userHandle,
            string appHandle,
            string queryingUserHandle,
            bool checkFollowingRelationship = false)
        {
            Task<ITopicEntity> taskTopic = null;
            if (topicEntity == null)
            {
                taskTopic = this.topicsStore.QueryTopic(topicHandle);
            }

            var taskLikesCount = this.likesStore.QueryLikesCount(topicHandle);
            var taskCommentsCount = this.commentsStore.QueryTopicCommentsCount(topicHandle);

            Task<ILikeLookupEntity> taskLikeLookup = null;
            Task<IPinLookupEntity> taskPinLookup = null;
            Task<ITopicRelationshipLookupEntity> taskTopicFollower = null;

            if (queryingUserHandle != null)
            {
                taskLikeLookup = this.likesStore.QueryLike(topicHandle, queryingUserHandle);
                taskPinLookup = this.pinsStore.QueryPin(queryingUserHandle, topicHandle);
                taskTopicFollower = this.topicRelationshipsStore.QueryTopicFollowerRelationship(topicHandle, queryingUserHandle, appHandle);
            }

            Task<UserCompactView> taskUser = null;
            if (userHandle != null)
            {
                taskUser = this.GetUserCompactView(userHandle, appHandle, queryingUserHandle);
            }

            Task<AppCompactView> taskApp = null;
            if (appHandle != null)
            {
                taskApp = this.GetAppCompactView(appHandle);
            }

            if (taskTopic != null)
            {
                topicEntity = await taskTopic;
            }

            if (topicEntity == null)
            {
                return null;
            }

            var allowed = await this.ReviewStatusIsAllowed(topicEntity.ReviewStatus, topicEntity.AppHandle);

            // do not return a topic if the review status prohibits it
            if (!allowed)
            {
                return null;
            }

            if (userHandle != null && topicEntity.UserHandle != userHandle)
            {
                return null;
            }

            if (appHandle != null && topicEntity.AppHandle != appHandle)
            {
                return null;
            }

            // if the topic is published by a user, then get the topic author (the user who created the topic)
            UserCompactView topicAuthor = null;
            if (taskUser != null && topicEntity.PublisherType == PublisherType.User)
            {
                topicAuthor = await taskUser;

                if (topicAuthor == null)
                {
                    // this can happen when the topic author is deleted; do not return a topic view
                    return null;
                }

                // if we have a valid topic author and the querying user is not the user who created the topic,
                // then we must check that the queryingUser is allowed to see this topic
                if (queryingUserHandle != userHandle)
                {
                    if (topicAuthor.FollowerStatus == UserRelationshipStatus.Blocked)
                    {
                        // do not allow the queriying user to see this topic
                        return null;
                    }

                    if (topicAuthor.Visibility == UserVisibilityStatus.Private
                        && topicAuthor.FollowerStatus != UserRelationshipStatus.Follow)
                    {
                        // do not allow the queriying user to see this topic
                        return null;
                    }
                }
            }

            // if we should check the following relationship
            if (checkFollowingRelationship)
            {
                // first, check whether the user is a follower of the topic author
                if (!IsTopicAuthorFollower(topicAuthor))
                {
                    // determine the topic follower relationship for the querying user
                    ITopicRelationshipLookupEntity topicFollowerRelationship = null;
                    if (taskTopicFollower != null)
                    {
                        topicFollowerRelationship = await taskTopicFollower;
                    }

                    // check whether the user is a topic follower
                    if (!IsTopicFollower(topicFollowerRelationship))
                    {
                        // if we reach here, we know that the user is not a follower of the topic author
                        // and not a follower of the topic.
                        return null;
                    }
                }
            }

            string blobUrl = await this.GetCdnUrl(topicEntity.BlobType, topicEntity.BlobHandle);

            TopicView view = new TopicView()
            {
                TopicHandle = topicHandle,
                CreatedTime = topicEntity.CreatedTime,
                LastUpdatedTime = topicEntity.LastUpdatedTime,
                PublisherType = topicEntity.PublisherType,
                User = topicAuthor,
                Title = topicEntity.Title,
                Text = topicEntity.Text,
                BlobType = topicEntity.BlobType,
                BlobHandle = topicEntity.BlobHandle,
                BlobUrl = blobUrl,
                Categories = topicEntity.Categories,
                Language = topicEntity.Language,
                Group = topicEntity.Group,
                DeepLink = topicEntity.DeepLink,
                FriendlyName = topicEntity.FriendlyName,
                TotalLikes = 0, // will be filled in below
                TotalComments = 0, // will be filled in below
                Liked = false, // may be filled in below
                Pinned = false, // may be filled in below
                ContentStatus = topicEntity.ReviewStatus,
                App = null // will be filled in below
            };

            long? likesCount = await taskLikesCount;
            long? commentsCount = await taskCommentsCount;
            view.TotalLikes = likesCount ?? 0;
            view.TotalComments = commentsCount ?? 0;

            if (taskLikeLookup != null)
            {
                var likeLookupEntity = await taskLikeLookup;
                if (likeLookupEntity != null)
                {
                    view.Liked = likeLookupEntity.Liked;
                }
            }

            if (taskPinLookup != null)
            {
                var pinLookupEntity = await taskPinLookup;
                if (pinLookupEntity != null)
                {
                    view.Pinned = pinLookupEntity.Pinned;
                }
            }

            if (taskApp != null)
            {
                view.App = await taskApp;
                if (view.App == null)
                {
                    return null;
                }
            }

            return view;
        }

        /// <summary>
        /// Obtains the CDN URL for this blob handle
        /// </summary>
        /// <param name="blobType">the type of blob</param>
        /// <param name="blobHandle">unique identifier for the blob</param>
        /// <returns>string that contains the CDN URL to access this blob</returns>
        private async Task<string> GetCdnUrl(BlobType blobType, string blobHandle)
        {
            Uri cdnUrl = null;

            if (blobType == BlobType.Image)
            {
                cdnUrl = await this.blobsStore.QueryImageCdnUrl(blobHandle);
            }
            else
            {
                cdnUrl = await this.blobsStore.QueryBlobCdnUrl(blobHandle);
            }

            if (cdnUrl == null)
            {
                return null;
            }

            return cdnUrl.ToString();
        }

        /// <summary>
        /// Implements the policy that determines if content should be presented to a user based on the content's review status
        /// </summary>
        /// <param name="reviewStatus">review status</param>
        /// <param name="appHandle">app handle</param>
        /// <returns>true if content is allowed</returns>
        private async Task<bool> ReviewStatusIsAllowed(ReviewStatus reviewStatus, string appHandle)
        {
            if (reviewStatus == ReviewStatus.Banned)
            {
                return false;
            }

            // get the app-specific policy on mature content
            var validationConfig = await this.appsStore.QueryValidationConfiguration(appHandle);
            bool allowMatureContent = validationConfig?.AllowMatureContent ?? false;

            // content is not allowed if the review status is Mature and the app does not allow mature content
            if (reviewStatus == ReviewStatus.Mature && allowMatureContent == false)
            {
                return false;
            }

            return true;
        }
    }
}
