// <copyright file="IAppsStore.cs" company="Microsoft">
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
    /// Apps store interface
    /// </summary>
    public interface IAppsStore
    {
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
        Task InsertApp(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string developerId,
            string name,
            string iconHandle,
            PlatformType platformType,
            string deepLink,
            string storeLink,
            DateTime createdTime,
            bool disableHandleValidation);

        /// <summary>
        /// Delete app
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete app task</returns>
        Task DeleteApp(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle);

        /// <summary>
        /// Update app profile
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="appProfileEntity">App profile entity</param>
        /// <returns>Update app profile task</returns>
        Task UpdateAppProfile(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            IAppProfileEntity appProfileEntity);

        /// <summary>
        /// Query app profile
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>App profile entity</returns>
        Task<IAppProfileEntity> QueryAppProfile(string appHandle);

        /// <summary>
        /// Insert developer app
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="developerId">Developer id</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Insert developer app task</returns>
        Task InsertDeveloperApp(
            StorageConsistencyMode storageConsistencyMode,
            string developerId,
            string appHandle);

        /// <summary>
        /// Delete developer app
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="developerId">Developer id</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete developer app task</returns>
        Task DeleteDeveloperApp(
            StorageConsistencyMode storageConsistencyMode,
            string developerId,
            string appHandle);

        /// <summary>
        /// Query developer apps
        /// </summary>
        /// <param name="developerId">Developer id</param>
        /// <returns>App feed entities</returns>
        Task<IList<IAppFeedEntity>> QueryDeveloperApps(string developerId);

        /// <summary>
        /// Insert user into table of app adminstrators
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Insert app administrator task</returns>
        Task InsertAdminUser(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string userHandle);

        /// <summary>
        /// Delete user from table of app administrators
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Delete app administrator task</returns>
        Task DeleteAdminUser(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string userHandle);

        /// <summary>
        /// Query to determine if a user is an app administrator
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>App administrator entity</returns>
        Task<IAppAdminEntity> QueryAdminUser(string appHandle, string userHandle);

        /// <summary>
        /// Insert all app
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Insert all app task</returns>
        Task InsertAllApp(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle);

        /// <summary>
        /// Delete all app
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete all app task</returns>
        Task DeleteAllApp(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle);

        /// <summary>
        /// Query all apps
        /// </summary>
        /// <returns>App feed entities</returns>
        Task<IList<IAppFeedEntity>> QueryAllApps();

        /// <summary>
        /// Update validation configuration
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="validationConfigurationEntity">Validation configuration entity</param>
        /// <returns>Update validation configuration task</returns>
        Task UpdateValidationConfiguration(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            IValidationConfigurationEntity validationConfigurationEntity);

        /// <summary>
        /// Query validation configuration
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>Validation configuration entity</returns>
        Task<IValidationConfigurationEntity> QueryValidationConfiguration(string appHandle);

        /// <summary>
        /// Update push notifications configuration
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="platformType">Platform type</param>
        /// <param name="pushNotificationsConfigurationEntity">Push notifications configuration entity</param>
        /// <returns>Update push notifications configuration task</returns>
        Task UpdatePushNotificationsConfiguration(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            PlatformType platformType,
            IPushNotificationsConfigurationEntity pushNotificationsConfigurationEntity);

        /// <summary>
        /// Query push notifications configuration
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="platformType">Platform type</param>
        /// <returns>Validation configuration entity</returns>
        Task<IPushNotificationsConfigurationEntity> QueryPushNotificationsConfiguration(string appHandle, PlatformType platformType);

        /// <summary>
        /// Update identity provider credentials
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="identityProviderCredentialsEntity">Identity provider credentials entity</param>
        /// <returns>Update identity provider credentials task</returns>
        Task UpdateIdentityProviderCredentials(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            IdentityProviderType identityProviderType,
            IIdentityProviderCredentialsEntity identityProviderCredentialsEntity);

        /// <summary>
        /// Query identity provider credentials for an app
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <returns>Identity provider credentials entity</returns>
        Task<IIdentityProviderCredentialsEntity> QueryIdentityProviderCredentials(
            string appHandle,
            IdentityProviderType identityProviderType);

        /// <summary>
        /// Insert app key
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="appKey">App key</param>
        /// <param name="createdTime">Created time</param>
        /// <returns>Insert app key task</returns>
        Task InsertAppKey(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string appKey,
            DateTime createdTime);

        /// <summary>
        /// Delete app key
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="appKey">App key</param>
        /// <returns>Delete app key task</returns>
        Task DeleteAppKey(
            StorageConsistencyMode storageConsistencyMode,
            string appHandle,
            string appKey);

        /// <summary>
        /// Query app keys
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>App key feed entities</returns>
        Task<IList<IAppKeyFeedEntity>> QueryAppKeys(string appHandle);

        /// <summary>
        /// Create app key index
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appKey">App key</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Create app key index task</returns>
        Task InsertAppKeyIndex(
            StorageConsistencyMode storageConsistencyMode,
            string appKey,
            string appHandle);

        /// <summary>
        /// Update app key index
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appKey">App key</param>
        /// <param name="appKeyLookupEntity">App Key Lookup Entity</param>
        /// <returns>Create app key index task</returns>
        Task UpdateAppKeyIndex(
            StorageConsistencyMode storageConsistencyMode,
            string appKey,
            IAppLookupEntity appKeyLookupEntity);

        /// <summary>
        /// Delete app key index
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appKey">App key</param>
        /// <returns>Delete app key index task</returns>
        Task DeleteAppKeyIndex(
            StorageConsistencyMode storageConsistencyMode,
            string appKey);

        /// <summary>
        /// Query app key index
        /// </summary>
        /// <param name="appKey">App key</param>
        /// <returns>App lookup entity</returns>
        Task<IAppLookupEntity> QueryAppKeyIndex(string appKey);

        /// <summary>
        /// Insert client config
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="developerId">Developer id</param>
        /// <param name="clientName">Client name</param>
        /// <param name="serverSideAppKey">Server-side app key</param>
        /// <param name="clientConfigJson">Client config JSON</param>
        /// <returns>Create a client config task</returns>
        Task InsertClientConfig(
            StorageConsistencyMode storageConsistencyMode,
            string developerId,
            string clientName,
            string serverSideAppKey,
            string clientConfigJson);

        /// <summary>
        /// Update client config
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="developerId">Developer id</param>
        /// <param name="clientName">Client name</param>
        /// <param name="clientConfigEntity">Client config entity</param>
        /// <returns>Update client config task</returns>
        Task UpdateClientConfig(
            StorageConsistencyMode storageConsistencyMode,
            string developerId,
            string clientName,
            IClientConfigEntity clientConfigEntity);

        /// <summary>
        /// Delete client config
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="developerId">Developer id</param>
        /// <param name="clientName">Client name</param>
        /// <returns>Delete client config task</returns>
        Task DeleteClientConfig(
            StorageConsistencyMode storageConsistencyMode,
            string developerId,
            string clientName);

        /// <summary>
        /// Query client config
        /// </summary>
        /// <param name="developerId">Developer id</param>
        /// <param name="clientName">Client name</param>
        /// <returns>Client name lookup entity</returns>
        Task<IClientConfigEntity> QueryClientConfig(string developerId, string clientName);

        /// <summary>
        /// Insert client name into ClientNamesFeed in AppKey container
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appKey">App key</param>
        /// <param name="clientName">Client name</param>
        /// <returns>Insert client name into its feed</returns>
        Task InsertClientName(
            StorageConsistencyMode storageConsistencyMode,
            string appKey,
            string clientName);

        /// <summary>
        /// Delete client name from ClientNamesFeed in AppKey container
        /// </summary>
        /// <param name="storageConsistencyMode">Storage consistency mode</param>
        /// <param name="appKey">App key</param>
        /// <param name="clientName">Client name</param>
        /// <returns>Delete client name from its feed</returns>
        Task DeleteClientName(
            StorageConsistencyMode storageConsistencyMode,
            string appKey,
            string clientName);

        /// <summary>
        /// Query client name from ClientNamesFeed in AppKey container
        /// </summary>
        /// <param name="appKey">App key</param>
        /// <returns>Client names</returns>
        Task<IList<IClientNamesFeedEntity>> QueryClientNames(string appKey);
    }
}
