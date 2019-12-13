// <copyright file="IUsersManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Principal;

    /// <summary>
    /// Users manager interface
    /// </summary>
    public interface IUsersManager
    {
        /// <summary>
        /// Create user linked account and profile
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="bio">User bio</param>
        /// <param name="photoHandle">Photo handle</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="requestId">Request id</param>
        /// <returns>Create user task</returns>
        Task CreateUserAndUserProfile(
            ProcessType processType,
            string userHandle,
            IdentityProviderType identityProviderType,
            string accountId,
            string appHandle,
            string firstName,
            string lastName,
            string bio,
            string photoHandle,
            DateTime createdTime,
            string requestId);

        /// <summary>
        /// Create user profile
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="bio">User bio</param>
        /// <param name="photoHandle">Photo handle</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="requestId">Request id</param>
        /// <returns>Create user profile task</returns>
        Task CreateUserProfile(
            ProcessType processType,
            string userHandle,
            string appHandle,
            string firstName,
            string lastName,
            string bio,
            string photoHandle,
            DateTime createdTime,
            string requestId);

        /// <summary>
        /// Update user info
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="bio">User bio</param>
        /// <param name="lastUpdatedTime">Last updated bio</param>
        /// <param name="userProfileEntity">User profile entity</param>
        /// <returns>Update user info task</returns>
        Task UpdateUserInfo(
            ProcessType processType,
            string userHandle,
            string appHandle,
            string firstName,
            string lastName,
            string bio,
            DateTime lastUpdatedTime,
            IUserProfileEntity userProfileEntity);

        /// <summary>
        /// Update user photo
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="photoHandle">Photo handle</param>
        /// <param name="lastUpdatedTime">Last updated bio</param>
        /// <param name="userProfileEntity">User profile entity</param>
        /// <returns>Update user photo task</returns>
        Task UpdateUserPhoto(
            string userHandle,
            string appHandle,
            string photoHandle,
            DateTime lastUpdatedTime,
            IUserProfileEntity userProfileEntity);

        /// <summary>
        /// Update user visibility
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="visibility">User visibility status</param>
        /// <param name="lastUpdatedTime">Last updated bio</param>
        /// <param name="userProfileEntity">User profile entity</param>
        /// <returns>Update user visibility task</returns>
        Task UpdateUserVisibility(
            string userHandle,
            string appHandle,
            UserVisibilityStatus visibility,
            DateTime lastUpdatedTime,
            IUserProfileEntity userProfileEntity);

        /// <summary>
        /// Update user review status
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="userProfileEntity">User profile entity</param>
        /// <returns>Update user review status task</returns>
        Task UpdateUserReviewStatus(
            string userHandle,
            string appHandle,
            ReviewStatus reviewStatus,
            IUserProfileEntity userProfileEntity);

        /// <summary>
        /// Remove user profile
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Remove user task</returns>
        Task DeleteUserProfile(
            ProcessType processType,
            string userHandle,
            string appHandle);

        /// <summary>
        /// Link new identity account
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userPrincipal">User principal</param>
        /// <returns>Link account task</returns>
        Task CreateLinkedAccount(
            ProcessType processType,
            UserPrincipal userPrincipal);

        /// <summary>
        /// Unlink identity account
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id</param>
        /// <returns>Link account task</returns>
        Task DeleteLinkedAccount(
            ProcessType processType,
            string userHandle,
            IdentityProviderType identityProviderType,
            string accountId);

        /// <summary>
        /// Read user profile
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User profile entity</returns>
        Task<IUserProfileEntity> ReadUserProfile(string userHandle, string appHandle);

        /// <summary>
        /// Read user by linked account
        /// </summary>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id</param>
        /// <returns>User lookup entity</returns>
        Task<IUserLookupEntity> ReadUserByLinkedAccount(IdentityProviderType identityProviderType, string accountId);

        /// <summary>
        /// Read user linked account
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <returns>User linked account entity</returns>
        Task<ILinkedAccountFeedEntity> ReadLinkedAccount(string userHandle, IdentityProviderType identityProviderType);

        /// <summary>
        /// Read user linked accounts
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <returns>List of user linked account entities</returns>
        Task<IList<ILinkedAccountFeedEntity>> ReadLinkedAccounts(string userHandle);

        /// <summary>
        /// Read user apps
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <returns>List of app feed entities</returns>
        Task<IList<IAppFeedEntity>> ReadUserApps(string userHandle);

        /// <summary>
        /// Read users the current user is following in another app but not in the current app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">Current app handle</param>
        /// <param name="queryAppHandle">Query app handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user feed entities</returns>
        Task<IList<IUserFeedEntity>> ReadFollowingDifference(string userHandle, string appHandle, string queryAppHandle, string cursor, int limit);
    }
}
