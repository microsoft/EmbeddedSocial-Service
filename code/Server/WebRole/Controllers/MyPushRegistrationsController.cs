//-----------------------------------------------------------------------
// <copyright file="MyPushRegistrationsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class MyPushRegistrationsController.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.WebRole.Versioning;
    using SocialPlus.Server.WebRoleCommon;

    /// <summary>
    /// API calls to register and unregister for push notifications
    /// </summary>
    [RoutePrefix("users/me/push_registrations")]
    public class MyPushRegistrationsController : BaseController
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Push notifications manager
        /// </summary>
        private readonly IPushNotificationsManager pushNotificationsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyPushRegistrationsController"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="pushNotificationsManager">Push notifications manager</param>
        public MyPushRegistrationsController(ILog log, IPushNotificationsManager pushNotificationsManager)
        {
            this.log = log;
            this.pushNotificationsManager = pushNotificationsManager;
        }

        /// <summary>
        /// Register for push notifications or update an existing registration
        /// </summary>
        /// <remarks>
        /// A push notification will be generated and sent for each activity in my
        /// notifications feed where the unread status is true.
        /// If multiple devices register for push notifications, then all those devices
        /// will get push notifications.
        /// Each push notification will have three components: (1) a human readable string
        /// that the mobile OS should display to the user, (2) a "publisher" string with
        /// value "EmbeddedSocial" to identify that the push notification came from
        /// this service, and (3) an "activityHandle" that identifies which activity
        /// in the notification feed this push notification is for.
        /// </remarks>
        /// <param name="platform">Platform type</param>
        /// <param name="registrationId">Unique registration ID provided by the mobile OS.
        /// You must URL encode the registration ID.
        /// For Android, this is the GCM or FCM registration ID.
        /// For Windows, this is the PushNotificationChannel URI.
        /// For iOS, this is the device token.</param>
        /// <param name="request">Put push registration request</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="400">Bad request. The request structure fields are not valid.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{platform}/{registrationId}")]
        [HttpPut]
        [VersionRange("v0.6-Cur")]
        public async Task<IHttpActionResult> PutPushRegistration(
            PlatformType platform,
            string registrationId,
            [FromBody]PutPushRegistrationRequest request)
        {
            string className = "MyPushRegistrationsController";
            string methodName = "PutPushRegistration";
            string logEntry = $"Platform = {platform}, RegistrationId = {registrationId}, LastUpdatedTime = {request?.LastUpdatedTime}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // check that the last updated time can be parsed as an ISO 8601 string
            DateTime lastUpdatedTime;
            if (!DateTime.TryParse(request.LastUpdatedTime, out lastUpdatedTime))
            {
                return this.BadRequest(ResponseStrings.DateTimeMalformed);
            }

            // check that the last updated time is not too far into the future
            if (this.pushNotificationsManager.IsRegistrationTooNew(lastUpdatedTime))
            {
                return this.BadRequest(ResponseStrings.PushRegistrationTimeTooNew);
            }

            // check that the last updated time is not too old
            if (this.pushNotificationsManager.HasRegistrationExpired(lastUpdatedTime))
            {
                return this.BadRequest(ResponseStrings.PushRegistrationTimeExpired);
            }

            // decode the registrationId
            string decodedRegistrationId = HttpContext.Current.Server.UrlDecode(registrationId);

            await this.pushNotificationsManager.CreateRegistration(
                ProcessType.Frontend,
                this.UserHandle,
                this.AppHandle,
                platform,
                decodedRegistrationId,
                request.Language,
                lastUpdatedTime);

            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }

        /// <summary>
        /// Unregister from push notifications
        /// </summary>
        /// <param name="platform">Platform type</param>
        /// <param name="registrationId">Unique registration ID provided by the mobile OS.
        /// You must URL encode the registration ID.
        /// For Android, this is the GCM registration ID.
        /// For Windows, this is the PushNotificationChannel URI.
        /// For iOS, this is the device token.</param>
        /// <returns>No content on success</returns>
        /// <response code="204">No Content. The request was successful.</response>
        /// <response code="401">Unauthorized. The user or the app is not authorized.</response>
        /// <response code="404">Not Found. The push registrationId was not found.</response>
        /// <response code="500">Internal Server Error. The server raised an exception.</response>
        [Route("{platform}/{registrationId}")]
        [HttpDelete]
        [VersionRange("v0.6-Cur")]
        public async Task<IHttpActionResult> DeletePushRegistration(PlatformType platform, string registrationId)
        {
            string className = "MyPushRegistrationsController";
            string methodName = "DeletePushRegistration";
            string logEntry = $"Platform = {platform}, RegistrationId = {registrationId}";
            this.LogControllerStart(this.log, className, methodName, logEntry);

            // decode the registrationId
            string decodedRegistrationId = HttpContext.Current.Server.UrlDecode(registrationId);

            var pushRegistrationFeedEntity = await this.pushNotificationsManager.ReadRegistration(this.UserHandle, this.AppHandle, decodedRegistrationId);
            if (pushRegistrationFeedEntity == null)
            {
                return this.NotFound(ResponseStrings.PushRegistrationNotFound);
            }

            await this.pushNotificationsManager.DeleteRegistration(
                ProcessType.Frontend,
                this.UserHandle,
                this.AppHandle,
                pushRegistrationFeedEntity);

            this.LogControllerEnd(this.log, className, methodName, logEntry);
            return this.NoContent();
        }
    }
}
