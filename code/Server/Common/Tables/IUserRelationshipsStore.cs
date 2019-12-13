// <copyright file="IUserRelationshipsStore.cs" company="Microsoft">
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
    /// User relationships store interface
    /// </summary>
    public interface IUserRelationshipsStore
    {
        /// <summary>
        /// Update follower relationship.
        /// Follower user : someone who follows me.
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="relationshipHandle">Relationship handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userRelationshipStatus">User relationship status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="readUserRelationshipLookupEntity">Read user relationship lookup entity</param>
        /// <returns>Update follower relationship task</returns>
        Task UpdateFollowerRelationship(
            StorageConsistencyMode storageConsistencyMode,
            string relationshipHandle,
            string userHandle,
            string relationshipUserHandle,
            string appHandle,
            UserRelationshipStatus userRelationshipStatus,
            DateTime lastUpdatedTime,
            IUserRelationshipLookupEntity readUserRelationshipLookupEntity);

        /// <summary>
        /// Update following relationship.
        /// Following user : someone who i am following.
        /// </summary>
        /// <param name="storageConsistencyMode">Consistency mode</param>
        /// <param name="relationshipHandle">Relationship handle</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipUserHandle">Relationship user handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userRelationshipStatus">User relationship status</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="readUserRelationshipLookupEntity">Read user relationship lookup entity</param>
        /// <returns>Update following relationship task</returns>
        Task UpdateFollowingRelationship(
            StorageConsistencyMode storageConsistencyMode,
            string relationshipHandle,
            string userHandle,
            string relationshipUserHandle,
            string appHandle,
            UserRelationshipStatus userRelationshipStatus,
            DateTime lastUpdatedTime,
            IUserRelationshipLookupEntity readUserRelationshipLookupEntity);

        /// <summary>
        /// Query follower relationship.
        /// Follower user : someone who follows me.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipUserHandle">Relationship handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User relationship lookup entity</returns>
        Task<IUserRelationshipLookupEntity> QueryFollowerRelationship(
            string userHandle,
            string relationshipUserHandle,
            string appHandle);

        /// <summary>
        /// Query following relationship.
        /// Following user : someone who i am following.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="relationshipUserHandle">Relationship handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User relationship lookup entity</returns>
        Task<IUserRelationshipLookupEntity> QueryFollowingRelationship(
            string userHandle,
            string relationshipUserHandle,
            string appHandle);

        /// <summary>
        /// Query followers for a user in an app.
        /// Follower users : people who follow me.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        Task<IList<IUserRelationshipFeedEntity>> QueryFollowers(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Query count of followers for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Followers count for a user handle and app handle</returns>
        Task<long?> QueryFollowersCount(string userHandle, string appHandle);

        /// <summary>
        /// Query following for a user in an app.
        /// Following users : people who i am following.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        Task<IList<IUserRelationshipFeedEntity>> QueryFollowing(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Query count of following for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Following count for a user handle and app handle</returns>
        Task<long?> QueryFollowingCount(string userHandle, string appHandle);

        /// <summary>
        /// Query pending users for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        Task<IList<IUserRelationshipFeedEntity>> QueryPendingUsers(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Query count of pending users for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Pending users count for a user handle and app handle</returns>
        Task<long?> QueryPendingUsersCount(string userHandle, string appHandle);

        /// <summary>
        /// Query blocked users for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user relationship feed entities</returns>
        Task<IList<IUserRelationshipFeedEntity>> QueryBlockedUsers(string userHandle, string appHandle, string cursor, int limit);

        /// <summary>
        /// Query count of blocked users for a user in an app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Blocked users count for a user handle and app handle</returns>
        Task<long?> QueryBlockedUsersCount(string userHandle, string appHandle);
    }
}
