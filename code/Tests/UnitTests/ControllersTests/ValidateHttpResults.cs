// <copyright file="ValidateHttpResults.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System.Web.Http;
    using System.Web.Http.Results;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Server.WebRoleCommon;

    /// <summary>
    /// Class that validates http results
    /// </summary>
    public static class ValidateHttpResults
    {
        /// <summary>
        /// Check that request result is 200
        /// </summary>
        /// <param name="actionResult">result of action</param>
        public static void CheckHTTPResult200(IHttpActionResult actionResult)
        {
            // Check that delete worked properly
            Assert.IsInstanceOfType(actionResult, typeof(OkResult));
        }

        /// <summary>
        /// Check that request result is 204
        /// </summary>
        /// <param name="actionResult">result of action</param>
        public static void CheckHTTPResult204(IHttpActionResult actionResult)
        {
            // Check that delete worked properly
            Assert.IsInstanceOfType(actionResult, typeof(NoContentResult));
        }

        /// <summary>
        /// Check that request result is 401
        /// </summary>
        /// <param name="actionResult">result of action</param>
        public static void CheckHTTPResult401(IHttpActionResult actionResult)
        {
            // Check that delete worked properly
            Assert.IsInstanceOfType(actionResult, typeof(UnauthorizedMessageResult));
        }

        /// <summary>
        /// Check that request result is 404
        /// </summary>
        /// <param name="actionResult">result of action</param>
        public static void CheckHTTPResult404(IHttpActionResult actionResult)
        {
            // Check that delete worked properly
            Assert.IsInstanceOfType(actionResult, typeof(NotFoundMessageResult));
        }

        /// <summary>
        /// Check that request result is 409
        /// </summary>
        /// <param name="actionResult">result of action</param>
        public static void CheckHTTPResult409(IHttpActionResult actionResult)
        {
            // Check that create user worked
            Assert.IsInstanceOfType(actionResult, typeof(ConflictMessageResult));
        }
    }
}
