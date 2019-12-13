// <copyright file="MyFollowingControllerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http.Results;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Models;
    using SocialPlus.Server.WebRoleCommon;

    /// <summary>
    /// Class encapsulating tests of MyFollowing controller
    /// </summary>
    [TestClass]
    public class MyFollowingControllerTests
    {
        /// <summary>
        /// Facebook Access token. Have to get it manually
        /// Get Access Token from here: https://developers.facebook.com/tools/explorer/ -- make sure to select the scope "user_friends"
        /// </summary>
        private const string FBAccessToken = "EAACEdEose0cBAEgCLEqL9iGa0LlXTw6kLKK5a3LnAslT2Ebqcr2Mhi3GhkPGrLTc4TB02ZA6UVMHDdYGDE0ywTAaZA9yfdEHo6FZCmVgcZAxyh83WmES9tNZBTmqh4viYBZBxNMPBf7EWVyOZBoLLxm9jHqQ8CZBENb45zZASCRoTCxoBUKIFFF307FGGZBiLOzOMZD";

        /// <summary>
        /// Get my suggestions of users to follow with a valid token.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task GetSuggestionsUsersFacebookValidToken()
        {
            var managersContext = new ManagersContext();
            var principalsContext = await PrincipalsContext.ConstructPrincipalsContext(managersContext, TestConstants.AppKey);
            var myFollowingController = new MyFollowingController(managersContext, principalsContext, FBAccessToken);

            var actionResult = await myFollowingController.GetSuggestionsUsers();

            // Check that the controller returned bad request
            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<List<UserCompactView>>));
        }

        /// <summary>
        /// Get my suggestions of users to follow with an expired token.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task GetSuggestionsUsersFacebookExpiredToken()
        {
            var managersContext = new ManagersContext();
            var principalsContext = await PrincipalsContext.ConstructPrincipalsContext(managersContext, TestConstants.AppKey);
            var myFollowingController = new MyFollowingController(managersContext, principalsContext, FBAccessToken);

            var actionResult = await myFollowingController.GetSuggestionsUsers();

            // Check that the controller returned bad request
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestErrorMessageResult));
        }

        /// <summary>
        /// Get my suggestions of users to follow with a bad access token.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task GetSuggestionsUsersFacebookBadAccessToken()
        {
            var managersContext = new ManagersContext();
            var principalsContext = await PrincipalsContext.ConstructPrincipalsContext(managersContext, TestConstants.AppKey);
            var myFollowingController = new MyFollowingController(managersContext, principalsContext, FBAccessToken);

            var actionResult = await myFollowingController.GetSuggestionsUsers();

            // Check that the controller returned bad request
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestErrorMessageResult));
        }

        /// <summary>
        /// Get my suggestions of users to follow.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task GetSuggestionsUsers()
        {
            var managersContext = new ManagersContext();
            var principalsContext = await PrincipalsContext.ConstructPrincipalsContext(managersContext, TestConstants.AppKey);
            var myFollowingController = new MyFollowingController(managersContext, principalsContext, FBAccessToken);

            var actionResult = await myFollowingController.GetSuggestionsUsers();
            Assert.IsInstanceOfType(actionResult, typeof(NotImplementedResult));
        }
    }
}
