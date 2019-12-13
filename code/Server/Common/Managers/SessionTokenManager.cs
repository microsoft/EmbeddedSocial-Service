// <copyright file="SessionTokenManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Tokens;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Server.Principal;

    /// <summary>
    /// Session token manager default implementation.
    /// A SocialPlus token is a JWT token issued by an appPrincipal to a userPrincipal (i.e., audience is set to userPrincipal)
    /// </summary>
    public class SessionTokenManager : ISessionTokenManager
    {
        /// <summary>
        /// Token handler
        /// </summary>
        private readonly JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        /// <summary>
        /// Variable for key vault
        /// </summary>
        private readonly IKV kv;

        /// <summary>
        /// Connection string provider
        /// </summary>
        private readonly IConnectionStringProvider connectionStringProvider;

        /// <summary>
        /// Internal lock. Together with the flag below, its role is to ensure the method <c>Init</c> has completed before
        /// we attempt to perform the real work done by the public methods in this class.
        /// </summary>
        private readonly object locker = new object();

        /// <summary>
        /// Internal flag. Together with the flag below, its role is to ensure the method <c>Init</c> has completed before
        /// we attempt to perform the real work done by the public methods in this class.
        /// </summary>
        private bool initStarted = false;

        /// <summary>
        /// Internal flag. Its role is to provide a barrier so that no work gets done until <c>Init</c> is done.
        /// </summary>
        private ManualResetEvent initDone = new ManualResetEvent(false);

        /// <summary>
        /// Signing key
        /// </summary>
        private InMemorySymmetricSecurityKey signingKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionTokenManager"/> class.
        /// </summary>
        /// <param name="kv">key vault</param>
        /// <param name="connectionStringProvider">connection string provider</param>
        public SessionTokenManager(IKV kv, IConnectionStringProvider connectionStringProvider)
        {
            if (kv == null)
            {
                throw new ArgumentNullException("Session token manager cannot take a null kv.");
            }

            if (connectionStringProvider == null)
            {
                throw new ArgumentNullException("Session token manager cannot take a null connection string provider.");
            }

            this.kv = kv;
            this.connectionStringProvider = connectionStringProvider;
        }

        /// <summary>
        /// Initializes the session token manager with the signing key bytes
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Init()
        {
            // Guard that ensures Init is executed once only
            lock (this.locker)
            {
                if (this.initStarted == true)
                {
                    return;
                }

                this.initStarted = true;
            }

            string hashingKeyUrl = await this.connectionStringProvider.GetHashingKey();
            string signingKeyValue = await this.kv.GetSecretByUrlAsync(hashingKeyUrl);
            byte[] signingKeyBytes = Encoding.UTF8.GetBytes(signingKeyValue);

            if (signingKeyBytes == null)
            {
                throw new ArgumentNullException("Session token manager cannot take a null signing key.");
            }

            if (signingKeyBytes.Length != 32)
            {
                throw new ArgumentException("Signing key must be 32 bytes in length.");
            }

            this.signingKey = new InMemorySymmetricSecurityKey(signingKeyBytes);

            // Init done
            this.initDone.Set();
        }

        /// <summary>
        /// Create SocialPlus session token
        /// </summary>
        /// <param name="appPrincipal">app principal (cannot be null)</param>
        /// <param name="userPrincipal">user principal (cannot be null)</param>
        /// <param name="duration">lifetime of the token</param>
        /// <returns>SocialPlus session token</returns>
        public async Task<string> CreateToken(AppPrincipal appPrincipal, UserPrincipal userPrincipal, TimeSpan duration)
        {
            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init();
            }

            if (appPrincipal == null || userPrincipal == null)
            {
                throw new ArgumentNullException(
                    string.Format(
                        "CreateToken constructor can't take null parameters. appPrincipal: {0}, userPrincipal: {1}",
                        appPrincipal,
                        userPrincipal));
            }

            if (this.signingKey == null)
            {
                throw new InvalidOperationException("Token manager has a null signing key and thus cannot create tokens.");
            }

            // A session token has two claims: an app principal claim and a user principal claim
            Claim[] claims = new Claim[1] { this.GenerateUserClaim(userPrincipal) };

            // A token descriptor is like a legend of what the token should include
            var tokenDescriptor = this.GenerateSessionTokenDescriptor(appPrincipal, claims, duration);

            // Using the token descriptor, we are ready to generate and return the token
            var token = this.tokenHandler.CreateToken(tokenDescriptor);
            return this.tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validate token and returns its claims if token is valid
        /// </summary>
        /// <param name="token">Session token</param>
        /// <returns>list of principals found in token</returns>
        public async Task<List<IPrincipal>> ValidateToken(string token)
        {
            // If init not called, call init.
            if (this.initStarted == false)
            {
                await this.Init();
            }

            // Create the token validation parameters.
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                IssuerSigningKey = this.signingKey,

                // Implementing our own lifetime validator to ensure that the expiration time is checked against UTC and not local time
                LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) =>
                {
                    if (expires != null && expires >= DateTime.UtcNow)
                    {
                        return true;
                    }

                    return false;
                }
            };

            SecurityToken validatedToken = new JwtSecurityToken();
            var cp = this.tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);

            // app is the "issuer" field.
            List<IPrincipal> principals = new List<IPrincipal>();
            foreach (var claim in cp.Claims)
            {
                switch (claim.Type)
                {
                    case "iss":
                        principals.Add(new AppPrincipal(claim.Value));
                        break;
                    case "aud":
                        principals.Add(new UserPrincipal(claim.Value));
                        break;
                }
            }

            return principals;
        }

        /// <summary>
        /// Method that generates the token descriptor of a SocialPlus session token
        /// </summary>
        /// <param name="appPrincipal">app principal</param>
        /// <param name="claims">token's claims</param>
        /// <param name="duration">lifetime of the token</param>
        /// <returns>token descriptor</returns>
        private SecurityTokenDescriptor GenerateSessionTokenDescriptor(AppPrincipal appPrincipal, Claim[] claims, TimeSpan duration)
        {
            var now = DateTime.UtcNow;
            return new SecurityTokenDescriptor
            {
                TokenIssuerName = appPrincipal.ToString(),
                Subject = new ClaimsIdentity(claims),
                Lifetime = new Lifetime(null, now.Add(duration)),
                SigningCredentials = new SigningCredentials(this.signingKey, SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest),
            };
        }

        /// <summary>
        /// Generates a claim from an appPrincipal
        /// </summary>
        /// <param name="appPrincipal">appPrincipal input</param>
        /// <returns>claim</returns>
        private Claim GenerateAppClaim(AppPrincipal appPrincipal)
        {
            return new Claim("iss", appPrincipal.ToString());
        }

        /// <summary>
        /// Generates a claim from a userPrincipal
        /// </summary>
        /// <param name="userPrincipal">userPrincipal input</param>
        /// <returns>claim</returns>
        private Claim GenerateUserClaim(UserPrincipal userPrincipal)
        {
            return new Claim("aud", userPrincipal.ToString());
        }
    }
}
