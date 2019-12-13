// <copyright file="PushNotificationsHubBase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.PushNotificationsHub
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Azure.NotificationHubs;
    using SocialPlus.Logging;

    /// <summary>
    /// Implements some parts of Push Notifications Hub that are common across the different mobile OS specific classes.
    /// </summary>
    public abstract class PushNotificationsHubBase
    {
        /// <summary>
        /// Push notifications will include this string to identify the source of push notifications
        /// </summary>
        protected const string PublisherId = "EmbeddedSocial";

        /// <summary>
        /// Connection string to the Azure Push Notification Hub service
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// Access key for the Azure Push Notification Hub service
        /// </summary>
        private readonly string accessKey;

        /// <summary>
        /// Uniquely identifies the app
        /// </summary>
        private readonly string appId;

        /// <summary>
        /// Push Registrations expire after this duration
        /// </summary>
        private readonly TimeSpan registrationTtl;

        /// <summary>
        /// Connection to the Azure Push Notifications Service Bus
        /// </summary>
        private NamespaceManager pushNamespaceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PushNotificationsHubBase"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="connectionString">Connection string to the Azure Push Notification Hub service</param>
        /// <param name="appId">Uniquely identifies the app</param>
        /// <param name="registrationTtl">Push Registrations expire after this duration</param>
        protected PushNotificationsHubBase(ILog log, string connectionString, string appId, TimeSpan registrationTtl)
        {
            this.Log = log;

            // check inputs
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                this.Log.LogException("got empty connection string");
            }

            if (string.IsNullOrWhiteSpace(appId))
            {
                this.Log.LogException("got empty app ID");
            }

            this.connectionString = connectionString;
            this.appId = appId;
            this.registrationTtl = registrationTtl;
            this.accessKey = this.ExtractAccessKey(connectionString);
        }

        /// <summary>
        /// Gets log
        /// </summary>
        protected ILog Log { get; private set; }

        /// <summary>
        /// Gets connection to the Azure Push Notifications Service Bus
        /// </summary>
        private NamespaceManager PushNamespaceManager
        {
            get
            {
                // if this has already been setup, return it
                if (this.pushNamespaceManager != null)
                {
                    return this.pushNamespaceManager;
                }

                // create connection client
                this.Log.LogInformation("creating connection to Azure Hub");
                this.pushNamespaceManager = NamespaceManager.CreateFromConnectionString(this.connectionString);
                if (this.pushNamespaceManager == null)
                {
                    this.Log.LogException("failed to create a NamespaceManager");
                }

                return this.pushNamespaceManager;
            }
        }

        /// <summary>
        /// Create or update push notification hub for the app
        /// </summary>
        /// <returns>Create or update push notification hub task</returns>
        public async Task Create()
        {
            // get existing hub description or create a new one
            bool newHub = false;
            NotificationHubDescription hubDescription = await this.GetHubDescriptionForApp(this.appId);
            if (hubDescription == null || hubDescription.Path != this.appId)
            {
                hubDescription = new NotificationHubDescription(this.appId);
                newHub = true;
            }

            // sets the credentials that are mobile OS specific
            this.CreateCredentialForHubDescription(hubDescription);

            // set some common values
            hubDescription.RegistrationTtl = this.registrationTtl;
            hubDescription.IsDisabled = false;

            // set the access passwords if accessKey is valid
            if (!string.IsNullOrWhiteSpace(this.accessKey))
            {
                hubDescription.SetDefaultAccessPasswords(this.accessKey, this.accessKey);
            }

            // create it or update it
            NotificationHubDescription newHubDescription;
            if (newHub)
            {
                newHubDescription = await this.PushNamespaceManager.CreateNotificationHubAsync(hubDescription);
            }
            else
            {
                newHubDescription = await this.PushNamespaceManager.UpdateNotificationHubAsync(hubDescription);
            }

            // check the new hub matches the old one
            if ((newHubDescription == null) ||
                (newHubDescription.Path != hubDescription.Path) ||
                (newHubDescription.RegistrationTtl != hubDescription.RegistrationTtl) ||
                newHubDescription.IsDisabled)
            {
                this.Log.LogException("new hub does not match what was requested");
            }
        }

        /// <summary>
        /// Delete notification hub for this app across all mobile OS's
        /// </summary>
        /// <returns>Delete notification hub task</returns>
        public async Task Delete()
        {
            // get existing hub description
            NotificationHubDescription hubDescription = await this.GetHubDescriptionForApp(this.appId);
            NotificationHubDescription newHubDescription = null;
            if (hubDescription != null && hubDescription.Path == this.appId)
            {
                // deletes the credentials that are mobile OS specific
                this.DeleteCredentialForHubDescription(hubDescription);

                // update it
                newHubDescription = await this.PushNamespaceManager.UpdateNotificationHubAsync(hubDescription);
            }

            // if no mobile OSes are setup, then delete it
            if (newHubDescription != null &&
                !AreAnyCredentialsForHubDescriptionConfigured(newHubDescription))
            {
                await this.PushNamespaceManager.DeleteNotificationHubAsync(this.appId);
            }
        }

        /// <summary>
        /// Create a push notification hub registration for a new app instance with the push notification hub for this app
        /// </summary>
        /// <param name="clientRegistrationId">Push notification registration id from the mobile OS</param>
        /// <param name="tags">Registration tags</param>
        /// <returns>Hub registration id</returns>
        public async Task<string> CreateRegistration(string clientRegistrationId, IEnumerable<string> tags)
        {
            // check input
            if (string.IsNullOrWhiteSpace(clientRegistrationId))
            {
                this.Log.LogException("got empty client registration ID");
            }

            // get the hub client; this call does not hit the network
            NotificationHubClient hubClient = NotificationHubClient.CreateClientFromConnectionString(this.connectionString, this.appId);

            // create the registration; this call will hit the network
            RegistrationDescription registrationDescription = await this.CreateRegistrationAsync(hubClient, clientRegistrationId, tags);
            if (registrationDescription == null || registrationDescription.IsReadOnly ||
                string.IsNullOrWhiteSpace(registrationDescription.RegistrationId))
            {
                this.Log.LogException("did not get registration back from Azure Hub");
            }

            return registrationDescription.RegistrationId;
        }

        /// <summary>
        /// Delete a push notification hub registration for an existing app instance
        /// </summary>
        /// <param name="hubRegistrationId">Hub registration id</param>
        /// <returns>Delete registration task</returns>
        public async Task DeleteRegistration(string hubRegistrationId)
        {
            // check input
            if (string.IsNullOrWhiteSpace(hubRegistrationId))
            {
                this.Log.LogException("got empty hub registration ID");
            }

            // get the hub client; this call does not hit the network
            NotificationHubClient hubClient = NotificationHubClient.CreateClientFromConnectionString(this.connectionString, this.appId);

            // get the registration & delete it. These calls will hit the network
            RegistrationDescription registrationDescription = await this.GetRegistrationAsync(hubClient, hubRegistrationId);
            if (registrationDescription != null)
            {
                await hubClient.DeleteRegistrationAsync(registrationDescription);
            }
        }

        /// <summary>
        /// Send a push notification through this notification hub
        /// </summary>
        /// <param name="tags">Tags to send notification to</param>
        /// <param name="message">Notification message to send</param>
        /// <param name="activityHandle">Uniquely identifies the activity that the notification message is about</param>
        /// <returns>Send notification task. True if success.</returns>
        public async Task<bool> SendNotification(IEnumerable<string> tags, string message, string activityHandle)
        {
            // check inputs
            if (string.IsNullOrWhiteSpace(message))
            {
                this.Log.LogException("got empty message to send");
            }
            else if (string.IsNullOrWhiteSpace(activityHandle))
            {
                this.Log.LogException("got empty activityHandle to send");
            }

            // form filter string
            string tagExpression = string.Empty;
            bool isFirstTag = true;
            foreach (string tag in tags)
            {
                if (!isFirstTag)
                {
                    tagExpression += " && ";
                }

                tagExpression += "(" + tag + ")";
                isFirstTag = false;
            }

            // get the push notification hub client for this app; this call does not hit the network
            NotificationHubClient hubClient = NotificationHubClient.CreateClientFromConnectionString(this.connectionString, this.appId);

            // send it off to a mobile OS specific method
            NotificationOutcome outcome = await this.SendNotificationAsync(hubClient, tagExpression, message, activityHandle);

            // check response & return appropriate value
            if (outcome.Failure != 0 || outcome.State != NotificationOutcomeState.Enqueued)
            {
                this.Log.LogError("got error from Azure Service; failure: " + outcome.Failure.ToString() + ", state: " + System.Enum.GetName(typeof(NotificationOutcomeState), outcome.State));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Send a push notification
        /// </summary>
        /// <param name="hubClient">Push notification hub client</param>
        /// <param name="tagExpression">Tag expression to send it to</param>
        /// <param name="message">Notification message to send</param>
        /// <param name="activityHandle">Uniquely identifies the activity that the notification message is about</param>
        /// <returns>Notification outcome</returns>
        protected abstract Task<NotificationOutcome> SendNotificationAsync(NotificationHubClient hubClient, string tagExpression, string message, string activityHandle);

        /// <summary>
        /// Get push notification hub registration for an app instance
        /// </summary>
        /// <param name="hubClient">Push notification hub client</param>
        /// <param name="hubRegistrationId">Push notification hub registration id</param>
        /// <returns>Registration description</returns>
        protected abstract Task<RegistrationDescription> GetRegistrationAsync(NotificationHubClient hubClient, string hubRegistrationId);

        /// <summary>
        /// Register a new app install's push notification settings
        /// </summary>
        /// <param name="hubClient">Push notification hub client for this app</param>
        /// <param name="clientRegistrationId">push registration id from the mobile OS</param>
        /// <param name="tags">registration tags</param>
        /// <returns>Hub registration description</returns>
        protected abstract Task<RegistrationDescription> CreateRegistrationAsync(NotificationHubClient hubClient, string clientRegistrationId, IEnumerable<string> tags);

        /// <summary>
        /// Fill in mobile OS specific credentials into push notification hub description
        /// </summary>
        /// <param name="hubDescription">Push notification hub description</param>
        protected abstract void CreateCredentialForHubDescription(NotificationHubDescription hubDescription);

        /// <summary>
        /// Remove mobile OS specific credentials from push notification hub description
        /// </summary>
        /// <param name="hubDescription">Push notification hub description</param>
        protected abstract void DeleteCredentialForHubDescription(NotificationHubDescription hubDescription);

        /// <summary>
        /// Are any mobile OS specific credentials configured in push notification hub description?
        /// </summary>
        /// <param name="hubDescription">Push notification hub description</param>
        /// <returns>true if any mobile OS is configured</returns>
        private static bool AreAnyCredentialsForHubDescriptionConfigured(NotificationHubDescription hubDescription)
        {
            return hubDescription.ApnsCredential != null ||
                   hubDescription.BaiduCredential != null ||
                   hubDescription.GcmCredential != null ||
                   hubDescription.MpnsCredential != null ||
                   hubDescription.WnsCredential != null;
        }

        /// <summary>
        /// Extract access key from connection string
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Access key</returns>
        private string ExtractAccessKey(string connectionString)
        {
            // check the connection string
            const string KeyName = "SharedAccessKey=";
            if (!connectionString.Contains(KeyName))
            {
                this.Log.LogException("connectionString does not have " + KeyName);
            }

            // extract it
            int index = connectionString.IndexOf(KeyName, StringComparison.Ordinal);
            string possibleKey = connectionString.Substring(index + KeyName.Length);

            // make sure there is nothing else at the end
            if (possibleKey.Contains(";"))
            {
                int index2 = possibleKey.IndexOf(";", StringComparison.Ordinal);
                possibleKey = possibleKey.Substring(0, index2);
            }

            // check for trailing =
            if (!possibleKey.EndsWith("="))
            {
                this.Log.LogException("key does not end in =");
            }

            return possibleKey;
        }

        /// <summary>
        /// Retrieve a hub description for an app. This will hit the network.
        /// </summary>
        /// <param name="appId">uniquely identifies the app</param>
        /// <returns>Push Notification Hub Description for the app or null if not found</returns>
        private async Task<NotificationHubDescription> GetHubDescriptionForApp(string appId)
        {
            // get the hub description
            NotificationHubDescription hubDescription = null;
            try
            {
                // this call hits the network
                hubDescription = await this.PushNamespaceManager.GetNotificationHubAsync(appId);
            }
            catch (Exception e)
            {
                // does the app hub not exist?
                if (!string.IsNullOrWhiteSpace(e.Message) &&
                    e.Message.StartsWith("The messaging entity ") &&
                    e.Message.EndsWith("could not be found."))
                {
                    this.Log.LogInformation("did not find requested notification hub " + appId);
                }
                else
                {
                    this.Log.LogException(e);
                }
            }

            return hubDescription;
        }
    }
}
