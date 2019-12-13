// <copyright file="AppsStore.cs" company="Microsoft">
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
    /// Apps store class
    /// </summary>
    public class AppsStore : IAppsStore
    {
        /// <summary>
        /// cached table storage manager
        /// </summary>
        private readonly ICTStoreManager tableStoreManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppsStore"/> class
        /// </summary>
        /// <param name="tableStoreManager">cached tablet store manager</param>
        public AppsStore(ICTStoreManager tableStoreManager)
        {
            this.tableStoreManager = tableStoreManager;
        }

        /// <summary>
        /// Insert app
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="developerId">Developer id</param>
        /// <param name="name">App name</param>
        /// <param name="iconHandle">Icon handle</param>
        /// <param name="platformType">Platform type</param>
        /// <param name="deepLink">Deep link</param>
        /// <param name="storeLink">Store link</param>
        /// <param name="createdTime">Created time</param>
        /// <param name="disableHandleValidation">whether to disable validation of app-provided handles</param>
        /// <returns>Create app task</returns>
        public async Task InsertApp(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string developerId,
            string name,
            string iconHandle,
            PlatformType platformType,
            string deepLink,
            string storeLink,
            DateTime createdTime,
            bool disableHandleValidation)
        {
            AppProfileEntity appProfileEntity = new AppProfileEntity()
            {
                DeveloperId = developerId,
                Name = name,
                IconHandle = iconHandle,
                PlatformType = platformType,
                DeepLink = deepLink,
                StoreLink = storeLink,
                CreatedTime = createdTime,
                LastUpdatedTime = createdTime,
                AppStatus = AppStatus.Active,
                DisableHandleValidation = disableHandleValidation,
            };

            IdentityProviderCredentialsEntity facebookCredentialsEntity = new IdentityProviderCredentialsEntity();
            IdentityProviderCredentialsEntity microsoftCredentialsEntity = new IdentityProviderCredentialsEntity();
            IdentityProviderCredentialsEntity googleCredentialsEntity = new IdentityProviderCredentialsEntity();
            IdentityProviderCredentialsEntity twitterCredentialsEntity = new IdentityProviderCredentialsEntity();
            IdentityProviderCredentialsEntity aadCredentialsEntity = new IdentityProviderCredentialsEntity();

            ValidationConfigurationEntity validationConfigurationEntity = new ValidationConfigurationEntity()
            {
                Enabled = false
            };

            PushNotificationsConfigurationEntity windowsPushNotificationsConfigurationEntity = new PushNotificationsConfigurationEntity()
            {
                Enabled = false
            };

            PushNotificationsConfigurationEntity androidPushNotificationsConfigurationEntity = new PushNotificationsConfigurationEntity()
            {
                Enabled = false
            };

            PushNotificationsConfigurationEntity iosPushNotificationsConfigurationEntity = new PushNotificationsConfigurationEntity()
            {
                Enabled = false
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Apps);
            ObjectTable profilesTable = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppProfilesObject) as ObjectTable;
            ObjectTable credentialsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppIdentityProviderCredentialsObject) as ObjectTable;
            ObjectTable validationConfigsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppValidationConfigurationsObject) as ObjectTable;
            ObjectTable pushConfigsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppPushNotificationsConfigurationsObject) as ObjectTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.Insert(profilesTable, appHandle, appHandle, appProfileEntity));
            transaction.Add(Operation.Insert(credentialsTable, appHandle, IdentityProviderType.Facebook.ToString(), facebookCredentialsEntity));
            transaction.Add(Operation.Insert(credentialsTable, appHandle, IdentityProviderType.Microsoft.ToString(), microsoftCredentialsEntity));
            transaction.Add(Operation.Insert(credentialsTable, appHandle, IdentityProviderType.Google.ToString(), googleCredentialsEntity));
            transaction.Add(Operation.Insert(credentialsTable, appHandle, IdentityProviderType.Twitter.ToString(), twitterCredentialsEntity));
            transaction.Add(Operation.Insert(credentialsTable, appHandle, IdentityProviderType.AADS2S.ToString(), aadCredentialsEntity));
            transaction.Add(Operation.Insert(validationConfigsTable, appHandle, appHandle, validationConfigurationEntity));
            transaction.Add(Operation.Insert(pushConfigsTable, appHandle, PlatformType.Windows.ToString(), windowsPushNotificationsConfigurationEntity));
            transaction.Add(Operation.Insert(pushConfigsTable, appHandle, PlatformType.Android.ToString(), androidPushNotificationsConfigurationEntity));
            transaction.Add(Operation.Insert(pushConfigsTable, appHandle, PlatformType.IOS.ToString(), iosPushNotificationsConfigurationEntity));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete app
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete app task</returns>
        public async Task DeleteApp(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Apps);
            ObjectTable profilesTable = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppProfilesObject) as ObjectTable;
            ObjectTable credentialsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppIdentityProviderCredentialsObject) as ObjectTable;
            ObjectTable validationConfigsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppValidationConfigurationsObject) as ObjectTable;
            ObjectTable pushConfigsTable = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppPushNotificationsConfigurationsObject) as ObjectTable;
            Transaction transaction = new Transaction();
            transaction.Add(Operation.Delete(profilesTable, appHandle, appHandle));
            transaction.Add(Operation.Delete(credentialsTable, appHandle, IdentityProviderType.Facebook.ToString()));
            transaction.Add(Operation.Delete(credentialsTable, appHandle, IdentityProviderType.Microsoft.ToString()));
            transaction.Add(Operation.Delete(credentialsTable, appHandle, IdentityProviderType.Google.ToString()));
            transaction.Add(Operation.Delete(credentialsTable, appHandle, IdentityProviderType.Twitter.ToString()));
            transaction.Add(Operation.Delete(credentialsTable, appHandle, IdentityProviderType.AADS2S.ToString()));
            transaction.Add(Operation.Delete(validationConfigsTable, appHandle, appHandle));
            transaction.Add(Operation.Delete(pushConfigsTable, appHandle, PlatformType.Windows.ToString()));
            transaction.Add(Operation.Delete(pushConfigsTable, appHandle, PlatformType.Android.ToString()));
            transaction.Add(Operation.Delete(pushConfigsTable, appHandle, PlatformType.IOS.ToString()));
            await store.ExecuteTransactionAsync(transaction, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Update app profile
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="appProfileEntity">App profile entity</param>
        /// <returns>Update app profile task</returns>
        public async Task UpdateAppProfile(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            IAppProfileEntity appProfileEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Apps);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppProfilesObject) as ObjectTable;
            Operation operation = Operation.Replace(table, appHandle, appHandle, appProfileEntity as AppProfileEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query app profile
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>App profile entity</returns>
        public async Task<IAppProfileEntity> QueryAppProfile(string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Apps);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppProfilesObject) as ObjectTable;
            AppProfileEntity appProfileEntity = await store.QueryObjectAsync<AppProfileEntity>(table, appHandle, appHandle);
            return appProfileEntity;
        }

        /// <summary>
        /// Insert developer app
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="developerId">Developer id</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Insert developer app task</returns>
        public async Task InsertDeveloperApp(
            StorageConsistencyMode storageConsistencyMode,
            string developerId,
            string appHandle)
        {
            AppFeedEntity appFeedEntity = new AppFeedEntity()
            {
                AppHandle = appHandle
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.DeveloperApps);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.DeveloperApps, TableIdentifier.DeveloperAppsFeed) as FeedTable;
            Operation operation = Operation.Insert(table, developerId, this.tableStoreManager.DefaultFeedKey, appHandle, appFeedEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete developer app
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="developerId">Developer id</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete developer app task</returns>
        public async Task DeleteDeveloperApp(
            StorageConsistencyMode storageConsistencyMode,
            string developerId,
            string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.DeveloperApps);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.DeveloperApps, TableIdentifier.DeveloperAppsFeed) as FeedTable;
            Operation operation = Operation.Delete(table, developerId, this.tableStoreManager.DefaultFeedKey, appHandle);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query developer apps
        /// </summary>
        /// <param name="developerId">Developer id</param>
        /// <returns>App feed entities</returns>
        public async Task<IList<IAppFeedEntity>> QueryDeveloperApps(string developerId)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.DeveloperApps);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.DeveloperApps, TableIdentifier.DeveloperAppsFeed) as FeedTable;
            var result = await store.QueryFeedAsync<AppFeedEntity>(table, developerId, this.tableStoreManager.DefaultFeedKey, null, int.MaxValue);
            return result.ToList<IAppFeedEntity>();
        }

        /// <summary>
        /// Insert user into table of app adminstrators
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Insert app administrator task</returns>
        public async Task InsertAdminUser(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string userHandle)
        {
            var entity = new AppAdminEntity
            {
                IsAdmin = true
            };
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AppAdmins);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.AppAdmins, TableIdentifier.AppAdminsObject) as ObjectTable;
            var objectKey = this.GetAppAdminObjectKey(appHandle, userHandle);
            Operation operation = Operation.Insert(table, appHandle, objectKey, entity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete user from table of app administrators
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Delete app administrator task</returns>
        public async Task DeleteAdminUser(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string userHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AppAdmins);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.AppAdmins, TableIdentifier.AppAdminsObject) as ObjectTable;
            var objectKey = this.GetAppAdminObjectKey(appHandle, userHandle);
            Operation operation = Operation.Delete(table, appHandle, objectKey);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query to determine if a user is an app administrator
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>App administrator entity</returns>
        public async Task<IAppAdminEntity> QueryAdminUser(string appHandle, string userHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AppAdmins);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.AppAdmins, TableIdentifier.AppAdminsObject) as ObjectTable;
            var objectKey = this.GetAppAdminObjectKey(appHandle, userHandle);
            return await store.QueryObjectAsync<AppAdminEntity>(table, appHandle, objectKey);
        }

        /// <summary>
        /// Insert all app
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Insert all app task</returns>
        public async Task InsertAllApp(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle)
        {
            AppFeedEntity appFeedEntity = new AppFeedEntity()
            {
                AppHandle = appHandle
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AllApps);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.AllApps, TableIdentifier.AllAppsFeed) as FeedTable;
            Operation operation = Operation.Insert(table, ContainerIdentifier.AllApps.ToString(), this.tableStoreManager.DefaultFeedKey, appHandle, appFeedEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete all app
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete all app task</returns>
        public async Task DeleteAllApp(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AllApps);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.AllApps, TableIdentifier.AllAppsFeed) as FeedTable;
            Operation operation = Operation.Delete(table, ContainerIdentifier.AllApps.ToString(), this.tableStoreManager.DefaultFeedKey, appHandle);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query all apps
        /// </summary>
        /// <returns>App feed entities</returns>
        public async Task<IList<IAppFeedEntity>> QueryAllApps()
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AllApps);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.AllApps, TableIdentifier.AllAppsFeed) as FeedTable;
            var result = await store.QueryFeedAsync<AppFeedEntity>(table, ContainerIdentifier.AllApps.ToString(), this.tableStoreManager.DefaultFeedKey, null, int.MaxValue);
            return result.ToList<IAppFeedEntity>();
        }

        /// <summary>
        /// Update validation configuration
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="validationConfigurationEntity">Validation configuration entity</param>
        /// <returns>Update validation configuration task</returns>
        public async Task UpdateValidationConfiguration(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            IValidationConfigurationEntity validationConfigurationEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Apps);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppValidationConfigurationsObject) as ObjectTable;
            Operation operation = Operation.Replace(table, appHandle, appHandle, validationConfigurationEntity as ValidationConfigurationEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query validation configuration
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>Validation configuration entity</returns>
        public async Task<IValidationConfigurationEntity> QueryValidationConfiguration(string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Apps);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppValidationConfigurationsObject) as ObjectTable;
            ValidationConfigurationEntity validationConfigurationEntity = await store.QueryObjectAsync<ValidationConfigurationEntity>(table, appHandle, appHandle);
            return validationConfigurationEntity;
        }

        /// <summary>
        /// Update push notifications configuration
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="platformType">Platform type</param>
        /// <param name="pushNotificationsConfigurationEntity">Push notifications configuration entity</param>
        /// <returns>Update push notifications configuration task</returns>
        public async Task UpdatePushNotificationsConfiguration(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            PlatformType platformType,
            IPushNotificationsConfigurationEntity pushNotificationsConfigurationEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Apps);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppPushNotificationsConfigurationsObject) as ObjectTable;
            Operation operation = Operation.Replace(table, appHandle, platformType.ToString(), pushNotificationsConfigurationEntity as PushNotificationsConfigurationEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query push notifications configuration
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="platformType">Platform type</param>
        /// <returns>Validation configuration entity</returns>
        public async Task<IPushNotificationsConfigurationEntity> QueryPushNotificationsConfiguration(string appHandle, PlatformType platformType)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Apps);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppPushNotificationsConfigurationsObject) as ObjectTable;
            PushNotificationsConfigurationEntity pushNotificationsConfigurationEntity = await store.QueryObjectAsync<PushNotificationsConfigurationEntity>(table, appHandle, platformType.ToString());
            return pushNotificationsConfigurationEntity;
        }

        /// <summary>
        /// Update identity provider credentials
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="identityProviderCredentialsEntity">Identity provider credentials entity</param>
        /// <returns>Update identity provider credentials task</returns>
        public async Task UpdateIdentityProviderCredentials(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            IdentityProviderType identityProviderType,
            IIdentityProviderCredentialsEntity identityProviderCredentialsEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Apps);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppIdentityProviderCredentialsObject) as ObjectTable;
            Operation operation = Operation.Replace(table, appHandle, identityProviderType.ToString(), identityProviderCredentialsEntity as IdentityProviderCredentialsEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query identity provider credentials for an app
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <returns>Identity provider credentials entity</returns>
        public async Task<IIdentityProviderCredentialsEntity> QueryIdentityProviderCredentials(
            string appHandle,
            IdentityProviderType identityProviderType)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Apps);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppIdentityProviderCredentialsObject) as ObjectTable;
            IdentityProviderCredentialsEntity identityProviderCredentialsEntity = await store.QueryObjectAsync<IdentityProviderCredentialsEntity>(table, appHandle, identityProviderType.ToString());
            return identityProviderCredentialsEntity;
        }

        /// <summary>
        /// Insert app key
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="appKey">App key</param>
        /// <param name="createdTime">Created time</param>
        /// <returns>Insert app key task</returns>
        public async Task InsertAppKey(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string appKey,
            DateTime createdTime)
        {
            AppKeyFeedEntity appKeyFeedEntity = new AppKeyFeedEntity()
            {
                AppKey = appKey,
                CreatedTime = createdTime
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Apps);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppKeysFeed) as FeedTable;
            Operation operation = Operation.Insert(table, appHandle, this.tableStoreManager.DefaultFeedKey, appKey, appKeyFeedEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete app key
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="appKey">App key</param>
        /// <returns>Delete app key task</returns>
        public async Task DeleteAppKey(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string appKey)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Apps);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppKeysFeed) as FeedTable;
            Operation operation = Operation.Delete(table, appHandle, this.tableStoreManager.DefaultFeedKey, appKey);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query app keys
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>App key feed entities</returns>
        public async Task<IList<IAppKeyFeedEntity>> QueryAppKeys(string appHandle)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.Apps);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.Apps, TableIdentifier.AppKeysFeed) as FeedTable;
            var result = await store.QueryFeedAsync<AppKeyFeedEntity>(table, appHandle, this.tableStoreManager.DefaultFeedKey, null, int.MaxValue);
            return result.ToList<IAppKeyFeedEntity>();
        }

        /// <summary>
        /// Create app key index
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appKey">App key</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Create app key index task</returns>
        public async Task InsertAppKeyIndex(
            StorageConsistencyMode storageConsistencyMode,
            string appKey,
            string appHandle)
        {
            AppLookupEntity appLookupEntity = new AppLookupEntity()
            {
                AppHandle = appHandle
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AppKeys);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.AppKeys, TableIdentifier.AppKeysIndex) as ObjectTable;
            Operation operation = Operation.Insert(table, appKey, appKey, appLookupEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Update app key index
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appKey">App key</param>
        /// <param name="appLookupEntity">App lookup entity</param>
        /// <returns>Create app key index task</returns>
        public async Task UpdateAppKeyIndex(
            StorageConsistencyMode storageConsistencyMode,
            string appKey,
            IAppLookupEntity appLookupEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AppKeys);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.AppKeys, TableIdentifier.AppKeysIndex) as ObjectTable;
            Operation operation = Operation.Replace(table, appKey, appKey, appLookupEntity as AppLookupEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete app key index
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appKey">App key</param>
        /// <returns>Delete app key index task</returns>
        public async Task DeleteAppKeyIndex(
            StorageConsistencyMode storageConsistencyMode,
            string appKey)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AppKeys);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.AppKeys, TableIdentifier.AppKeysIndex) as ObjectTable;
            Operation operation = Operation.Delete(table, appKey, appKey);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query app key index
        /// </summary>
        /// <param name="appKey">App key</param>
        /// <returns>App lookup entity</returns>
        public async Task<IAppLookupEntity> QueryAppKeyIndex(string appKey)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AppKeys);
            ObjectTable table = this.tableStoreManager.GetTable(ContainerIdentifier.AppKeys, TableIdentifier.AppKeysIndex) as ObjectTable;
            return await store.QueryObjectAsync<AppLookupEntity>(table, appKey, appKey);
        }

        /// <summary>
        /// Insert client config
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="developerId">Developer id</param>
        /// <param name="clientName">Client name</param>
        /// <param name="serverSideAppKey">Server-side app key</param>
        /// <param name="clientConfigJson">Client config</param>
        /// <returns>Create a client config task</returns>
        public async Task InsertClientConfig(
            StorageConsistencyMode storageConsistencyMode,
            string developerId,
            string clientName,
            string serverSideAppKey,
            string clientConfigJson)
        {
            ClientConfigEntity clientConfigEntity = new ClientConfigEntity()
            {
                ServerSideAppKey = serverSideAppKey,
                ClientConfigJson = clientConfigJson
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.ClientConfigs);
            ObjectTable clientConfigsTable = this.tableStoreManager.GetTable(ContainerIdentifier.ClientConfigs, TableIdentifier.ClientConfigsObject) as ObjectTable;
            var objectKey = this.GetClientConfigsObjectKey(developerId, clientName);
            Operation operation = Operation.InsertOrReplace(clientConfigsTable, objectKey, objectKey, clientConfigEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Update client config
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="developerId">Developer id</param>
        /// <param name="clientName">Client name</param>
        /// <param name="clientConfigEntity">Client config entity</param>
        /// <returns>Update client config task</returns>
        public async Task UpdateClientConfig(
            StorageConsistencyMode storageConsistencyMode,
            string developerId,
            string clientName,
            IClientConfigEntity clientConfigEntity)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.ClientConfigs);
            ObjectTable clientConfigsTable = this.tableStoreManager.GetTable(ContainerIdentifier.ClientConfigs, TableIdentifier.ClientConfigsObject) as ObjectTable;
            var objectKey = this.GetClientConfigsObjectKey(developerId, clientName);
            Operation operation = Operation.Replace(clientConfigsTable, objectKey, objectKey, clientConfigEntity as ClientConfigEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete client config
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="developerId">Developer id</param>
        /// <param name="clientName">Client name</param>
        /// <returns>Delete client config task</returns>
        public async Task DeleteClientConfig(
            StorageConsistencyMode storageConsistencyMode,
            string developerId,
            string clientName)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.ClientConfigs);
            ObjectTable clientConfigsTable = this.tableStoreManager.GetTable(ContainerIdentifier.ClientConfigs, TableIdentifier.ClientConfigsObject) as ObjectTable;
            var objectKey = this.GetClientConfigsObjectKey(developerId, clientName);
            Operation operation = Operation.Delete(clientConfigsTable, objectKey, objectKey);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query client config
        /// </summary>
        /// <param name="developerId">Developer id</param>
        /// <param name="clientName">Client name</param>
        /// <returns>Client config entity</returns>
        public async Task<IClientConfigEntity> QueryClientConfig(string developerId, string clientName)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.ClientConfigs);
            ObjectTable clientNameTable = this.tableStoreManager.GetTable(ContainerIdentifier.ClientConfigs, TableIdentifier.ClientConfigsObject) as ObjectTable;
            var objectKey = this.GetClientConfigsObjectKey(developerId, clientName);
            return await store.QueryObjectAsync<ClientConfigEntity>(clientNameTable, objectKey, objectKey);
        }

        /// <summary>
        /// Insert client name into ClientNamesFeed in AppKey container
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appKey">App key</param>
        /// <param name="clientName">Client name</param>
        /// <returns>Insert client name into its feed</returns>
        public async Task InsertClientName(
            StorageConsistencyMode storageConsistencyMode,
            string appKey,
            string clientName)
        {
            ClientNamesFeedEntity clientNameFeedEntity = new ClientNamesFeedEntity()
            {
                ClientName = clientName
            };

            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AppKeys);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.AppKeys, TableIdentifier.ClientNamesFeed) as FeedTable;
            Operation operation = Operation.Insert(table, appKey, this.tableStoreManager.DefaultFeedKey, clientName, clientNameFeedEntity);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Delete client name from ClientNamesFeed in AppKey container
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appKey">App key</param>
        /// <param name="clientName">Client name</param>
        /// <returns>Delete client name from its feed</returns>
        public async Task DeleteClientName(
            StorageConsistencyMode storageConsistencyMode,
            string appKey,
            string clientName)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AppKeys);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.AppKeys, TableIdentifier.ClientNamesFeed) as FeedTable;
            Operation operation = Operation.Delete(table, appKey, this.tableStoreManager.DefaultFeedKey, clientName);
            await store.ExecuteOperationAsync(operation, storageConsistencyMode.ToConsistencyMode());
        }

        /// <summary>
        /// Query client name from ClientNamesFeed in AppKey container
        /// </summary>
        /// <param name="appKey">App key</param>
        /// <returns>Client names</returns>
        public async Task<IList<IClientNamesFeedEntity>> QueryClientNames(string appKey)
        {
            CTStore store = await this.tableStoreManager.GetStore(ContainerIdentifier.AppKeys);
            FeedTable table = this.tableStoreManager.GetTable(ContainerIdentifier.AppKeys, TableIdentifier.ClientNamesFeed) as FeedTable;
            var result = await store.QueryFeedAsync<ClientNamesFeedEntity>(table, appKey, this.tableStoreManager.DefaultFeedKey, null, int.MaxValue);
            return result.ToList<IClientNamesFeedEntity>();
        }

        /// <summary>
        /// Constructs the object key for the app admin table
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Get app admins key task</returns>
        private string GetAppAdminObjectKey(string appHandle, string userHandle)
        {
            return string.Join("+", appHandle, userHandle);
        }

        /// <summary>
        /// Constructs the object key for the client configs object table
        /// </summary>
        /// <param name="developerId">Developer id</param>
        /// <param name="clientName">Client name</param>
        /// <returns>Get client name and config key task</returns>
        private string GetClientConfigsObjectKey(string developerId, string clientName)
        {
            return string.Join("+", developerId, clientName);
        }
    }
}
