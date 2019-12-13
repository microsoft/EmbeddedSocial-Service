using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SocialPlus;
using SocialPlus.Models;

namespace SocialPlus.Server.EndToEndTests
{
    [TestClass]
    class LikeFeedTests
    {
        UserRequest user1req;
        UserRequest user2req;
        UserRequest user3req;

        string TopicHandle;

        public LikeFeedTests()
        {
            user1req = new UserRequest()
            {
                UserHandle = "alecw",
                AppHandle = "coins",
                Agent = null,
                UserSessionExpirationTime = 0,
                UserSessionSignature = "OK",
                InstanceHandle = "TestMachine",
                Location = null,
                Time = DateTime.Now.ToFileTime()
            };

            user2req = new UserRequest()
            {
                UserHandle = "alec2",
                AppHandle = "coins",
                Agent = null,
                UserSessionExpirationTime = 0,
                UserSessionSignature = "OK",
                InstanceHandle = "Test2Machine",
                Location = null,
                Time = DateTime.Now.ToFileTime()
            };

            user3req = new UserRequest()
            {
                UserHandle = "alec3",
                AppHandle = "coins",
                Agent = null,
                UserSessionExpirationTime = 0,
                UserSessionSignature = "OK",
                InstanceHandle = "Test3Machine",
                Location = null,
                Time = DateTime.Now.ToFileTime()
            };
        }

        // user1 creates a topic.  user2 likes it.  user3 likes it.  
        // get the like feed, and check the user names.  check the like count.
        // clean up - delete the topic.
        public async Task DoLikeFeedTest()
        {
            // step 1, create a topic
            AddTopicRequest addTopicRequest = new AddTopicRequest(user1req)
            {
                TopicCategory = "Photo",
                TopicFriendlyName = "abcdef",
                TopicTitle = "Rarest coin",
                TopicText = "Egyptian coin",
                TopicBlobType = (int)BlobType1.Image,
                TopicBlobUrl = "http://coinquest.com/cgi-data/cq_ro/response_380/egypt_10_milliemes_1958.jpg",
                TopicDeepLink = "coins:abcdef",
                TopicType = (int)TopicType.New
            };

            AddTopicResponse addResponse = await ServerTask<AddTopicRequest, AddTopicResponse>.PostRequest(addTopicRequest);
            //Assert.AreEqual(addResponse.ResponseCode, ResponseCode.Success);
            // extract topic handle from the response
            this.TopicHandle = addResponse.TopicHandle;
            Console.WriteLine("LikeFeedTest: Added topic");
            await Task.Delay(Constants.ServiceWriteDelay);

            // user2 likes the topic
            AddLikeRequest addLikeRequest = new AddLikeRequest(user2req)
            {
                ContentHandle = this.TopicHandle,
                ContentType = (int)ContentType1.Topic,
                SequenceId = Environment.TickCount
            };
            AddLikeResponse addLikeResponse = await ServerTask<AddLikeRequest, AddLikeResponse>.PostRequest(addLikeRequest);
            //Assert.AreEqual(addLikeResponse.ResponseCode, ResponseCode.Success);
            Console.WriteLine("LikeFeedTest: user2 liked topic");
            await Task.Delay(Constants.ServiceWriteDelay);

            // user3 likes the topic
            addLikeRequest = new AddLikeRequest(user3req)
            {
                ContentHandle = this.TopicHandle,
                ContentType = (int)ContentType1.Topic,
                SequenceId = Environment.TickCount
            };
            addLikeResponse = await ServerTask<AddLikeRequest, AddLikeResponse>.PostRequest(addLikeRequest);
            //Assert.AreEqual(addLikeResponse.ResponseCode, ResponseCode.Success);
            Console.WriteLine("LikeFeedTest: user3 liked topic");
            await Task.Delay(Constants.ServiceWriteDelay);

            // get the like feed for the topic
            GetLikeFeedRequest getLikesReq = new GetLikeFeedRequest()
            {
                ContentHandle = this.TopicHandle,
                BatchSize = 10
            };
            GetLikeFeedResponse getLikesResponse = await ServerTask<GetLikeFeedRequest, GetLikeFeedResponse>.PostRequest(getLikesReq);
            //Assert.AreEqual(getLikesResponse.ResponseCode, ResponseCode.Success);
            Console.WriteLine("Likes list:");
            foreach (var user in getLikesResponse.Users) 
            {
                Console.WriteLine("User = {0}", user.UserHandle);
            }
        }
    }
}
