// <copyright file="IPushNotificationsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Threading.Tasks;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Push notifications manager interface
    /// </summary>
    public interface IPushNotificationsManager
    {
        /// <summary>
        /// Create push notifications registration
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">uniquely identifies the user</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="platformType">mobile OS</param>
        /// <param name="registrationId">registration id from the mobile OS</param>
        /// <param name="language">mobile app language</param>
        /// <param name="lastUpdatedTime">time at which the registration id was obtained from the underlying mobile OS</param>
        /// <returns>Create registration task</returns>
        Task CreateRegistration(
            ProcessType processType,
            string userHandle,
            string appHandle,
            PlatformType platformType,
            string registrationId,
            string language,
            DateTime lastUpdatedTime);

        /// <summary>
        /// Delete push notifications registration
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">uniquely identifies the user</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="pushRegistrationFeedEntity">Push registration to delete</param>
        /// <returns>Delete registration task</returns>
        Task DeleteRegistration(
            ProcessType processType,
            string userHandle,
            string appHandle,
            IPushRegistrationFeedEntity pushRegistrationFeedEntity);

        /// <summary>
        /// Read push notifications registration
        /// </summary>
        /// <param name="userHandle">uniquely identifies the user</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="registrationId">registration id from the mobile OS</param>
        /// <returns>Push registration entity</returns>
        Task<IPushRegistrationFeedEntity> ReadRegistration(string userHandle, string appHandle, string registrationId);

        /// <summary>
        /// Delete all push registrations for a user
        /// </summary>
        /// <param name="processType">Process type</param>
        /// <param name="userHandle">uniquely identifies the user</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <returns>Delete user registrations task</returns>
        Task DeleteUserRegistrations(ProcessType processType, string userHandle, string appHandle);

        /// <summary>
        /// Create or update a push notifications hub
        /// </summary>
        /// <param name="platformType">mobile OS</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="path">Depends on the mobile OS.
        /// Windows: Windows Package SID.
        /// Android: Leave this empty.
        /// iOS: iOS certificate path.</param>
        /// <param name="key">Depends on the mobile OS.
        /// Windows: Windows secret key.
        /// Android: Android API key.
        /// iOS: iOS Certificate key.</param>
        /// <returns>Create hub task</returns>
        Task CreateHub(PlatformType platformType, string appHandle, string path, string key);

        /// <summary>
        /// Delete push notifications hub for an app on a particular platform type
        /// </summary>
        /// <param name="platformType">mobile OS</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <returns>Delete hub task</returns>
        Task DeleteHub(PlatformType platformType, string appHandle);

        /// <summary>
        /// Delete push notifications hub for an app across all platform types
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <returns>Delete hub task</returns>
        Task DeleteHub(string appHandle);

        /// <summary>
        /// Send a push notification
        /// </summary>
        /// <param name="userHandle">uniquely identifies the user</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="activityHandle">uniquely identifies the activity</param>
        /// <param name="activityType">type of activity</param>
        /// <param name="actorUserHandle">which user performed the activity</param>
        /// <param name="actedOnUserHandle">which user the activity is being performed on</param>
        /// <param name="actedOnContentType">type of content the activity is being performed on</param>
        /// <param name="actedOnContentHandle">content the activity is being performed on</param>
        /// <param name="createdTime">time the activity was created</param>
        /// <returns>Send notification task</returns>
        Task SendNotification(
            string userHandle,
            string appHandle,
            string activityHandle,
            ActivityType activityType,
            string actorUserHandle,
            string actedOnUserHandle,
            ContentType actedOnContentType,
            string actedOnContentHandle,
            DateTime createdTime);

        /// <summary>
        /// Has this push notification registration expired?
        /// </summary>
        /// <param name="registrationTime">the UTC DateTime on which this push registration was last updated</param>
        /// <returns>true if more than 30 days old</returns>
        bool HasRegistrationExpired(DateTime registrationTime);

        /// <summary>
        /// Is this push notification registration too far in the future?
        /// </summary>
        /// <param name="registrationTime">the UTC DateTime on which this push registration was last updated</param>
        /// <returns>true if more than 24 hours in the future</returns>
        bool IsRegistrationTooNew(DateTime registrationTime);
    }
}
