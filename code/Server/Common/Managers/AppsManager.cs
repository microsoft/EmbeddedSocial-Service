// <copyright file="AppsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Apps manager class
    /// </summary>
    public class AppsManager : IAppsManager
    {
        /// <summary>
        /// Apps store
        /// </summary>
        private IAppsStore appsStore;

        /// <summary>
        /// Push notifications manager
        /// </summary>
        private IPushNotificationsManager pushNotificationsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppsManager"/> class
        /// </summary>
        /// <param name="appsStore">Apps store</param>
        public AppsManager(IAppsStore appsStore)
        {
            this.appsStore = appsStore;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppsManager"/> class
        /// </summary>
        /// <param name="appsStore">Apps store</param>
        /// <param name="pushNotificationsManager">Push notifications manager</param>
        public AppsManager(IAppsStore appsStore, IPushNotificationsManager pushNotificationsManager)
        {
            this.appsStore = appsStore;
            this.pushNotificationsManager = pushNotificationsManager;
        }

        /// <summary>
        /// Create app
        /// </summary>
        /// <param name="processType">Process type</param>
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
        public async Task CreateApp(
            ProcessType processType,
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
            await this.appsStore.InsertApp(
                StorageConsistencyMode.Strong,
                appHandle,
                developerId,
                name,
                iconHandle,
                platformType,
                deepLink,
                storeLink,
                createdTime,
                disableHandleValidation);

            await this.appsStore.InsertDeveloperApp(
                StorageConsistencyMode.Strong,
                developerId,
                appHandle);

            await this.appsStore.InsertAllApp(
                StorageConsistencyMode.Strong, appHandle);
        }

        /// <summary>
        /// Delete app
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="developerId">Developer id</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete app task</returns>
        public async Task DeleteApp(
            ProcessType processType,
            string developerId,
            string appHandle)
        {
            IList<IAppKeyFeedEntity> appKeyFeedEntities = await this.ReadAppKeys(appHandle);
            foreach (var appKeyFeedEntity in appKeyFeedEntities)
            {
                await this.DeleteAppKey(processType, appHandle, appKeyFeedEntity.AppKey);
            }

            await this.appsStore.DeleteAllApp(StorageConsistencyMode.Strong, appHandle);
            await this.appsStore.DeleteDeveloperApp(StorageConsistencyMode.Strong, developerId, appHandle);
            await this.appsStore.DeleteApp(StorageConsistencyMode.Strong, appHandle);
            await this.pushNotificationsManager.DeleteHub(appHandle);
        }

        /// <summary>
        /// Update app profile info
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="name">App name</param>
        /// <param name="iconHandle">Icon handle</param>
        /// <param name="platformType">Platform type</param>
        /// <param name="deepLink">Deep link</param>
        /// <param name="storeLink">Store link</param>
        /// <param name="lastUpdatedTime">Last updated time</param>
        /// <param name="disableHandleValidation">whether to disable validation of app-provided handles</param>
        /// <param name="appProfileEntity">App profile entity</param>
        /// <returns>Update app profile task</returns>
        public async Task UpdateAppProfileInfo(
            ProcessType processType,
            string appHandle,
            string name,
            string iconHandle,
            PlatformType platformType,
            string deepLink,
            string storeLink,
            DateTime lastUpdatedTime,
            bool disableHandleValidation,
            IAppProfileEntity appProfileEntity)
        {
            appProfileEntity.Name = name;
            appProfileEntity.IconHandle = iconHandle;
            appProfileEntity.PlatformType = platformType;
            appProfileEntity.DeepLink = deepLink;
            appProfileEntity.StoreLink = storeLink;
            appProfileEntity.LastUpdatedTime = lastUpdatedTime;
            appProfileEntity.DisableHandleValidation = disableHandleValidation;

            await this.appsStore.UpdateAppProfile(StorageConsistencyMode.Strong, appHandle, appProfileEntity);
        }

        /// <summary>
        /// Update app profile icon
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="iconHandle">Icon handle</param>
        /// <param name="appProfileEntity">App profile entity</param>
        /// <returns>Update app profile icon task</returns>
        public async Task UpdateAppProfileIcon(
            ProcessType processType,
            string appHandle,
            string iconHandle,
            IAppProfileEntity appProfileEntity)
        {
            appProfileEntity.IconHandle = iconHandle;
            await this.appsStore.UpdateAppProfile(StorageConsistencyMode.Strong, appHandle, appProfileEntity);
        }

        /// <summary>
        /// Read app profile
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>App profile entity</returns>
        public async Task<IAppProfileEntity> ReadAppProfile(string appHandle)
        {
            return await this.appsStore.QueryAppProfile(appHandle);
        }

        /// <summary>
        /// Read developer apps
        /// </summary>
        /// <param name="developerId">Developer id</param>
        /// <returns>App feed entities</returns>
        public async Task<IList<IAppFeedEntity>> ReadDeveloperApps(string developerId)
        {
            return await this.appsStore.QueryDeveloperApps(developerId);
        }

        /// <summary>
        /// Inserts a user into the list of application administrators
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Insert admin user task</returns>
        public async Task InsertAdminUser(string appHandle, string userHandle)
        {
            await this.appsStore.InsertAdminUser(StorageConsistencyMode.Strong, appHandle, userHandle);
        }

        /// <summary>
        /// Deletes a user from the list of application administrators
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Delete admin user task</returns>
        public async Task DeleteAdminUser(string appHandle, string userHandle)
        {
            await this.appsStore.DeleteAdminUser(StorageConsistencyMode.Strong, appHandle, userHandle);
        }

        /// <summary>
        /// Determines if the user is an administrator for this app
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>True if the user is an adminstrator</returns>
        public async Task<bool> IsAdminUser(string appHandle, string userHandle)
        {
            var adminUserEntity = await this.appsStore.QueryAdminUser(appHandle, userHandle);
            if (adminUserEntity == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Update validation configuration
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="enabled">Is enabled</param>
        /// <param name="validateText">Should validate text</param>
        /// <param name="validateImages">Should validate images</param>
        /// <param name="userReportThreshold">User report threshold</param>
        /// <param name="contentReportThreshold">Content report threshold</param>
        /// <param name="allowMatureContent">Should allow mature content</param>
        /// <param name="validationConfigurationEntity">Validation configuration entity</param>
        /// <returns>Update validation configuration task</returns>
        public async Task UpdateValidationConfiguration(
            ProcessType processType,
            string appHandle,
            bool enabled,
            bool validateText,
            bool validateImages,
            int userReportThreshold,
            int contentReportThreshold,
            bool allowMatureContent,
            IValidationConfigurationEntity validationConfigurationEntity)
        {
            validationConfigurationEntity.Enabled = enabled;
            validationConfigurationEntity.ValidateText = validateText;
            validationConfigurationEntity.ValidateImages = validateImages;
            validationConfigurationEntity.UserReportThreshold = userReportThreshold;
            validationConfigurationEntity.ContentReportThreshold = contentReportThreshold;
            validationConfigurationEntity.AllowMatureContent = allowMatureContent;

            await this.appsStore.UpdateValidationConfiguration(StorageConsistencyMode.Strong, appHandle, validationConfigurationEntity);
        }

        /// <summary>
        /// Read validation configuration
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>Validation configuration entity</returns>
        public async Task<IValidationConfigurationEntity> ReadValidationConfiguration(string appHandle)
        {
            return await this.appsStore.QueryValidationConfiguration(appHandle);
        }

        /// <summary>
        /// Update push notifications configuration
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="platformType">Platform type</param>
        /// <param name="enabled">A value indicating whether push notifications is enabled</param>
        /// <param name="path">Gets or sets push notifications hub path.
        /// Windows: Windows Package SID
        /// Android: Leave this empty
        /// iOS: iOS certificate path</param>
        /// <param name="key">Gets or sets push notifications key.
        /// Windows: Windows secret key
        /// Android: Android API key
        /// iOS: iOS Certificate key</param>
        /// <param name="pushNotificationsConfiguarationEntity">Push notifications configuration entity</param>
        /// <returns>Update push notifications configuration task</returns>
        public async Task UpdatePushNotificationsConfiguration(
            ProcessType processType,
            string appHandle,
            PlatformType platformType,
            bool enabled,
            string path,
            string key,
            IPushNotificationsConfigurationEntity pushNotificationsConfiguarationEntity)
        {
            bool oldEnabled = pushNotificationsConfiguarationEntity.Enabled;
            string oldPath = pushNotificationsConfiguarationEntity.Path;
            string oldKey = pushNotificationsConfiguarationEntity.Key;

            if (oldEnabled && !enabled)
            {
                await this.pushNotificationsManager.DeleteHub(platformType, appHandle);
            }

            if (!oldEnabled && enabled)
            {
                await this.pushNotificationsManager.CreateHub(platformType, appHandle, path, key);
            }

            if (oldEnabled && enabled && !(oldPath == path && oldKey == key))
            {
                // Hub key and path have changed. So, we delete the old hub and create the new hub.
                await this.pushNotificationsManager.DeleteHub(platformType, appHandle);
                await this.pushNotificationsManager.CreateHub(platformType, appHandle, path, key);
            }

            // update the app store table
            pushNotificationsConfiguarationEntity.Enabled = enabled;
            pushNotificationsConfiguarationEntity.Path = path;
            pushNotificationsConfiguarationEntity.Key = key;
            await this.appsStore.UpdatePushNotificationsConfiguration(
                StorageConsistencyMode.Strong,
                appHandle,
                platformType,
                pushNotificationsConfiguarationEntity);
        }

        /// <summary>
        /// Read push notifications configuration
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="platformType">Platform type</param>
        /// <returns>Push notifications configuration entity</returns>
        public async Task<IPushNotificationsConfigurationEntity> ReadPushNotificationsConfiguration(string appHandle, PlatformType platformType)
        {
            return await this.appsStore.QueryPushNotificationsConfiguration(appHandle, platformType);
        }

        /// <summary>
        /// Update identity provider credentials
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="clientId">Client id</param>
        /// <param name="clientSecret">Client secret</param>
        /// <param name="clientRedirectUri"><c>OAuth</c> callback</param>
        /// <param name="identityProviderCredentialsEntity">Identity provider credentials entity</param>
        /// <returns>Update identity provider credentials task</returns>
        public async Task UpdateIdentityProviderCredentials(
            ProcessType processType,
            string appHandle,
            IdentityProviderType identityProviderType,
            string clientId,
            string clientSecret,
            string clientRedirectUri,
            IIdentityProviderCredentialsEntity identityProviderCredentialsEntity)
        {
            identityProviderCredentialsEntity.ClientId = clientId;
            identityProviderCredentialsEntity.ClientSecret = clientSecret;
            identityProviderCredentialsEntity.ClientRedirectUri = clientRedirectUri;

            await this.appsStore.UpdateIdentityProviderCredentials(StorageConsistencyMode.Strong, appHandle, identityProviderType, identityProviderCredentialsEntity);
        }

        /// <summary>
        /// Read identity provider credentials for an app
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <returns>Identity provider credentials entity</returns>
        public async Task<IIdentityProviderCredentialsEntity> ReadIdentityProviderCredentials(string appHandle, IdentityProviderType identityProviderType)
        {
            return await this.appsStore.QueryIdentityProviderCredentials(appHandle, identityProviderType);
        }

        /// <summary>
        /// Create app key
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="appKey">App key</param>
        /// <param name="createdTime">Created time</param>
        /// <returns>Create app key task</returns>
        public async Task CreateAppKey(
            ProcessType processType,
            string appHandle,
            string appKey,
            DateTime createdTime)
        {
            await this.appsStore.InsertAppKey(StorageConsistencyMode.Strong, appHandle, appKey, createdTime);
            await this.appsStore.InsertAppKeyIndex(StorageConsistencyMode.Strong, appKey, appHandle);
        }

        /// <summary>
        /// Delete app key
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="appKey">App key</param>
        /// <returns>Delete app key task</returns>
        public async Task DeleteAppKey(
            ProcessType processType,
            string appHandle,
            string appKey)
        {
            await this.appsStore.DeleteAppKeyIndex(StorageConsistencyMode.Strong, appKey);
            await this.appsStore.DeleteAppKey(StorageConsistencyMode.Strong, appHandle, appKey);
        }

        /// <summary>
        /// Read app keys
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>App key feed entities</returns>
        public async Task<IList<IAppKeyFeedEntity>> ReadAppKeys(string appHandle)
        {
            return await this.appsStore.QueryAppKeys(appHandle);
        }

        /// <summary>
        /// Read app by app key
        /// </summary>
        /// <param name="appKey">App key</param>
        /// <returns>App lookup entity</returns>
        public async Task<IAppLookupEntity> ReadAppByAppKey(string appKey)
        {
            return await this.appsStore.QueryAppKeyIndex(appKey);
        }

        /// <summary>
        /// Read all apps
        /// </summary>
        /// <returns>App feed entities</returns>
        public async Task<IList<IAppFeedEntity>> ReadAllApps()
        {
            return await this.appsStore.QueryAllApps();
        }

        /// <summary>
        /// Create client name and config
        /// </summary>
        /// <param name="appKey">App key</param>
        /// <param name="clientName">Client name</param>
        /// <param name="serverSideAppKey">server-side app key</param>
        /// <param name="clientConfigJson">client config JSON</param>
        /// <returns>Create client name and config task</returns>
        public async Task CreateClientNameAndConfig(string appKey, string clientName, string serverSideAppKey, string clientConfigJson)
        {
            // We want to allow for different developers to share the same client names. Since a client name is used
            // to lookup the server-side app key and client configuration, we have to first retrieve the developerId associated
            // with this app key. The developerId + client name act as keys to server-side app key and client configuration
            var appLookupEntity = await this.ReadAppByAppKey(appKey);
            var appHandle = appLookupEntity?.AppHandle;
            var appProfileEntity = await this.ReadAppProfile(appHandle);
            var developerId = appProfileEntity?.DeveloperId;

            // TODO: Re-visit the order of these inserts; right now leave them as is for code consistency.
            await this.appsStore.InsertClientName(StorageConsistencyMode.Strong, appKey, clientName);
            await this.appsStore.InsertClientConfig(StorageConsistencyMode.Strong, developerId, clientName, serverSideAppKey, clientConfigJson);
        }

        /// <summary>
        /// Delete client name and config
        /// </summary>
        /// <param name="appKey">App key</param>
        /// <param name="clientName">Client name</param>
        /// <returns>Delete client name and config task</returns>
        public async Task DeleteClientNameAndConfig(string appKey, string clientName)
        {
            var appLookupEntity = await this.ReadAppByAppKey(appKey);
            var appHandle = appLookupEntity?.AppHandle;
            var appProfileEntity = await this.ReadAppProfile(appHandle);
            var developerId = appProfileEntity?.DeveloperId;

            await this.appsStore.DeleteClientConfig(StorageConsistencyMode.Strong, developerId, clientName);
            await this.appsStore.DeleteClientName(StorageConsistencyMode.Strong, appKey, clientName);
        }

        /// <summary>
        /// Query client names for a given app key
        /// </summary>
        /// <param name="appKey">App key</param>
        /// <returns>List of client names</returns>
        public async Task<IList<IClientNamesFeedEntity>> QueryClientNames(string appKey)
        {
            return await this.appsStore.QueryClientNames(appKey);
        }

        /// <summary>
        /// Read client config for a given developer id and client name
        /// </summary>
        /// <param name="developerId">developer id</param>
        /// <param name="clientName">client name</param>
        /// <returns>client's config</returns>
        public async Task<IClientConfigEntity> ReadClientConfig(string developerId, string clientName)
        {
            return await this.appsStore.QueryClientConfig(developerId, clientName);
        }

        /// <summary>
        /// Does this application allow mature content to be displayed to a user
        /// </summary>
        /// <param name="appHandle">app handle</param>
        /// <returns>true if the app allows mature content</returns>
        public async Task<bool> IsMatureContentAllowed(string appHandle)
        {
            var validationConfig = await this.appsStore.QueryValidationConfiguration(appHandle);
            if (validationConfig != null)
            {
                return validationConfig.AllowMatureContent;
            }

            return false;
        }
    }
}
