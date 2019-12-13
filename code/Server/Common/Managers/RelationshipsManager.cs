// <copyright file="RelationshipsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// User relationships manager class
    /// </summary>
    public class RelationshipsManager : IRelationshipsManager
    {
        /// <summary>
        /// Update popular users score every PopularUsersUpdateFollowersCount
        /// </summary>
        private const int PopularUsersUpdateFollowersCount = 1;

        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// User relationships store
        /// </summary>
        private IUserRelationshipsStore userRelationshipsStore;

        /// <summary>
        /// Topic relationships store
        /// </summary>
        private ITopicRelationshipsStore topicRelationshipsStore;

        /// <summary>
        /// Relationships queue
        /// </summary>
        private IRelationshipsQueue relationshipsQueue;

        /// <summary>
        /// <c>Fanout</c> activities queue
        /// </summary>
        private IFanoutActivitiesQueue fanoutActivitiesQueue;

        /// <summary>
        /// Following imports queue
        /// </summary>
        private IFollowingImportsQueue followingImportsQueue;

        /// <summary>
        /// Popular users manager
        /// </summary>
        private IPopularUsersManager popularUsersManager;

        /// <summary>
        /// Notifications manager
        /// </summary>
        private INotificationsManager notificationsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipsManager"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="userRelationshipsStore">User relationships store</param>
        /// <param name="topicRelationshipsStore">Topic relationships store</param>
        /// <param name="relationshipsQueue">Relationships queue</param>
        /// <param name="fanoutActivitiesQueue"><c>Fanout</c> activities queue</param>
        /// <param name="followingImportsQueue">Following imports queue</param>
        /// <param name="popularUsersManager">Popular users manager</param>
        /// <param name="notificationsManager">Notifications manager</param>
        public RelationshipsManager(
            ILog log,
            IUserRelationshipsStore userRelationshipsStore,
            ITopicRelationshipsStore topicRelationshipsStore,
            IRelationshipsQueue relationshipsQueue,
            IFanoutActivitiesQueue fanoutActivitiesQueue,
            IFollowingImportsQueue followingImportsQueue,
            IPopularUsersManager popularUsersManager,
            INotificationsManager notificationsManager)
        {
            this.log = log;
            this.userRelationshipsStore = userRelationshipsStore;
            this.topicRelationshipsStore = topicRelationshipsStore;
            this.relationshipsQueue = relationshipsQueue;
            this.fanoutActivitiesQueue = fanoutActivitiesQueue;
            this.followingImportsQueue = followingImportsQueue;
            this.popularUsersManager = popularUsersManager;
            this.notificationsManager = notificationsManager;
        }

        /// <summary>
        /// Update a relationship between two users
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="relationshipOperation">User relationship operation</param>
        /// <param name="relationshipHandle">Relationship handle</param>
        /// <param name="followerKeyUserHandle">Follower key user handle</param>
        /// <param name="followingKeyUserHandle">Following key user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="followerRelationshipLookupEntity">Follower relationship lookup entity</param>
        /// <param name="followingRelationshipLookupEntity">Following relationship lookup entity</param>
        /// <returns>Update relationship task</returns>
        public async Task UpdateRelationshipToUser(
            ProcessType processType,
            RelationshipOperation relationshipOperation,
            string relationshipHandle,
            string followerKeyUserHandle,
            string followingKeyUserHandle,
            string appHandle,
            DateTime lastUpdatedTime,
            IUserRelationshipLookupEntity followerRelationshipLookupEntity,
            IUserRelationshipLookupEntity followingRelationshipLookupEntity)
        {
            UserRelationshipStatus userRelationshipStatus = this.GetRelationshipStatus(relationshipOperation);

            if (processType == ProcessType.Frontend)
            {
                await this.userRelationshipsStore.UpdateFollowerRelationship(
                    StorageConsistencyMode.Strong,
                    relationshipHandle,
                    followerKeyUserHandle,
                    followingKeyUserHandle,
                    appHandle,
                    userRelationshipStatus,
                    lastUpdatedTime,
                    followerRelationshipLookupEntity);

                await this.userRelationshipsStore.UpdateFollowingRelationship(
                    StorageConsistencyMode.Strong,
                    relationshipHandle,
                    followingKeyUserHandle,
                    followerKeyUserHandle,
                    appHandle,
                    userRelationshipStatus,
                    lastUpdatedTime,
                    followingRelationshipLookupEntity);

                await this.relationshipsQueue.SendRelationshipMessage(
                    relationshipOperation,
                    relationshipHandle,
                    followerKeyUserHandle,
                    followingKeyUserHandle,
                    appHandle,
                    lastUpdatedTime);
            }
            else if (processType == ProcessType.Backend || processType == ProcessType.BackendRetry)
            {
                if (relationshipOperation == RelationshipOperation.FollowUser)
                {
                    await this.notificationsManager.CreateNotification(
                        processType,
                        followerKeyUserHandle,
                        appHandle,
                        relationshipHandle,
                        ActivityType.Following,
                        followingKeyUserHandle,
                        followerKeyUserHandle,
                        ContentType.Unknown,
                        null,
                        lastUpdatedTime);

                    await this.fanoutActivitiesQueue.SendFanoutActivityMessage(
                        followingKeyUserHandle,
                        appHandle,
                        relationshipHandle,
                        ActivityType.Following,
                        followingKeyUserHandle,
                        followerKeyUserHandle,
                        ContentType.Unknown,
                        null,
                        lastUpdatedTime);

                    await this.followingImportsQueue.SendFollowingImportMessage(
                        followingKeyUserHandle,
                        appHandle,
                        followerKeyUserHandle);
                }
                else if (relationshipOperation == RelationshipOperation.PendingUser)
                {
                    await this.notificationsManager.CreateNotification(
                        processType,
                        followerKeyUserHandle,
                        appHandle,
                        relationshipHandle,
                        ActivityType.FollowRequest,
                        followingKeyUserHandle,
                        followerKeyUserHandle,
                        ContentType.Unknown,
                        null,
                        lastUpdatedTime);
                }
                else if (relationshipOperation == RelationshipOperation.AcceptUser)
                {
                    await this.notificationsManager.CreateNotification(
                        processType,
                        followingKeyUserHandle,
                        appHandle,
                        relationshipHandle,
                        ActivityType.FollowAccept,
                        followerKeyUserHandle,
                        followingKeyUserHandle,
                        ContentType.Unknown,
                        null,
                        lastUpdatedTime);

                    await this.followingImportsQueue.SendFollowingImportMessage(
                        followingKeyUserHandle,
                        appHandle,
                        followerKeyUserHandle);
                }

                // Update popular user feed when follower count changes
                if (relationshipOperation == RelationshipOperation.FollowUser ||
                    relationshipOperation == RelationshipOperation.UnfollowUser ||
                    relationshipOperation == RelationshipOperation.DeleteFollower ||
                    relationshipOperation == RelationshipOperation.BlockUser ||
                    relationshipOperation == RelationshipOperation.AcceptUser)
                {
                    long? followersCount = await this.userRelationshipsStore.QueryFollowersCount(followerKeyUserHandle, appHandle);
                    long followersCountValue = followersCount.HasValue ? followersCount.Value : 0;
                    if (followersCountValue % PopularUsersUpdateFollowersCount == 0)
                    {
                        await this.popularUsersManager.UpdatePopularUser(processType, followerKeyUserHandle, appHandle, followersCountValue);
                    }
                }
            }
        }

        /// <summary>
        /// Update a relationship between a user and a topic
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="relationshipOperation">Relationship operation</param>
        /// <param name="relationshipHandle">Relationship handle</param>
        /// <param name="followerUserHandle">Follower user handle</param>
        /// <param name="followingTopicHandle">Following topic handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="followerRelationshipLookupEntity">Follower relationship lookup entity</param>
        /// <param name="followingRelationshipLookupEntity">Following relationship lookup entity</param>
        /// <returns>Update relationship task</returns>
        public async Task UpdateRelationshipToTopic(
            ProcessType processType,
            RelationshipOperation relationshipOperation,
            string relationshipHandle,
            string followerUserHandle,
            string followingTopicHandle,
            string appHandle,
            DateTime lastUpdatedTime,
            ITopicRelationshipLookupEntity followerRelationshipLookupEntity,
            ITopicRelationshipLookupEntity followingRelationshipLookupEntity)
        {
            TopicRelationshipStatus topicRelationshipStatus = this.GetTopicRelationshipStatus(relationshipOperation);

            if (processType == ProcessType.Frontend)
            {
                await this.topicRelationshipsStore.UpdateTopicFollowerRelationship(
                    StorageConsistencyMode.Strong,
                    relationshipHandle,
                    followingTopicHandle,
                    followerUserHandle,
                    appHandle,
                    topicRelationshipStatus,
                    lastUpdatedTime,
                    followerRelationshipLookupEntity);

                await this.topicRelationshipsStore.UpdateTopicFollowingRelationship(
                    StorageConsistencyMode.Strong,
                    relationshipHandle,
                    followerUserHandle,
                    followingTopicHandle,
                    appHandle,
                    topicRelationshipStatus,
                    lastUpdatedTime,
                    followingRelationshipLookupEntity);

                // fanout an activity indicating that the followerUser is now following the followingTopicHandle
                await this.fanoutActivitiesQueue.SendFanoutActivityMessage(
                    followerUserHandle,
                    appHandle,
                    relationshipHandle,
                    ActivityType.Following,
                    followerUserHandle,
                    null,
                    ContentType.Topic,
                    followingTopicHandle,
                    lastUpdatedTime);
            }
        }

        /// <summary>
        /// Read follower relationship in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User relationship lookup entity</returns>
        public async Task<IUserRelationshipLookupEntity> ReadFollowerRelationship(
            string userHandle,
            string relationshipUserHandle,
            string appHandle)
        {
            return await this.userRelationshipsStore.QueryFollowerRelationship(userHandle, relationshipUserHandle, appHandle);
        }

        /// <summary>
        /// Read following relationship in an app between two users
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User relationship lookup entity</returns>
        public async Task<IUserRelationshipLookupEntity> ReadFollowingRelationshipToUser(
            string userHandle,
            string relationshipUserHandle,
            string appHandle)
        {
            return await this.userRelationshipsStore.QueryFollowingRelationship(userHandle, relationshipUserHandle, appHandle);
        }

        /// <summary>
        /// Read topic follower relationship in an app
        /// </summary>
        /// <param name="topicHandle">topic handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Topic relationship lookup entity</returns>
        public async Task<ITopicRelationshipLookupEntity> ReadTopicFollowerRelationship(
            string topicHandle,
            string relationshipUserHandle,
            string appHandle)
        {
            return await this.topicRelationshipsStore.QueryTopicFollowerRelationship(topicHandle, relationshipUserHandle, appHandle);
        }

        /// <summary>
        /// Read following relationship in an app between a user and a topic
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipTopicHandle">Relationship topic handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Topic relationship lookup entity</returns>
        public async Task<ITopicRelationshipLookupEntity> ReadFollowingRelationshipToTopic(string userHandle, string relationshipTopicHandle, string appHandle)
        {
            return await this.topicRelationshipsStore.QueryTopicFollowingRelationship(userHandle, relationshipTopicHandle, appHandle);
        }

        /// <summary>
        /// Read followers in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        public async Task<IList<IUserRelationshipFeedEntity>> ReadFollowers(
            string userHandle,
            string appHandle,
            string cursor,
            int limit)
        {
            return await this.userRelationshipsStore.QueryFollowers(userHandle, appHandle, cursor, limit);
        }

        /// <summary>
        /// Read following users in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        public async Task<IList<IUserRelationshipFeedEntity>> ReadFollowing(
            string userHandle,
            string appHandle,
            string cursor,
            int limit)
        {
            return await this.userRelationshipsStore.QueryFollowing(userHandle, appHandle, cursor, limit);
        }

        /// <summary>
        /// Read following topics in an app.
        /// This is the list of topics a given user is following.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        public async Task<IList<ITopicRelationshipFeedEntity>> ReadTopicFollowing(
            string userHandle,
            string appHandle,
            string cursor,
            int limit)
        {
            return await this.topicRelationshipsStore.QueryTopicFollowing(userHandle, appHandle, cursor, limit);
        }

        /// <summary>
        /// Read pending users in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        public async Task<IList<IUserRelationshipFeedEntity>> ReadPendingUsers(
            string userHandle,
            string appHandle,
            string cursor,
            int limit)
        {
            return await this.userRelationshipsStore.QueryPendingUsers(userHandle, appHandle, cursor, limit);
        }

        /// <summary>
        /// Read blocked users in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        public async Task<IList<IUserRelationshipFeedEntity>> ReadBlockedUsers(
            string userHandle,
            string appHandle,
            string cursor,
            int limit)
        {
            return await this.userRelationshipsStore.QueryBlockedUsers(userHandle, appHandle, cursor, limit);
        }

        /// <summary>
        /// Read count of followers in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Followers count in an app</returns>
        public async Task<long?> ReadFollowersCount(string userHandle, string appHandle)
        {
            return await this.userRelationshipsStore.QueryFollowersCount(userHandle, appHandle);
        }

        /// <summary>
        /// Read count of following users in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Following users count in an app</returns>
        public async Task<long?> ReadFollowingCount(string userHandle, string appHandle)
        {
            return await this.userRelationshipsStore.QueryFollowingCount(userHandle, appHandle);
        }

        /// <summary>
        /// Read count of pending users in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Pending users count in an app</returns>
        public async Task<long?> ReadPendingUsersCount(string userHandle, string appHandle)
        {
            return await this.userRelationshipsStore.QueryPendingUsersCount(userHandle, appHandle);
        }

        /// <summary>
        /// Read count of blocked users in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Blocked users count in an app</returns>
        public async Task<long?> ReadBlockedUsersCount(string userHandle, string appHandle)
        {
            return await this.userRelationshipsStore.QueryBlockedUsersCount(userHandle, appHandle);
        }

        /// <summary>
        /// Read if user resources are visible to querying user
        /// </summary>
        /// <param name="userProfileEntity">User profile entity</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Boolean indicating whether user resources are visible</returns>
        public async Task<bool> ReadRelationshipVisibility(IUserProfileEntity userProfileEntity, string userHandle, string queryingUserHandle, string appHandle)
        {
            if (userProfileEntity.Visibility == UserVisibilityStatus.Public)
            {
                return true;
            }

            if (userHandle == queryingUserHandle)
            {
                return true;
            }

            var relationshipLookupEntity = await this.ReadFollowerRelationship(userHandle, queryingUserHandle, appHandle);
            if (relationshipLookupEntity != null && relationshipLookupEntity.UserRelationshipStatus != UserRelationshipStatus.Follow)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get topic relationship status
        /// </summary>
        /// <param name="relationshipOperation">Relationship operation</param>
        /// <returns>Topic relationship status</returns>
        private TopicRelationshipStatus GetTopicRelationshipStatus(RelationshipOperation relationshipOperation)
        {
            TopicRelationshipStatus topicRelationshipStatus = TopicRelationshipStatus.None;

            if (relationshipOperation == RelationshipOperation.UnfollowTopic)
            {
                topicRelationshipStatus = TopicRelationshipStatus.None;
            }
            else if (relationshipOperation == RelationshipOperation.FollowTopic)
            {
                topicRelationshipStatus = TopicRelationshipStatus.Follow;
            }
            else
            {
                this.log.LogError("Invalid relationship operation on topic");
            }

            return topicRelationshipStatus;
        }

        /// <summary>
        /// Get target relationship status
        /// </summary>
        /// <param name="relationshipOperation">Relationship operation</param>
        /// <returns>Relationship status</returns>
        private UserRelationshipStatus GetRelationshipStatus(RelationshipOperation relationshipOperation)
        {
            UserRelationshipStatus userRelationshipStatus = UserRelationshipStatus.None;
            switch (relationshipOperation)
            {
                case RelationshipOperation.BlockUser:
                    userRelationshipStatus = UserRelationshipStatus.Blocked;
                    break;
                case RelationshipOperation.UnblockUser:
                    userRelationshipStatus = UserRelationshipStatus.None;
                    break;
                case RelationshipOperation.AcceptUser:
                    userRelationshipStatus = UserRelationshipStatus.Follow;
                    break;
                case RelationshipOperation.RejectUser:
                    userRelationshipStatus = UserRelationshipStatus.None;
                    break;
                case RelationshipOperation.FollowUser:
                    userRelationshipStatus = UserRelationshipStatus.Follow;
                    break;
                case RelationshipOperation.PendingUser:
                    userRelationshipStatus = UserRelationshipStatus.Pending;
                    break;
                case RelationshipOperation.UnfollowUser:
                    userRelationshipStatus = UserRelationshipStatus.None;
                    break;
                case RelationshipOperation.DeleteFollower:
                    userRelationshipStatus = UserRelationshipStatus.None;
                    break;
            }

            return userRelationshipStatus;
        }
    }
}
