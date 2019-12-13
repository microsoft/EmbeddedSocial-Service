// <copyright file="BaseController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Controllers
{
    using System;
    using System.Web;
    using System.Web.Http;

    using SocialPlus.Logging;
    using SocialPlus.Server.Principal;

    /// <summary>
    /// Base controller whose role is to resolve the user and app principals and the access token.
    /// In this way, the principals and the access token are always available to all controllers' actions.
    /// This controller always has a valid appHandle and appKey. It is possible however to have
    /// a null user handle.
    /// </summary>
    public abstract class BaseController : ApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class.
        /// </summary>
        /// <remarks>This boiler-plate constructor is used by all service code.</remarks>
        public BaseController()
        {
        }

        /// <summary>
        /// Our controller methods are instrumented to log information useful for creating a trace for research purposes.
        /// Each method logs info at the start of the method and at the end. This enum implements the names of the two
        /// points we currently log -- start and end of method.
        /// </summary>
        private enum LogControllerInstrumentationPoints
        {
            /// <summary>
            /// Start of method
            /// </summary>
            Start,

            /// <summary>
            /// End of method
            /// </summary>
            End
        }

        /// <summary>
        /// Gets user handle
        /// </summary>
        public string UserHandle
        {
            get
            {
                if (this.UserPrincipal != null)
                {
                    return this.UserPrincipal.UserHandle;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets an app key
        /// </summary>
        public string AppKey
        {
            get
            {
                if (this.AppPrincipal != null)
                {
                    return this.AppPrincipal.AppKey;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets an app handle
        /// </summary>
        public string AppHandle
        {
            get
            {
                if (this.AppPrincipal != null)
                {
                    return this.AppPrincipal.AppHandle;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the guid of the request
        /// </summary>
        public virtual string RequestGuid
        {
            get
            {
                Guid g = (Guid)HttpContext.Current.Items["RequestGuid"];
                return g.ToString();
            }
        }

        /// <summary>
        /// Gets user principal
        /// </summary>
        public virtual UserPrincipal UserPrincipal
        {
            get
            {
                return HttpContext.Current.User as UserPrincipal;
            }
        }

        /// <summary>
        /// Gets app principal
        /// </summary>
        public virtual AppPrincipal AppPrincipal
        {
            get
            {
                return HttpContext.Current.Items["AppPrincipal"] as AppPrincipal;
            }
        }

        /// <summary>
        /// Gets authorization header
        /// </summary>
        /// <remarks>
        /// By making the authorization header method virtual, our unit test can overwrite them with whatever value they want
        /// </remarks>
        public virtual string AuthorizationHeader
        {
            get
            {
                return HttpContext.Current.Request.Headers["Authorization"];
            }
        }

        /// <summary>
        /// Gets absolute Uri of incoming request
        /// </summary>
        /// <remarks>
        /// By making this method virtual, our unit tests can override this with whatever value they want
        /// </remarks>
        public virtual string RequestAbsoluteUri
        {
            get
            {
                return this.Request.RequestUri.AbsoluteUri;
            }
        }

        /// <summary>
        /// Logs information useful for recreating a trace later on from the logs.
        /// </summary>
        /// <remarks>
        /// This should be called by every method in the controllers before starting to execute.
        /// We only log handles, counters, and content types. We do not log private information such as user names or user queries.
        /// Note that we could infer the class name and method name using reflection, however we decided against it for performance reasons.
        /// </remarks>
        /// <param name="log">log</param>
        /// <param name="className">name of the controller class</param>
        /// <param name="methodName">name of method insider controller class (should correspond to an HTTP action)</param>
        /// <param name="logEntry">log entry (can be null; in the case, we log app and user handles only)</param>
        public void LogControllerStart(ILog log, string className, string methodName, string logEntry = null)
        {
            this.LogTracingInfo(LogControllerInstrumentationPoints.Start, log, className, methodName, logEntry);
        }

        /// <summary>
        /// Logs information useful for recreating a trace later on from the logs.
        /// </summary>
        /// <remarks>
        /// This should be called by every method in the controllers before returning data to the client.
        /// We only log handles, counters, and content types. We do not log private information such as user names or user queries.
        /// Note that we could infer the class name and method name using reflection, however we decided against it for performance reasons.
        /// </remarks>
        /// <param name="log">log</param>
        /// <param name="className">name of the controller class</param>
        /// <param name="methodName">name of method insider controller class (should correspond to an HTTP action)</param>
        /// <param name="logEntry">log entry (can be null; in the case, we log app and user handles only)</param>
        public void LogControllerEnd(ILog log, string className, string methodName, string logEntry = null)
        {
            this.LogTracingInfo(LogControllerInstrumentationPoints.End, log, className, methodName, logEntry);
        }

        /// <summary>
        /// Logs information useful for recreating a trace later on from the logs.
        /// </summary>
        /// <remarks>
        /// We only log handles, counters, and content types. We do not log private information such as user names or user queries.
        /// Note that we could infer the class name and method name using reflection, however we decided against it for performance reasons.
        /// </remarks>
        /// <param name="instrumentationPoint">instrumentation point inside the method</param>
        /// <param name="log">log</param>
        /// <param name="className">name of the controller class</param>
        /// <param name="methodName">name of method insider controller class (should correspond to an HTTP action)</param>
        /// <param name="logEntry">log entry (can be null; in the case, we log app and user handles only)</param>
        private void LogTracingInfo(LogControllerInstrumentationPoints instrumentationPoint, ILog log, string className, string methodName, string logEntry = null)
        {
            // All entries have class and method names, the point of instrumentation, the request guid, and app and user handles, and the name of the identity provider
            string message = $"{className}.{methodName}: {instrumentationPoint.ToString()} ";
            message += $"Guid = {this.RequestGuid}, AppHandle = {this.AppHandle}, UserHandle = {this.UserHandle}, IdentityProvider = {this.UserPrincipal?.IdentityProvider}";

            // If log entry is not null, append the app handle and the user handle only, otherwise append the log entry
            if (logEntry != null)
            {
                message += ", " + logEntry;
            }

            log.LogInformation(message);
        }
    }
}
