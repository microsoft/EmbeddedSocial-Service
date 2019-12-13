// <copyright file="WindowsPushNotificationsHub.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.PushNotificationsHub
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Microsoft.Azure.NotificationHubs;
    using SocialPlus.Logging;

    /// <summary>
    /// Windows specific support for push notification hub
    /// </summary>
    public class WindowsPushNotificationsHub : PushNotificationsHubBase, IPushNotificationsHub
    {
        /// <summary>
        /// Package SID provided by Microsoft to the app developer
        /// </summary>
        private readonly string packageSid;

        /// <summary>
        /// Secret key provided by Microsoft to the app developer
        /// </summary>
        private readonly string secretKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsPushNotificationsHub"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="connectionString">Connection string to the Azure Push Notification Hub service</param>
        /// <param name="appId">Uniquely identifies the app</param>
        /// <param name="registrationTtl">Push Registrations expire after this duration</param>
        /// <param name="packageSid">Package SID provided by Microsoft to the app developer</param>
        /// <param name="secretKey">Secret key provided by Microsoft to the app developer</param>
        public WindowsPushNotificationsHub(ILog log, string connectionString, string appId, TimeSpan registrationTtl, string packageSid, string secretKey)
            : base(log, connectionString, appId, registrationTtl)
        {
            this.packageSid = packageSid;
            this.secretKey = secretKey;
        }

        /// <summary>
        /// Create a registration for an app instance
        /// </summary>
        /// <param name="hubClient">Notification hub client for this app</param>
        /// <param name="clientRegistrationId">Client registration id from Windows for this app instance</param>
        /// <param name="tags">Registration tags that describe the user and the app</param>
        /// <returns>Hub registration id</returns>
        protected override async Task<RegistrationDescription> CreateRegistrationAsync(NotificationHubClient hubClient, string clientRegistrationId, IEnumerable<string> tags)
        {
            return await hubClient.CreateWindowsNativeRegistrationAsync(clientRegistrationId, tags);
        }

        /// <summary>
        /// Get the registration for an app instance
        /// </summary>
        /// <param name="hubClient">Notification hub client for this app</param>
        /// <param name="hubRegistrationId">Hub registration id for this app instance</param>
        /// <returns>Registration description for the app instance</returns>
        protected override async Task<RegistrationDescription> GetRegistrationAsync(NotificationHubClient hubClient, string hubRegistrationId)
        {
            return await hubClient.GetRegistrationAsync<WindowsRegistrationDescription>(hubRegistrationId);
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
            // form toast string
            XElement[] payload =
            {
                new XElement("text", new XAttribute("id", "1"), message),
                new XElement("publisher", PublisherId),
                new XElement("activityHandle", activityHandle)
            };

            XElement xml = new XElement("toast", new XElement("visual", new XElement("binding", new XAttribute("template", "ToastText01"), payload)));
            string toast = xml.ToString(SaveOptions.DisableFormatting);

            // submit it
            this.Log.LogInformation("sending to: " + tagExpression + " message: " + toast + " activityHandle: " + activityHandle);
            return await hubClient.SendWindowsNativeNotificationAsync(toast, tagExpression);
        }

        /// <summary>
        /// Insert credentials into the hub description for this app
        /// </summary>
        /// <param name="hubDescription">Hub description for this app</param>
        protected override void CreateCredentialForHubDescription(NotificationHubDescription hubDescription)
        {
            // check inputs
            if (string.IsNullOrWhiteSpace(this.packageSid))
            {
                this.Log.LogException("got empty packageSid");
            }

            if (string.IsNullOrWhiteSpace(this.secretKey))
            {
                this.Log.LogException("got empty secretKey");
            }

            WnsCredential wnsCredential = new WnsCredential(this.packageSid, this.secretKey);
            hubDescription.WnsCredential = wnsCredential;
        }

        /// <summary>
        /// Remove credentials from the hub description for this app
        /// </summary>
        /// <param name="hubDescription">Hub description for this app</param>
        protected override void DeleteCredentialForHubDescription(NotificationHubDescription hubDescription)
        {
            hubDescription.WnsCredential = null;
        }
    }
}
