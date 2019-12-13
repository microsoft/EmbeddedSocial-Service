// <copyright file="SessionsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Results;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Models;
    using SocialPlus.Server.Principal;

    /// <summary>
    /// Class derived from sessions controller used for unit tests.
    /// </summary>
    public class SessionsController : SocialPlus.Server.Controllers.SessionsController
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
        /// Initializes a new instance of the <see cref="SessionsController"/> class
        /// </summary>
        /// <param name="managersContext">managers context</param>
        /// <param name="principalsContext">principals context</param>
        public SessionsController(ManagersContext managersContext, PrincipalsContext principalsContext)
            : base(
                  managersContext.Log,
                  managersContext.IdentitiesManager,
                  managersContext.SessionTokenManager,
                  managersContext.UsersManager,
                  managersContext.AppsManager,
                  managersContext.ApplicationMetrics)
        {
            this.appPrincipal = principalsContext.AppPrincipal;
            this.userPrincipal = principalsContext.UserPrincipal;
            this.guid = Guid.NewGuid();
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
        /// Create session
        /// </summary>
        /// <returns>result of create session operation</returns>
        public async Task<IHttpActionResult> PostSession()
        {
            var postSessionRequest = new PostSessionRequest
            {
                InstanceId = "SessionsControllerTests",
                UserHandle = this.UserHandle,
            };

            return await this.PostSession(postSessionRequest);
        }

        /// <summary>
        /// Create session for fake user
        /// </summary>
        /// <returns>result of create session operation</returns>
        public async Task<IHttpActionResult> PostSessionFakeUser()
        {
            var postSessionRequest = new PostSessionRequest
            {
                InstanceId = "SessionsControllerTests",
                UserHandle = "FakeUserHandle",
            };

            return await this.PostSession(postSessionRequest);
        }

        /// <summary>
        /// Check that result of post session is Created (201), and that the user handle returned is matches
        /// the one passed found in the UserPrincipal.
        /// </summary>
        /// <param name="actionResultPostUser">result of create session operation</param>
        public void CheckPostSessionResult201(IHttpActionResult actionResultPostUser)
        {
            // Check that create user worked
            Assert.IsInstanceOfType(actionResultPostUser, typeof(CreatedNegotiatedContentResult<PostSessionResponse>));
            PostSessionResponse postSessionResponse = (actionResultPostUser as CreatedNegotiatedContentResult<PostSessionResponse>).Content;
            if (this.UserPrincipal.UserHandle != null)
            {
                Assert.AreEqual(this.UserPrincipal.UserHandle, postSessionResponse.UserHandle);
            }
        }
    }
}
