// <copyright file="ISessionTokenManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using SocialPlus.Server.Principal;

    /// <summary>
    /// Session token manager interface. The role of the session token manager is to mint new tokens.
    /// Validation of tokens is done by the SocialPlus auth manager
    /// </summary>
    public interface ISessionTokenManager
    {
        /// <summary>
        /// Create SocialPlus session token
        /// </summary>
        /// <param name="appPrincipal">app principal (cannot be null)</param>
        /// <param name="userPrincipal">user principal (cannot be null)</param>
        /// <param name="duration">lifetime of the token</param>
        /// <returns>SocialPlus session token</returns>
        Task<string> CreateToken(AppPrincipal appPrincipal, UserPrincipal userPrincipal, TimeSpan duration);

        /// <summary>
        /// Validate token and returns its claims if token is valid
        /// </summary>
        /// <param name="token">Session token</param>
        /// <returns>list of principals found in token</returns>
        Task<List<IPrincipal>> ValidateToken(string token);
    }
}