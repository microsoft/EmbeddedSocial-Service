// <copyright file="UsersManager.cs" company="Microsoft">
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
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Users manager class
    /// </summary>
    public class UsersManager : IUsersManager
    {
        /// <summary>
        /// Users store
        /// </summary>
        private IUsersStore usersStore;

        /// <summary>
        /// Push notifications manager
        /// </summary>
        private IPushNotificationsManager pushNotificationsManager;

        /// <summary>
        /// Popular users manager
        /// </summary>
        private IPopularUsersManager popularUsersManager;

        /// <summary>
        /// Search queue
        /// </summary>
        private ISearchQueue searchQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersManager"/> class
        /// </summary>
        /// <param name="usersStore">Users store</param>
        /// <param name="pushNotificationsManager">Push notifications manager</param>
        /// <param name="popularUsersManager">Popular users manager</param>
        /// <param name="searchQueue">Search queue</param>
        public UsersManager(IUsersStore usersStore, IPushNotificationsManager pushNotificationsManager, IPopularUsersManager popularUsersManager, ISearchQueue searchQueue)
        {
            this.usersStore = usersStore;
            this.popularUsersManager = popularUsersManager;
            this.pushNotificationsManager = pushNotificationsManager;
            this.searchQueue = searchQueue;
        }

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
        public async Task CreateUserAndUserProfile(
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
            string requestId)
        {
            await this.usersStore.InsertUser(
                StorageConsistencyMode.Strong,
                userHandle,
                identityProviderType,
                accountId,
                appHandle,
                firstName,
                lastName,
                bio,
                photoHandle,
                UserVisibilityStatus.Public,
                createdTime,
                requestId);

            await this.usersStore.InsertLinkedAccountIndex(
                StorageConsistencyMode.Strong,
                userHandle,
                identityProviderType,
                accountId);

            await this.searchQueue.SendSearchIndexUserMessage(userHandle, appHandle, createdTime);
        }

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
        public async Task CreateUserProfile(
            ProcessType processType,
            string userHandle,
            string appHandle,
            string firstName,
            string lastName,
            string bio,
            string photoHandle,
            DateTime createdTime,
            string requestId)
        {
            await this.usersStore.InsertUserProfile(
                StorageConsistencyMode.Strong,
                userHandle,
                appHandle,
                firstName,
                lastName,
                bio,
                photoHandle,
                UserVisibilityStatus.Public,
                createdTime,
                requestId);

            await this.searchQueue.SendSearchIndexUserMessage(userHandle, appHandle, createdTime);
        }

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
        public async Task UpdateUserInfo(
            ProcessType processType,
            string userHandle,
            string appHandle,
            string firstName,
            string lastName,
            string bio,
            DateTime lastUpdatedTime,
            IUserProfileEntity userProfileEntity)
        {
            userProfileEntity.FirstName = firstName;
            userProfileEntity.LastName = lastName;
            userProfileEntity.Bio = bio;
            userProfileEntity.LastUpdatedTime = lastUpdatedTime;

            await this.usersStore.UpdateUserProfile(StorageConsistencyMode.Strong, userHandle, appHandle, userProfileEntity);
            await this.searchQueue.SendSearchIndexUserMessage(userHandle, appHandle, lastUpdatedTime);
        }

        /// <summary>
        /// Update user photo
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="photoHandle">Photo handle</param>
        /// <param name="lastUpdatedTime">Last updated bio</param>
        /// <param name="userProfileEntity">User profile entity</param>
        /// <returns>Update user photo task</returns>
        public async Task UpdateUserPhoto(
            string userHandle,
            string appHandle,
            string photoHandle,
            DateTime lastUpdatedTime,
            IUserProfileEntity userProfileEntity)
        {
            userProfileEntity.PhotoHandle = photoHandle;
            userProfileEntity.LastUpdatedTime = lastUpdatedTime;

            await this.usersStore.UpdateUserProfile(StorageConsistencyMode.Strong, userHandle, appHandle, userProfileEntity);
        }

        /// <summary>
        /// Update user visibility
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="visibility">User visibility status</param>
        /// <param name="lastUpdatedTime">Last updated bio</param>
        /// <param name="userProfileEntity">User profile entity</param>
        /// <returns>Update user visibility task</returns>
        public async Task UpdateUserVisibility(
            string userHandle,
            string appHandle,
            UserVisibilityStatus visibility,
            DateTime lastUpdatedTime,
            IUserProfileEntity userProfileEntity)
        {
            userProfileEntity.Visibility = visibility;
            userProfileEntity.LastUpdatedTime = lastUpdatedTime;

            await this.usersStore.UpdateUserProfile(StorageConsistencyMode.Strong, userHandle, appHandle, userProfileEntity);
        }

        /// <summary>
        /// Update user review status
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="reviewStatus">Review status</param>
        /// <param name="userProfileEntity">User profile entity</param>
        /// <returns>Update user review status task</returns>
        public async Task UpdateUserReviewStatus(
            string userHandle,
            string appHandle,
            ReviewStatus reviewStatus,
            IUserProfileEntity userProfileEntity)
        {
            // note that we do not update the last updated time when we modify the review status
            userProfileEntity.ReviewStatus = reviewStatus;
            await this.usersStore.UpdateUserProfile(StorageConsistencyMode.Strong, userHandle, appHandle, userProfileEntity);
        }

        /// <summary>
        /// Delete user profile
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete user task</returns>
        public async Task DeleteUserProfile(
            ProcessType processType,
            string userHandle,
            string appHandle)
        {
            await this.usersStore.DeleteUserProfile(StorageConsistencyMode.Strong, userHandle, appHandle);
            var appFeedEntities = await this.ReadUserApps(userHandle);
            if (appFeedEntities.Count == 0)
            {
                var linkedAccountFeedEntities = await this.ReadLinkedAccounts(userHandle);
                foreach (var linkedAccountFeedEntity in linkedAccountFeedEntities)
                {
                    await this.DeleteLinkedAccount(processType, userHandle, linkedAccountFeedEntity.IdentityProviderType, linkedAccountFeedEntity.AccountId);
                }
            }

            await this.searchQueue.SendSearchRemoveUserMessage(userHandle, appHandle);
            await this.popularUsersManager.DeletePopularUser(processType, userHandle, appHandle);
            await this.pushNotificationsManager.DeleteUserRegistrations(processType, userHandle, appHandle);
        }

        /// <summary>
        /// Link new identity account
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userPrincipal">User principal</param>
        /// <returns>Create linked account task</returns>
        public async Task CreateLinkedAccount(
            ProcessType processType,
            UserPrincipal userPrincipal)
        {
            if (userPrincipal == null)
            {
                throw new ArgumentNullException("User principal is null in CreateLinkedAccount");
            }

            await this.usersStore.InsertLinkedAccount(
                StorageConsistencyMode.Strong,
                userPrincipal.UserHandle,
                userPrincipal.IdentityProvider,
                userPrincipal.IdentityProviderAccountId);

            await this.usersStore.InsertLinkedAccountIndex(
                StorageConsistencyMode.Strong,
                userPrincipal.UserHandle,
                userPrincipal.IdentityProvider,
                userPrincipal.IdentityProviderAccountId);
        }

        /// <summary>
        /// Unlink identity account
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id</param>
        /// <returns>Delete linked account task</returns>
        public async Task DeleteLinkedAccount(
            ProcessType processType,
            string userHandle,
            IdentityProviderType identityProviderType,
            string accountId)
        {
            await this.usersStore.DeleteLinkedAccountIndex(
                StorageConsistencyMode.Strong,
                identityProviderType,
                accountId);

            await this.usersStore.DeleteLinkedAccount(
                StorageConsistencyMode.Strong,
                userHandle,
                identityProviderType);
        }

        /// <summary>
        /// Read user profile
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User profile entity</returns>
        public async Task<IUserProfileEntity> ReadUserProfile(string userHandle, string appHandle)
        {
            return await this.usersStore.QueryUserProfile(userHandle, appHandle);
        }

        /// <summary>
        /// Read user linked account
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <returns>User linked account entity</returns>
        public async Task<ILinkedAccountFeedEntity> ReadLinkedAccount(string userHandle, IdentityProviderType identityProviderType)
        {
            return await this.usersStore.QueryLinkedAccount(userHandle, identityProviderType);
        }

        /// <summary>
        /// Read user linked accounts
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <returns>List of user linked account entities</returns>
        public async Task<IList<ILinkedAccountFeedEntity>> ReadLinkedAccounts(string userHandle)
        {
            return await this.usersStore.QueryLinkedAccounts(userHandle);
        }

        /// <summary>
        /// Read user by linked account
        /// </summary>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id</param>
        /// <returns>User lookup entity</returns>
        public async Task<IUserLookupEntity> ReadUserByLinkedAccount(IdentityProviderType identityProviderType, string accountId)
        {
            return await this.usersStore.QueryLinkedAccountIndex(identityProviderType, accountId);
        }

        /// <summary>
        /// Read user apps
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <returns>List of app feed entities</returns>
        public async Task<IList<IAppFeedEntity>> ReadUserApps(string userHandle)
        {
            return await this.usersStore.QueryUserApps(userHandle);
        }

        /// <summary>
        /// Read users the current user is following in another app but not in the current app
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">Current app handle</param>
        /// <param name="queryAppHandle">Query app handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>List of user feed entities</returns>
        public async Task<IList<IUserFeedEntity>> ReadFollowingDifference(
            string userHandle,
            string appHandle,
            string queryAppHandle,
            string cursor,
            int limit)
        {
            await Task.Delay(0);
            return null;
        }
    }
}
