// <copyright file="SocialPlusAuthManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Server.Principal;
    using SocialPlus.Utils;

    /// <summary>
    /// Authentication functionality for SocialPlus session tokens
    /// </summary>
    public class SocialPlusAuthManager : CommonAuthManager, IAuthManager
    {
        /// <summary>
        /// Session token manager
        /// </summary>
        private readonly ISessionTokenManager sessionTokenManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialPlusAuthManager"/> class.
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="appsManager">apps manager</param>
        /// <param name="usersManager">users manager</param>
        /// <param name="sessionTokenManager">session token manager</param>
        public SocialPlusAuthManager(ILog log, IAppsManager appsManager, IUsersManager usersManager, ISessionTokenManager sessionTokenManager)
            : base(log, appsManager, usersManager)
        {
            this.sessionTokenManager = sessionTokenManager ?? throw new ArgumentNullException("SocialPlusAuthManager constructor: sessionTokenManager is null");
        }

        /// <summary>
        /// Gets a user's friends from SocialPlus
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
        /// The SocialPlus auth header must include a token. This token is our own session token that includes an app and a user principal
        /// This method:
        ///     - validates the token
        ///     - validates the app principal
        ///     - validates the user principal
        /// </summary>
        /// <param name="authParameter">authorization parameter</param>
        /// <returns>list of principals</returns>
        public async Task<List<IPrincipal>> ValidateAuthParameter(string authParameter)
        {
            string token;

            // Parse the authorization header
            var authDictionary = new Dictionary<string, string>();
            this.ParseAuthParameter(authParameter, authDictionary);

            // Get the token and validate it.
            authDictionary.TryGetValue("tk", out token);
            var principals = await this.sessionTokenManager.ValidateToken(token);

            // Before returning the principals, we must validate them
            AppPrincipal appPrincipal = null;
            UserPrincipal userPrincipal = null;
            foreach (var p in principals)
            {
                if (p is AppPrincipal)
                {
                    appPrincipal = p as AppPrincipal;
                }
                else
                {
                    userPrincipal = p as UserPrincipal;
                }
            }

            // Fire the validation tasks in parallel
            var task1 = this.ValidateAppKey(appPrincipal.AppKey);
            var task2 = this.ValidateUserPrincipal(userPrincipal, appPrincipal.AppHandle);
            await Task.WhenAll(task1, task2);

            // Is app key valid?
            if (task1.Result == false)
            {
                string errorMessage = $"Invalid AppKey. AppPrincipal={appPrincipal.ToString()}";
                this.Log.LogException(errorMessage);
            }

            // Is user principal valid?
            if (task2.Result == false)
            {
                string errorMessage = $"Invalid UserPrincipal. UserPrincipal={userPrincipal.ToString()}, AppHandle={appPrincipal.AppHandle}";
                this.Log.LogException(errorMessage);
            }

            return principals;
        }
    }
}
