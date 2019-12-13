// <copyright file="MyLinkedAccountControllerTests.cs" company="Microsoft">
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
    using SocialPlus.Models;
    using SocialPlus.TestUtils;

    /// <summary>
    /// Class encapsulating tests of MyLinkedAccount controller
    /// </summary>
    [TestClass]
    public class MyLinkedAccountControllerTests
    {
        /// <summary>
        /// Gets or sets controllers context with proper app and user principals (user principal is in the Twitter namespace)
        /// </summary>
        private ControllersContext ControllersContext { get; set; }

        /// <summary>
        /// Gets or sets controllers context with a proper app principal and a user principals with a null user handle (user principal is in MSA namespace)
        /// </summary>
        private ControllersContext ControllersContextWithNullUserHandle { get; set; }

        /// <summary>
        /// Gets or sets session token
        /// </summary>
        private string SessionToken { get; set; }

        /// <summary>
        /// Gets tuple of post linked account action and check action's result is 204 (done with a controller context with null user handle)
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> PostLinkedAccount204
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContextWithNullUserHandle.MyLinkedAccountsController.PostLinkedAccount(this.SessionToken); }, (u) => ValidateHttpResults.CheckHTTPResult204(u));
            }
        }

        /// <summary>
        /// Gets tuple of delete linked account action and check action's result is 409 (done with a controller context with null user handle)
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> PostLinkedAccount409
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContextWithNullUserHandle.MyLinkedAccountsController.PostLinkedAccount(this.SessionToken); }, (u) => ValidateHttpResults.CheckHTTPResult409(u));
            }
        }

        /// <summary>
        /// Gets tuple of post linked account action and check action's result is 204
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> DeleteLinkedAccount204
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContext.MyLinkedAccountsController.DeleteLinkedAccount(this.ControllersContextWithNullUserHandle.PrincipalsContext.UserPrincipal.IdentityProvider); }, (u) => ValidateHttpResults.CheckHTTPResult204(u));
            }
        }

        /// <summary>
        /// Gets tuple of post linked account action and check action's result is 404
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> DeleteLinkedAccount404
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContext.MyLinkedAccountsController.DeleteLinkedAccount(this.ControllersContextWithNullUserHandle.PrincipalsContext.UserPrincipal.IdentityProvider); }, (u) => ValidateHttpResults.CheckHTTPResult404(u));
            }
        }

        /// <summary>
        /// Gets tuple of get linked accounts action and check action's result is 200
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> GetLinkedAccounts200
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContext.MyLinkedAccountsController.GetLinkedAccounts(); }, (u) => this.ControllersContextWithNullUserHandle.MyLinkedAccountsController.CheckGetLinkedAccountsResult200(u));
            }
        }

        /// <summary>
        /// Gets tuple of get linked accounts action and check action's result is 404
        /// </summary>
        private Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> GetLinkedAccounts404
        {
            get
            {
                return Tuple.Create<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>(() => { return this.ControllersContext.MyLinkedAccountsController.GetLinkedAccounts(); }, (u) => ValidateHttpResults.CheckHTTPResult404(u));
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
        /// This test checks 16 sequences of five calls each.
        /// First call is always PostUser
        /// Second call is one of PostLinkedAccount, DeleteLinkedAccount, GetLinkedAccount, DeleteUser
        /// Third call is one of PostLinkedAccount, DeleteLinkedAccount, GetLinkedAccount, DeleteUser
        /// Fourth call is always DeleteUser
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [TestMethod]
        public async Task ApiSequenceTest()
        {
            this.ControllersContext = await ControllersContext.ConstructControllersContext();
            this.ControllersContextWithNullUserHandle = await ControllersContext.ConstructControllersContextWithNullUserHandle();

            //// Sequence #1: PostUser, PostLinkedAccount, PostLinkedAccount, DeleteUser

            var result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            PostUserResponse postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            var seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.PostLinkedAccount204, this.PostLinkedAccount409, this.DeleteUser204 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #2: PostUser, PostLinkedAccount, DeleteLinkedAccount, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.PostLinkedAccount204, this.DeleteLinkedAccount204, this.DeleteUser204 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #3: PostUser, PostLinkedAccount, GetLinkedAccount, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.PostLinkedAccount204, this.GetLinkedAccounts200, this.DeleteUser204 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #4: PostUser, PostLinkedAccount, DeleteUser, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.PostLinkedAccount204, this.DeleteUser204, this.DeleteUser404 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #5: PostUser, DeleteLinkedAccount, PostLinkedAccount, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.DeleteLinkedAccount404, this.PostLinkedAccount204, this.DeleteUser204 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #6: PostUser, DeleteLinkedAccount, DeleteLinkedAccount, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.DeleteLinkedAccount404, this.DeleteLinkedAccount404, this.DeleteUser204 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #7: PostUser, DeleteLinkedAccount, GetLinkedAccount, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.DeleteLinkedAccount404, this.GetLinkedAccounts200, this.DeleteUser204 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #8: PostUser, DeleteLinkedAccount, DeleteUser, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.DeleteLinkedAccount404, this.DeleteUser204, this.DeleteUser404 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #9: PostUser, GetLinkedAccount, PostLinkedAccount, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.GetLinkedAccounts200, this.PostLinkedAccount204, this.DeleteUser204 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #10: PostUser, GetLinkedAccount, DeleteLinkedAccount, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.GetLinkedAccounts200, this.DeleteLinkedAccount404, this.DeleteUser204 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #11: PostUser, GetLinkedAccount, GetLinkedAccount, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.GetLinkedAccounts200, this.GetLinkedAccounts200, this.DeleteUser204 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #12: PostUser, GetLinkedAccount, DeleteUser, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.GetLinkedAccounts200, this.DeleteUser204, this.DeleteUser404 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #13: PostUser, DeleteUser, PostLinkedAccount, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.DeleteUser204, this.PostLinkedAccount204, this.DeleteUser404 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #14: PostUser, DeleteUser, DeleteLinkedAccount, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.DeleteUser204, this.DeleteLinkedAccount404, this.DeleteUser404 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #15: PostUser, DeleteUser, GetLinkedAccount, DeleteUser

            result = await ApiSequenceTestingFramework.Execute(this.PostUser201);
            postUserResponse = (result as CreatedNegotiatedContentResult<PostUserResponse>).Content;
            this.SessionToken = postUserResponse.SessionToken;

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.DeleteUser204, this.GetLinkedAccounts200, this.DeleteUser404 };
            await ApiSequenceTestingFramework.Execute(seq);

            //// Sequence #16: PostUser, DeleteUser, DeleteUser, DeleteUser

            seq = new List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> { this.PostUser201, this.DeleteUser204, this.DeleteUser404, this.DeleteUser404 };
            await ApiSequenceTestingFramework.Execute(seq);
        }
    }
}
