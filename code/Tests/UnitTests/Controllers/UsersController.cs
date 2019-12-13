// <copyright file="UsersController.cs" company="Microsoft">
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
    using SocialPlus.Utils;

    /// <summary>
    /// Class derived from users controller used for unit tests.
    /// </summary>
    public class UsersController : SocialPlus.Server.Controllers.UsersController
    {
        /// <summary>
        /// Sequence number used to as string suffix to name creation
        /// </summary>
        private readonly TimeOrderedSequenceNumber seqNumber = new TimeOrderedSequenceNumber();

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
        /// Initializes a new instance of the <see cref="UsersController"/> class
        /// </summary>
        /// <param name="managersContext">managers context</param>
        /// <param name="principalsContext">principals context</param>
        public UsersController(ManagersContext managersContext, PrincipalsContext principalsContext)
            : base(
                  managersContext.Log,
                  managersContext.IdentitiesManager,
                  managersContext.SessionTokenManager,
                  managersContext.UsersManager,
                  managersContext.PopularUsersManager,
                  managersContext.AppsManager,
                  managersContext.ViewsManager,
                  managersContext.HandleGenerator,
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
        /// Create a user
        /// </summary>
        /// <returns>result of create user operation</returns>
        public async Task<IHttpActionResult> PostUser()
        {
            var postUserRequest = new PostUserRequest
            {
                InstanceId = "UserControllerTests",
                FirstName = "Barack" + this.seqNumber.GenerateStronglyOrderedSequenceNumber(),
                LastName = "Obama" + this.seqNumber.GenerateStronglyOrderedSequenceNumber(),
            };

            return await this.PostUser(postUserRequest);
        }

        /// <summary>
        /// Check that result of post user is Created (201), and that the user handle returned is matches
        /// the one passed found in the UserPrincipal.
        /// </summary>
        /// <param name="actionResultPostUser">result of create user operation</param>
        public void CheckPostUserResult201(IHttpActionResult actionResultPostUser)
        {
            // Check that create user worked
            Assert.IsInstanceOfType(actionResultPostUser, typeof(CreatedNegotiatedContentResult<PostUserResponse>));
            PostUserResponse postUserResponse = (actionResultPostUser as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            if (this.UserPrincipal.UserHandle != null)
            {
                Assert.AreEqual(this.UserPrincipal.UserHandle, postUserResponse.UserHandle);
            }
        }

        /// <summary>
        /// Update user profle
        /// </summary>
        /// <returns>result of put user operation</returns>
        public async Task<IHttpActionResult> PutUser()
        {
            var putUserInfoRequest = new PutUserInfoRequest
            {
                FirstName = "Barack" + this.seqNumber.GenerateStronglyOrderedSequenceNumber(),
                LastName = "Obama" + this.seqNumber.GenerateStronglyOrderedSequenceNumber(),
                Bio = "President" + this.seqNumber.GenerateStronglyOrderedSequenceNumber(),
            };

            return await this.PutUserInfo(putUserInfoRequest);
        }

        /// <summary>
        /// Get user profile
        /// </summary>
        /// <returns>result of get user operation</returns>
        public async Task<IHttpActionResult> GetUser()
        {
            return await this.GetMyProfile();
        }

        /// <summary>
        /// Check that result of get user is OK (200), and that the user handle returned is matches
        /// the one passed found in the UserPrincipal.
        /// </summary>
        /// <param name="actionResultGetUser">result of get user operation</param>
        public void CheckGetUserResult200(IHttpActionResult actionResultGetUser)
        {
            // Check that get user worked
            Assert.IsInstanceOfType(actionResultGetUser, typeof(OkNegotiatedContentResult<UserProfileView>));
            var userProfileView = (actionResultGetUser as OkNegotiatedContentResult<UserProfileView>).Content;
            if (this.UserPrincipal.UserHandle != null)
            {
                Assert.AreEqual(this.UserPrincipal.UserHandle, userProfileView.UserHandle);
            }
        }
    }
}
