// <copyright file="AuthenticationFilter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using SocialPlus.Logging;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Principal;
    using SocialPlus.Server.WebRoleCommon;
    using SocialPlus.Utils;

    /// <summary>
    /// Implements SocialPlus authentication filter. The role of this filter is two construct two principals:
    /// a user principal, and an application principal from the authorization header field. This filter only checks
    /// for ASP.NET authentication constraints, such as whether an incoming authorization scheme is allowed to make
    /// the incoming call (e.g., Anon requests are not allowed to call controllers not labelled with the
    /// Allow Anonymous attribute)
    /// </summary>
    public class AuthenticationFilter : IAuthenticationFilter
    {
        /// <summary>
        /// Apps manager
        /// </summary>
        private readonly IAppsManager appsManager;

        /// <summary>
        /// Users manager
        /// </summary>
        private readonly IUsersManager usersManager;

        /// <summary>
        /// Auth manager
        /// </summary>
        private readonly ICompositeAuthManager authManager;

        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationFilter"/> class.
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="appsManager">Apps manager</param>
        /// <param name="usersManager">Users manager</param>
        /// <param name="authManager">auth manager</param>
        public AuthenticationFilter(
            ILog log,
            IAppsManager appsManager,
            IUsersManager usersManager,
            ICompositeAuthManager authManager)
        {
            this.log = log ?? throw new ArgumentNullException("AuthenticationFilter constructor: log is null");
            this.appsManager = appsManager ?? throw new ArgumentNullException("AuthenticationFilter constructor: appsManager is null");
            this.usersManager = usersManager ?? throw new ArgumentNullException("AuthenticationFilter constructor: usersManager is null");
            this.authManager = authManager ?? throw new ArgumentNullException("AuthenticationFilter constructor: authManager is null");
        }

        /// <summary>
        /// Gets scheme name for Anon
        /// </summary>
        public static string AnonScheme
        {
            get { return "Anon"; }
        }

        /// <summary>
        /// Gets a value indicating whether more than one instance of the indicated attribute can be specified for a single program element.
        /// </summary>
        public virtual bool AllowMultiple
        {
            get { return false; }
        }

        /// <summary>
        /// Authenticate async.
        /// This method:
        ///     - checks that there is an authorization field
        ///     - checks that if the authorization scheme is Anon the controller called is labelled with the Allow Anonymous attribute
        ///     - dispatches the authorization parameter to the appropriate auth manager
        ///     - populates the HTTP context with the user and app principals
        /// </summary>
        /// <remarks>
        /// Requirements for different schemes:
        ///  - SocialPlus: must include token
        ///  - Anon: must include app key
        ///  - OAuth: must include app key and token
        ///  - AADS2S: must include app key and token (and optionally user handle)
        /// In summary: the app key cannot be null unless AuthScheme is SocialPlus; token cannot be null unless AuthScheme is Anon
        /// </remarks>
        /// <param name="context">Http context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Authenticate async task</returns>
        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            HttpActionContext actionContext = context.ActionContext;
            HttpRequestMessage request = context.Request;
            int versionMajor = 0;
            int versionMinor = 0;
            List<IPrincipal> principals = null;

            // parse out the api version number from the request url
            try
            {
                UrlParser.GetMajorMinorAPIVersion(request.RequestUri, out versionMajor, out versionMinor);
            }
            catch (Exception e)
            {
                string errorMessage = "Request API version is malformed.";
                this.log.LogError(errorMessage, e);

                context.ErrorResult = new BadRequestMessageResult(ResponseStrings.VersionNumbersMalformed, request);
                return;
            }

            AuthenticationHeaderValue authorization = request.Headers.Authorization;
            if (authorization == null)
            {
                string errorMessage = "No authorization header field present.";
                this.log.LogError(errorMessage);

                context.ErrorResult = new UnauthorizedMessageResult(ResponseStrings.GenericUnauthorizedError, context.Request);
                return;
            }

            // Anon authorization is only allowed on controllers labelled with the AllowAnonymous attribute
            if (authorization.Scheme == AnonScheme && (actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any() == false))
            {
                string errorMessage = $"Anon auth attempted on a controller/action not labelled AllowAnonymous. Auth: {authorization.ToString()}";
                this.log.LogError(errorMessage);

                context.ErrorResult = new UnauthorizedMessageResult(ResponseStrings.GenericUnauthorizedError, context.Request);
                return;
            }

            // Validate the authorization string
            try
            {
                principals = await this.authManager.ValidateAuth(authorization.Parameter, authorization.Scheme);
            }
            catch (Exception e)
            {
                // Catch unauthorized exceptions, log them, and return unauthorized
                string errorMessage = $"Invalid authorization header: {authorization.ToString()}";
                this.log.LogError(errorMessage, e);

                context.ErrorResult = new UnauthorizedMessageResult(ResponseStrings.GenericUnauthorizedError, context.Request);
                return;
            }

            // Save user and app principal in the HTTP context
            foreach (var p in principals)
            {
                if (p is AppPrincipal)
                {
                    HttpContext.Current.Items["AppPrincipal"] = p;
                }
                else if (p is UserPrincipal)
                {
                    HttpContext.Current.User = p;
                }
            }
        }

        /// <summary>
        /// Challenge async is a mechanism for adding challenges to unauthenticated calls. We don't use it.
        /// </summary>
        /// <param name="context">Http context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Challenge async task</returns>
        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
