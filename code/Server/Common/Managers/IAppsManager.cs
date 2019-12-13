// <copyright file="IAppsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Apps manager interface
    /// </summary>
    public interface IAppsManager
    {
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
        Task CreateApp(
            ProcessType processType,
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
        /// <param name="processType">Process type</param>
        /// <param name="developerId">Developer id</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Delete app task</returns>
        Task DeleteApp(
            ProcessType processType,
            string developerId,
            string appHandle);

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
        Task UpdateAppProfileInfo(
            ProcessType processType,
            string appHandle,
            string name,
            string iconHandle,
            PlatformType platformType,
            string deepLink,
            string storeLink,
            DateTime lastUpdatedTime,
            bool disableHandleValidation,
            IAppProfileEntity appProfileEntity);

        /// <summary>
        /// Update app profile icon
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="iconHandle">Icon handle</param>
        /// <param name="appProfileEntity">App profile entity</param>
        /// <returns>Update app profile icon task</returns>
        Task UpdateAppProfileIcon(
            ProcessType processType,
            string appHandle,
            string iconHandle,
            IAppProfileEntity appProfileEntity);

        /// <summary>
        /// Read app profile
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>App profile entity</returns>
        Task<IAppProfileEntity> ReadAppProfile(string appHandle);

        /// <summary>
        /// Read developer apps
        /// </summary>
        /// <param name="developerId">Developer id</param>
        /// <returns>App feed entities</returns>
        Task<IList<IAppFeedEntity>> ReadDeveloperApps(string developerId);

        /// <summary>
        /// Inserts a user into the list of application administrators
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Insert admin user task</returns>
        Task InsertAdminUser(string appHandle, string userHandle);

        /// <summary>
        /// Deletes a user from the list of application administrators
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>Delete admin user task</returns>
        Task DeleteAdminUser(string appHandle, string userHandle);

        /// <summary>
        /// Determines if the user is an administrator for this app
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="userHandle">User handle</param>
        /// <returns>True if the user is an adminstrator</returns>
        Task<bool> IsAdminUser(string appHandle, string userHandle);

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
        Task UpdateValidationConfiguration(
            ProcessType processType,
            string appHandle,
            bool enabled,
            bool validateText,
            bool validateImages,
            int userReportThreshold,
            int contentReportThreshold,
            bool allowMatureContent,
            IValidationConfigurationEntity validationConfigurationEntity);

        /// <summary>
        /// Read validation configuration
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>Validation configuration entity</returns>
        Task<IValidationConfigurationEntity> ReadValidationConfiguration(string appHandle);

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
        Task UpdatePushNotificationsConfiguration(
            ProcessType processType,
            string appHandle,
            PlatformType platformType,
            bool enabled,
            string path,
            string key,
            IPushNotificationsConfigurationEntity pushNotificationsConfiguarationEntity);

        /// <summary>
        /// Read push notifications configuration
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="platformType">Platform type</param>
        /// <returns>Push notifications configuration entity</returns>
        Task<IPushNotificationsConfigurationEntity> ReadPushNotificationsConfiguration(string appHandle, PlatformType platformType);

        /// <summary>
        /// Update identity provider credentials
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="clientId">Client id</param>
        /// <param name="clientSecret">Client secret</param>
        /// <param name="clientRedirectUri">Client redirect uri</param>
        /// <param name="identityProviderCredentialsEntity">Identity provider credentials entity</param>
        /// <returns>Update identity provider credentials task</returns>
        Task UpdateIdentityProviderCredentials(
            ProcessType processType,
            string appHandle,
            IdentityProviderType identityProviderType,
            string clientId,
            string clientSecret,
            string clientRedirectUri,
            IIdentityProviderCredentialsEntity identityProviderCredentialsEntity);

        /// <summary>
        /// Read identity provider credentials for an app
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <returns>Identity provider credentials entity</returns>
        Task<IIdentityProviderCredentialsEntity> ReadIdentityProviderCredentials(string appHandle, IdentityProviderType identityProviderType);

        /// <summary>
        /// Create app key
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="appKey">App key</param>
        /// <param name="createdTime">Created time</param>
        /// <returns>Create app key task</returns>
        Task CreateAppKey(
            ProcessType processType,
            string appHandle,
            string appKey,
            DateTime createdTime);

        /// <summary>
        /// Delete app key
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="appKey">App key</param>
        /// <returns>Delete app key task</returns>
        Task DeleteAppKey(
            ProcessType processType,
            string appHandle,
            string appKey);

        /// <summary>
        /// Read app keys
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <returns>App key feed entities</returns>
        Task<IList<IAppKeyFeedEntity>> ReadAppKeys(string appHandle);

        /// <summary>
        /// Read app by app key
        /// </summary>
        /// <param name="appKey">App key</param>
        /// <returns>App lookup entity</returns>
        Task<IAppLookupEntity> ReadAppByAppKey(string appKey);

        /// <summary>
        /// Read all apps
        /// </summary>
        /// <returns>App feed entities</returns>
        Task<IList<IAppFeedEntity>> ReadAllApps();

        /// <summary>
        /// Create client name and config
        /// </summary>
        /// <param name="appKey">App key</param>
        /// <param name="clientName">Client name</param>
        /// <param name="serverSideAppKey">server-side app key</param>
        /// <param name="clientConfigJson">client config JSON</param>
        /// <returns>Create client name and config task</returns>
        Task CreateClientNameAndConfig(string appKey, string clientName, string serverSideAppKey, string clientConfigJson);

        /// <summary>
        /// Delete client name and config
        /// </summary>
        /// <param name="appKey">App key</param>
        /// <param name="clientName">Client name</param>
        /// <returns>Delete client name and config task</returns>
        Task DeleteClientNameAndConfig(string appKey, string clientName);

        /// <summary>
        /// Query client names for a given app key
        /// </summary>
        /// <param name="appKey">App key</param>
        /// <returns>List of client names</returns>
        Task<IList<IClientNamesFeedEntity>> QueryClientNames(string appKey);

        /// <summary>
        /// Read client config for a given developer id and client name
        /// </summary>
        /// <param name="developerId">developer id</param>
        /// <param name="clientName">client name</param>
        /// <returns>client's config</returns>
        Task<IClientConfigEntity> ReadClientConfig(string developerId, string clientName);

        /// <summary>
        /// Does this application allow mature content to be displayed to a user
        /// </summary>
        /// <param name="appHandle">app handle</param>
        /// <returns>true if the app allows mature content</returns>
        Task<bool> IsMatureContentAllowed(string appHandle);
    }
}
