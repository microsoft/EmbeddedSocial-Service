// <copyright file="PrincipalsContext.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Principal;

    /// <summary>
    /// Class encapsulating an app and a user principal.
    /// Creating an app principal requires looking up an app key in the apps table. Thus, we can't create the app principal
    /// in the constructor (due to the lookup being async). To sidestep this issue, we use a pattern where a caller must call the "ConstructPrincipalsContext"
    /// method to obtain a principals context.
    /// </summary>
    public class PrincipalsContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrincipalsContext"/> class.
        /// </summary>
        /// <param name="appPrincipal">app principal</param>
        /// <param name="userPrincipal">user principal</param>
        private PrincipalsContext(AppPrincipal appPrincipal, UserPrincipal userPrincipal)
        {
            this.AppPrincipal = appPrincipal;
            this.UserPrincipal = userPrincipal;
        }

        /// <summary>
        /// Gets app principal
        /// </summary>
        public AppPrincipal AppPrincipal { get; private set; }

        /// <summary>
        /// Gets user principal
        /// </summary>
        public UserPrincipal UserPrincipal { get; private set; }

        /// <summary>
        /// Construct the principals context from an app key only. The user principal will be constructed with a null user handle
        /// </summary>
        /// <param name="managersContext">managers context</param>
        /// <param name="appKey">app key</param>
        /// <param name="identityProviderType">identity provider type (defaults to Twitter)</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<PrincipalsContext> ConstructNullUserHandlePrincipalsContext(ManagersContext managersContext, string appKey, IdentityProviderType identityProviderType = IdentityProviderType.Twitter)
        {
            string accountId = managersContext.HandleGenerator.GenerateShortHandle();
            var userPrincipal = new UserPrincipal(managersContext.Log, null, identityProviderType, accountId);
            return await PrincipalsContext.ConstructPrincipalsContext(managersContext, appKey, identityProviderType, userPrincipal);
        }

        /// <summary>
        /// Construct the principals context from an app key only. A user principal will be constructed by default
        /// </summary>
        /// <param name="managersContext">managers context</param>
        /// <param name="appKey">app key</param>
        /// <param name="identityProviderType">identity provider type (defaults to Twitter)</param>
        /// <param name="userPrincipal">user principal</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<PrincipalsContext> ConstructPrincipalsContext(ManagersContext managersContext, string appKey, IdentityProviderType identityProviderType = IdentityProviderType.Twitter, UserPrincipal userPrincipal = null)
        {
            // Read app profile
            var appLookupEntity = await managersContext.AppsManager.ReadAppByAppKey(appKey);
            if (appLookupEntity == null)
            {
                throw new InvalidOperationException("No app key found in our tables.");
            }

            // App principal is ready to be created
            var appPrincipal = new AppPrincipal(appLookupEntity.AppHandle, appKey);

            // Create user principal if the one passed in is null.
            if (userPrincipal == null)
            {
                string userHandle = managersContext.HandleGenerator.GenerateShortHandle();
                string accountId = managersContext.HandleGenerator.GenerateShortHandle();
                userPrincipal = new UserPrincipal(managersContext.Log, userHandle, identityProviderType, accountId);
            }

            return new PrincipalsContext(appPrincipal, userPrincipal);
        }
    }
}
