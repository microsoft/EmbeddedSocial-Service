// <copyright file="AuthTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System.Configuration;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;
    using SocialPlus.Utils;

    /// <summary>
    /// Tests that authentication works. These steps are done using a valid certificate
    /// of a valid SocialPlus client, a valid certificate of an invalid SocialPlus client, and a malformed token.
    /// </summary>
    [TestClass]
    public class AuthTests
    {
        /// <summary>
        /// settings for a valid SocialPlus client
        /// </summary>
        private static readonly AADSettings ValidSPClient = new AADSettings(ConfigurationManager.AppSettings["AADEmbeddedSocialTestClient1Settings"]);

        /// <summary>
        /// settings for an invalid SocialPlus client
        /// </summary>
        private static readonly AADSettings InvalidSPClient = new AADSettings(ConfigurationManager.AppSettings["AADEmbeddedSocialTestClient2Settings"]);

        /// <summary>
        /// handle generator. AAD tests need valid user handles
        /// </summary>
        private static readonly HandleGenerator HandleGenerator = new HandleGenerator();

        /// <summary>
        /// Tests a valid AAD token of a valid SocialPlus by creating and deleting a user
        /// </summary>
        /// <returns>Task</returns>
        [TestMethod]
        public async Task CreateDeleteUserWithValidAADTokenValidSPClient()
        {
            // Get the access token for a valid AAD application
            CertificateHelper certHelper = new CertificateHelper(ValidSPClient.CertThumbprint, ValidSPClient.ClientId, StoreLocation.CurrentUser);
            string accessToken = await certHelper.GetAccessToken(ValidSPClient.Authority, ValidSPClient.AppUri);
            string userHandle = HandleGenerator.GenerateShortHandle();
            string auth = AuthHelper.CreateAADS2SAuth(accessToken, TestConstants.AppKey, userHandle);

            // Set up initial stuff
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserRequest postUserRequest =
                new PostUserRequest(instanceId: TestConstants.InstanceId, firstName: "Joseph", lastName: "Johnson", bio: "Some Bio");
            HttpOperationResponse<PostUserResponse> postUserOperationResponse =
                await client.Users.PostUserWithHttpMessagesAsync(request: postUserRequest, authorization: auth);

            HttpOperationResponse<object> deleteUserOperationResponse =
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // Assert correct HTTP error codes
            Assert.IsTrue(postUserOperationResponse.Response.IsSuccessStatusCode);
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);

            // PostUser also returns a non-empty session token and the user handle
            Assert.IsFalse(string.IsNullOrEmpty(postUserOperationResponse.Body.SessionToken));
            Assert.AreEqual(userHandle, postUserOperationResponse.Body.UserHandle);
        }

        /// <summary>
        /// Tests an invalid AAD token
        /// </summary>
        /// <returns>Task</returns>
        [TestMethod]
        public async Task CreateDeleteUserWithInvalidAADToken()
        {
            // Create fake access token
            string fakeAccessToken = "Stefan Rules!";
            string userHandle = HandleGenerator.GenerateShortHandle();
            string auth = AuthHelper.CreateAADS2SAuth(fakeAccessToken, TestConstants.AppKey, userHandle);

            // Set up initial stuff
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserRequest postUserRequest =
                new PostUserRequest(instanceId: TestConstants.InstanceId, firstName: "Joseph", lastName: "Johnson", bio: "Some Bio");
            HttpOperationResponse<PostUserResponse> postUserOperationResponse =
                await client.Users.PostUserWithHttpMessagesAsync(request: postUserRequest, authorization: auth);

            // the above post user operation should fail.  but in case it doesn't, we clean up the user we created
            HttpOperationResponse<object> deleteUserOperationResponse =
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // both the create and delete operations should fail
            Assert.AreEqual(HttpStatusCode.Unauthorized, postUserOperationResponse.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, deleteUserOperationResponse.Response.StatusCode);
        }

        /// <summary>
        /// Tests authentication using a valid AAD token, but with the wrong value for the audience.  Each SocialPlus server
        /// instance is configured with an App that uses AADS2S authentication and requires that the AAD token's audience
        /// must be "https://embeddedsocial.microsoft.com/testclient1". In this test, instead we use a token whose audience
        /// is http://SocialPlus.
        /// </summary>
        /// <returns>Task</returns>
        [TestMethod]
        public async Task CreateDeleteUserWithValidAADTokenInvalidAudience()
        {
            // Get the access token for SocialPlus AAD application. While this is a valid AAD token,
            // the service checks that the token's audience is "https://embeddedsocial.microsoft.com/testclient1".
            // In this case, it is not. Instead, the audience is "https://embeddedsocial.microsoft.com/testclient2"
            CertificateHelper certHelper = new CertificateHelper(InvalidSPClient.CertThumbprint, InvalidSPClient.ClientId, StoreLocation.CurrentUser);
            string accessToken = await certHelper.GetAccessToken(InvalidSPClient.Authority, InvalidSPClient.AppUri);
            string userHandle = HandleGenerator.GenerateShortHandle();
            string auth = AuthHelper.CreateAADS2SAuth(accessToken, TestConstants.AppKey, userHandle);

            // Set up initial stuff
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserRequest postUserRequest =
                new PostUserRequest(instanceId: TestConstants.InstanceId, firstName: "Joseph", lastName: "Johnson", bio: "Some Bio");
            HttpOperationResponse<PostUserResponse> postUserOperationResponse =
                await client.Users.PostUserWithHttpMessagesAsync(request: postUserRequest, authorization: accessToken);

            // the above post user operation should fail.  but in case it doesn't, we clean up the user we created
            HttpOperationResponse<object> deleteUserOperationResponse =
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: accessToken);

            // both the create and delete operations should fail
            Assert.AreEqual(HttpStatusCode.Unauthorized, postUserOperationResponse.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, deleteUserOperationResponse.Response.StatusCode);
        }

        /// <summary>
        /// Tests creating a user twice should return conflict.
        /// Create User 1
        /// Create User 1 (returns conflict).
        /// Delete User 1
        /// Delete User 1
        /// </summary>
        /// <returns>Task</returns>
        [TestMethod]
        public async Task CreateUserTwiceDeleteUserTwice()
        {
            // Get the access token for the Embedded Social Test Client 1 AAD application
            CertificateHelper certHelper = new CertificateHelper(ValidSPClient.CertThumbprint, ValidSPClient.ClientId, StoreLocation.CurrentUser);
            string accessToken = await certHelper.GetAccessToken(ValidSPClient.Authority, ValidSPClient.AppUri);
            string userHandle = HandleGenerator.GenerateShortHandle();
            string auth = AuthHelper.CreateAADS2SAuth(accessToken, TestConstants.AppKey, userHandle);

            // Set up initial stuff
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserRequest postUserRequest =
                new PostUserRequest(instanceId: TestConstants.InstanceId, firstName: "Joseph", lastName: "Johnson", bio: "Some Bio");

            HttpOperationResponse<PostUserResponse> postUserOperationResponse1 =
                await client.Users.PostUserWithHttpMessagesAsync(request: postUserRequest, authorization: auth);

            HttpOperationResponse<PostUserResponse> postUserOperationResponse2;
            postUserOperationResponse2 = await client.Users.PostUserWithHttpMessagesAsync(request: postUserRequest, authorization: auth);

            HttpOperationResponse<object> deleteUserOperationResponse1 =
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            HttpOperationResponse<object> deleteUserOperationResponse2 =
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // Assert correct HTTP error codes
            Assert.IsTrue(postUserOperationResponse1.Response.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.Conflict, postUserOperationResponse2.Response.StatusCode);
            Assert.IsTrue(deleteUserOperationResponse1.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// Tests authenticating with a Microsoft access token.
        /// This test should not be used in our E2E test suite because the test has no way to create a valid
        /// Microsoft access token automatically. Instead, a Microsoft access token must be created via some out of band mechanism
        /// and hardcoded each time the test needs to run. Each fresh token has a lifetime of about 10 minutes.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task CreateGetDeleteUserWithMSAAccessToken()
        {
            // Need to Manually get the Access Token for a Microsoft Account before running tests.
            // Directions:
            // 1) Put this URL in browser:
            // https://login.live.com/oauth20_authorize.srf?client_id=0000000040169587&scope=wl.signin%20wl.basic&response_type=token&redirect_uri=http://research.microsoft.com
            // 2) Redirects to RMC - copy the redirected URL in MyURL constant below ...
            // Will look like this:
            string myURL = "https://www.microsoft.com/en-us/research/#access_token=EwD4Aq1DBAAUGCCXc8wU/zFu9QnLdZXy%2bYnElFkAAbU00OJc7OUURhrM11nflV99uPC4rgUL1k7bE%2bVOqRMK0i5YY%2bvF7TennG%2beoRFxYeuiDWJiwxSxe8%2bwuDALuEUN%2bf2fZ7AjJoGfAZ7tZXQGAQLJ9bbz/7bglfs6pSYpLJT8uJhEKN5jC3%2b0pMqRwOUOzaZ3TquL3DMydR6IiNpspPeIzS7NpuvLYCHW/r8iLrasHD7ntoke6IXLxQ4o3VlfmYf/v5PuJh7Gh2ip%2bGxdfX67INfR/HhVRj7Nrse2XLkQaGXSqPPZ%2bI4O0hBX/n1qeNkxowNLvTSSSowDcKbBc/j6MR8FjcTZH3AO7gOK69fCqV0SZ0mFNKGkb34iWBEDZgAACBydF1wP9oeEyAFQizCqMAQY8DNfr1ng7z/X1J%2bIj3NYIQFPliNp1ueTMSN4fnUGDORw8sNbkG/5OI3yWl2NOUtEStIAigp3ZDx3Z7ReoG4BtAycYx4ulfXbqMtcz2KqKzH0Qct8ZN5G2cpi6GApxH9t7ElJBVV9i3tsm0Rs0hEWt7Pyiyqz6HoBbSSd/Wq4BzrevpaxP6uzT54z1oRdNOCNc4Hidaj7xvjDjwd/av6gW72ZeH1xvx9k129pPgLFrEVWV0k6/82mvt%2bN6C8RoZLgInrsbQd6vyLL7sqDHh1j59ZSPeSufJ5Sku4qvriDtdj6FyZ4PY2f3qWN2FnZ6gWzHlsGkjcVLIpHSEC2txEzIJMyL3pHYdLtTIsX5Nrf1tpKK6ODn2e14QpmtG%2bD%2b3j5yzupImOkn7DRkud8RYhghIqWm7OnAqCFZFRddj9UKiE/ok7mr3A2QnseSay%2bwi9Y3Zudb47CgDk8Nr9ROuGqatBJgm%2bZvKq7ue%2bds3JSZUUh8iTHVGq7JjYa9kD0IDzAJWXxQipH1XZlEyIpmY6beP5vPz6OaN%2bWueKOt%2bAQvUEQdMcWimL8mmAZIvUI8FskpxxS8ds%2bpGuBaGjXa/h3BsfvAQ%3d%3d&token_type=bearer&expires_in=3600&scope=wl.signin%20wl.basic&authentication_token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiIsImtpZCI6IjEifQ.eyJ2ZXIiOjEsImlzcyI6InVybjp3aW5kb3dzOmxpdmVpZCIsImV4cCI6MTQ3NTExMDU1OCwidWlkIjoiOTc1ZWQ2NjRiZDA0ZTQyOTliNTgzYTlhOTcyNTY1OTQiLCJhdWQiOiJzcC1kZXYtdGVzdC5jbG91ZGFwcC5uZXQiLCJ1cm46bWljcm9zb2Z0OmFwcHVyaSI6ImFwcGlkOi8vMDAwMDAwMDA0MDE2OTU4NyIsInVybjptaWNyb3NvZnQ6YXBwaWQiOiIwMDAwMDAwMDQwMTY5NTg3In0.WKb0Nxua1jkQeUf8mIKVnZV5bQUdr00OSk6o7h5e1Zk&user_id=975ed664bd04e4299b583a9a97256594";

            string auth = "Microsoft AK = " + TestConstants.AppKey + "| TK = " + AuthHelper.GetMSAAccessTokenFromURL(myURL);

            // Set up initial stuff
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserRequest postUserRequest =
                new PostUserRequest(instanceId: TestConstants.InstanceId, firstName: "Joseph", lastName: "Johnson", bio: "Some Bio");
            HttpOperationResponse<PostUserResponse> postUserOperationResponse =
                await client.Users.PostUserWithHttpMessagesAsync(request: postUserRequest, authorization: auth);

            // Assert correct HTTP error codes. If incorrect, we do not need to delete the user any longer
            Assert.IsTrue(postUserOperationResponse.Response.IsSuccessStatusCode);

            // PostUser also returns a non-empty session token and the user handle
            Assert.IsFalse(string.IsNullOrEmpty(postUserOperationResponse.Body.SessionToken));
            Assert.IsFalse(string.IsNullOrEmpty(postUserOperationResponse.Body.UserHandle));

            // Get the user using the OAuth credentials
            HttpOperationResponse<UserProfileView> userProfile = await client.Users.GetMyProfileWithHttpMessagesAsync(authorization: auth);

            // Assert correct user profile
            Assert.IsTrue(userProfile.Response.IsSuccessStatusCode);
            Assert.AreEqual(postUserOperationResponse.Body.UserHandle, userProfile.Body.UserHandle);

            // Delete the user using the session token
            auth = AuthHelper.CreateSocialPlusAuth(postUserOperationResponse.Body.SessionToken);
            HttpOperationResponse<object> deleteUserOperationResponse =
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // Assert delete was successful
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);
        }

        /// <summary>
        /// Tests authenticating with a Facebook access token.
        /// This test should not be used in our E2E test suite because the test has no way to create a valid
        /// Facebook access token automatically. Instead, a Facebook access token must be created via some out of band mechanism
        /// and hardcoded each time the test needs to run. Each fresh token has a lifetime of about 10 minutes.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task CreateGetDeleteUserWithFacebookAccessToken()
        {
            // Need to Manually get the Access Token for a Facebook Account before running tests.
            // Directions for using an app registered by Stefan but for whom Sharad and Alec have access also:
            // 1) Put this URL in browser:
            // https://www.facebook.com/v2.5/dialog/oauth?client_id=552285811539105&response_type=token&redirect_uri=https%3A%2F%2Fwww.facebook.com/connect/login_success.html
            // 2) Login to Facebook. Once log-in is successful, copy the URL immediately (sometimes the browser will show the URL for 1 second and then hide it).
            // Will look like this:
            string myURL = "https://www.facebook.com/connect/login_success.html#access_token=EAAH2TQZBcfKEBAFKQKTpgMjpnLvBIcX82IH248LqZCfcxtRwlczQHATBH8eMwaYgQB7AW9eZBnPZClOeXCkC9IZByo9GWx4dOZCP2RPx0tg5JBfNxDQQfTZB5ppjbS5cJ85c4615VboF7Eo1WEOELaAI88EUUrkr4wZD&expires_in=5183996";

            string auth = "Facebook AK = " + TestConstants.AppKey + "| TK = " + AuthHelper.GetFacebookAccessTokenFromURL(myURL);

            // Set up initial stuff
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            PostUserRequest postUserRequest =
                new PostUserRequest(instanceId: TestConstants.InstanceId, firstName: "Joseph", lastName: "Johnson", bio: "Some Bio");
            HttpOperationResponse<PostUserResponse> postUserOperationResponse =
                await client.Users.PostUserWithHttpMessagesAsync(request: postUserRequest, authorization: auth);

            // Assert correct HTTP error codes. If incorrect, we do not need to delete the user any longer
            Assert.IsTrue(postUserOperationResponse.Response.IsSuccessStatusCode);

            // PostUser also returns a non-empty session token and the user handle
            Assert.IsFalse(string.IsNullOrEmpty(postUserOperationResponse.Body.SessionToken));
            Assert.IsFalse(string.IsNullOrEmpty(postUserOperationResponse.Body.UserHandle));

            // Get the user using the OAuth credentials
            HttpOperationResponse<UserProfileView> userProfile = await client.Users.GetMyProfileWithHttpMessagesAsync(authorization: auth);

            // Assert correct user profile
            Assert.IsTrue(userProfile.Response.IsSuccessStatusCode);
            Assert.AreEqual(postUserOperationResponse.Body.UserHandle, userProfile.Body.UserHandle);

            // Delete the user using the session token
            auth = AuthHelper.CreateSocialPlusAuth(postUserOperationResponse.Body.SessionToken);
            HttpOperationResponse<object> deleteUserOperationResponse =
                await client.Users.DeleteUserWithHttpMessagesAsync(authorization: auth);

            // Assert delete was successful
            Assert.IsTrue(deleteUserOperationResponse.Response.IsSuccessStatusCode);
        }
    }
}
