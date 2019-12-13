// <copyright file="CommonAuthManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Principal;
    using SocialPlus.Utils;

    /// <summary>
    /// This generic auth manager implements functionality common across all auth manager.
    /// Each auth manager (Facebook, Microsoft, SocialPlus, and so on must inherit from this generic one).
    /// </summary>
    public class CommonAuthManager
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
        /// Initializes a new instance of the <see cref="CommonAuthManager"/> class.
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="appsManager">apps manager</param>
        /// <param name="usersManager">users manager</param>
        public CommonAuthManager(ILog log, IAppsManager appsManager, IUsersManager usersManager)
        {
            this.Log = log ?? throw new ArgumentNullException("CommonAuthManager constructor: log is null");
            this.appsManager = appsManager ?? throw new ArgumentNullException("CommonAuthManager constructor: appsManager is null");
            this.usersManager = usersManager ?? throw new ArgumentNullException("CommonAuthManager constructor: usersManager is null");
        }

        /// <summary>
        /// Gets the log
        /// </summary>
        protected ILog Log { get; private set; }

        /// <summary>
        /// Validates a user handle by reading its profile linked to an app handle and by reading a linked account.
        /// If both reads pass, the user handle is valid.
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProviderType">type of identity provider</param>
        /// <param name="identityProviderAccountId">the account id for the linked account for this identity provider</param>
        /// <param name="appHandle">app handle</param>
        /// <returns>true or false</returns>
        protected async Task<bool> ValidateUserHandle(string userHandle, IdentityProviderType identityProviderType, string identityProviderAccountId, string appHandle)
        {
            userHandle.IfNullOrEmptyThrowEx("CommonAuthManager:ValidateUserHandle: userHandle is null");
            identityProviderAccountId.IfNullOrEmptyThrowEx("CommonAuthManager:ValidateUserHandle: identityProviderAccountId is null");
            appHandle.IfNullOrEmptyThrowEx("CommonAuthManager:ValidateUserHandle: appHandle is null");

            // Is user handle registered with this app?
            var userProfileEntity = await this.usersManager.ReadUserProfile(userHandle, appHandle);
            if (userProfileEntity == null)
            {
                string errorMessage = $"No user profile found for this app. userHandle: {userHandle}, appHandle: {appHandle}";
                this.Log.LogError(errorMessage);
                return false;
            }

            // For the current identity provider, is user handle's linked account the same as the one passed in?
            var linkedAccountEntity = await this.usersManager.ReadLinkedAccount(userHandle, identityProviderType);
            if (linkedAccountEntity == null || linkedAccountEntity.AccountId != identityProviderAccountId)
            {
                string errorMessage = $"No linked account found. userHandle: {userHandle}, identityProviderType: {identityProviderType}, identityProviderAccountId: {identityProviderAccountId}";
                this.Log.LogError(errorMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a user principal by validating its user handle
        /// </summary>
        /// <param name="userPrincipal">User principal</param>
        /// <param name="appHandle">app handle</param>
        /// <returns>true or false</returns>
        protected async Task<bool> ValidateUserPrincipal(UserPrincipal userPrincipal, string appHandle)
        {
            userPrincipal = userPrincipal ?? throw new ArgumentNullException("CommonAuthManager:ValidateUserPrincipal: userPrincipal is null");
            appHandle.IfNullOrEmptyThrowEx("CommonAuthManager:ValidateUserPrincipal: appHandle is null");

            bool isUserHandleValid = await this.ValidateUserHandle(userPrincipal.UserHandle, userPrincipal.IdentityProvider, userPrincipal.IdentityProviderAccountId, appHandle);
            if (isUserHandleValid == false)
            {
                string errorMessage = $"CommonAuthManager:ValidateUserPrincipal: userHandle is invalid. userPrincipal: {userPrincipal.ToString()}, appHandle: {appHandle}";
                this.Log.LogError(errorMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates an app key
        /// </summary>
        /// <param name="appKey">app key</param>
        /// <returns>true or false</returns>
        protected async Task<bool> ValidateAppKey(string appKey)
        {
            appKey.IfNullOrEmptyThrowEx("CommonAuthManager:ValidateAppKey: appKey is null");

            // Lookup the appKey to resolve the appHandle
            IAppLookupEntity appLookupEntity = await this.appsManager.ReadAppByAppKey(appKey);
            if (appLookupEntity == null)
            {
                // The app key is not found (i.e., not bound to an app handle). One possibility is that the appKey
                // used to exist but was invalidated via the dev portal.
                string errorMessage = $"CommonAuthManager:ValidateAppKey: appKey not found. appKey: {appKey}";
                this.Log.LogError(errorMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses the authorization header parameter (and not the authorization header scheme).
        /// The authorization header parameter is a list of key values separated by '|'.
        /// An exception is thrown on authorization header parameters that are malformed or unparsable.
        /// </summary>
        /// <remarks>
        /// This code assumes:
        /// 1. Neither the key nor the value can include the symbol '|'. This symbol is used only as a delimiter.
        /// 2. All ' ' characters are ignored.
        /// 3. The key and the value are separated by '='.
        /// 4. the key cannot contain the symbol '='. The value is allowed to contain '='.
        /// 5. the key must be either 'ak', 'uh', 'tk', or 'rt', case-insensitive. These keys correspond to appKey, userHandle, token, and request token.
        /// </remarks>
        /// <param name="authParameter">authorization parameter</param>
        /// <param name="authDictionary">output dictionary of values indexed by keys</param>
        protected void ParseAuthParameter(string authParameter, Dictionary<string, string> authDictionary)
        {
            if (authDictionary == null)
            {
                string errorMessage = $"Auth dictionary is null. Auth parameter: {authParameter}";
                this.Log.LogException(errorMessage);
            }

            // Remove all ' ' characters
            authParameter = authParameter?.Replace(" ", string.Empty);

            // If the delimeter character appears at the beginning or the end of authParameter, trim it
            authParameter = authParameter?.Trim('|');

            if (string.IsNullOrEmpty(authParameter))
            {
                string errorMessage = $"Auth header parameter is null or empty. Auth parameter: {authParameter}";
                this.Log.LogException(errorMessage);
            }

            var keyValueList = authParameter.Split('|');
            foreach (var keyValue in keyValueList)
            {
                // If '=' not found, raise exception, the authorization header parameter is malformed. It includes a string delimited by '|' but lacking a '=' separator
                if (!keyValue.Contains("="))
                {
                    string errorMessage = $"Auth header parameter is malformed. It includes a string delimited by '|' but lacking a '=' separator. {keyValue}";
                    this.Log.LogException(errorMessage);
                }

                // Split keyValue on the first symbol '=' encountered. For this, we call String.Split restricting the number of split strings returned to 2.
                // This ensures that we split on the first '=' encountered. This is the correct behavior because the key cannot contain the symbol '='.
                var splitKeyValue = keyValue.Split(new char[] { '=' }, 2);
                string key = splitKeyValue[0];
                string value = splitKeyValue[1];

                // We only accept keys that specify appKey, userHandle, token, and requestToken. Everything else is ignored.
                key = key.ToLower();
                if (key == "ak" || key == "uh" || key == "tk" || key == "rt")
                {
                    authDictionary[key] = value;
                }
            }
        }

        /// <summary>
        /// Reads an app handle by an app key. If app handle is not found, it returns null
        /// </summary>
        /// <param name="appKey">app key</param>
        /// <returns>Corresponding app handle (throws exception if app key is invalid)</returns>
        protected async Task<string> ReadAppHandle(string appKey)
        {
            // Lookup the appKey to resolve the appHandle
            IAppLookupEntity appLookupEntity = await this.appsManager.ReadAppByAppKey(appKey);
            return appLookupEntity?.AppHandle;
        }

        /// <summary>
        /// Reads the credentials of an application for a specific identity provider
        /// </summary>
        /// <param name="appHandle">app handle</param>
        /// <param name="identityProviderType">type of identity provider (null if not found)</param>
        /// <returns>credentials</returns>
        protected async Task<IIdentityProviderCredentialsEntity> ReadIdentityProviderCredentials(string appHandle, IdentityProviderType identityProviderType)
        {
            return await this.appsManager.ReadIdentityProviderCredentials(appHandle, identityProviderType);
        }

        /// <summary>
        /// Reads the profile of an application
        /// </summary>
        /// <param name="appHandle">app handle</param>
        /// <returns>app profile</returns>
        protected async Task<IAppProfileEntity> ReadAppProfile(string appHandle)
        {
            return await this.appsManager.ReadAppProfile(appHandle);
        }

        /// <summary>
        /// Read user by linked account
        /// </summary>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accountId">Account id</param>
        /// <returns>User lookup entity</returns>
        protected async Task<IUserLookupEntity> ReadUserByLinkedAccount(IdentityProviderType identityProviderType, string accountId)
        {
            return await this.usersManager.ReadUserByLinkedAccount(identityProviderType, accountId);
        }
    }
}
