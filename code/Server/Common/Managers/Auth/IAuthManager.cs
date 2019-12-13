// <copyright file="IAuthManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using SocialPlus.Server.Principal;

    /// <summary>
    /// Interface for an authentication manager
    /// </summary>
    public interface IAuthManager
    {
        /// <summary>
        /// Validates authorization header
        /// </summary>
        /// <param name="authParameter">authorization parameter</param>
        /// <returns>list of principals found in the header</returns>
        Task<List<IPrincipal>> ValidateAuthParameter(string authParameter);

        /// <summary>
        /// Gets a user's friends from a third-party auth identity provider
        /// </summary>
        /// <param name="auth">authorization header</param>
        /// <returns>list of user principals</returns>
        Task<List<IPrincipal>> GetFriends(string auth);
    }
}
