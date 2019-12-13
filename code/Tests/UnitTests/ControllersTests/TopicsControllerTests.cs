// <copyright file="TopicsControllerTests.cs" company="Microsoft">
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
    using SocialPlus.TestUtils;

    /// <summary>
    /// Class encapsulating tests of Topics controller
    /// </summary>
    [TestClass]
    public class TopicsControllerTests
    {
        /// <summary>
        /// Create a topic, get properties and delete.
        /// </summary>
        /// <returns>Task representing the test</returns>
        [TestMethod]
        public async Task CreateVerifyDeleteTopicUnitTest()
        {
            var managersContext = new ManagersContext();
            var principalsContext = await PrincipalsContext.ConstructPrincipalsContext(managersContext, TestConstants.AppKey);
            var topicController = new TopicsController(managersContext, principalsContext);

            // Create topic and check creation was successful
            var resultPostTopic = await topicController.PostTopic(PublisherType.User);
            topicController.CheckPostUserResult201(resultPostTopic);

            // Get topic and check get was successful
            string topicHandle = (resultPostTopic as CreatedNegotiatedContentResult<PostTopicResponse>).Content.TopicHandle;
            var resultGetTopic = await topicController.GetTopic(topicHandle);
            Assert.AreEqual(topicHandle, (resultGetTopic as OkNegotiatedContentResult<TopicView>).Content.TopicHandle);

            // Delete topic
            var resultDeleteTopic = await topicController.DeleteTopic(topicHandle);
        }

        /// <summary>
        /// Runs two calls to post topic concurrently
        /// </summary>
        /// <returns>Task representing the test</returns>
        [TestMethod]
        public async Task ConcurrentPostTopicTest()
        {
            int numTopics = 2;
            var managersContext = new ManagersContext();
            var principalsContext = await PrincipalsContext.ConstructPrincipalsContext(managersContext, TestConstants.AppKey);
            var usersController = new UsersController(managersContext, principalsContext);
            var topicController = new TopicsController(managersContext, principalsContext);

            // Create a user
            var postUserResponse = await usersController.PostUser();

            // Create a func out of the PostTopic method
            Func<Task<IHttpActionResult>> postTopicFunc = () => topicController.PostTopic(PublisherType.User);

            // Fire 2 calls in parallel
            var actionResultList = await ConcurrentCalls<IHttpActionResult>.FireInParallel(postTopicFunc, numTopics);

            // Delete the topics created
            for (int i = 0; i < numTopics; i += 1)
            {
                var topicHandle = (actionResultList[i] as CreatedNegotiatedContentResult<PostTopicResponse>).Content.TopicHandle;
                await topicController.DeleteTopic(topicHandle);
            }

            // Delete the user created
            await usersController.DeleteUser();
        }
    }
}
