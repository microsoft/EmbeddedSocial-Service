// <copyright file="MyFollowingController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using SocialPlus.Server.Principal;

    /// <summary>
    /// Class derived from my following controller used for unit tests.
    /// </summary>
    public class MyFollowingController : SocialPlus.Server.Controllers.MyFollowingController
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
        /// Authorization header used for testing
        /// </summary>
        private string authHeader;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyFollowingController"/> class
        /// </summary>
        /// <param name="managersContext">managers context</param>
        /// <param name="principalsContext">principals context</param>
        /// <param name="authHeader">authentication header</param>
        public MyFollowingController(ManagersContext managersContext, PrincipalsContext principalsContext, string authHeader)
            : base(
                 managersContext.Log,
                 managersContext.RelationshipsManager,
                 managersContext.UsersManager,
                 managersContext.TopicsManager,
                 managersContext.ActivitiesManager,
                 managersContext.ViewsManager,
                 managersContext.AuthManager,
                 managersContext.HandleGenerator)
        {
            this.appPrincipal = principalsContext.AppPrincipal;
            this.userPrincipal = principalsContext.UserPrincipal;
            this.guid = Guid.NewGuid();
            this.authHeader = authHeader;
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
        /// Gets auth header
        /// </summary>
        public override string AuthorizationHeader
        {
            get
            {
                return this.authHeader;
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
    }
}
