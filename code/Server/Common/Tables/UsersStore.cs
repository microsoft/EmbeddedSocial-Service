// <copyright file="UsersStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.CTStore;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Managers;

    /// <summary>
    /// Users store class
    /// </summary>
    public class UsersStore : IUsersStore
    {
        /// <summary>
        /// CTStore manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached table store manager</param>
        public UsersStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

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
        public async Task InsertUser(
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
            string requestId)
        {
            UserProfileEntity userProfileEntity = new UserProfileEntity()
            {
                FirstName = firstName,
                LastName = lastName,
                Bio = bio,
                CreatedTime = createdTime,
                LastUpdatedTime = createdTime,
                Visibility = visibility,
                PhotoHandle = photoHandle,
                ReviewStatus = ReviewStatus.Active,
                RequestId = requestId
            };

            AppFeedEntity appFeedEntity = new AppFeedEntity()
            {
                AppHandle = appHandle
            };

            LinkedAccountFeedEntity linkedAccountFeedEntity = new LinkedAccountFeedEntity()
            {
                IdentityProviderType = identityProviderType,
                AccountId = accountId
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Users);
            ObjectTable profilesTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserProfilesObject) as ObjectTable;
            FeedTable appsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserAppsFeed) as FeedTable;
            FeedTable accountsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserLinkedAccountsFeed) as FeedTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.Insert(profilesTable, userHandle, appHandle, userProfileEntity));
            transaction.Add(Operation.Insert(appsTable, userHandle, this.tableStoreManager.DefaultFeedKey, appHandle, appFeedEntity));
            transaction.Add(Operation.Insert(accountsTable, userHandle, this.tableStoreManager.DefaultFeedKey, identityProviderType.ToString(), linkedAccountFeedEntity));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Insert user
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
        /// <returns>Insert user task</returns>
        public async Task InsertUserProfile(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            string firstName,
            string lastName,
            string bio,
            string photoHandle,
            UserVisibilityStatus visibility,
            DateTime createdTime,
            string requestId)
        {
            UserProfileEntity userProfileEntity = new UserProfileEntity()
            {
                FirstName = firstName,
                LastName = lastName,
                Bio = bio,
                CreatedTime = createdTime,
                LastUpdatedTime = createdTime,
                Visibility = visibility,
                PhotoHandle = photoHandle,
                ReviewStatus = ReviewStatus.Active,
                RequestId = requestId
            };

            AppFeedEntity appFeedEntity = new AppFeedEntity()
            {
                AppHandle = appHandle
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Users);
            ObjectTable profilesTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserProfilesObject) as ObjectTable;
            FeedTable appsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserAppsFeed) as FeedTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.Insert(profilesTable, userHandle, appHandle, userProfileEntity));
            transaction.Add(Operation.Insert(appsTable, userHandle, this.tableStoreManager.DefaultFeedKey, appHandle, appFeedEntity));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userProfileEntity">User profile entity</param>
        /// <returns>Update user task</returns>
        public async Task UpdateUserProfile(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            IUserProfileEntity userProfileEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Users);
            ObjectTable profilesTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserProfilesObject) as ObjectTable;
            Operation operation = Operation.Replace(profilesTable, userHandle, appHandle, userProfileEntity as UserProfileEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete user task</returns>
        public async Task DeleteUserProfile(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Users);
            ObjectTable profilesTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserProfilesObject) as ObjectTable;
            FeedTable appsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserAppsFeed) as FeedTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.Delete(profilesTable, userHandle, appHandle));
            transaction.Add(Operation.Delete(appsTable, userHandle, this.tableStoreManager.DefaultFeedKey, appHandle));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query user
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>User profile entity</returns>
        public async Task<IUserProfileEntity> QueryUserProfile(string userHandle, string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Users);
            ObjectTable profilesTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserProfilesObject) as ObjectTable;
            UserProfileEntity userProfileEntity = await store.QueryObjectAsync<UserProfileEntity>(profilesTable, userHandle, appHandle);
            return userProfileEntity;
        }

        /// <summary>
        /// Insert linked account
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id from identity provider</param>
        /// <returns>Insert linked account task</returns>
        public async Task InsertLinkedAccount(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            IdentityProviderType identityProviderType,
            string accountId)
        {
            LinkedAccountFeedEntity linkedAccountFeedEntity = new LinkedAccountFeedEntity()
            {
                IdentityProviderType = identityProviderType,
                AccountId = accountId
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Users);
            FeedTable accountsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserLinkedAccountsFeed) as FeedTable;
            Operation operation = Operation.Insert(accountsTable, userHandle, this.tableStoreManager.DefaultFeedKey, identityProviderType.ToString(), linkedAccountFeedEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete linked account
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <returns>Delete linked account task</returns>
        public async Task DeleteLinkedAccount(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            IdentityProviderType identityProviderType)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Users);
            FeedTable accountsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserLinkedAccountsFeed) as FeedTable;
            Operation operation = Operation.Delete(accountsTable, userHandle, this.tableStoreManager.DefaultFeedKey, identityProviderType.ToString());
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query linked account
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <returns>Linked account entity</returns>
        public async Task<ILinkedAccountFeedEntity> QueryLinkedAccount(
            string userHandle,
            IdentityProviderType identityProviderType)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Users);
            FeedTable accountsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserLinkedAccountsFeed) as FeedTable;
            return await store.QueryFeedItemAsync<LinkedAccountFeedEntity>(accountsTable, userHandle, this.tableStoreManager.DefaultFeedKey, identityProviderType.ToString());
        }

        /// <summary>
        /// Query all linked accounts
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <returns>Linked account feed entities</returns>
        public async Task<IList<ILinkedAccountFeedEntity>> QueryLinkedAccounts(string userHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Users);
            FeedTable accountsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserLinkedAccountsFeed) as FeedTable;
            var result = await store.QueryFeedAsync<LinkedAccountFeedEntity>(accountsTable, userHandle, this.tableStoreManager.DefaultFeedKey, null, int.MaxValue);
            return result.ToList<ILinkedAccountFeedEntity>();
        }

        /// <summary>
        /// Insert linked account index for user
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id from identity provider</param>
        /// <returns>Insert linked account lookup task</returns>
        public async Task InsertLinkedAccountIndex(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            IdentityProviderType identityProviderType,
            string accountId)
        {
            UserLookupEntity userLookupEntity = new UserLookupEntity()
            {
                UserHandle = userHandle
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.LinkedAccounts);
            ObjectTable indexTable = this.tableStoreManager.GetTable(ContainerIdentifier.LinkedAccounts, TableIdentifier.LinkedAccountsIndex) as ObjectTable;

            string key = this.GetLinkedAccountKey(identityProviderType, accountId);
            Operation operation = Operation.Insert(indexTable, key, this.tableStoreManager.DefaultObjectKey, userLookupEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete linked account index
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id</param>
        /// <returns>Delete linked account index task</returns>
        public async Task DeleteLinkedAccountIndex(
            StorageConsistencyMode storageConsistencyMode,
            IdentityProviderType identityProviderType,
            string accountId)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.LinkedAccounts);
            ObjectTable indexTable = this.tableStoreManager.GetTable(ContainerIdentifier.LinkedAccounts, TableIdentifier.LinkedAccountsIndex) as ObjectTable;
            string key = this.GetLinkedAccountKey(identityProviderType, accountId);
            Operation operation = Operation.Delete(indexTable, key, this.tableStoreManager.DefaultObjectKey);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query linked account index
        /// </summary>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id</param>
        /// <returns>User lookup entity</returns>
        public async Task<IUserLookupEntity> QueryLinkedAccountIndex(
            IdentityProviderType identityProviderType,
            string accountId)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.LinkedAccounts);
            ObjectTable indexTable = this.tableStoreManager.GetTable(ContainerIdentifier.LinkedAccounts, TableIdentifier.LinkedAccountsIndex) as ObjectTable;
            string key = this.GetLinkedAccountKey(identityProviderType, accountId);
            return await store.QueryObjectAsync<UserLookupEntity>(indexTable, key, this.tableStoreManager.DefaultObjectKey);
        }

        /// <summary>
        /// Query user apps
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <returns>Apps used by user</returns>
        public async Task<IList<IAppFeedEntity>> QueryUserApps(string userHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Users);
            FeedTable indexTable = this.tableStoreManager.GetTable(ContainerIdentifier.Users, TableIdentifier.UserAppsFeed) as FeedTable;
            var result = await store.QueryFeedAsync<AppFeedEntity>(indexTable, userHandle, this.tableStoreManager.DefaultFeedKey, null, int.MaxValue);
            return result.ToList<IAppFeedEntity>();
        }

        /// <summary>
        /// Insert popular user
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="score">Entity value</param>
        /// <returns>Insert popular user task</returns>
        public async Task InsertPopularUser(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle,
            long score)
        {
            UserRankFeedEntity userRankFeedEntity = new UserRankFeedEntity()
            {
                AppHandle = appHandle,
                UserHandle = userHandle
            };
            var serializedEntity = StoreSerializers.MinimalUserRankFeedEntitySerialize(userRankFeedEntity);

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PopularUsers);
            RankFeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PopularUsers, TableIdentifier.PopularUsersFeed) as RankFeedTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.InsertOrReplace(table, ContainerIdentifier.PopularUsers.ToString(), appHandle, serializedEntity, score));
            transaction.Add(Operation.InsertOrReplace(table, ContainerIdentifier.PopularUsers.ToString(), MasterApp.AppHandle, serializedEntity, score));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete popular user
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete popular user topic task</returns>
        public async Task DeletePopularUser(
            StorageConsistencyMode storageConsistencyMode,
            string userHandle,
            string appHandle)
        {
            UserRankFeedEntity userRankFeedEntity = new UserRankFeedEntity()
            {
                AppHandle = appHandle,
                UserHandle = userHandle
            };
            var serializedEntity = StoreSerializers.MinimalUserRankFeedEntitySerialize(userRankFeedEntity);

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PopularUsers);
            RankFeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PopularUsers, TableIdentifier.PopularUsersFeed) as RankFeedTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.DeleteIfExists(table, ContainerIdentifier.PopularUsers.ToString(), appHandle, serializedEntity));
            transaction.Add(Operation.DeleteIfExists(table, ContainerIdentifier.PopularUsers.ToString(), MasterApp.AppHandle, serializedEntity));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query popular users
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="cursor">Read cursor</param>
        /// <param name="limit">Number of items to return</param>
        /// <returns>User feed entities</returns>
        public async Task<IList<IUserFeedEntity>> QueryPopularUsers(string appHandle, int cursor, int limit)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.PopularUsers);
            RankFeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.PopularUsers, TableIdentifier.PopularUsersFeed) as RankFeedTable;
            var result = await store.QueryRankFeedAsync(table, ContainerIdentifier.PopularUsers.ToString(), appHandle, cursor.ToString(), limit);
            var convertedResults = result.Select(item => StoreSerializers.MinimalUserRankFeedEntityDeserialize(item.ItemKey));
            return convertedResults.ToList<IUserFeedEntity>();
        }

        /// <summary>
        /// Get linked account key for table
        /// </summary>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account ide</param>
        /// <returns>Linked account key</returns>
        private string GetLinkedAccountKey(IdentityProviderType identityProviderType, string accountId)
        {
            return string.Join("+", identityProviderType.ToString(), accountId);
        }
    }
}
