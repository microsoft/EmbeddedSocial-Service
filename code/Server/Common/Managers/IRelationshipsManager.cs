// <copyright file="IRelationshipsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Server.Entities;

    /// <summary>
    /// User relationships manager interface
    /// </summary>
    public interface IRelationshipsManager
    {
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
        Task UpdateRelationshipToUser(
            ProcessType processType,
            RelationshipOperation relationshipOperation,
            string relationshipHandle,
            string followerKeyUserHandle,
            string followingKeyUserHandle,
            string appHandle,
            DateTime lastUpdatedTime,
            IUserRelationshipLookupEntity followerRelationshipLookupEntity,
            IUserRelationshipLookupEntity followingRelationshipLookupEntity);

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
        Task UpdateRelationshipToTopic(
            ProcessType processType,
            RelationshipOperation relationshipOperation,
            string relationshipHandle,
            string followerUserHandle,
            string followingTopicHandle,
            string appHandle,
            DateTime lastUpdatedTime,
            ITopicRelationshipLookupEntity followerRelationshipLookupEntity,
            ITopicRelationshipLookupEntity followingRelationshipLookupEntity);

        /// <summary>
        /// Read follower relationship in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User relationship lookup entity</returns>
        Task<IUserRelationshipLookupEntity> ReadFollowerRelationship(string userHandle, string relationshipUserHandle, string appHandle);

        /// <summary>
        /// Read following relationship in an app between two users
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User relationship lookup entity</returns>
        Task<IUserRelationshipLookupEntity> ReadFollowingRelationshipToUser(string userHandle, string relationshipUserHandle, string appHandle);

        /// <summary>
        /// Read topic follower relationship in an app
        /// </summary>
        /// <param name="topicHandle">topic handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Topic relationship lookup entity</returns>
        Task<ITopicRelationshipLookupEntity> ReadTopicFollowerRelationship(string topicHandle, string relationshipUserHandle, string appHandle);

        /// <summary>
        /// Read following relationship in an app between a user and a topic
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipTopicHandle">Relationship topic handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Topic relationship lookup entity</returns>
        Task<ITopicRelationshipLookupEntity> ReadFollowingRelationshipToTopic(string userHandle, string relationshipTopicHandle, string appHandle);

        /// <summary>
        /// Read followers in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        Task<IList<IUserRelationshipFeedEntity>> ReadFollowers(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Read following users in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        Task<IList<IUserRelationshipFeedEntity>> ReadFollowing(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Read following topics in an app.
        /// This is the list of topics a given user is following.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        Task<IList<ITopicRelationshipFeedEntity>> ReadTopicFollowing(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Read pending users in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        Task<IList<IUserRelationshipFeedEntity>> ReadPendingUsers(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Read blocked users in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        Task<IList<IUserRelationshipFeedEntity>> ReadBlockedUsers(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Read count of followers in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Followers count in an app</returns>
        Task<long?> ReadFollowersCount(string userHandle, string appHandle);

        /// <summary>
        /// Read count of following users in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Following users count in an app</returns>
        Task<long?> ReadFollowingCount(string userHandle, string appHandle);

        /// <summary>
        /// Read count of pending users in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Pending users count in an app</returns>
        Task<long?> ReadPendingUsersCount(string userHandle, string appHandle);

        /// <summary>
        /// Read count of blocked users in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Blocked users count in an app</returns>
        Task<long?> ReadBlockedUsersCount(string userHandle, string appHandle);

        /// <summary>
        /// Read if user resources are visible to querying user
        /// </summary>
        /// <param name="userProfileEntity">User profile entity</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="queryingUserHandle">Querying user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Boolean indicating whether user resources are visible</returns>
        Task<bool> ReadRelationshipVisibility(IUserProfileEntity userProfileEntity, string userHandle, string queryingUserHandle, string appHandle);
    }
}
