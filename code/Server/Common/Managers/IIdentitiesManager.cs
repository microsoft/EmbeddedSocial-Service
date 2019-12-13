// <copyright file="IIdentitiesManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Identities manager interface
    /// </summary>
    public interface IIdentitiesManager
    {
        /// <summary>
        /// Get request token
        /// </summary>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="clientId">Client id given by identity provider</param>
        /// <param name="clientSecret">Client secret given by identity provider</param>
        /// <param name="clientRedirectUri"><c>OAuth</c> callback given by identity provider</param>
        /// <returns>Request token</returns>
        Task<string> GetRequestToken(
            IdentityProviderType identityProviderType,
            string clientId,
            string clientSecret,
            string clientRedirectUri);

        /// <summary>
        /// Validate third-party identity and return the user information
        /// Throws InvalidIdentityException if identity is invalid
        /// Throws IdentityProviderException if validation fails for other reasons
        /// </summary>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accessToken">Third party access token (for implicit flow) or user code/verifier (for authorization grant flow) or accountID (for AADS2S)</param>
        /// <param name="requestToken">Request token. If request token is present, the verifier must be present and access token must be null.</param>
        /// <param name="identityProviderCredentials">Application's credentials for the third party identity provider.</param>
        /// <returns>User information from identity provider</returns>
        Task<IdentityProviderUser> GetIdentityProviderUser(
            IdentityProviderType identityProviderType,
            string accessToken,
            string requestToken,
            IIdentityProviderCredentialsEntity identityProviderCredentials);
    }
}
