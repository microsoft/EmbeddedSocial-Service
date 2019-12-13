// <copyright file="UsersControllerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.TestUtils;

    /// <summary>
    /// Class encapsulating tests of users controller
    /// </summary>
    [TestClass]
    public class UsersControllerTests
    {
        /// <summary>
        /// Gets or sets controllers context with proper app and user principals (user principal is in the Twitter namespace)
        /// </summary>
        private ControllersContext ControllersContext { get; set; }

        /// <summary>
        /// Gets tuple of post user action and check action's result is 201
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> PostUser201
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContext.UsersController.PostUser(); }, (u) => this.ControllersContext.UsersController.CheckPostUserResult201(u));
            }
        }

        /// <summary>
        /// Gets tuple of post user action and check action's result is 409
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> PostUser409
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContext.UsersController.PostUser(); }, (u) => ValidateHttpResults.CheckHTTPResult409(u));
            }
        }

        /// <summary>
        /// Gets tuple of delete user action and check action's result is 204
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> DeleteUser204
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContext.UsersController.DeleteUser(); }, (u) => ValidateHttpResults.CheckHTTPResult204(u));
            }
        }

        /// <summary>
        /// Gets tuple of delete user action and check action's result is 404
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> DeleteUser404
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContext.UsersController.DeleteUser(); }, (u) => ValidateHttpResults.CheckHTTPResult404(u));
            }
        }

        /// <summary>
        /// Gets tuple of put user action and check action's result is 204
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> PutUser204
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContext.UsersController.PutUser(); }, (u) => ValidateHttpResults.CheckHTTPResult204(u));
            }
        }

        /// <summary>
        /// Gets tuple of get user action and check action's result is 200
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> GetUser200
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContext.UsersController.GetUser(); }, (u) => this.ControllersContext.UsersController.CheckGetUserResult200(u));
            }
        }

        /// <summary>
        /// This test checks five sequences of three calls each, all to the users controller.
        /// The first and last call are always Create and Delete user.
        /// The second call is one of Null (nop), PostUser, DeleteUser, PutUser, GetUser
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [TestMethod]
        public async Task ApiSequenceTests()
        {
            this.ControllersContext = await ControllersContext.ConstructControllersContext();

            var seq1 = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.PostUser201, null, this.DeleteUser204 };
            var seq2 = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.PostUser201, this.PostUser409, this.DeleteUser204 };
            var seq3 = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.PostUser201, this.DeleteUser204, this.DeleteUser404 };
            var seq4 = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.PostUser201, this.PutUser204, this.DeleteUser204 };
            var seq5 = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.PostUser201, this.GetUser200, this.DeleteUser204 };

            await ApiSequenceTestingFramework.Execute(seq1);
            await ApiSequenceTestingFramework.Execute(seq2);
            await ApiSequenceTestingFramework.Execute(seq3);
            await ApiSequenceTestingFramework.Execute(seq4);
            await ApiSequenceTestingFramework.Execute(seq5);
        }
    }
}
