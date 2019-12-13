// <copyright file="ControllersContext.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System.Threading.Tasks;

    using SocialPlus.Models;

    /// <summary>
    /// Class encapsulating the context needed for controllers.
    /// </summary>
    public class ControllersContext
    {
        /// <summary>
        /// Gets managers context
        /// </summary>
        public ManagersContext ManagersContext { get; private set; }

        /// <summary>
        /// Gets or sets gets principals context
        /// </summary>
        public PrincipalsContext PrincipalsContext { get; protected set; }

        /// <summary>
        /// Gets users controller
        /// </summary>
        public UsersController UsersController { get; private set; }

        /// <summary>
        /// Gets sessions controller
        /// </summary>
        public SessionsController SessionsController { get; private set; }

        /// <summary>
        /// Gets my linked accounts controller
        /// </summary>
        public MyLinkedAccountsController MyLinkedAccountsController { get; private set; }

        /// <summary>
        /// Constructs controllers context with proper app and user principal.
        /// </summary>
        /// <remarks>
        /// App principal is using TestConstants.AppKey
        /// User principal is using a randomly generated user handle and a Twitter account type with a randomly generated id
        /// </remarks>
        /// <returns>controllers context</returns>
        public static async Task<ControllersContext> ConstructControllersContext()
        {
            var controllersContext = new ControllersContext();
            controllersContext.ManagersContext = new ManagersContext();
            controllersContext.PrincipalsContext = await PrincipalsContext.ConstructPrincipalsContext(controllersContext.ManagersContext, TestConstants.AppKey);
            controllersContext.UsersController = new UsersController(controllersContext.ManagersContext, controllersContext.PrincipalsContext);
            controllersContext.SessionsController = new SessionsController(controllersContext.ManagersContext, controllersContext.PrincipalsContext);
            controllersContext.MyLinkedAccountsController = new MyLinkedAccountsController(controllersContext.ManagersContext, controllersContext.PrincipalsContext);

            return controllersContext;
        }

        /// <summary>
        /// Constructs controllers context with proper app, but a user principal whose user handle is null.
        /// </summary>
        /// <remarks>
        /// App principal is using TestConstants.AppKey
        /// User principal is using a null user handle and an MSA account with a randomly generated id
        /// </remarks>
        /// <returns>controllers context</returns>
        public static async Task<ControllersContext> ConstructControllersContextWithNullUserHandle()
        {
            var controllersContext = new ControllersContext();
            controllersContext.ManagersContext = new ManagersContext();
            controllersContext.PrincipalsContext = await PrincipalsContext.ConstructNullUserHandlePrincipalsContext(controllersContext.ManagersContext, TestConstants.AppKey, IdentityProviderType.Microsoft);
            controllersContext.UsersController = new UsersController(controllersContext.ManagersContext, controllersContext.PrincipalsContext);
            controllersContext.SessionsController = new SessionsController(controllersContext.ManagersContext, controllersContext.PrincipalsContext);
            controllersContext.MyLinkedAccountsController = new MyLinkedAccountsController(controllersContext.ManagersContext, controllersContext.PrincipalsContext);

            return controllersContext;
        }
    }
}
