// <copyright file="ApplePushNotificationsHub.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.PushNotificationsHub
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Azure.NotificationHubs;
    using Newtonsoft.Json.Linq;
    using SocialPlus.Logging;

    /// <summary>
    /// iOS specific support for push notification hub
    /// </summary>
    public class ApplePushNotificationsHub : PushNotificationsHubBase, IPushNotificationsHub
    {
        /// <summary>
        /// Certificate path provided by Apple to the app developer
        /// </summary>
        private readonly string certificatePath;

        /// <summary>
        /// Certificate key provided by Apple to the app developer
        /// </summary>
        private readonly string certificateKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplePushNotificationsHub"/> class
        /// </summary>
        /// <param name="log">Logger</param>
        /// <param name="connectionString">Connection string to the Azure Push Notification Hub service</param>
        /// <param name="appId">Uniquely identifies the app</param>
        /// <param name="registrationTtl">Push Registrations expire after this duration</param>
        /// <param name="certificatePath">Certificate path provided by iOS to the app developer</param>
        /// <param name="certificateKey">Certificate key provided by iOS to the app developer</param>
        public ApplePushNotificationsHub(ILog log, string connectionString, string appId, TimeSpan registrationTtl, string certificatePath, string certificateKey = null)
            : base(log, connectionString, appId, registrationTtl)
        {
            this.certificatePath = certificatePath;
            this.certificateKey = certificateKey;
        }

        /// <summary>
        /// Create a registration for an app instance
        /// </summary>
        /// <param name="hubClient">Notification hub client for this app</param>
        /// <param name="clientRegistrationId">Client registration id from iOS for this app instance</param>
        /// <param name="tags">Registration tags that describe the user and the app</param>
        /// <returns>Hub registration id</returns>
        protected override async Task<RegistrationDescription> CreateRegistrationAsync(NotificationHubClient hubClient, string clientRegistrationId, IEnumerable<string> tags)
        {
            return await hubClient.CreateAppleNativeRegistrationAsync(clientRegistrationId, tags);
        }

        /// <summary>
        /// Get the registration for an app instance
        /// </summary>
        /// <param name="hubClient">Notification hub client for this app</param>
        /// <param name="hubRegistrationId">Hub registration id for this app instance</param>
        /// <returns>Registration description for the app instance</returns>
        protected override async Task<RegistrationDescription> GetRegistrationAsync(NotificationHubClient hubClient, string hubRegistrationId)
        {
            return await hubClient.GetRegistrationAsync<AppleRegistrationDescription>(hubRegistrationId);
        }

        /// <summary>
        /// Properly form and send a push notification
        /// </summary>
        /// <param name="hubClient">Notification hub client for this app</param>
        /// <param name="tagExpression">Tag expression that describes which app instances to send this to</param>
        /// <param name="message">Notification message to send</param>
        /// <param name="activityHandle">Uniquely identifies the activity that the notification message is about</param>
        /// <returns>Notification outcome</returns>
        protected override async Task<NotificationOutcome> SendNotificationAsync(NotificationHubClient hubClient, string tagExpression, string message, string activityHandle)
        {
            // form the notification payload
            JProperty[] properties =
            {
                new JProperty("alert", message),
                new JProperty("publisher", PublisherId),
                new JProperty("activityHandle", activityHandle)
            };

            JObject payload = new JObject(new JProperty("aps", new JObject(properties)));
            string alert = payload.ToString(Newtonsoft.Json.Formatting.None);

            // submit it
            this.Log.LogInformation("sending to: " + tagExpression + " message: " + alert + " activityHandle: " + activityHandle);
            return await hubClient.SendAppleNativeNotificationAsync(alert, tagExpression);
        }

        /// <summary>
        /// Insert credentials into the hub description for this app
        /// </summary>
        /// <param name="hubDescription">Hub description for this app</param>
        protected override void CreateCredentialForHubDescription(NotificationHubDescription hubDescription)
        {
            // check inputs
            if (string.IsNullOrWhiteSpace(this.certificatePath))
            {
                this.Log.LogException("got empty certificate path");
            }

            ApnsCredential apnsCredential = new ApnsCredential(this.certificatePath, this.certificateKey);
            hubDescription.ApnsCredential = apnsCredential;
        }

        /// <summary>
        /// Delete credentials from the hub description for this app
        /// </summary>
        /// <param name="hubDescription">Hub description for this app</param>
        protected override void DeleteCredentialForHubDescription(NotificationHubDescription hubDescription)
        {
            hubDescription.ApnsCredential = null;
        }
    }
}
