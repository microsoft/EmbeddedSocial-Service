// <copyright file="TopicsController.cs" company="Microsoft">
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
    /// Class derived from topics controller controller used for unit tests.
    /// </summary>
    public class TopicsController : SocialPlus.Server.Controllers.TopicsController
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
        /// Initializes a new instance of the <see cref="TopicsController"/> class
        /// </summary>
        /// <param name="managersContext">managers context</param>
        /// <param name="principalsContext">principals context</param>
        public TopicsController(ManagersContext managersContext, PrincipalsContext principalsContext)
            : base(
                  managersContext.Log,
                  managersContext.UsersManager,
                  managersContext.TopicsManager,
                  managersContext.AppsManager,
                  managersContext.PopularTopicsManager,
                  managersContext.ViewsManager,
                  managersContext.TopicNamesManager,
                  managersContext.HandleGenerator)
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
        /// Gets the request absolute Uri
        /// </summary>
        /// <remarks>
        /// We hardcode the Uri for unit tests
        /// </remarks>
        public override string RequestAbsoluteUri
        {
            get
            {
                return "http://localhost:1324";
            }
        }

        /// <summary>
        /// Create a topic
        /// </summary>
        /// <param name="publisherType">publisher type</param>
        /// <returns>result of create topic operation</returns>
        public async Task<IHttpActionResult> PostTopic(PublisherType publisherType)
        {
            var postTopicRequest = new PostTopicRequest
            {
                PublisherType = publisherType,
                Text = "Text" + this.seqNumber.GenerateStronglyOrderedSequenceNumber()
            };

            return await this.PostTopic(postTopicRequest);
        }

        /// <summary>
        /// Check that result of post topic is Created (201).
        /// </summary>
        /// <param name="actionResultPostTopic">result of create topic operation</param>
        public void CheckPostUserResult201(IHttpActionResult actionResultPostTopic)
        {
            // Check that create user worked
            Assert.IsInstanceOfType(actionResultPostTopic, typeof(CreatedNegotiatedContentResult<PostTopicResponse>));
            PostTopicResponse postTopicResponse = (actionResultPostTopic as CreatedNegotiatedContentResult<PostTopicResponse>).Content;
            Assert.IsNotNull(postTopicResponse.TopicHandle);
        }

        /// <summary>
        /// Update a topic
        /// </summary>
        /// <param name="topicHandle">Topic handle</param>
        /// <returns>result of put topic operation</returns>
        public async Task<IHttpActionResult> PutTopic(string topicHandle)
        {
            var putTopicRequest = new PutTopicRequest
            {
                Text = "Text" + this.seqNumber.GenerateStronglyOrderedSequenceNumber()
            };

            return await this.PutTopic(topicHandle, putTopicRequest);
        }
    }
}
