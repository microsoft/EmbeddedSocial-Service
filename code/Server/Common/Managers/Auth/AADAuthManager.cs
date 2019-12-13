// <copyright file="AADAuthManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using Microsoft.IdentityModel.Protocols;
    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Principal;
    using SocialPlus.Utils;

    /// <summary>
    /// Class implementing authentication functionality for Azure Active Directory
    /// This class takes in its constructor a list of tenants and appIDUris.
    /// Given a token, it verifies whether the token was issued by AAD to a tenant
    /// </summary>
    public class AADAuthManager : CommonAuthManager, IAuthManager
    {
        /// <summary>
        /// String describing the format of the discovery Url for AAD.
        /// </summary>
        private static string discoveryUrlFormat = "https://login.microsoftonline.com/{0}";

        /// <summary>
        /// String describing the format of the openId relative URL
        /// </summary>
        private static string discoveryOpenIdFormat = "{0}/.well-known/openid-configuration";

        /// <summary>
        /// Security token handler used to validate tokens
        /// </summary>
        private readonly JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        /// <summary>
        /// This alert function is called when the validation parameters fail to self-refresh.
        /// This in turns delegates the call to Alerts.Error
        /// </summary>
        private readonly Action<string, Exception> alert = null;

        /// <summary>
        /// Dictionary of self-refreshing variables for each tenant's validation parameters.
        /// The parameters have to be refreshed every 24 hours.
        /// </summary>
        private readonly ConcurrentDictionary<string, SelfRefreshingVar<TokenValidationParameters>> srvValidationParameters
            = new ConcurrentDictionary<string, SelfRefreshingVar<TokenValidationParameters>>();

        /// <summary>
        /// Variable that holds a security token once a token has been validated
        /// </summary>
        private SecurityToken validatedToken = new JwtSecurityToken();

        /// <summary>
        /// Initializes a new instance of the <see cref="AADAuthManager"/> class.
        /// </summary>
        /// <param name="log">log</param>
        /// <param name="appsManager">apps manager</param>
        /// <param name="usersManager">users manager</param>
        public AADAuthManager(ILog log, IAppsManager appsManager, IUsersManager usersManager)
            : base(log, appsManager, usersManager)
        {
            this.alert = (string msg, Exception ex) => this.Log.LogError(msg, ex);
        }

        /// <summary>
        /// Gets a user's friends from AAD
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
        /// Validate authorization header for AAD authentication scheme.
        /// An AAD auth header must include an appKey, a token, and optionally a user handle.
        /// This method:
        ///     - validates the app key
        ///     - validates the token
        ///     - constructs an app principal
        ///     - if user handle is present and validate it if the app requires handle validation
        ///     - if user handle is present construct a user principal
        /// </summary>
        /// <param name="authParameter">authorization parameter</param>
        /// <returns>list of principals</returns>
        public async Task<List<IPrincipal>> ValidateAuthParameter(string authParameter)
        {
            string token;
            string appKey;
            string userHandle;
            List<IPrincipal> principals = new List<IPrincipal>();

            // Parse the authorization header
            var authDictionary = new Dictionary<string, string>();
            this.ParseAuthParameter(authParameter, authDictionary);

            // Get the app key, the token,
            authDictionary.TryGetValue("ak", out appKey);
            authDictionary.TryGetValue("tk", out token);

            // Validate app key and construct app Principal
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

            // Validate token
            ClaimsPrincipal cp = await this.ValidateToken(token, appHandle);

            // Read user handle
            authDictionary.TryGetValue("uh", out userHandle);

            // Read app profile
            var appProfile = await this.ReadAppProfile(appHandle);
            if (appProfile == null)
            {
                string errorMessage = $"App's profile is missing. appHandle: {appHandle}.";
                this.Log.LogException(errorMessage);
            }

            // If user handle is present, check whether we need to validate it
            if (userHandle != null && !appProfile.DisableHandleValidation)
            {
                bool isUserHandleValid = await this.ValidateUserHandle(userHandle, IdentityProviderType.AADS2S, userHandle, appHandle);
                if (isUserHandleValid == false)
                {
                    string errorMessage = $"AADAuthManager:ValidateAuthParameter: Invalid userHandle. userHandle: {userHandle}; appHandle: {appHandle}";
                    this.Log.LogException(errorMessage);
                }
            }

            // Construct user principal. It's ok even if the user handle is empty
            principals.Add(new UserPrincipal(this.Log, userHandle, IdentityProviderType.AADS2S, userHandle));

            return principals;
        }

        /// <summary>
        /// Retrieves an OpenIdConnect configuration given a format and a tenant ID
        /// </summary>
        /// <param name="discoveryOpenIdFormat">OpenId discovery format</param>
        /// <param name="discoveryUrlFormat">Url format</param>
        /// <param name="tenantId">tenant id</param>
        /// <returns>OpenID configuration</returns>
        private static async Task<OpenIdConnectConfiguration> GetOpenIdConnectConfiguration(string discoveryOpenIdFormat, string discoveryUrlFormat, string tenantId)
        {
            // Create the discovery point on AAD for this tenant
            string aadAuthority = string.Format(CultureInfo.InvariantCulture, discoveryUrlFormat, tenantId);
            string tenantDiscoveryEndpoint = string.Format(discoveryOpenIdFormat, aadAuthority);

            // Get tenant information used to validate incoming jwt tokens
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(tenantDiscoveryEndpoint);
            return await configManager.GetConfigurationAsync();
        }

        /// <summary>
        /// Validates a token against every single app identity registered with us
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="appHandle">app handle</param>
        /// <returns>list of claims found in the token</returns>
        private async Task<ClaimsPrincipal> ValidateToken(string token, string appHandle)
        {
            token.IfNullOrEmptyThrowEx("AADAuthManager:ValidateToken: token is null");
            appHandle.IfNullOrEmptyThrowEx("AADAuthManager:ValidateToken: appHandle is null");

            // Lookup the validation parameters corresponding to this app handle in our dictionary of srv
            SelfRefreshingVar<TokenValidationParameters> appTokenValidationParameter = null;
            this.srvValidationParameters.TryGetValue(appHandle, out appTokenValidationParameter);

            // If can't find validation parameters, create new ones.
            if (appTokenValidationParameter == null)
            {
                // Construct new token validation parameters
                var appIdentity = await this.ReadIdentityProviderCredentials(appHandle, IdentityProviderType.AADS2S);
                if (appIdentity == null)
                {
                    string errorMessage = $"AADAuthManager:ValidateToken: AAD credentials are missing. appHandle: {appHandle}";
                    this.Log.LogException(errorMessage);
                }

                // Create an initialized self-refreshing variable for the token validation parameters. Refresh the variable every 24 hours.
                var validationParameters = await this.ConstructTokenValidationParameters(appIdentity.ClientId, appIdentity.ClientRedirectUri);
                appTokenValidationParameter = new SelfRefreshingVar<TokenValidationParameters>(validationParameters, TimeUtils.TwentyFourHours, () => this.ConstructTokenValidationParameters(appIdentity.ClientId, appIdentity.ClientRedirectUri), this.alert);

                // Insert the new validation parameters in the dictionary. In case they are already there (due to a race condition), it's ok, we'll overwrite the old ones.
                if (this.srvValidationParameters.TryAdd(appHandle, appTokenValidationParameter) == false)
                {
                    // The self-refreshing variable has been added to the concurrent dictionary by somebody else. Dispose ours
                    appTokenValidationParameter.Dispose();

                    // If we just disposed ours, read the ones from dictionary
                    this.srvValidationParameters.TryGetValue(appHandle, out appTokenValidationParameter);

                    // Sanity check: the read must be successful. We must have non-null validation parameters by now
                    appTokenValidationParameter = appTokenValidationParameter ?? throw new InvalidOperationException("Code should never reach this point.");
                }
            }

            return this.tokenHandler.ValidateToken(token, appTokenValidationParameter.GetValue(), out this.validatedToken);
        }

        /// <summary>
        /// Methods that constructs the token validation parameter for a given app. It pro-actively reads AAD configuration
        /// to obtain the value of the issuer and its signing tokens
        /// </summary>
        /// <param name="clientId">the client Id of the app</param>
        /// <param name="clientRedirectUri">the redirect URI of the app</param>
        /// <returns>A self-refreshing variable that contains token validation parameters</returns>
        private async Task<TokenValidationParameters> ConstructTokenValidationParameters(string clientId, string clientRedirectUri)
        {
            // Read AAD configuration
            OpenIdConnectConfiguration config = await AADAuthManager.GetOpenIdConnectConfiguration(discoveryOpenIdFormat, discoveryUrlFormat, clientId);

            // Create the data structure used to validate tokens.
            // Turn on validating the issuer and the audience
            // Set the valid audience to the appIDUri
            // Set the name claim to tenantID. It'll be used during tenant configuration to construct the URL
            // for discovering AAD tenant parameters.
            // Set the valid issuer and its tokens to values read from AAD
            // We'll make sure TenantConfig will run before validating tokens.
            return new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = clientRedirectUri,
                ValidIssuer = config.Issuer,
                IssuerSigningTokens = config.SigningTokens?.ToList(),
                CertificateValidator = X509CertificateValidator.None,
                NameClaimType = clientId
            };
        }
    }
}
