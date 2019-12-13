// <copyright file="CompositeAuthManager.cs" company="Microsoft">
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
    /// Authentication manager composite
    /// </summary>
    public class CompositeAuthManager : ICompositeAuthManager
    {
        /// <summary>
        /// AAD auth manager
        /// </summary>
        private readonly IAuthManager aadAuthManager;

        /// <summary>
        /// SocialPlus auth manager
        /// </summary>
        private readonly IAuthManager spAuthManager;

        /// <summary>
        /// Anon auth manager
        /// </summary>
        private readonly IAuthManager anonAuthManager;

        /// <summary>
        /// Microsoft auth manager
        /// </summary>
        private readonly IAuthManager msaAuthManager;

        /// <summary>
        /// Facebook auth manager
        /// </summary>
        private readonly IAuthManager fbAuthManager;

        /// <summary>
        /// Google auth manager
        /// </summary>
        private readonly IAuthManager gAuthManager;

        /// <summary>
        /// Twitter auth manager
        /// </summary>
        private readonly IAuthManager tAuthManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeAuthManager"/> class.
        /// </summary>
        /// <param name="aadAuthManager">AAD auth manager</param>
        /// <param name="spAuthManager">SocialPlus auth manager</param>
        /// <param name="anonAuthManager">Anon auth manager</param>
        /// <param name="msaAuthManager">Microsoft auth manager</param>
        /// <param name="fbAuthManager">Facebook auth manager</param>
        /// <param name="gAuthManager">Google auth manager</param>
        /// <param name="tAuthManager">Twitter auth manager</param>
        public CompositeAuthManager(
            IAuthManager aadAuthManager,
            IAuthManager spAuthManager,
            IAuthManager anonAuthManager,
            IAuthManager msaAuthManager,
            IAuthManager fbAuthManager,
            IAuthManager gAuthManager,
            IAuthManager tAuthManager)
        {
            this.aadAuthManager = aadAuthManager ?? throw new ArgumentNullException("CompositeAuthManager constructor: aadAuthManager is null");
            this.spAuthManager = spAuthManager ?? throw new ArgumentNullException("CompositeAuthManager constructor: spAuthManager is null");
            this.anonAuthManager = anonAuthManager ?? throw new ArgumentNullException("CompositeAuthManager constructor: anonAuthManager is null");
            this.msaAuthManager = msaAuthManager ?? throw new ArgumentNullException("CompositeAuthManager constructor: msaAuthManager is null");
            this.fbAuthManager = fbAuthManager ?? throw new ArgumentNullException("CompositeAuthManager constructor: fbAuthManager is null");
            this.gAuthManager = gAuthManager ?? throw new ArgumentNullException("CompositeAuthManager constructor: gAuthManager is null");
            this.tAuthManager = tAuthManager ?? throw new ArgumentNullException("CompositeAuthManager constructor: tAuthManager is null");
        }

        /// <summary>
        /// Validates authorization header
        /// </summary>
        /// <param name="authParameter">authorization parameter</param>
        /// <param name="authScheme">authorization scheme</param>
        /// <returns>list of principals found in the header</returns>
        public async Task<List<IPrincipal>> ValidateAuth(string authParameter, string authScheme)
        {
            switch (authScheme)
            {
                case "AADS2S":
                    return await this.aadAuthManager.ValidateAuthParameter(authParameter);
                case "SocialPlus":
                    return await this.spAuthManager.ValidateAuthParameter(authParameter);
                case "Anon":
                    return await this.anonAuthManager.ValidateAuthParameter(authParameter);
                case "Microsoft":
                    return await this.msaAuthManager.ValidateAuthParameter(authParameter);
                case "Facebook":
                    return await this.fbAuthManager.ValidateAuthParameter(authParameter);
                case "Google":
                    return await this.gAuthManager.ValidateAuthParameter(authParameter);
                case "Twitter":
                    return await this.tAuthManager.ValidateAuthParameter(authParameter);
                default:
                    throw new InvalidOperationException("Composite manager called with unknown scheme: " + authScheme);
            }
        }

        /// <summary>
        /// Gets a user's friends from a third-party auth identity provider
        /// </summary>
        /// <param name="auth">authorization header</param>
        /// <param name="authScheme">authorization scheme</param>
        /// <returns>list of user principals</returns>
        public async Task<List<IPrincipal>> GetFriends(string auth, string authScheme)
        {
            switch (authScheme)
            {
                case "AADS2S":
                    return await this.aadAuthManager.GetFriends(auth);
                case "SocialPlus":
                    return await this.spAuthManager.GetFriends(auth);
                case "Anon":
                    return await this.anonAuthManager.GetFriends(auth);
                case "Microsoft":
                    return await this.msaAuthManager.GetFriends(auth);
                case "Facebook":
                    return await this.fbAuthManager.GetFriends(auth);
                case "Google":
                    return await this.gAuthManager.GetFriends(auth);
                case "Twitter":
                    return await this.tAuthManager.GetFriends(auth);
                default:
                    throw new InvalidOperationException("Composite manager called with unknown scheme: " + authScheme);
            }
        }
    }
}
