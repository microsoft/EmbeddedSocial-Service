// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.PopulateClient
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;
    using SocialPlus.Utils;

    /// <summary>
    /// Program that populates the SocialPlus service with synthetic users and data
    /// </summary>
    public class Program
    {
        private static RandUtils randUtils = new RandUtils();

        /// <summary>
        /// Int that determines how many users should be created
        /// </summary>
        private static int numUsers = 0;

        /// <summary>
        /// Int that determines how many topics each user should post
        /// </summary>
        private static int numTopicsPerUser = 2;

        /// <summary>
        /// Int that determines how many comments each user should post
        /// </summary>
        private static int numCommentsPerUser = 4;

        /// <summary>
        /// Int that determines how many replies each user should post
        /// </summary>
        private static int numRepliesPerUser = 4;

        /// <summary>
        /// Int that determines how many topics each user should like
        /// </summary>
        private static int numTopicLikesPerUser = 2;

        /// <summary>
        /// Int that determines how many comments each user should like
        /// </summary>
        private static int numCommentLikesPerUser = 4;

        /// <summary>
        /// Int that determines how many replies each user should like
        /// </summary>
        private static int numReplyLikesPerUser = 4;

        /// <summary>
        /// Int that determines how many users each user should follow
        /// </summary>
        private static int numUsersToFollow = 0;

        /// <summary>
        /// Flag that determines whether to perform the operation without a prompt
        /// </summary>
        private static bool forceOperation = false;

        /// <summary>
        /// Name of the file in which to write information about the generated users
        /// </summary>
        private static string userFileName = "users.txt";

        /// <summary>
        /// Name of the file in which to write information about the generated topics
        /// </summary>
        private static string topicFileName = "topics.txt";

        /// <summary>
        /// The entry point of the program.
        /// </summary>
        /// <param name="args">command line arguments</param>
        public static void Main(string[] args)
        {
            AsyncMain(args).Wait();
        }

        /// <summary>
        /// Async version of the main method.
        /// </summary>
        /// <param name="args">command line arguments</param>
        /// <returns>a task</returns>
        public static async Task AsyncMain(string[] args)
        {
            ParseArgs(args);

            if (forceOperation == false)
            {
                // get user approval
                Console.WriteLine();
                Console.WriteLine("WARNING: this program will add synthetic users and data to the service at the following URL: " + TestConstants.ServerApiBaseUrl);
                Console.WriteLine();
                Console.Write("Are you sure you want to proceed? [y/n] : ");
                ConsoleKeyInfo keyInfo = Console.ReadKey(false);
                Console.WriteLine();
                if (keyInfo.KeyChar != 'y')
                {
                    return;
                }
            }

            await Populate(numUsers, numTopicsPerUser, numCommentsPerUser, numRepliesPerUser, numUsersToFollow, numTopicLikesPerUser, numCommentLikesPerUser, numReplyLikesPerUser);
        }

        /// <summary>
        /// Print a usage message for the program
        /// </summary>
        private static void Usage()
        {
            Console.WriteLine("Usage: PopulateClient -Users=<num-users> [-TopicsPerUser=<num-topics>] [-CommentsPerUser=<num-comments>]");
            Console.WriteLine("                      [-RepliesPerUser=<num-replies>] [-UsersToFollow=<num-users>] [-TopicLikesPerUser=<num-likes>]");
            Console.WriteLine("                      [-CommentLikesPerUser=<num-likes>] [-ReplyLikesPerUser=<num-likes>] [-UserFile=<user-file>]");
            Console.WriteLine("                      [-TopicFile=<topic-file>] [-Force]");
        }

        /// <summary>
        /// Parse the command-line arguments supplied to the program
        /// </summary>
        /// <param name="args">command-line arguments</param>
        private static void ParseArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-Users=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-Users=".Length;
                    numUsers = int.Parse(args[i].Substring(prefixLen));
                }
                else if (args[i].StartsWith("-TopicsPerUser=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-TopicsPerUser=".Length;
                    numTopicsPerUser = int.Parse(args[i].Substring(prefixLen));
                }
                else if (args[i].StartsWith("-CommentsPerUser=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-CommentsPerUser=".Length;
                    numCommentsPerUser = int.Parse(args[i].Substring(prefixLen));
                }
                else if (args[i].StartsWith("-RepliesPerUser=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-RepliesPerUser=".Length;
                    numRepliesPerUser = int.Parse(args[i].Substring(prefixLen));
                }
                else if (args[i].StartsWith("-UsersToFollow=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-UsersToFollow=".Length;
                    numUsersToFollow = int.Parse(args[i].Substring(prefixLen));
                }
                else if (args[i].StartsWith("-TopicLikesPerUser=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-TopicLikesPerUser=".Length;
                    numTopicLikesPerUser = int.Parse(args[i].Substring(prefixLen));
                }
                else if (args[i].StartsWith("-CommentLikesPerUser=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-CommentLikesPerUser=".Length;
                    numCommentLikesPerUser = int.Parse(args[i].Substring(prefixLen));
                }
                else if (args[i].StartsWith("-ReplyLikesPerUser=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-ReplyLikesPerUser=".Length;
                    numReplyLikesPerUser = int.Parse(args[i].Substring(prefixLen));
                }
                else if (args[i].StartsWith("-UserFile=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-UserFile=".Length;
                    userFileName = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-TopicFile=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-TopicFile=".Length;
                    topicFileName = args[i].Substring(prefixLen);
                }
                else if (args[i].StartsWith("-Force", StringComparison.CurrentCultureIgnoreCase))
                {
                    forceOperation = true;
                }
                else
                {
                    Console.WriteLine("Unrecognized argument: " + args[i]);
                    Environment.Exit(0);
                }
            }

            if (numUsers < 1)
            {
                Console.WriteLine("Usage error: Must specify a value for Users greater than 0");
                Usage();
                Environment.Exit(0);
            }

            if (numUsersToFollow >= numUsers)
            {
                Console.WriteLine("Usage error: UsersToFollow must be less than Users");
                Usage();
                Environment.Exit(0);
            }

            if (numTopicLikesPerUser > numTopicsPerUser * numUsers)
            {
                Console.WriteLine("Usage error: TopicLikesPerUser must be less than or equal to Users * TopicsPerUser");
            }

            if (numCommentLikesPerUser > numCommentsPerUser * numUsers)
            {
                Console.WriteLine("Usage error: CommentLikesPerUser must be less than or equal to Users * CommentsPerUser");
            }

            if (numReplyLikesPerUser > numRepliesPerUser * numUsers)
            {
                Console.WriteLine("Usage error: ReplyLikesPerUser must be less than or equal to Users * RepliesPerUser");
            }
        }

        /// <summary>
        /// Populate the service with synthetic users, topics, and comments
        /// </summary>
        /// <param name="numUsers">Number of users to create</param>
        /// <param name="numTopicsPerUser">Number of topics each user will post</param>
        /// <param name="numCommentsPerUser">Number of comments each user will make (on topics chosen randomly with replacement)</param>
        /// <param name="numRepliesPerUser">Number of replies each user will make (on comments chosen randomly with replacement)</param>
        /// <param name="numUsersToFollow">Number of users each user will follow (chosen randomly without replacement). Must be less than numUsers</param>
        /// <param name="numTopicLikesPerUser">Number of topics each user will like (chosen randomly without replacement). Must be less than or equal to numUsers * numTopicsPerUser</param>
        /// <param name="numCommentLikesPerUser">Number of comments each user will like (chosen randomly without replacement). Must be less than or equal to numUsers * numCommentsPerUser</param>
        /// <param name="numReplyLikesPerUser">Number of replies each user will like (chosen randomly without replacement). Must be less than or equal to numUsers * numRepliesPerUser</param>
        /// <returns>A task</returns>
        private static async Task Populate(int numUsers, int numTopicsPerUser, int numCommentsPerUser, int numRepliesPerUser, int numUsersToFollow, int numTopicLikesPerUser, int numCommentLikesPerUser, int numReplyLikesPerUser)
        {
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            List<UserInfo> users = await CreateUsers(client, numUsers);
            List<TopicInfo> topics = await CreateTopics(client, users, numTopicsPerUser);
            List<string> commentHandles = await CreateComments(client, users, topics, numCommentsPerUser);
            List<string> replyHandles = await CreateReplies(client, users, commentHandles, numRepliesPerUser);
            await FollowUsers(client, users, numUsersToFollow);
            await AddTopicLikes(client, users, topics, numTopicLikesPerUser);
            await AddCommentLikes(client, users, commentHandles, numCommentLikesPerUser);
            await AddReplyLikes(client, users, replyHandles, numReplyLikesPerUser);

            /*
             * We save information about the generated users and topics into files so that other
             * utilities (such as benchmarks) can reference the created data. The users'
             * bearer tokens allow other clients to authenticate as the created users, and the
             * user and topic handles allow other clients to perform requests operating on
             * the created users and topics. Each user and topic is assigned an index, and
             * topics are associated with the userIndex of the creating user, in order to allow
             * the reconstruction of the social graph from the output files.
             */
            using (StreamWriter file = new StreamWriter(userFileName))
            {
                file.WriteLine("#userIndex,userHandle,bearerToken");
                foreach (UserInfo userInfo in users)
                {
                    file.WriteLine("{0},{1},{2}", userInfo.UserIndex, userInfo.UserHandle, userInfo.BearerToken);
                }
            }

            Console.WriteLine("Bearer tokens and user handles for {0} users have been written to {1}", users.Count, userFileName);

            using (StreamWriter file = new StreamWriter(topicFileName))
            {
                file.WriteLine("#topicIndex,topicHandle,userIndex");
                foreach (TopicInfo topicInfo in topics)
                {
                    file.WriteLine("{0},{1},{2}", topicInfo.TopicIndex, topicInfo.TopicHandle, topicInfo.UserIndex);
                }
            }

            Console.WriteLine("Information for {0} topics has been written to {1}", topics.Count, topicFileName);
        }

        /// <summary>
        /// Create synthetic users
        /// </summary>
        /// <param name="client">a valid SocialPlusClient</param>
        /// <param name="numUsers">the number of users to create</param>
        /// <returns>a list of UserInfo objects corresponding to the created users</returns>
        private static async Task<List<UserInfo>> CreateUsers(SocialPlusClient client, int numUsers)
        {
            List<UserInfo> users = new List<UserInfo>();

            for (int userIndex = 0; userIndex < numUsers; userIndex++)
            {
                string firstName = "UserFirst" + userIndex;
                string lastName = "UserLast" + userIndex;
                string bio = string.Empty;
                PostUserResponse response = await TestUtilities.DoLogin(client, firstName, lastName, bio);
                UserInfo userInfo = default(UserInfo);
                userInfo.UserHandle = response.UserHandle;
                userInfo.BearerToken = "Bearer " + response.SessionToken;
                userInfo.UserIndex = userIndex;
                users.Add(userInfo);
            }

            return users;
        }

        /// <summary>
        /// Create synthetic topics by having each of the supplied users post the given number of topics
        /// </summary>
        /// <param name="client">a valid SocialPlusClient</param>
        /// <param name="users">a list of UserInfo objects representing the users who will post</param>
        /// <param name="numTopicsPerUser">the number of topics each user should post</param>
        /// <returns>a list of TopicInfo objects corresponding to the created topics</returns>
        private static async Task<List<TopicInfo>> CreateTopics(SocialPlusClient client, List<UserInfo> users, int numTopicsPerUser)
        {
            List<TopicInfo> topics = new List<TopicInfo>();

            for (int userIndex = 0; userIndex < users.Count; userIndex++)
            {
                for (int i = 0; i < numTopicsPerUser; i++)
                {
                    PostTopicResponse postTopicResponse = await TestUtilities.PostGenericTopic(client, users[userIndex].BearerToken);
                    string topicHandle = postTopicResponse.TopicHandle;
                    TopicInfo topicInfo = default(TopicInfo);
                    topicInfo.TopicIndex = (userIndex * numTopicsPerUser) + i;
                    topicInfo.TopicHandle = topicHandle;
                    topicInfo.UserIndex = userIndex;
                    topics.Add(topicInfo);
                }
            }

            return topics;
        }

        /// <summary>
        /// Create synthetic comments by having each of the supplied users post the given number of comments
        /// on topics chosen randomly with replacement
        /// </summary>
        /// <param name="client">a valid SocialPlusClient</param>
        /// <param name="users">a list of UserInfo objects representing the users who will post comments</param>
        /// <param name="topics">a list of TopicInfo objects representing the topics to comment on</param>
        /// <param name="numCommentsPerUser">the number of comments each user should post</param>
        /// <returns>a list of comment handles for the created comments</returns>
        private static async Task<List<string>> CreateComments(SocialPlusClient client, List<UserInfo> users, List<TopicInfo> topics, int numCommentsPerUser)
        {
            List<string> commentHandles = new List<string>();

            for (int userIndex = 0; userIndex < users.Count; userIndex++)
            {
                for (int i = 0; i < numCommentsPerUser; i++)
                {
                    string topicHandle = topics[GenerateRandomInt(0, topics.Count)].TopicHandle;
                    PostCommentResponse postCommentResponse = await TestUtilities.PostGenericComment(client, users[userIndex].BearerToken, topicHandle);
                    string commentHandle = postCommentResponse.CommentHandle;
                    commentHandles.Add(commentHandle);
                }
            }

            return commentHandles;
        }

        /// <summary>
        /// Create synthetic replies by having each of the supplied users post the given number of replies
        /// on comments chosen randomly with replacement
        /// </summary>
        /// <param name="client">a valid SocialPlusClient</param>
        /// <param name="users">a list of UserInfo objects representing the users who will post comments</param>
        /// <param name="commentHandles">a list of comment handles corresponding to the comments to be replied to</param>
        /// <param name="numRepliesPerUser">the number of replies each user should post</param>
        /// <returns>a list of reply handles for the created replies</returns>
        private static async Task<List<string>> CreateReplies(SocialPlusClient client, List<UserInfo> users, List<string> commentHandles, int numRepliesPerUser)
        {
            List<string> replyHandles = new List<string>();

            for (int userIndex = 0; userIndex < users.Count; userIndex++)
            {
                for (int i = 0; i < numRepliesPerUser; i++)
                {
                    string commentHandle = commentHandles[GenerateRandomInt(0, commentHandles.Count)];
                    string text = "A reply to a comment";
                    PostReplyRequest request = new PostReplyRequest(text);
                    HttpOperationResponse<PostReplyResponse> response = await client.CommentReplies.PostReplyWithHttpMessagesAsync(commentHandle, request, users[userIndex].BearerToken);
                    replyHandles.Add(response.Body.ReplyHandle);
                }
            }

            return replyHandles;
        }

        /// <summary>
        /// Create synthetic following relationships by having each user follow the given number of other users
        /// chosen randomly without replacement
        /// </summary>
        /// <param name="client">a valid SocialPlusClient</param>
        /// <param name="users">a list of UserInfo objects representing the pool of users in which to create following relationships</param>
        /// <param name="numUsersToFollow">the number of users each user should follow</param>
        /// <returns>a task</returns>
        private static async Task FollowUsers(SocialPlusClient client, List<UserInfo> users, int numUsersToFollow)
        {
            for (int userIndex = 0; userIndex < users.Count; userIndex++)
            {
                List<UserInfo> shuffledUsers = GetShuffledList(users);
                shuffledUsers.Remove(users[userIndex]);

                for (int i = 0; i < numUsersToFollow; i++)
                {
                    PostFollowingUserRequest request = new PostFollowingUserRequest(shuffledUsers[i].UserHandle);
                    await client.MyFollowing.PostFollowingUserWithHttpMessagesAsync(request, users[userIndex].BearerToken);
                }
            }
        }

        /// <summary>
        /// Create synthetic topic likes by having each of the supplied users like the given number of topics
        /// chosen randomly without replacement
        /// </summary>
        /// <param name="client">a valid SocialPlusClient</param>
        /// <param name="users">a list of UserInfo objects representing the users who will like topics</param>
        /// <param name="topics">a list of TopicInfo objects representing the topics to like</param>
        /// <param name="numTopicLikesPerUser">the number of topics each user should like</param>
        /// <returns>a task</returns>
        private static async Task AddTopicLikes(SocialPlusClient client, List<UserInfo> users, List<TopicInfo> topics, int numTopicLikesPerUser)
        {
            for (int userIndex = 0; userIndex < users.Count; userIndex++)
            {
                List<TopicInfo> shuffledTopics = GetShuffledList(topics);
                for (int i = 0; i < numTopicLikesPerUser; i++)
                {
                    await client.TopicLikes.PostLikeWithHttpMessagesAsync(shuffledTopics[i].TopicHandle, users[userIndex].BearerToken);
                }
            }
        }

        /// <summary>
        /// Create synthetic comment likes by having each of the supplied users like the given number of comments
        /// chosen randomly without replacement
        /// </summary>
        /// <param name="client">a valid SocialPlusClient</param>
        /// <param name="users">a list of UserInfo objects representing the users who will like comments</param>
        /// <param name="commentHandles">a list of comment handles corresponding to the comments to like</param>
        /// <param name="numCommentLikesPerUser">the number of comments each user should like</param>
        /// <returns>a task</returns>
        private static async Task AddCommentLikes(SocialPlusClient client, List<UserInfo> users, List<string> commentHandles, int numCommentLikesPerUser)
        {
            for (int userIndex = 0; userIndex < users.Count; userIndex++)
            {
                List<string> shuffledCommentHandles = GetShuffledList(commentHandles);
                for (int i = 0; i < numCommentLikesPerUser; i++)
                {
                    await client.CommentLikes.PostLikeWithHttpMessagesAsync(shuffledCommentHandles[i], users[userIndex].BearerToken);
                }
            }
        }

        /// <summary>
        /// Create synthetic reply likes by having each of the supplied users like the given number of replies
        /// chosen randomly without replacement
        /// </summary>
        /// <param name="client">a valid SocialPlusClient</param>
        /// <param name="users">a list of UserInfo objects representing the users who will like replies</param>
        /// <param name="replyHandles">a list of reply handles corresponding to the replies to like</param>
        /// <param name="numReplyLikesPerUser">the number of replies each user should like</param>
        /// <returns>a task</returns>
        private static async Task AddReplyLikes(SocialPlusClient client, List<UserInfo> users, List<string> replyHandles, int numReplyLikesPerUser)
        {
            for (int userIndex = 0; userIndex < users.Count; userIndex++)
            {
                List<string> shuffledReplyHandles = GetShuffledList(replyHandles);
                for (int i = 0; i < numReplyLikesPerUser; i++)
                {
                    await client.ReplyLikes.PostLikeWithHttpMessagesAsync(shuffledReplyHandles[i], users[userIndex].BearerToken);
                }
            }
        }

        /// <summary>
        /// Randomize a list using a Fisher-Yates shuffle
        /// </summary>
        /// <typeparam name="T">the type of the list</typeparam>
        /// <param name="list">the list to shuffle</param>
        /// <returns>the shuffled list</returns>
        private static List<T> GetShuffledList<T>(List<T> list)
        {
            List<T> shuffledList = new List<T>();
            for (int k = 0; k < list.Count; k++)
            {
                shuffledList.Add(list[k]);
            }

            for (int i = shuffledList.Count - 1; i >= 1; i--)
            {
                int j = GenerateRandomInt(0, i + 1);
                T temp = shuffledList[i];
                shuffledList[i] = shuffledList[j];
                shuffledList[j] = temp;
            }

            return shuffledList;
        }

        /// <summary>
        /// Generate a random integer using the GenerateRandomUint() function from RandUtils
        /// </summary>
        /// <param name="min">the minimum value (inclusive)</param>
        /// <param name="max">the maximum value (exclusive)</param>
        /// <returns>the random integer</returns>
        private static int GenerateRandomInt(int min, int max)
        {
            if (min >= max)
            {
                throw new ArgumentException("min must be less than max");
            }

            return (int)(randUtils.GenerateRandomUint() % (uint)checked(max - min)) + min;
        }

        private struct UserInfo
        {
            public int UserIndex;
            public string UserHandle;
            public string BearerToken;
        }

        private struct TopicInfo
        {
            public int TopicIndex;
            public string TopicHandle;
            public int UserIndex;
        }
    }
}
