// <copyright file="IUsersStore.cs" company="Microsoft">
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
    /// Users store interface
    /// </summary>
    public interface IUsersStore
    {
        /// <summary>
        /// Insert user
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="bio">User bio</param>
        /// <param name="photoHandle">Photo handle</param>
        /// <param name="visibility">User visibility</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="requestId">Request Id</param>
        /// <returns>Insert user task</returns>
        Task InsertUser(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            IdentityProviderType identityProviderType,
            string accountId,
            string appHandle,
            string firstName,
            string lastName,
            string bio,
            string photoHandle,
            UserVisibilityStatus visibility,
            DateTime createdTime,
            string requestId);

        /// <summary>
        /// Insert user profile
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="bio">User bio</param>
        /// <param name="photoHandle">Photo handle</param>
        /// <param name="visibility">User visibility</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="requestId">Request Id</param>
        /// <returns>Insert user profile task</returns>
        Task InsertUserProfile(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string firstName,
            string lastName,
            string bio,
            string photoHandle,
            UserVisibilityStatus visibility,
            DateTime createdTime,
            string requestId);

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userProfileEntity">User profile entity</param>
        /// <returns>Update user task</returns>
        Task UpdateUserProfile(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            IUserProfileEntity userProfileEntity);

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete user task</returns>
        Task DeleteUserProfile(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle);

        /// <summary>
        /// Query user
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User profile entity</returns>
        Task<IUserProfileEntity> QueryUserProfile(string userHandle, string appHandle);

        /// <summary>
        /// Insert linked account
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id from identity provider</param>
        /// <returns>Insert linked account task</returns>
        Task InsertLinkedAccount(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            IdentityProviderType identityProviderType,
            string accountId);

        /// <summary>
        /// Delete linked account
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <returns>Delete linked account task</returns>
        Task DeleteLinkedAccount(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            IdentityProviderType identityProviderType);

        /// <summary>
        /// Query linked account
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <returns>Linked account entity</returns>
        Task<ILinkedAccountFeedEntity> QueryLinkedAccount(
            string userHandle,
            IdentityProviderType identityProviderType);

        /// <summary>
        /// Query all linked accounts
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <returns>Linked account feed entities</returns>
        Task<IList<ILinkedAccountFeedEntity>> QueryLinkedAccounts(string userHandle);

        /// <summary>
        /// Query user apps
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <returns>Apps used by user</returns>
        Task<IList<IAppFeedEntity>> QueryUserApps(string userHandle);

        /// <summary>
        /// Insert linked account index for user
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id from identity provider</param>
        /// <returns>Insert linked account lookup task</returns>
        Task InsertLinkedAccountIndex(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            IdentityProviderType identityProviderType,
            string accountId);

        /// <summary>
        /// Delete linked account index
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id</param>
        /// <returns>Delete linked account index task</returns>
        Task DeleteLinkedAccountIndex(
            StorageConsistencyMode storageConsistencyMode,
            IdentityProviderType identityProviderType,
            string accountId);

        /// <summary>
        /// Query linked account index
        /// </summary>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id</param>
        /// <returns>User lookup entity</returns>
        Task<IUserLookupEntity> QueryLinkedAccountIndex(
            IdentityProviderType identityProviderType,
            string accountId);

        /// <summary>
        /// Insert popular user
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="score">Entity value</param>
        /// <returns>Insert popular user task</returns>
        Task InsertPopularUser(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            long score);

        /// <summary>
        /// Delete popular user
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete popular user topic task</returns>
        Task DeletePopularUser(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle);

        /// <summary>
        /// Query popular users
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>User feed entities</returns>
        Task<IList<IUserFeedEntity>> QueryPopularUsers(string appHandle, int cursor, int limit);
    }
}
