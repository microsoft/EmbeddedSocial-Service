// <copyright file="SessionTokenTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.IdentityModel.Tokens;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Principal;
    using SocialPlus.Utils;

    /// <summary>
    /// Unit tests for SocialPlus session tokens (JWT tokens)
    /// </summary>
    [TestClass]
    public class SessionTokenTests
    {
        /// <summary>
        /// utilities for randomness
        /// </summary>
        private readonly RandUtils randUtils = new RandUtils();

        /// <summary>
        /// Managers context
        /// </summary>
        private readonly ManagersContext managersContext = new ManagersContext();

        /// <summary>
        /// Test creating a session token manager with null KV
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateTokenManagerWithNullKV()
        {
            var tokenManager = new SessionTokenManager(null, this.managersContext.ConnectionStringProvider);
        }

        /// <summary>
        /// Test creating a session token manager with null hashing key
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateTokenManagerWithNullHashingKey()
        {
            var tokenManager = new SessionTokenManager(this.managersContext.KeyVault, null);
        }

        /// <summary>
        /// Test calling create token token with null app principal
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateTokenWithNullAppPrincipal()
        {
            AppPrincipal appPrincipal = null;
            UserPrincipal userPrincipal = new UserPrincipal(this.managersContext.Log, "genericUserHandle", IdentityProviderType.AADS2S, "genericUserHandle");
            await this.managersContext.SessionTokenManager.CreateToken(appPrincipal, userPrincipal, TimeSpan.FromTicks(1));
        }

        /// <summary>
        /// Test calling create token token with null user principal
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateTokenWithNullUserPrincipal()
        {
            AppPrincipal appPrincipal = new AppPrincipal("genericAppHandle", "genenericAppKey");
            UserPrincipal userPrincipal = null;
            await this.managersContext.SessionTokenManager.CreateToken(appPrincipal, userPrincipal, TimeSpan.FromTicks(1));
        }

        /// <summary>
        /// Test calling validate token with invalid token
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ValidateInvalidToken()
        {
            await this.managersContext.SessionTokenManager.ValidateToken("token");
        }

        /// <summary>
        /// Test calling validate token with expired token
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        [ExpectedException(typeof(SecurityTokenInvalidLifetimeException))]
        public async Task ValidateExpiredToken()
        {
            AppPrincipal appPrincipal = new AppPrincipal("genericAppHandle", "genenericAppKey");
            UserPrincipal userPrincipal = new UserPrincipal(this.managersContext.Log, "genericUserHandle", IdentityProviderType.AADS2S, "genericUserHandle");
            string tokenWithShortLifetime = await this.managersContext.SessionTokenManager.CreateToken(appPrincipal, userPrincipal, TimeSpan.FromTicks(1));
            await this.managersContext.SessionTokenManager.ValidateToken(tokenWithShortLifetime);
        }

        /// <summary>
        /// Test calling validate proper token
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task ValidateProperToken()
        {
            AppPrincipal appPrincipal = new AppPrincipal("genericAppHandle", "genenericAppKey");
            UserPrincipal userPrincipal = new UserPrincipal(this.managersContext.Log, "genericUserHandle", IdentityProviderType.AADS2S, "genericUserHandle");
            string token = await this.managersContext.SessionTokenManager.CreateToken(appPrincipal, userPrincipal, TimeSpan.FromMinutes(1));
            var principals = await this.managersContext.SessionTokenManager.ValidateToken(token);

            // Extract app and user principals from session token.
            AppPrincipal sessionTokenAppPrincipal = null;
            UserPrincipal sessionTokenUserPrincipal = null;
            foreach (IPrincipal p in principals)
            {
                if (p is AppPrincipal)
                {
                    sessionTokenAppPrincipal = p as AppPrincipal;
                }
                else
                {
                    sessionTokenUserPrincipal = p as UserPrincipal;
                }
            }

            Assert.AreEqual(appPrincipal, sessionTokenAppPrincipal);
            Assert.AreEqual(userPrincipal, sessionTokenUserPrincipal);
        }
    }
}
