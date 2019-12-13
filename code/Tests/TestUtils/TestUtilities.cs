// <copyright file="TestUtilities.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.TestUtils
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.Logging;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Utils;

    /// <summary>
    /// Class to handle all the generic test utilties
    /// </summary>
    public class TestUtilities
    {
        /// <summary>
        /// handle generator
        /// </summary>
        private static readonly HandleGenerator HandleGenerator = new HandleGenerator();

        /// <summary>
        /// Sequence number used to as string suffix to name creation
        /// </summary>
        private static readonly TimeOrderedSequenceNumber SeqNumber = new TimeOrderedSequenceNumber();

        /// <summary>
        /// Helper routine for posting a generic user
        /// </summary>
        /// <param name="client">Client object</param>
        /// <returns>Post user response</returns>
        public static async Task<PostUserResponse> PostGenericUser(SocialPlusClient client)
        {
            string uniqueSuffix = TestUtilities.CreateUniqueDigits();
            string firstName = "Barack" + uniqueSuffix;
            string lastName = "Obama" + uniqueSuffix;
            string bio = "44th President" + uniqueSuffix;
            return await TestUtilities.DoLogin(client, firstName, lastName, bio);
        }

        /// <summary>
        /// Helper routine for posting a generic topic.
        /// </summary>
        /// <param name="client">Client object</param>
        /// <param name="auth">authorization header value</param>
        /// <returns>A post topic response</returns>
        public static async Task<PostTopicResponse> PostGenericTopic(SocialPlusClient client, string auth)
        {
            string uniqueSuffix = TestUtilities.CreateUniqueDigits();
            string topicTitle = "Topic Title" + uniqueSuffix;
            string topicText = "Topic text is a long description" + uniqueSuffix;
            BlobType blobType = BlobType.Image;
            string blobHandle = "http://myBlobHandle/" + uniqueSuffix;
            string language = "en-US";

            PostTopicRequest postTopicRequest = new PostTopicRequest(publisherType: PublisherType.User, text: topicText, title: topicTitle, blobType: blobType, blobHandle: blobHandle, language: language);
            var httpResponse = await client.Topics.PostTopicWithHttpMessagesAsync(request: postTopicRequest, authorization: auth);
            if (httpResponse == null || httpResponse.Response == null || !httpResponse.Response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(string.Format("PostGenericTopic failed for auth {0} with status code {1}", auth, httpResponse.Response.StatusCode));
            }

            return httpResponse.Body;
        }

        /// <summary>
        /// Helper routine for posting a generic comment
        /// </summary>
        /// <param name="client">Client object</param>
        /// <param name="auth">authorization header value</param>
        /// <param name="topicHandle">The topic handle for the comment.</param>
        /// <returns>A post comment response</returns>
        public static async Task<PostCommentResponse> PostGenericComment(SocialPlusClient client, string auth, string topicHandle)
        {
            if (string.IsNullOrEmpty(topicHandle))
            {
                throw new ArgumentNullException("PostGenericComment called with empty topicHandle");
            }

            string uniqueSuffix = TestUtilities.CreateUniqueDigits();
            string text = "The text of a comment" + uniqueSuffix;
            string language = "en-US";

            PostCommentRequest postCommentRequest = new PostCommentRequest(text: text, language: language);
            var httpResponse = await client.TopicComments.PostCommentWithHttpMessagesAsync(topicHandle: topicHandle, request: postCommentRequest, authorization: auth);
            if (httpResponse == null || httpResponse.Response == null || !httpResponse.Response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(string.Format("PostGenericComment failed for  auth {0}, topicHandle {1}", auth, topicHandle));
            }

            return httpResponse.Body;
        }

        /// <summary>
        /// Helper routine for posting a generic reply
        /// </summary>
        /// <param name="client">Client object</param>
        /// <param name="auth">authorization header value</param>
        /// <param name="commentHandle">The comment handle for the reply.</param>
        /// <returns>A post comment response</returns>
        public static async Task<PostReplyResponse> PostGenericReply(SocialPlusClient client, string auth, string commentHandle)
        {
            if (string.IsNullOrEmpty(commentHandle))
            {
                throw new ArgumentNullException("PostGenericReply called with empty commentHandle");
            }

            string uniqueSuffix = TestUtilities.CreateUniqueDigits();
            string text = "The text of a reply" + uniqueSuffix;
            string language = "en-US";

            PostReplyRequest postCommentRequest = new PostReplyRequest(text: text, language: language);
            var httpResponse = await client.CommentReplies.PostReplyWithHttpMessagesAsync(commentHandle: commentHandle, request: postCommentRequest, authorization: auth);
            if (httpResponse == null || httpResponse.Response == null || !httpResponse.Response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(string.Format("PostGenericReply failed for  auth {0}", auth));
            }

            return httpResponse.Body;
        }

        /// <summary>
        /// Generic call to get create a user using the new communication library
        /// </summary>
        /// <param name="client"> social plus client</param>
        /// <param name="firstName"> first name of user to be created</param>
        /// <param name="lastName"> last name of user to be created</param>
        /// <param name="bio"> bio of user to be created</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<PostUserResponse> DoLogin(SocialPlusClient client, string firstName, string lastName, string bio)
        {
            PostUserRequest postUserRequest = new PostUserRequest(instanceId: "test", firstName: firstName, lastName: lastName, bio: bio);

            string userHandle = HandleGenerator.GenerateShortHandle();
            string auth = await TestUtilities.GetAADAuth(userHandle);
            HttpOperationResponse<PostUserResponse> postUserOperationResponse = await client.Users.PostUserWithHttpMessagesAsync(request: postUserRequest, authorization: auth);
            if (!postUserOperationResponse.Response.IsSuccessStatusCode)
            {
                throw new Exception("got " + postUserOperationResponse.Response.StatusCode);
            }

            return postUserOperationResponse.Body;
        }

        /// <summary>
        /// Gets an access token from AAD
        /// </summary>
        /// <returns>access token</returns>
        public static async Task<string> GetAADAccessToken()
        {
            // Get the access token for the Embedded Social Test Client 1 AAD application
            CertificateHelper certHelper = new CertificateHelper(TestConstants.ValidSPClient.CertThumbprint, TestConstants.ValidSPClient.ClientId, StoreLocation.CurrentUser);
            return await certHelper.GetAccessToken(TestConstants.ValidSPClient.Authority, TestConstants.ValidSPClient.AppUri);
        }

        /// <summary>
        /// Gets the authorization value for AAD
        /// </summary>
        /// <param name="userHandle">user handle (can be null if caller app cannot provide its own handles)</param>
        /// <returns>authorization value</returns>
        public static async Task<string> GetAADAuth(string userHandle = null)
        {
            string accessToken = await TestUtilities.GetAADAccessToken();
            return AuthHelper.CreateAADS2SAuth(accessToken, TestConstants.AppKey, userHandle);
        }

        /// <summary>
        /// Gets the authorization value for Anon authentication (appKey-only)
        /// </summary>
        /// <returns>authorization value</returns>
        public static string GetAnonAuth()
        {
            return AuthHelper.CreateAnonAuth(TestConstants.AppKey);
        }

        /// <summary>
        /// Helper routine to deal with reads that depend on service bus.
        /// Because service bus latency is variable, we need a long timeout but we don't want the test suite
        /// to take a very long time to finish when service bus is behaving well.
        ///
        /// Also, note that we must run the verify action inside a try/catch so that the test does not fail
        /// without having a chance to run its required clean-up actions.
        /// </summary>
        /// <param name="readOperations"> operations</param>
        /// <param name="verify"> verification calls</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task AutoRetryServiceBusHelper(Func<Task> readOperations, Action verify)
        {
            // execute the read action after the short delay
            await Task.Delay(TestConstants.ServiceBusShortDelay);
            await readOperations();

            // perform the verification to see if the test passes
            bool testFailed = false;
            try
            {
                // see if the test passes
                verify();
            }
            catch (Exception e)
            {
                Console.WriteLine("Auto Retry: attempt #1 failed. Exception:" + e.Message);

                // we interpret any exception as indication that the test failed
                testFailed = true;
            }

            var numRetries = 0;
            while (testFailed && numRetries < 3)
            {
                testFailed = false;
                numRetries++;

                if (numRetries == 1)
                {
                    // wait for the medium delay period, and then execute the read action again
                    await Task.Delay(TestConstants.ServiceBusMediumDelay);
                }
                else if (numRetries > 1)
                {
                    // wait for the long delay period, and then execute the read action again
                    await Task.Delay(TestConstants.ServiceBusLongDelay);
                }

                // redo the reads
                await readOperations();

                try
                {
                    // see if the test passes
                    verify();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Auto Retry: attempt #{0} failed. Exception {1}.", numRetries + 1, e.Message);
                    testFailed = true;
                }
            }

            // enable the following code if you want to attach a debugger when a test fails
#if notdef
            // after retries, if the test still fails, then launch a debugger and break.
            if (testFailed)
            {
                // launch a debugger if there isn't one already attached
                Debugger.Launch();
                Debugger.Break();
            }
#endif
        }

        /// <summary>
        /// Helper routine to clean up a user using the SocialPlusClient api.
        /// </summary>
        /// <param name="client">Client object</param>
        /// <param name="authorization">authorization header value</param>
        /// <returns>An <see cref="HttpOperationResponse"/> for the deletion.</returns>
        public static async Task<HttpOperationResponse<object>> DeleteUser(SocialPlusClient client, string authorization)
        {
            return await client.Users.DeleteUserWithHttpMessagesAsync(authorization: authorization);
        }

        /// <summary>
        /// Helper routine to clean up a topic using the SocialPlusClient api.
        /// </summary>
        /// <param name="client">Client object</param>
        /// <param name="topicHandle">The handle for the topic, can be null.</param>
        /// <param name="authorization">authorization header value</param>
        /// <returns>An <see cref="HttpOperationResponse"/> for the deletion.</returns>
        public static async Task<HttpOperationResponse<object>> DeleteTopic(SocialPlusClient client, string topicHandle, string authorization)
        {
            HttpOperationResponse<object> deleteTopicResponse = null;
            if (!string.IsNullOrWhiteSpace(topicHandle))
            {
                deleteTopicResponse = await client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: authorization);
            }

            return deleteTopicResponse;
        }

        /// <summary>
        /// Helper routine to clean up a comment using the SocialPlusClient api.
        /// </summary>
        /// <param name="client">Client object</param>
        /// <param name="commentHandle">The handle for the comment, can be null.</param>
        /// <param name="authorization">authorization header value</param>
        /// <returns>An <see cref="HttpOperationResponse"/> for the deletion.</returns>
        public static async Task<HttpOperationResponse<object>> DeleteComment(SocialPlusClient client, string commentHandle, string authorization)
        {
            HttpOperationResponse<object> deleteCommentResponse = null;
            if (!string.IsNullOrWhiteSpace(commentHandle))
            {
                deleteCommentResponse = await client.Comments.DeleteCommentWithHttpMessagesAsync(commentHandle: commentHandle, authorization: authorization);
            }

            return deleteCommentResponse;
        }

        /// <summary>
        /// Helper routine to clean up a reply using the SocialPlusClient api.
        /// This version takes a replyHandle.
        /// </summary>
        /// <param name="client">Client object</param>
        /// <param name="replyHandle">The handle for the reply, can be null.</param>
        /// <param name="authorization">authorization header value</param>
        /// <returns>An <see cref="HttpOperationResponse"/> for the deletion.</returns>
        public static async Task<HttpOperationResponse<object>> DeleteReply(SocialPlusClient client, string replyHandle, string authorization)
        {
            HttpOperationResponse<object> deleteReplyResponse = null;
            if (!string.IsNullOrWhiteSpace(replyHandle))
            {
                deleteReplyResponse = await client.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: replyHandle, authorization: authorization);
            }

            return deleteReplyResponse;
        }

        /// <summary>
        /// Helper routine to clean up a reply using the SocialPlusClient api.
        /// This version takes a replyResponse, for when a handle hasn't been extracted.
        /// </summary>
        /// <param name="client">Client object</param>
        /// <param name="postReplyResponse">The reply response, can be null.</param>
        /// <param name="authorization">authorization header value</param>
        /// <returns>An <see cref="HttpOperationResponse"/> for the deletion.</returns>
        public static async Task<HttpOperationResponse<object>> DeleteReply(SocialPlusClient client, HttpOperationResponse<PostReplyResponse> postReplyResponse, string authorization)
        {
            HttpOperationResponse<object> deleteReplyResponse = null;
            if (postReplyResponse != null && postReplyResponse.Response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(postReplyResponse.Body.ReplyHandle))
            {
                deleteReplyResponse = await client.Replies.DeleteReplyWithHttpMessagesAsync(replyHandle: postReplyResponse.Body.ReplyHandle, authorization: authorization);
            }

            return deleteReplyResponse;
        }

        /// <summary>
        /// Creates a string of digits that is always unique across test runs.
        /// </summary>
        /// <returns>string of unit digits</returns>
        public static string CreateUniqueDigits()
        {
            return Convert.ToString(SeqNumber.GenerateStronglyOrderedSequenceNumber());
        }

        /// <summary>
        /// Removes undesired prefixes and suffixies of the server's name. This can be used for
        /// prettifying output.
        /// </summary>
        /// <param name="serverName">a server's name</param>
        /// <returns>Server Name</returns>
        public static string PrettyServerName(Uri serverName)
        {
            string[] suffixesToRemove = new string[1] { ".cloudapp.net" };
            string result = serverName.Host;

            foreach (string suffixToRemove in suffixesToRemove)
            {
                if (serverName.Host.EndsWith(suffixToRemove))
                {
                    result = serverName.Host.Substring(0, serverName.Host.Length - suffixToRemove.Length);
                }
            }

            return result;
        }

        /// <summary>
        /// Summarize exception into an e-mail
        /// </summary>
        /// <param name="methodName">name of method</param>
        /// <param name="ex">exception that fired</param>
        /// <returns>Email message as a string</returns>
        public static string Exception2HtmlEmail(string methodName, Exception ex)
        {
            string msg = "Unit test failed in " + methodName + " at " + DateTime.Now.ToString("HH:mm:ss");
            msg += "<br><table>";
            msg += "<tr><td><b>Server</b><td>" + TestConstants.ServerApiBaseUrl.Host;
            msg += "<tr><td><b>ServerApiBaseUrl</b><td>" + TestConstants.ServerApiBaseUrl;
            msg += "<tr><td><b>AppKey</b><td>" + TestConstants.AppKey;
            if (ex != null)
            {
                msg += "<tr><td><b>Exception type</b><td>" + ex.GetType();
                msg += "</table><br><br>" + ex.ToString();
            }
            else
            {
                msg += "</table>";
            }

            msg += "<br>";

            return msg;
        }

        /// <summary>
        /// Retrieves SendGrid key from KV
        /// </summary>
        /// <param name="configFile">config file path</param>
        /// <returns>SendGrid key</returns>
        public static string GetSendGridKey(string configFile)
        {
            var sr = new FileSettingsReader(configFile);
            var certThumbprint = sr.ReadValue(TestConstants.SocialPlusCertThumbprint);
            var clientID = sr.ReadValue(TestConstants.EmbeddedSocialClientIdSetting);
            var storeLocation = StoreLocation.CurrentUser;
            var vaultUrl = sr.ReadValue(TestConstants.SocialPlusVaultUrlSetting);
            ICertificateHelper cert = new CertificateHelper(certThumbprint, clientID, storeLocation);
            IKeyVaultClient client = new AzureKeyVaultClient(cert);

            var log = new Log(LogDestination.Console, Log.DefaultCategoryName);
            var kv = new KV(log, clientID, vaultUrl, certThumbprint, storeLocation, client);
            var kvReader = new KVSettingsReader(sr, kv);
            return kvReader.ReadValueAsync("SendGridInstrumentationKey").Result;
        }
    }
}