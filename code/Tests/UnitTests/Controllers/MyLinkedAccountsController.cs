// <copyright file="MyLinkedAccountsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Results;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Principal;
    using SocialPlus.Utils;

    /// <summary>
    /// Class derived from my linked accounts controller used for unit tests.
    /// </summary>
    public class MyLinkedAccountsController : SocialPlus.Server.Controllers.MyLinkedAccountsController
    {
        /// <summary>
        /// App principal used for testing
        /// </summary>
        private readonly AppPrincipal appPrincipal;

        /// <summary>
        /// User principal used for testing
        /// </summary>
        private readonly UserPrincipal userPrincipal;

        /// <summary>
        /// Guid for the request
        /// </summary>
        private readonly Guid guid;

        /// <summary>
        /// Handle generator
        /// </summary>
        private readonly HandleGenerator handleGenerator;

        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyLinkedAccountsController"/> class
        /// </summary>
        /// <param name="managersContext">managers context</param>
        /// <param name="principalsContext">principals context</param>
        public MyLinkedAccountsController(ManagersContext managersContext, PrincipalsContext principalsContext)
            : base(
                  managersContext.Log,
                  managersContext.IdentitiesManager,
                  managersContext.UsersManager,
                  managersContext.AppsManager,
                  managersContext.ViewsManager,
                  managersContext.SessionTokenManager)
        {
            this.appPrincipal = principalsContext.AppPrincipal;
            this.userPrincipal = principalsContext.UserPrincipal;
            this.guid = Guid.NewGuid();
            this.handleGenerator = managersContext.HandleGenerator;
            this.log = managersContext.Log;
        }

        /// <summary>
        /// Gets app principal
        /// </summary>
        public override AppPrincipal AppPrincipal
        {
            get
            {
                return this.appPrincipal;
            }
        }

        /// <summary>
        /// Gets user principal
        /// </summary>
        public override UserPrincipal UserPrincipal
        {
            get
            {
                return this.userPrincipal;
            }
        }

        /// <summary>
        /// Gets guid
        /// </summary>
        public override string RequestGuid
        {
            get
            {
                return this.guid.ToString();
            }
        }

        /// <summary>
        /// Create linked account.
        /// </summary>
        /// <param name="sessionToken">session token</param>
        /// <returns>result of create linked account operation</returns>
        public async Task<IHttpActionResult> PostLinkedAccount(string sessionToken)
        {
            var postLinkedAccountRequest = new PostLinkedAccountRequest
            {
                SessionToken = sessionToken
            };

            return await this.PostLinkedAccount(postLinkedAccountRequest);
        }

        /// <summary>
        /// Check that result of get linked accounts is OK (200), the number of linked accounts is at least two,
        /// and exactly one of the linked accounts is a SocialPlus account.
        /// </summary>
        /// <param name="actionResultGetLinkedAccounts">result of get linked accounts operation</param>
        public void CheckGetLinkedAccountsResult200(IHttpActionResult actionResultGetLinkedAccounts)
        {
            // Check that get user worked
            Assert.IsInstanceOfType(actionResultGetLinkedAccounts, typeof(OkNegotiatedContentResult<List<LinkedAccountView>>));
            var linkedAccountsView = (actionResultGetLinkedAccounts as OkNegotiatedContentResult<List<LinkedAccountView>>).Content;

            // Check that the number of linked accounts is at least two (each user has at least a SocialPlus account and one third-party account)
            Assert.IsTrue(linkedAccountsView.Count >= 2);

            // Check that one of the linked accounts is a SocialPlus account
            bool foundSocialPlusAccount = false;
            foreach (var accountView in linkedAccountsView)
            {
                if (accountView.IdentityProvider == IdentityProviderType.SocialPlus)
                {
                    // Check that this is the first time we found a SocialPlus linked account
                    Assert.IsFalse(foundSocialPlusAccount);
                    foundSocialPlusAccount = true;
                }
            }

            Assert.IsTrue(foundSocialPlusAccount);
        }
    }
}
