// <copyright file="PushNotificationsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.PushNotificationsHub;
    using SocialPlus.Server.Tables;

    /// <summary>
    /// Register and unregister for push notifications and send push notifications
    /// </summary>
    public class PushNotificationsManager : IPushNotificationsManager
    {
        /// <summary>
        /// Connection string provider
        /// </summary>
        private readonly IConnectionStringProvider connectionStringProvider;

        /// <summary>
        /// Push Registrations expire after this duration
        /// </summary>
        private readonly TimeSpan registrationTtl = TimeSpan.FromDays(30);

        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// How long in the future will we allow a new push notifications registration to come from the client.
        /// This is primarily to account for clock skew between a mobile device and this service.
        /// </summary>
        private readonly TimeSpan registrationSkewAllowed = TimeSpan.FromHours(24);

        /// <summary>
        /// Table containing push notification registrations
        /// </summary>
        private IPushRegistrationsStore pushRegistrationsStore;

        /// <summary>
        /// Table containing app configurations
        /// </summary>
        private IAppsStore appsStore;

        /// <summary>
        /// Views manager
        /// </summary>
        private IViewsManager viewsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PushNotificationsManager"/> class
        /// </summary>
        /// <param name="log">log</param>
        /// <param name="pushRegistrationsStore">Table containing push notification registrations</param>
        /// <param name="appsStore">Table containing app configurations</param>
        /// <param name="viewsManager">Views manager</param>
        /// <param name="connectionStringProvider">Connection string provider</param>
        public PushNotificationsManager(ILog log, IPushRegistrationsStore pushRegistrationsStore, IAppsStore appsStore, IViewsManager viewsManager, IConnectionStringProvider connectionStringProvider)
        {
            this.log = log;
            this.pushRegistrationsStore = pushRegistrationsStore;
            this.appsStore = appsStore;
            this.viewsManager = viewsManager;
            this.connectionStringProvider = connectionStringProvider;
        }

        /// <summary>
        /// Register an app install's push notification registration.
        /// This includes adding an entry to the registration table and adding filters to the push notification hub service.
        /// Also call this to update an existing registration.
        /// </summary>
        /// <param name="processType">not needed (ignored)</param>
        /// <param name="userHandle">uniquely identifies the user</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="platformType">which mobile OS the registration is coming from</param>
        /// <param name="registrationId">Registration Id that the mobile OS gave to the app</param>
        /// <param name="language">language that the app is configured to use</param>
        /// <param name="lastUpdatedTime">the UTC DateTime on which this push registration was last updated on</param>
        /// <returns>Create registration task</returns>
        public async Task CreateRegistration(
            ProcessType processType,
            string userHandle,
            string appHandle,
            PlatformType platformType,
            string registrationId,
            string language,
            DateTime lastUpdatedTime)
        {
            // check inputs
            if (string.IsNullOrWhiteSpace(userHandle))
            {
                this.log.LogException("got empty user handle");
            }

            if (string.IsNullOrWhiteSpace(appHandle))
            {
                this.log.LogException("got empty app handle");
            }

            if (string.IsNullOrWhiteSpace(registrationId))
            {
                this.log.LogException("got empty registration ID");
            }

            if (this.HasRegistrationExpired(lastUpdatedTime))
            {
                this.log.LogException("got old registration");
            }

            if (this.IsRegistrationTooNew(lastUpdatedTime))
            {
                this.log.LogException("got registration that is too new");
            }

            // get the push notification hub interface for this app
            IPushNotificationsHub pushNotificationsHub = await this.GetPushNotificationsHub(platformType, appHandle);
            if (pushNotificationsHub == null)
            {
                this.log.LogException("did not find push notification hub for this app:" + appHandle);
            }

            // create a new registration in the hub interface for this app
            IEnumerable<string> tags = this.GetTags(userHandle, appHandle);
            string hubRegistrationId = await pushNotificationsHub.CreateRegistration(registrationId, tags);

            // keep a record of this in our table
            await this.pushRegistrationsStore.InsertPushRegistration(
                StorageConsistencyMode.Strong,
                userHandle,
                appHandle,
                platformType,
                registrationId,
                hubRegistrationId,
                language,
                lastUpdatedTime);
        }

        /// <summary>
        /// Unregister an app install's push notification registration.
        /// This includes removing the registration table entry and removing it from the notification hub service.
        /// </summary>
        /// <param name="processType">not needed (ignored)</param>
        /// <param name="userHandle">uniquely identifies the user</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="pushRegistrationFeedEntity">Push registration table entry</param>
        /// <returns>Delete registration task</returns>
        public async Task DeleteRegistration(
            ProcessType processType,
            string userHandle,
            string appHandle,
            IPushRegistrationFeedEntity pushRegistrationFeedEntity)
        {
            // check inputs
            if (string.IsNullOrWhiteSpace(userHandle))
            {
                this.log.LogException("got empty user handle");
            }

            if (string.IsNullOrWhiteSpace(appHandle))
            {
                this.log.LogException("got empty app handle");
            }

            // get the push notification hub interface for this app
            IPushNotificationsHub pushNotificationsHub = await this.GetPushNotificationsHub(pushRegistrationFeedEntity.PlatformType, appHandle);
            if (pushNotificationsHub == null)
            {
                this.log.LogException("did not find push notification hub for this app:" + appHandle);
            }

            // delete from the hub first, then delete from the table
            await pushNotificationsHub.DeleteRegistration(pushRegistrationFeedEntity.HubRegistrationId);
            await this.pushRegistrationsStore.DeletePushRegistration(
                StorageConsistencyMode.Strong,
                userHandle,
                appHandle,
                pushRegistrationFeedEntity.OSRegistrationId);
        }

        /// <summary>
        /// Get push registration entry from the table
        /// </summary>
        /// <param name="userHandle">uniquely identifies the user</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="registrationId">Registration Id that the mobile OS gave to the app</param>
        /// <returns>Push registration table entity</returns>
        public async Task<IPushRegistrationFeedEntity> ReadRegistration(string userHandle, string appHandle, string registrationId)
        {
            // check inputs
            if (string.IsNullOrWhiteSpace(userHandle))
            {
                this.log.LogException("got empty user handle");
            }

            if (string.IsNullOrWhiteSpace(appHandle))
            {
                this.log.LogException("got empty app handle");
            }

            if (string.IsNullOrWhiteSpace(registrationId))
            {
                this.log.LogException("got empty registration id");
            }

            return await this.pushRegistrationsStore.QueryPushRegistration(userHandle, appHandle, registrationId);
        }

        /// <summary>
        /// To be called when a user is deleted from this app.
        /// This will remove all registrations from table store and the push notification hub for this user and app combination.
        /// </summary>
        /// <param name="processType">not needed (ignored)</param>
        /// <param name="userHandle">uniquely identifies the user</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <returns>Delete user registrations task</returns>
        public async Task DeleteUserRegistrations(ProcessType processType, string userHandle, string appHandle)
        {
            // check inputs
            if (string.IsNullOrWhiteSpace(userHandle))
            {
                this.log.LogException("got empty user handle");
            }

            if (string.IsNullOrWhiteSpace(appHandle))
            {
                this.log.LogException("got empty app handle");
            }

            var pushRegistrationFeedEntities = await this.pushRegistrationsStore.QueryPushRegistrations(userHandle, appHandle);
            foreach (var pushRegistrationFeedEntity in pushRegistrationFeedEntities)
            {
                await this.DeleteRegistration(processType, userHandle, appHandle, pushRegistrationFeedEntity);
            }
        }

        /// <summary>
        /// Create a new push notification hub for a new app (or add/update a new OS platform to an existing hub)
        /// </summary>
        /// <param name="platformType">which mobile OS the app is registering for</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="path">
        /// Windows: Windows Package SID.
        /// Android: Leave this empty.
        /// iOS: iOS certificate path.</param>
        /// <param name="key">
        /// Windows: Windows secret key.
        /// Android: Android API key.
        /// iOS: iOS Certificate key.</param>
        /// <returns>Create hub task</returns>
        public async Task CreateHub(PlatformType platformType, string appHandle, string path, string key)
        {
            // key can be null only for iOS
            if (platformType != PlatformType.IOS && string.IsNullOrWhiteSpace(key))
            {
                this.log.LogException("got empty key");
            }

            if (string.IsNullOrWhiteSpace(appHandle))
            {
                this.log.LogException("got empty app handle");
            }

            string connectionString = await this.connectionStringProvider.GetPushNotificationsConnectionString(PushNotificationInstanceType.Default);
            IPushNotificationsHub pushNotificationsHub = this.GetPushNotificationsHub(
                platformType,
                connectionString,
                appHandle,
                this.registrationTtl,
                path,
                key);
            await pushNotificationsHub.Create();
        }

        /// <summary>
        /// Delete the Push Notification hub for an app
        /// </summary>
        /// <param name="platformType">which mobile OS the app is unregistering for</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <returns>Delete hub task</returns>
        public async Task DeleteHub(PlatformType platformType, string appHandle)
        {
            // check inputs
            if (string.IsNullOrWhiteSpace(appHandle))
            {
                this.log.LogException("got empty app handle");
            }

            string connectionString = await this.connectionStringProvider.GetPushNotificationsConnectionString(PushNotificationInstanceType.Default);
            IPushNotificationsHub pushNotificationsHub = this.GetPushNotificationsHub(
                platformType,
                connectionString,
                appHandle,
                TimeSpan.MinValue,
                null,
                null);

            // TODO: DELETE ALL TABLE ENTRIES. This is not convenient to do at this time
            // because the appropriate query is very expensive / not possible to do with
            // the current way in which we have structured the push notification registration
            // table. The main harm to not deleting entries is wasted space.
            //
            // Delete the push notifications hub for this app
            await pushNotificationsHub.Delete();
        }

        /// <summary>
        /// Delete the Push Notification hub for an app across all mobile OS's
        /// </summary>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <returns>Delete hub task</returns>
        public async Task DeleteHub(string appHandle)
        {
            foreach (PlatformType platform in Enum.GetValues(typeof(PlatformType)))
            {
                await this.DeleteHub(platform, appHandle);
            }
        }

        /// <summary>
        /// Send a push notification
        /// </summary>
        /// <param name="userHandle">uniquely identifies the user</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <param name="activityHandle">uniquely identifies the activity that the push notification is to be generated for</param>
        /// <param name="activityType">what type of activity has occurred</param>
        /// <param name="actorUserHandle">uniquely identifies the user that caused the activity</param>
        /// <param name="actedOnUserHandle">uniquely identifies the user that is the subject of the activity</param>
        /// <param name="actedOnContentType">what type of content is being acted on</param>
        /// <param name="actedOnContentHandle">uniquely identifies the content that has been acted on</param>
        /// <param name="createdTime">at what time the activity occurred</param>
        /// <returns>Send notification task</returns>
        public async Task SendNotification(
            string userHandle,
            string appHandle,
            string activityHandle,
            ActivityType activityType,
            string actorUserHandle,
            string actedOnUserHandle,
            ContentType actedOnContentType,
            string actedOnContentHandle,
            DateTime createdTime)
        {
            // check key inputs
            if (string.IsNullOrWhiteSpace(userHandle))
            {
                this.log.LogException("got empty user handle");
            }

            if (string.IsNullOrWhiteSpace(appHandle))
            {
                this.log.LogException("got empty app handle");
            }

            if (string.IsNullOrWhiteSpace(activityHandle))
            {
                this.log.LogException("got empty activity handle");
            }

            // get list of push registrations for this user
            var pushRegistrationFeedEntities = await this.GetActiveRegistrations(userHandle, appHandle);
            var pushRegistrationFeedEntitiesMasterApp = await this.GetActiveRegistrations(userHandle, MasterApp.AppHandle);

            // if there are any valid registrations, then convert the activity to a string
            string message = null;
            string messageForMasterApp = null;
            if ((pushRegistrationFeedEntities != null && pushRegistrationFeedEntities.Count > 0) ||
                (pushRegistrationFeedEntitiesMasterApp != null && pushRegistrationFeedEntitiesMasterApp.Count > 0))
            {
                ActivityView view = await this.viewsManager.GetActivityView(
                    activityHandle,
                    activityType,
                    actorUserHandle,
                    actedOnUserHandle,
                    actedOnContentType,
                    actedOnContentHandle,
                    appHandle,
                    createdTime,
                    null,
                    null);

                // sometimes an activity can be null, such as when the actor user has been deleted
                if (view == null)
                {
                    this.log.LogInformation("got null activity view for " + activityHandle);
                    return;
                }

                message = this.ActivityViewToString(view, userHandle);
                messageForMasterApp = this.AddAppNameToPushMessage(message, view.App);
            }

            // send it to the relevant app
            await this.SendNotification(message, pushRegistrationFeedEntities, appHandle, userHandle, activityHandle);

            // send it to the master app
            await this.SendNotification(messageForMasterApp, pushRegistrationFeedEntitiesMasterApp, MasterApp.AppHandle, userHandle, activityHandle);
        }

        /// <summary>
        /// Has this push notification registration expired?
        /// </summary>
        /// <param name="registrationTime">the UTC DateTime on which this push registration was last updated</param>
        /// <returns>true if expired</returns>
        public bool HasRegistrationExpired(DateTime registrationTime)
        {
            return registrationTime.Add(this.registrationTtl) < DateTime.UtcNow;
        }

        /// <summary>
        /// Is this push notification registration too far in the future?
        /// </summary>
        /// <param name="registrationTime">the UTC DateTime on which this push registration was last updated</param>
        /// <returns>true if more than 24 hours in the future</returns>
        public bool IsRegistrationTooNew(DateTime registrationTime)
        {
            TimeSpan skew = registrationTime - DateTime.UtcNow;
            return skew > this.registrationSkewAllowed;
        }

        /// <summary>
        /// Send a push notification
        /// </summary>
        /// <param name="message">the push notification message to send</param>
        /// <param name="registrations">the list of valid push registrations for this app, user combination</param>
        /// <param name="appHandle">uniquely identifies the app to send this message to</param>
        /// <param name="userHandle">uniquely identifies the user to send this message to</param>
        /// <param name="activityHandle">uniquely identifies the activity that the notification message is about</param>
        /// <returns>send notification task</returns>
        private async Task SendNotification(string message, IList<IPushRegistrationFeedEntity> registrations, string appHandle, string userHandle, string activityHandle)
        {
            // skip if there is nothing valid to send
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (registrations == null || registrations.Count == 0)
            {
                return;
            }

            var tags = this.GetTags(userHandle, appHandle);
            List<PlatformType> platforms = registrations.Select(registration => registration.PlatformType).Distinct().ToList();
            foreach (PlatformType platform in platforms)
            {
                IPushNotificationsHub pushNotificationsHub = await this.GetPushNotificationsHub(platform, appHandle);
                await pushNotificationsHub.SendNotification(tags, message, activityHandle);
            }
        }

        /// <summary>
        /// Get push notifications tags from user handle and app handle
        /// </summary>
        /// <param name="userHandle">uniquely identifies the user</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <returns>Tags that can be used to send a push to this user on this app</returns>
        private IEnumerable<string> GetTags(string userHandle, string appHandle)
        {
            HashSet<string> tags = new HashSet<string>();
            tags.Add("userHandle:" + userHandle);
            tags.Add("appHandle:" + appHandle);
            return tags;
        }

        /// <summary>
        /// Get push notifications hub
        /// </summary>
        /// <param name="platformType">Platform type</param>
        /// <param name="appHandle">App handle</param>
        /// <returns>Push notifications hub</returns>
        private async Task<IPushNotificationsHub> GetPushNotificationsHub(PlatformType platformType, string appHandle)
        {
            var pushNotificationsConfiguration = await this.appsStore.QueryPushNotificationsConfiguration(appHandle, platformType);
            if (pushNotificationsConfiguration != null && pushNotificationsConfiguration.Enabled)
            {
                string connectionString = await this.connectionStringProvider.GetPushNotificationsConnectionString(PushNotificationInstanceType.Default);
                return this.GetPushNotificationsHub(
                    platformType,
                    connectionString,
                    appHandle,
                    this.registrationTtl,
                    pushNotificationsConfiguration.Path,
                    pushNotificationsConfiguration.Key);
            }

            return null;
        }

        /// <summary>
        /// Get push notifications hub
        /// </summary>
        /// <param name="platformType">Platform type</param>
        /// <param name="connectionString">Connection string</param>
        /// <param name="appHandle">App handle</param>
        /// <param name="notificationExpiration">Notification expiration</param>
        /// <param name="path">Push notifications path</param>
        /// <param name="key">Push notifications key</param>
        /// <returns>Push notifications hub</returns>
        private IPushNotificationsHub GetPushNotificationsHub(
            PlatformType platformType,
            string connectionString,
            string appHandle,
            TimeSpan notificationExpiration,
            string path,
            string key)
        {
            switch (platformType)
            {
                case PlatformType.Windows:
                    return new WindowsPushNotificationsHub(
                        this.log,
                        connectionString,
                        appHandle,
                        notificationExpiration,
                        path,
                        key);
                case PlatformType.Android:
                    return new AndroidPushNotificationsHub(
                        this.log,
                        connectionString,
                        appHandle,
                        notificationExpiration,
                        key);
                case PlatformType.IOS:
                    return new ApplePushNotificationsHub(
                        this.log,
                        connectionString,
                        appHandle,
                        notificationExpiration,
                        path,
                        key);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets a list of active push registration entities
        /// </summary>
        /// <param name="userHandle">uniquely identifies the user</param>
        /// <param name="appHandle">uniquely identifies the app</param>
        /// <returns>List of active registrations</returns>
        private async Task<IList<IPushRegistrationFeedEntity>> GetActiveRegistrations(string userHandle, string appHandle)
        {
            // get list of app installations for this user
            IList<IPushRegistrationFeedEntity> pushRegistrationFeedEntities = await this.pushRegistrationsStore.QueryPushRegistrations(userHandle, appHandle);
            if (pushRegistrationFeedEntities != null && pushRegistrationFeedEntities.Count > 0)
            {
                foreach (IPushRegistrationFeedEntity pushRegistrationFeedEntity in pushRegistrationFeedEntities)
                {
                    // remove any expired push registrations
                    if (this.HasRegistrationExpired(pushRegistrationFeedEntity.LastUpdatedTime))
                    {
                        this.log.LogInformation("removing expired registration for userhandle: " + userHandle + ", appHandle: " + appHandle);
                        await this.DeleteRegistration(ProcessType.Backend, userHandle, appHandle, pushRegistrationFeedEntity);
                        pushRegistrationFeedEntities.Remove(pushRegistrationFeedEntity);
                    }
                }
            }

            // return whatever remains
            return pushRegistrationFeedEntities;
        }

        /// <summary>
        /// Get push notifications message string from activity view
        /// </summary>
        /// <param name="activity">Activity view</param>
        /// <param name="userHandle">which user this push notification is being sent to</param>
        /// <returns>Push notifications message string</returns>
        private string ActivityViewToString(ActivityView activity, string userHandle)
        {
            // check inputs
            if (activity == null)
            {
                this.log.LogException("got null activity");
            }

            if (string.IsNullOrWhiteSpace(userHandle))
            {
                this.log.LogException("got null userHandle");
            }

            string result = string.Empty;

            // first add the actor user name(s)
            switch ((ActivityType)activity.ActivityType)
            {
                // aggregate notification
                case ActivityType.Like:
                case ActivityType.Comment:
                case ActivityType.Reply:
                    if (activity.ActorUsers == null || activity.ActorUsers.Count < 1)
                    {
                        this.log.LogException("unexpected ActorUsers");
                    }

                    // add usernames
                    result += this.UserListToString(activity.ActorUsers, userHandle);

                    // do not send a notification if the result is null
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        // this can happen when the user is deleted before the push notification has been processed
                        this.log.LogInformation("got an empty user list, perhaps due to a race condition");
                        return null;
                    }

                    // add remaining count of users
                    if (activity.TotalActions > activity.ActorUsers.Count)
                    {
                        result += " & " + (activity.TotalActions - activity.ActorUsers.Count) + " other user";
                    }

                    if (activity.TotalActions > (activity.ActorUsers.Count + 1))
                    {
                        result += "s";
                    }

                    break;

                // non-aggregate notifications
                case ActivityType.CommentPeer:
                case ActivityType.ReplyPeer:
                case ActivityType.Following:
                case ActivityType.FollowRequest:
                case ActivityType.FollowAccept:
                    if (activity.ActorUsers == null || activity.ActorUsers.Count != 1 || activity.ActorUsers[0] == null)
                    {
                        // this can happen when the user is deleted before the push notification has been processed
                        this.log.LogInformation("got a null user, perhaps due to a race condition");
                        return null;
                    }

                    result += this.GetUserDisplayName(activity.ActorUsers[0]);

                    // sometimes, the user may be invalid
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        this.log.LogInformation("exiting due to empty actor string");
                        return null;
                    }

                    break;

                default:
                    this.log.LogException("got unusual activity type " + activity.ActivityType);
                    break;
            }

            // now add the action
            switch ((ActivityType)activity.ActivityType)
            {
                case ActivityType.Like:
                    result += " liked";
                    break;
                case ActivityType.Comment:
                    result += " commented on";
                    break;
                case ActivityType.Reply:
                    result += " replied to";
                    break;
                case ActivityType.CommentPeer:
                case ActivityType.ReplyPeer:
                    result += " mentioned";
                    break;
                case ActivityType.Following:
                    result += " is following";
                    break;
                case ActivityType.FollowRequest:
                    result += " requested to follow";
                    break;
                case ActivityType.FollowAccept:
                    result += " accepted a follow request from";
                    break;
                default:
                    this.log.LogException("got unusual activity type " + activity.ActivityType);
                    break;
            }

            // now add the acted on owner
            switch ((ActivityType)activity.ActivityType)
            {
                case ActivityType.Like:
                case ActivityType.Comment:
                case ActivityType.Reply:
                    if (activity.ActedOnUser == null || activity.ActedOnUser.UserHandle == userHandle)
                    {
                        result += " your";
                    }
                    else
                    {
                        string actedOnUserName = this.GetUserDisplayName(activity.ActedOnUser);
                        if (string.IsNullOrWhiteSpace(actedOnUserName))
                        {
                            // this can happen when the acted on user has been deleted
                            this.log.LogInformation("got null acted on user name");
                            return null;
                        }

                        result += " " + actedOnUserName + "'s";
                    }

                    break;
                case ActivityType.CommentPeer:
                case ActivityType.ReplyPeer:
                    result += " you in a";
                    break;
                case ActivityType.Following:
                case ActivityType.FollowRequest:
                case ActivityType.FollowAccept:
                    if (activity.ActedOnUser == null || activity.ActedOnUser.UserHandle == userHandle)
                    {
                        result += " you";
                    }
                    else
                    {
                        string actedOnUserName = this.GetUserDisplayName(activity.ActedOnUser);
                        if (string.IsNullOrWhiteSpace(actedOnUserName))
                        {
                            // this can happen when the acted on user has been deleted
                            this.log.LogInformation("got null acted on user name");
                            return null;
                        }

                        result += " " + actedOnUserName;
                    }

                    break;
                default:
                    this.log.LogException("got unusual activity type " + activity.ActivityType);
                    break;
            }

            // now add the acted on content type
            switch ((ActivityType)activity.ActivityType)
            {
                case ActivityType.Like:
                case ActivityType.Comment:
                case ActivityType.Reply:
                case ActivityType.CommentPeer:
                case ActivityType.ReplyPeer:
                    // type of content
                    if ((ContentType)activity.ActedOnContent.ContentType == ContentType.Comment)
                    {
                        result += " comment";
                    }
                    else if ((ContentType)activity.ActedOnContent.ContentType == ContentType.Reply)
                    {
                        result += " reply";
                    }
                    else if ((ContentType)activity.ActedOnContent.ContentType == ContentType.Topic)
                    {
                        result += " topic";
                    }
                    else
                    {
                        this.log.LogException("unexpected ActedOnContent Type " + activity.ActedOnContent.ContentType);
                    }

                    break;
                case ActivityType.Following:
                case ActivityType.FollowRequest:
                case ActivityType.FollowAccept:
                    // do nothing
                    break;
                default:
                    this.log.LogException("got unusual activity type " + activity.ActivityType);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Adds the name of the app to the push notification message
        /// </summary>
        /// <param name="message">push notification message</param>
        /// <param name="app">describes the app that generated the activity</param>
        /// <returns>message with the app name in it</returns>
        private string AddAppNameToPushMessage(string message, AppCompactView app)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            // this can happen in a race condition where the app is deleted before the push notification has been generated
            if (app == null || string.IsNullOrWhiteSpace(app.Name))
            {
                this.log.LogInformation("got null app");
                return null;
            }

            // add the app name
            message += " in " + app.Name;
            return message;
        }

        /// <summary>
        /// A user name to show in push notifications.
        /// First name in most cases, otherwise last name.
        /// If neither is available, then it will be null.
        /// </summary>
        /// <param name="user">information about the user</param>
        /// <returns>string to represent this user</returns>
        private string GetUserDisplayName(UserCompactView user)
        {
            if (user == null)
            {
                return null;
            }

            // use the first name if it is present
            if (!string.IsNullOrWhiteSpace(user.FirstName))
            {
                return user.FirstName;
            }

            // otherwise use the last name if it is present
            if (!string.IsNullOrWhiteSpace(user.LastName))
            {
                return user.LastName;
            }

            // otherwise skip this user
            return null;
        }

        /// <summary>
        /// Convert a list of users into a string of usernames
        /// </summary>
        /// <param name="users">list of users</param>
        /// <param name="myUserHandle">the calling user's handle</param>
        /// <returns>string of usernames separated by and</returns>
        private string UserListToString(List<UserCompactView> users, string myUserHandle)
        {
            string answer = string.Empty;

            // dedup the list of users
            List<UserCompactView> uniqueUsers = users.Distinct().ToList();

            foreach (UserCompactView user in uniqueUsers)
            {
                // skip empty users
                // this can happen because of a race condition where the user was deleted before this notification was processed
                if (user == null)
                {
                    this.log.LogInformation("got a null user, perhaps due to a race condition");
                    continue;
                }

                // separator between multiple users
                if (!string.IsNullOrWhiteSpace(answer))
                {
                    answer += " & ";
                }

                // replacing your username with you
                if (user.UserHandle == myUserHandle)
                {
                    if (string.IsNullOrWhiteSpace(answer))
                    {
                        answer += "You";
                    }
                    else
                    {
                        answer += "you";
                    }
                }
                else
                {
                    string userDisplayName = this.GetUserDisplayName(user);
                    if (!string.IsNullOrWhiteSpace(userDisplayName))
                    {
                        answer += userDisplayName;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return answer;
        }
    }
}
