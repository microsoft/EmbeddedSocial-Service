// <copyright file="AnonAuthManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Server.Principal;
    using SocialPlus.Utils;

    /// <summary>
    /// Authentication functionality for anonymous calls made to SocialPlus.
    /// Even though anonymous, these calls must include a proper app key in their headers.
    /// This class checks the app key is valid
    /// </summary>
    public class AnonAuthManager : CommonAuthManager, IAuthManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnonAuthManager"/> class.
        /// </summary>
        /// <param name="log">log</param>
        /// <param name="appsManager">apps manager</param>
        /// <param name="usersManager">users manager</param>
        public AnonAuthManager(ILog log, IAppsManager appsManager, IUsersManager usersManager)
            : base(log, appsManager, usersManager)
        {
        }

        /// <summary>
        /// Gets friends for an anonymous user
        /// </summary>
        /// <remarks>
        /// Not implemented
        /// </remarks>
        /// <param name="auth">authorization header</param>
        /// <returns>list of user principals</returns>
        public Task<List<IPrincipal>> GetFriends(string auth)
        {
            return null;
        }

        /// <summary>
        /// Validate authorization header.
        /// The Anon header must include an app key.
        /// This method:
        ///     - validate the app key
        /// </summary>
        /// <param name="authParameter">authorization parameter</param>
        /// <returns>list of principals</returns>
        public async Task<List<IPrincipal>> ValidateAuthParameter(string authParameter)
        {
            string appKey;
            List<IPrincipal> principals = new List<IPrincipal>();

            // Parse the authorization header
            var authDictionary = new Dictionary<string, string>();
            this.ParseAuthParameter(authParameter, authDictionary);

            // Validate app key and construct app Principal
            authDictionary.TryGetValue("ak", out appKey);
            bool isAppKeyValid = await this.ValidateAppKey(appKey);
            if (isAppKeyValid == false)
            {
                string errorMessage = $"Invalid App Key. Auth parameter: {authParameter}";
                this.Log.LogException(errorMessage);
            }

            string appHandle = await this.ReadAppHandle(appKey);
            if (appHandle == null)
            {
                // The app key is not found (i.e., not bound to an app handle). One possibility is that the appKey
                // used to exist but was invalidated via the dev portal.
                string errorMessage = $"App Key not found. App Key: {appKey}";
                this.Log.LogException(errorMessage);
            }

            principals.Add(new AppPrincipal(appHandle, appKey));

            return principals;
        }
    }
}
