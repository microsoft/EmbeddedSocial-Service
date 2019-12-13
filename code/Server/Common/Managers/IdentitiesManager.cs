// <copyright file="IdentitiesManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.OAuth;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.Exceptions;

    /// <summary>
    /// IdentitiesManager manager class
    /// </summary>
    public class IdentitiesManager : IIdentitiesManager
    {
        /// <summary>
        /// Get request token
        /// </summary>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="clientId">Client id given by identity provider</param>
        /// <param name="clientSecret">Client secret given by identity provider</param>
        /// <param name="clientRedirectUri"><c>OAuth</c> callback given by identity provider</param>
        /// <returns>Request token</returns>
        public async Task<string> GetRequestToken(
            IdentityProviderType identityProviderType,
            string clientId,
            string clientSecret,
            string clientRedirectUri)
        {
            if (identityProviderType == IdentityProviderType.Twitter)
            {
                return await OAuth.Instance.GetRequestToken(IdentityProviders.Twitter, clientId, clientSecret, clientRedirectUri);
            }

            throw new IdentityProviderException("Third party not supported.", null);
        }

        /// <summary>
        /// Validate third-party identity and return the user information
        /// Throws InvalidIdentityException if identity is invalid
        /// Throws IdentityValidationFailureException if validation fails for other reasons
        /// </summary>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accessToken">Third party access token (for implicit flow) or user code/verifier (for authorization grant flow) or accountID (for AADS2S)</param>
        /// <param name="requestToken">Third party request token</param>
        /// <param name="identityProviderCredentials">Application's credentials for the third party identity provider.</param>
        /// <returns>User information from identity provider</returns>
        public async Task<IdentityProviderUser> GetIdentityProviderUser(
            IdentityProviderType identityProviderType,
            string accessToken,
            string requestToken,
            IIdentityProviderCredentialsEntity identityProviderCredentials)
        {
            UserProfile userProfile = null;

            try
            {
                switch (identityProviderType)
                {
                    case IdentityProviderType.Microsoft:
                        userProfile = await OAuth.Instance.ImplicitFlow(IdentityProviders.Microsoft, accessToken);
                        break;
                    case IdentityProviderType.Facebook:
                        userProfile = await OAuth.Instance.ImplicitFlow(IdentityProviders.Facebook, accessToken);
                        break;
                    case IdentityProviderType.Google:
                        userProfile = await OAuth.Instance.ImplicitFlow(IdentityProviders.Google, accessToken);
                        break;
                    case IdentityProviderType.Twitter:
                        userProfile = await OAuth.Instance.AuthorizationCodeFlow(IdentityProviders.Twitter, accessToken, identityProviderCredentials.ClientId, identityProviderCredentials.ClientSecret, null, requestToken);
                        break;
                    case IdentityProviderType.AADS2S:
                        return new IdentityProviderUser
                        {
                            AccountId = accessToken
                        };
                    default:
                        throw new IdentityProviderException("Third party not supported.", null);
                }
            }
            catch (OAuthException ex)
            {
                throw new InvalidIdentityException("OAuth error.", ex);
            }

            return new IdentityProviderUser()
            {
                AccountId = userProfile.GenericUserProfile.AccountId,
                FirstName = userProfile.GenericUserProfile.FirstName,
                LastName = userProfile.GenericUserProfile.LastName
            };
        }
    }
}
