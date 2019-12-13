// <copyright file="OAuthManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.OAuth;
    using SocialPlus.Server.Principal;
    using SocialPlus.Utils;

    /// <summary>
    /// Authentication functionality for OAuth tokens.
    /// Currently, the implementation can authenticate tokens issued by MSA, Facebook, Google, and Twitter
    /// </summary>
    public class OAuthManager : CommonAuthManager, IAuthManager
    {
        /// <summary>
        /// OAuth Identity provider
        /// </summary>
        private readonly IdentityProviders oauthIdentityProvider;

        /// <summary>
        /// Identity provider type (used by server entities
        /// </summary>
        private readonly IdentityProviderType serverIdentityProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthManager"/> class.
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="appsManager">apps manager</param>
        /// <param name="usersManager">users manager</param>
        /// <param name="identityProvider">OAuth identity provider</param>
        public OAuthManager(ILog log, IAppsManager appsManager, IUsersManager usersManager, IdentityProviders identityProvider)
            : base(log, appsManager, usersManager)
        {
            this.oauthIdentityProvider = identityProvider;
            switch (this.oauthIdentityProvider)
            {
                case IdentityProviders.Microsoft:
                    this.serverIdentityProvider = IdentityProviderType.Microsoft;
                    break;
                case IdentityProviders.Facebook:
                    this.serverIdentityProvider = IdentityProviderType.Facebook;
                    break;
                case IdentityProviders.Google:
                    this.serverIdentityProvider = IdentityProviderType.Google;
                    break;
                case IdentityProviders.Twitter:
                    this.serverIdentityProvider = IdentityProviderType.Twitter;
                    break;
                default:
                    throw new InvalidOperationException("Code should never reach here.");
            }
        }

        /// <summary>
        /// Gets a user's friends from a third-party OAuth provider
        /// </summary>
        /// <param name="authParameter">authorization parameter</param>
        /// <returns>list of user principals</returns>
        public async Task<List<IPrincipal>> GetFriends(string authParameter)
        {
            // Parse the authorization header and extract Facebook's access token.
            // This code assumes the authorization header can be parsed correctly and that an access token is present.
            // Such an assumption is safe to make because our service's authentication filter would've rejected
            // an un-parsable authorization header or an absent access token already.
            // Parse the authorization header
            var authDictionary = new Dictionary<string, string>();
            this.ParseAuthParameter(authParameter, authDictionary);
            var accessToken = authDictionary["tk"];

            // Call into the OAuth library to retrieve a list of friends from the third-party provider
            var thirdpartyFriends = await OAuth.Instance.GetFriends(this.oauthIdentityProvider, accessToken);

            // Convert the friends found on the third-party network to user principals
            List<IPrincipal> friendList = new List<IPrincipal>();
            foreach (var userProfile in thirdpartyFriends)
            {
                // Try to lookup the user. If not found, that's ok -- there are legitimate cases for a call using OAuth credentials for a new user. For example, PostUser.
                var userLookupEntity = await this.ReadUserByLinkedAccount(this.serverIdentityProvider, userProfile.GenericUserProfile.AccountId);
                if (userLookupEntity != null)
                {
                    friendList.Add(new UserPrincipal(this.Log, userLookupEntity.UserHandle, this.serverIdentityProvider, userProfile.GenericUserProfile.AccountId));
                }
            }

            return friendList;
        }

        /// <summary>
        /// Validate authorization header
        /// The OAuth header must include a token and an app key
        /// This method:
        ///     - validates the app key
        ///     - construct app principal
        ///     - validates the token
        ///     - if user handle is present, validate it, and construct user principal
        /// </summary>
        /// <param name="authParameter">authorization parameter</param>
        /// <returns>list of principals</returns>
        public async Task<List<IPrincipal>> ValidateAuthParameter(string authParameter)
        {
            string token;
            string requestToken;
            string appKey;
            string userHandle;
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

            // Get the token and validate it
            authDictionary.TryGetValue("tk", out token);
            authDictionary.TryGetValue("rt", out requestToken);
            UserProfile userProfile = await this.ValidateToken(token, requestToken, appHandle);

            // Read user handle
            authDictionary.TryGetValue("uh", out userHandle);

            // If user handle is present, validate it and add it to our list. Otherwise, make an effort to lookup the user handle
            if (userHandle != null)
            {
                bool isUserHandleValid = await this.ValidateUserHandle(userHandle, this.serverIdentityProvider, userProfile.GenericUserProfile.AccountId, appHandle);
                if (isUserHandleValid == false)
                {
                    string errorMessage = $"Invalid user handle. userHandle: {userHandle}; ";
                    errorMessage += $"identityProvider: {this.serverIdentityProvider}; identityProviderAccountId: {userProfile.GenericUserProfile.AccountId}; appHandle: {appHandle}";
                    this.Log.LogException(errorMessage);
                }
            }
            else
            {
                // Try to lookup the user. If not found, that's ok -- there are legitimate cases for a call using OAuth credentials for a new user. For example, PostUser.
                var userLookupEntity = await this.ReadUserByLinkedAccount(this.serverIdentityProvider, userProfile.GenericUserProfile.AccountId);
                if (userLookupEntity != null)
                {
                    userHandle = userLookupEntity.UserHandle;
                }
            }

            // Construct user principal and add it to our list. It's ok even if userHandle is null
            principals.Add(new UserPrincipal(this.Log, userHandle, this.serverIdentityProvider, userProfile.GenericUserProfile.AccountId));

            return principals;
        }

        /// <summary>
        /// Validate token and returns its claims if token is valid
        /// </summary>
        /// <param name="token">Session token</param>
        /// <param name="requestToken">Requet token (for Twitter only)</param>
        /// <param name="appHandle">app handle</param>
        /// <returns>user profile</returns>
        private async Task<UserProfile> ValidateToken(string token, string requestToken, string appHandle)
        {
            // For Microsoft, Facebook, and Google we use implicit flow
            if (this.oauthIdentityProvider == IdentityProviders.Microsoft || this.oauthIdentityProvider == IdentityProviders.Facebook || this.oauthIdentityProvider == IdentityProviders.Google)
            {
                return await OAuth.Instance.ImplicitFlow(this.oauthIdentityProvider, token);
            }

            // To call all other flows, we need more information found in the app profile
            var identityProviderCredentialsEntity = await this.ReadIdentityProviderCredentials(appHandle, this.serverIdentityProvider);
            if (identityProviderCredentialsEntity == null)
            {
                string errorMessage = $"OAuthManager:ValidateToken: credentials of identity provider not found. ";
                errorMessage += $"identityProvider: {this.serverIdentityProvider}; appHandle: {appHandle}";
                this.Log.LogException(errorMessage);
            }

            return await OAuth.Instance.AuthorizationCodeFlow(this.oauthIdentityProvider, token, identityProviderCredentialsEntity.ClientId, identityProviderCredentialsEntity.ClientSecret, identityProviderCredentialsEntity.ClientRedirectUri, requestToken);
        }
    }
}
