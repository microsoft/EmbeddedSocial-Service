// <copyright file="SessionsControllerTests.cs" company="Microsoft">
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
    /// Class encapsulating tests of sessions controller
    /// </summary>
    [TestClass]
    public class SessionsControllerTests
    {
        /// <summary>
        /// Gets or sets controllers context with proper app and user principals (user principal is in the Twitter namespace)
        /// </summary>
        private ControllersContext ControllersContext { get; set; }

        /// <summary>
        /// Gets tuple of post session action and check action's result is 201
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> PostSession201
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContext.SessionsController.PostSession(); }, (u) => this.ControllersContext.SessionsController.CheckPostSessionResult201(u));
            }
        }

        /// <summary>
        /// Gets tuple of post session action and check action's result is 401
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> PostSession401
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContext.SessionsController.PostSessionFakeUser(); }, (u) => ValidateHttpResults.CheckHTTPResult401(u));
            }
        }

        /// <summary>
        /// Gets tuple of post session action and check action's result is 404
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> PostSession404
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContext.SessionsController.PostSession(); }, (u) => ValidateHttpResults.CheckHTTPResult404(u));
            }
        }

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
        /// This test checks three sequences of three calls each.
        /// First sequence is nop, PostSession, nop
        /// Second sequence is PostUser, PostSession, DeleteUser
        /// Third sequence is PostUser, PostSessionFakeUser, DeleteUser
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [TestMethod]
        public async Task ApiSequenceTests()
        {
            this.ControllersContext = await ControllersContext.ConstructControllersContext();

            var seq1 = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { null, this.PostSession404, null };
            var seq2 = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.PostUser201, this.PostSession201, this.DeleteUser204 };
            var seq3 = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.PostUser201, this.PostSession401, this.DeleteUser204 };

            await ApiSequenceTestingFramework.Execute(seq1);
            await ApiSequenceTestingFramework.Execute(seq2);
            await ApiSequenceTestingFramework.Execute(seq3);
        }
    }
}
