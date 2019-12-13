// <copyright file="FakeIdentitiesManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Entities;

    /// <summary>
    /// Fake Identities Manager class
    /// </summary>
    public class FakeIdentitiesManager : IIdentitiesManager
    {
        /// <summary>
        /// Get fake request token
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
            await Task.Delay(0);
            return "TestRequestToken";
        }

        /// <summary>
        /// Get fake identity provider user
        /// </summary>
        /// <param name="identityProviderType">Identity provider type</param>
        /// <param name="accessToken">Third party access token</param>
        /// <param name="requestToken">Third party request token</param>
        /// <param name="identityProviderCredentials">Application's credentials for the third party identity provider.</param>
        /// <returns>User information from identity provider</returns>
        public async Task<IdentityProviderUser> GetIdentityProviderUser(
            IdentityProviderType identityProviderType,
            string accessToken,
            string requestToken,
            IIdentityProviderCredentialsEntity identityProviderCredentials)
        {
            await Task.Delay(0);
            return new IdentityProviderUser()
            {
                AccountId = accessToken,
                FirstName = "TestFirstName",
                LastName = "TestLastName"
            };
        }
    }
}
