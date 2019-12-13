// <copyright file="OAuthTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace SocialPlus.UnitTests
{
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.OAuth;
    using SocialPlus.TestUtils;

    /// <summary>
    /// Basic functional tests of various OAuth features
    /// </summary>
    [TestClass]
    public class OAuthTests
    {
        // ***************************
        // Use for MSA Login
        // ***************************

        /// <summary>
        /// App Name for MSA
        /// </summary>
        public const string MSAAppName = "SocialPlusOAuthTest";

        /// <summary>
        /// Client ID
        /// </summary>
        public const string MSAClientID = "0000000040169587";

        /// <summary>
        /// Secret Key
        /// </summary>
        public const string MSASecretKey = "mQ52IpidrjN46aSsDkGH0S0cEfIGr5i";

        /// <summary>
        /// URL that is used to verify authentication
        /// </summary>
        public const string MSALoginAcctServiceURL = "https://login.live.com/oauth20_authorize.srf?";

        /// <summary>
        /// Need to Manually get the Access Token for a Microsoft Account before running tests.
        /// Directions:
        /// 1) Put this URL in browser:
        /// <![CDATA[https://login.live.com/oauth20_authorize.srf?client_id=0000000040169587&scope=wl.signin%20wl.basic&response_type=token&redirect_uri=http://research.microsoft.com]]>
        /// 2) Redirects to RMC - copy the redirected URL in MyURL constant below ... token will last for 10 mins
        /// 3) Run all tests
        /// Will look like this:
        /// </summary>
        public const string MyURL = "http://research.microsoft.com/en-us/#access_token=EwCAAq1DBAAUGCCXc8wU/zFu9QnLdZXy%2bYnElFkAAX1f9t4%2bdf2r0ZZkqUEBcKpCRvNmHAyOHPXy82EFlkd2rznqhW/BDcJAaYT1wgTyT8YTZZ37fOmXM3y4M6a1woiq839xinQQ6JeO6lYgFYSsab/JlelPNNtrmspYeTZHUvBfgKab9Zt64ChlHXSK2rHHADtLiqCYPCtnVAQeHcOBFd4hKszTKfpFkUXRwOCWHmw30QYR3fF4RaBivxiJEK1o%2bYW6Jx4Owsj3sH%2b/IhB9QtnbqFIXBlcU7QDQFoL3IpjXSM8cRGUxqxuvsrkzgRyz0mm97fVdnJceTKLiHqNjfnQwg6U8ZROGzzjok%2bfxb0mcEFRiFCZZb%2bI7cO8/QGQDZgAACJCGB6QUKWI7UAE8CNkrblUbsmihTSE8/Is5zK5ad5J66ii9tNbpJOyWXYTL3AZ2D/TC8rpaBQbwBIkM/n6wyoVQKaYH99zLCixQ9Aqf4ntkhYAX7wRnO0JVdHkRvOILh2ADYL5OpdkNnfNTXxF%2bMGjh1GlaHLEcsDI6w8As0lo8TwAJkNZez95ZqT5mr6DcbNeTibT6Qb1AEyHOMCM%2bKaGYB731kHkLcdRURU7KGQKD/fRwnGkALmu1hO2Wkd%2besXtnLSbS83BfTt%2bgdkaXhECRukyfW7HwesEWNnma0QykUyspGAuYJ5DmQL9bPL%2bQmdStkQWGef8McqQUgbCkcsy33KtFsgxzTtRqughMDe/aOMiUTFGug1wQsNyrs90OpR/7tgmDlgp3Hobbu6iK6Jc40EhbGHsxe%2bmsntj78sDzAZoORE12eXv3GZpuL1r04YJTLfvXav0I0kRMAQ%3d%3d&token_type=bearer&expires_in=3600&scope=wl.signin%20wl.basic&authentication_token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiIsImtpZCI6IjEifQ.eyJ2ZXIiOjEsImlzcyI6InVybjp3aW5kb3dzOmxpdmVpZCIsImV4cCI6MTQ0OTM0Mzg2MSwidWlkIjoiZjJkNTE4ZGQyZDQzYjA5ZGE5ZTAyNGQyZjQ5ZDY4NDkiLCJhdWQiOiJzcC1kZXYtdGVzdC5jbG91ZGFwcC5uZXQiLCJ1cm46bWljcm9zb2Z0OmFwcHVyaSI6ImFwcGlkOi8vMDAwMDAwMDA0MDE2OTU4NyIsInVybjptaWNyb3NvZnQ6YXBwaWQiOiIwMDAwMDAwMDQwMTY5NTg3In0.hYRG3GPBzkJOHrsHBrUaNOLPSvHIkjDr9rkj3NtdW2U&user_id=f2d518dd2d43b09da9e024d2f49d6849";

        // ***************************
        // Use for Facebook Login
        // ***************************

        /// <summary>
        /// Facebook app name
        /// </summary>
        public const string FBAppName = "SPTe­s­t­e­rApp";

        /// <summary>
        /// Facebook client id
        /// </summary>
        public const string FBClientID = "399457720179039";

        /// <summary>
        /// Facebook Secret Key
        /// </summary>
        public const string FBSecretKey = "0e80309683b7bd861a3052d1497981c2";

        /// <summary>
        /// URL to verify Facebook authentication
        /// </summary>
        public const string FBLoginAcctServiceURL = "https://graph.facebook.com/oauth/authorize?";

        /// <summary>
        /// Facebook Access token. Have to get it manually
        /// Get Access Token from here: https://developers.facebook.com/tools/explorer/ -- have to be logged in as SPTester
        /// <![CDATA[https://www.facebook.com/v2.5/dialog/oauth?client_id=552285811539105&response_type=token&redirect_uri=https%3A%2F%2Fwww.facebook.com/connect/login_success.html]]>
        /// </summary>
        public const string FBAccessToken = "EAAH2TQZBcfKEBAFZCT5WJrjBD7iJT0Ol1TjEjJcRZC1DLtyvZAVbNM2WiQDBlZAfsrZBdPZBep9LrtkR7Ej7pdbGl0ZASYi9JS9mdiKLjx3ehTcoyyEdDgtOi3oxLYhaypwlU6qbvHQurdcum8q9EYRJfZATokzFuHbwZD";

        // ***************************
        // Use for Google Login
        // ***************************

        /// <summary>
        /// Google app name
        /// </summary>
        public const string GOOGAppName = "SPTesterProject";

        /// <summary>
        /// Google client id
        /// </summary>
        public const string GOOGClientID = "78611932291-k70ri2j87pmd2ae4obimiq1otd75348v.apps.googleusercontent.com";

        /// <summary>
        /// Google Secret Key
        /// </summary>
        public const string GOOGSecretKey = "FoQmRqYWVNmk9NVYxhkeBJWY";

        /// <summary>
        /// Google Authorized User Code
        /// </summary>
        public const string GOOGAuthorizedUserCode = "4/qfx0n8jicZYsyydc5TMaKZZZTZHKSPFtSOaQaZos0_s"; // *** Manually get this!!

        /// <summary>
        /// Google redirect URL where Access Token will be appended too
        /// </summary>
        public const string GOOGRedirectURL = "http://www.cnn.com";

        /// <summary>
        /// Google project number - not sure if this is really used
        /// </summary>
        public const string GOOGProjNumber = "78611932291";

        /// <summary>
        /// Google authentication URL
        /// -- Gets Authorization Code in the redirected URL of CNN.COM:
        /// <![CDATA[https://accounts.google.com/o/oauth2/auth?redirect_uri=http%3A%2F%2Fwww.cnn.com&response_type=code&client_id=78611932291-k70ri2j87pmd2ae4obimiq1otd75348v.apps.googleusercontent.com&scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fadexchange.buyer&approval_prompt=force&access_type=offline]]>
        /// -- Can get authorization code and Access Token https://developers.google.com/oauthplayground/?code=4/ddfP3U0fXhbnZ-AEfMypUOHfanW7nreIWWoEHAJYg1c#
        /// </summary>
        public const string GOOGLoginAcctServiceURL = "https://accounts.google.com/o/oauth2/auth?";

        // ***************************
        // Use for Twitter Login
        // ***************************

        /// <summary>
        /// Twitter app name
        /// </summary>
        public const string TwitAppName = "SPTesterApp";

        /// <summary>
        /// Twitter Client ID
        /// </summary>
        public const string TwitClientID = "ekyojx32MC20Ogz9ITINP84Dr";

        /// <summary>
        /// Twitter Secret Key
        /// </summary>
        public const string TwitSecretKey = "6qGjMUpZzN8fZNMCLLza2QQ6X5Vw3H4UKY2rpQHXk2EbYmvR9q";

        /// <summary>
        /// Twitter Oauth Verifier ... Manually set this from twitter dev site
        /// </summary>
        public const string TwitOAuthVerifier = "gWuzMSZ8xU56UhVVBmyDF59GMSCLwqM8";

        /// <summary>
        /// Twitter OAuth Token .. (aka Requested Token) ... manually set this from twitter dev site
        /// </summary>
        public const string TwitOAuthToken = "qo1BmwAAAAAAjBfmAAABUW-LQ5g";

        /// <summary>
        /// Twitter redirect URL where Requested Token is attached to
        /// https://api.twitter.com/oauth/authenticate?oauth_token=mziQnwAAAAAAjBfmAAABUW-DCn4
        /// Example of data in redirect URL: oauth_token=mziQnwAAAAAAjBfmAAABUW-DCn4 oauth_verifier=KLd1tKPcidjjUWP3zJ6ZrP0gZIkeaWSn
        /// OAuthVerifier is the User Code parameter
        /// OAuthToken is the Requested Token
        /// Steps to get Twitter tests working:
        /// 1) Get Token from GetRequestToken in the test - basically have to run it to get RequestToken, copy it and stop the test
        /// 2) Put that Token in this call: https://api.twitter.com/oauth/authenticate?oauth_token=PUT-TOKEN-HERE
        /// 3) Will redirect and will have oauth_token and oauth_verifier in URL of redirected
        /// 4) Put oauth_token and oauth_verifier in the consts above
        /// 5) Run tests
        /// </summary>
        public const string TwitRedirectURL = "http://www.cnn.com";

        // The tests use the Social Plus OAuth testing ... make sure login using browser so automatically login
        // Logins:
        // Microsoft Account – L:SPTester@Hotmail.com – P:SocialPlusPWD!
        // FaceBook Account – L:SPTester@Hotmail.com – P:SocialPlusPWD! – DOB – 8/8/1980
        // Google Account – L:sptester93@Gmail.com- P: SocialPlusPWD!
        // Twitter Account – https://twitter.com/StevePTester – P: SocialPlusPWD! -- Used Hotmail acct as linked account

        /// <summary>
        /// ***************************************************
        /// **  Microsoft Login
        /// ***************************************************
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task ImplicitFlow_Microsoft_TokenTest_MANUAL_TEST()
        {
            // ** WARNING - Manually obtain the URL that has the Access Token and put in MyURL Const above
            // Strip the access token out of the redirected URL that was manually generated
            string accessToken = AuthHelper.GetMSAAccessTokenFromURL(MyURL);

            UserProfile msProfile = await OAuth.Instance.ImplicitFlow(IdentityProviders.Microsoft, accessToken);

            string testAccountFirstName = "Steve";
            string testAccountLastName = "Tester";

            // Validate it succeeds and gives info
            Assert.AreEqual(null, msProfile.FacebookProfile);
            Assert.AreEqual(null, msProfile.GoogleProfile);
            Assert.AreEqual(null, msProfile.TwitterProfile);
            Assert.AreEqual(IdentityProviders.Microsoft, msProfile.IdProvider);

            // This part is assuming using test test login account
            Assert.AreEqual(testAccountFirstName, msProfile.MicrosoftProfile.FirstName);
            Assert.AreEqual(testAccountLastName, msProfile.MicrosoftProfile.LastName);
            Assert.AreEqual(testAccountFirstName + " " + testAccountLastName, msProfile.MicrosoftProfile.Name);
            if (msProfile.MicrosoftProfile.Id == null)
            {
                Assert.Fail("FAIL! Profile ID is null!");
            }
        }

        /// <summary>
        /// Invalid Token sent to MSA
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test. Fail if exception</returns>
        [TestMethod]
        public async Task ImplicitFlow_Microsoft_InvalidTokenTest()
        {
            Assert.Fail("Bug #340 - Invalid token for Microsoft OAuth is returning 401 instead of invalid access token");

            try
            {
                string invalidAccessToken = "InvalidAccessTokenHere";

                UserProfile msProfile = await OAuth.Instance.ImplicitFlow(IdentityProviders.Microsoft, invalidAccessToken);

                // if gets here we know it failed as it shouldn't get to this line of code as call should pop a 401 error
                Assert.Fail("FAIL - ImplicitFlow call should have popped an 'Invalid OAuth access token' Exception, but didn't.");
            }
            catch (OAuthException ex)
            {
                if (ex.InnerException.ToString().Contains("Invalid OAuth access token.") == false)
                {
                    Assert.Fail("FAIL - Incorrect Exception. Looking for 'Invalid OAuth access token.' but found: " + ex.InnerException.ToString());
                }
            }
        }

        /// <summary>
        /// ***************************************************
        /// **  Facebook Login
        /// ***************************************************
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test. Fail if exception</returns>
        [TestMethod]
        public async Task ImplicitFlow_Facebook_TokenTest_MANUAL_TEST()
        {
            // ** WARNING - Manually obtain the Access Token and enter the string in the "FBAccessToken" const above
            string accessToken = FBAccessToken; // some day replace this with a method to programmitically get access token

            UserProfile fbProfile = await OAuth.Instance.ImplicitFlow(IdentityProviders.Facebook, accessToken);

            // Validate it succeeds and gives info
            Assert.AreEqual(null, fbProfile.MicrosoftProfile);
            Assert.AreEqual(null, fbProfile.GoogleProfile);
            Assert.AreEqual(null, fbProfile.TwitterProfile);
            Assert.AreEqual(IdentityProviders.Facebook, fbProfile.IdProvider);

            // Supposed to be logged in as FB user, but this is just the safe way to make sure got the right info instead of looking specifically for user info (Steve etc)
            if (fbProfile.FacebookProfile.Id == null)
            {
                Assert.Fail("FAIL! Profile ID is null!");
            }

            if (fbProfile.FacebookProfile.Name == null)
            {
                Assert.Fail("FAIL! Name is null!");
            }
        }

        /// <summary>
        /// Invalid Token sent to Facebook
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test. Fail if exception</returns>
        [TestMethod]
        public async Task ImplicitFlow_Facebook_InvalidTokenTest()
        {
            try
            {
                string invalidAccessToken = "InvalidAccessTokenHere";

                UserProfile fbProfile = await OAuth.Instance.ImplicitFlow(IdentityProviders.Facebook, invalidAccessToken);

                // if gets here we know it failed as it shouldn't get to this line of code as call should pop exception
                Assert.Fail("FAIL - ImplicitFlow call should have popped a 'Invalid OAuth access token' Exception, but didn't.");
            }
            catch (OAuthException ex)
            {
                if (ex.InnerException.Message.Contains("Invalid OAuth access token.") == false)
                {
                    Assert.Fail("FAIL - Incorrect Exception. Looking for a message that has 'Invalid OAuth access token.' but found: " + ex.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// ***************************************************
        /// **  Google Login
        /// ***************************************************
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test. Fail if exception</returns>
        [TestMethod]
        public async Task AuthorizationCodeFlow_Google_TokenTest_MANUAL_TEST()
        {
            Assert.Fail("Bug #359 - Google OAuth - Getting a HTTP 400 Bad Request when AuthorizationCodeFlow for Google");

            // ** WARNING - Manually obtain the Authorization User Code and set it in the GOOGAuthorizedUserCode const above
            UserProfile googProfile = await OAuth.Instance.AuthorizationCodeFlow(IdentityProviders.Google, GOOGAuthorizedUserCode, GOOGClientID, GOOGSecretKey, GOOGRedirectURL, null);

            // Validate it succeeds and gives info
            Assert.AreEqual(null, googProfile.MicrosoftProfile);
            Assert.AreEqual(null, googProfile.FacebookProfile);
            Assert.AreEqual(null, googProfile.TwitterProfile);
            Assert.AreEqual(IdentityProviders.Google, googProfile.IdProvider);

            Assert.Fail("TO DO YET!!!");

            // Supposed to be logged in as Google user, but this is just the safe way to make sure got the right info instead of looking specifically for user info (Steve etc)
            // if (GoogProfile.GoogleProfile. == null) Assert.Fail("FAIL! Profile ID is null!");
            // if (GoogProfile.FacebookProfile.Name == null) Assert.Fail("FAIL! Name is null!");
        }

        /// <summary>
        /// Invalid Auth User for Google
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test. Fail if exception</returns>
        [TestMethod]
        public async Task AuthorizationCodeFlow_Google_InvalidAuthUserCodeTest()
        {
            Assert.Fail("Bug #359 - Google OAuth - Getting a HTTP 400 Bad Request when AuthorizationCodeFlow for Google");

            try
            {
                string invalidAuthUserCode = "InvalidUserCode";

                UserProfile googProfile = await OAuth.Instance.AuthorizationCodeFlow(IdentityProviders.Google, invalidAuthUserCode, GOOGClientID, GOOGSecretKey, GOOGRedirectURL, null);

                // if gets here we know it failed as it shouldn't get to this line of code as call should pop exception
                Assert.Fail("FAIL - AuthCodeFlow call should have popped a 'Invalid OAuth access token' Exception, but didn't.");
            }
            catch (OAuthException ex)
            {
                if (ex.InnerException.Message.Contains("Invalid OAuth access token.") == false)
                {
                    Assert.Fail("FAIL - Incorrect Exception. Looking for a message that has 'Invalid OAuth access token.' but found: " + ex.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// Invalid Client ID for Google
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test. Fail if exception</returns>
        [TestMethod]
        public async Task AuthorizationCodeFlow_Google_InvalidClientIDTest_MANUAL_TEST()
        {
            Assert.Fail("Bug #359 - Google OAuth - Getting a HTTP 400 Bad Request when AuthorizationCodeFlow for Google");

            // ** WARNING - Manually obtain the Authorization User Code and set it in the GOOGAuthorizedUserCode const above
            try
            {
                string invalidClientID = "InvalidClientID";

                UserProfile googProfile = await OAuth.Instance.AuthorizationCodeFlow(IdentityProviders.Google, GOOGAuthorizedUserCode, invalidClientID, GOOGSecretKey, GOOGRedirectURL, null);

                // if gets here we know it failed as it shouldn't get to this line of code as call should pop exception
                Assert.Fail("FAIL - AuthCodeFlow call should have popped a 'Invalid OAuth Client ID' Exception, but didn't.");
            }
            catch (OAuthException ex)
            {
                if (ex.InnerException.Message.Contains("Invalid OAuth access token.") == false)
                {
                    Assert.Fail("FAIL - Incorrect Exception. Looking for a message that has 'Invalid OAuth access token.' but found: " + ex.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// Invalid Secret Key for Google
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test. Fail if exception</returns>
        [TestMethod]
        public async Task AuthorizationCodeFlow_Google_InvalidSecretTest_MANUAL_TEST()
        {
            Assert.Fail("Bug #359 - Google OAuth - Getting a HTTP 400 Bad Request when AuthorizationCodeFlow for Google");

            // ** WARNING - Manually obtain the Authorization User Code and set it in the GOOGAuthorizedUserCode const above
            try
            {
                string invalidSecret = "InvalidSecret";

                UserProfile googProfile = await OAuth.Instance.AuthorizationCodeFlow(IdentityProviders.Google, GOOGAuthorizedUserCode, GOOGClientID, invalidSecret, GOOGRedirectURL, null);

                // if gets here we know it failed as it shouldn't get to this line of code as call should pop exception
                Assert.Fail("FAIL - AuthCodeFlow call should have popped a 'Invalid OAuth Secret' Exception, but didn't.");
            }
            catch (OAuthException ex)
            {
                if (ex.InnerException.Message.Contains("Invalid OAuth access token.") == false)
                {
                    Assert.Fail("FAIL - Incorrect Exception. Looking for a message that has 'Invalid OAuth access token.' but found: " + ex.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// Using the Google token a second time
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test. Fail if exception</returns>
        [TestMethod]
        public async Task AuthorizationCodeFlow_Google_SecondUseTokenTest_MANUAL_TEST()
        {
            Assert.Fail("Bug #359 - Google OAuth - Getting a HTTP 400 Bad Request when AuthorizationCodeFlow for Google");

            // ** WARNING - Manually obtain the Authorization User Code and set it in the GOOGAuthorizedUserCode const above
            try
            {
                // Call it twice - should pop exception that it is a bad requeston second call
                UserProfile googProfile = await OAuth.Instance.AuthorizationCodeFlow(IdentityProviders.Google, GOOGAuthorizedUserCode, GOOGClientID, GOOGSecretKey, GOOGRedirectURL, null);
                UserProfile googProfile2 = await OAuth.Instance.AuthorizationCodeFlow(IdentityProviders.Google, GOOGAuthorizedUserCode, GOOGClientID, GOOGSecretKey, GOOGRedirectURL, null);

                // if gets here we know it failed as it shouldn't get to this line of code as call should pop exception
                Assert.Fail("FAIL - AuthCodeFlow call should have popped a 'Code was already redeemed.' Exception, but didn't.");
            }
            catch (OAuthException ex)
            {
                if (ex.InnerException.Message.Contains("Code was already redeemed.") == false)
                {
                    Assert.Fail("FAIL - Incorrect Exception. Looking for a message that has 'Code was already redeemed.' but found: " + ex.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// ***************************************************
        /// **  Twitter Login
        /// ***************************************************
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test. Fail if exception</returns>
        [TestMethod]
        public async Task AuthorizationCodeFlow_Twitter_TokenTest_MANUAL_TEST()
        {
            // ** WARNING - Manually obtain the token info and put in TwitOAuthToken const above

            // Basically this is called just to make sure it doesn't crash as we can't use it - also used to get the Token before the manual step
            string twitterRequestedToken = await SocialPlus.OAuth.OAuth.Instance.GetRequestToken(IdentityProviders.Twitter, TwitClientID, TwitSecretKey, TwitRedirectURL);

            // have to use same requested token that was used for the OAuthVerifier
            twitterRequestedToken = TwitOAuthToken;

            // Now call normal Authorization code flow
            UserProfile twitProfile = await OAuth.Instance.AuthorizationCodeFlow(IdentityProviders.Twitter, TwitOAuthVerifier, TwitClientID, TwitSecretKey, TwitRedirectURL, twitterRequestedToken);

            // Validate it succeeds and gives info
            Assert.AreEqual(null, twitProfile.MicrosoftProfile);
            Assert.AreEqual(null, twitProfile.GoogleProfile);
            Assert.AreEqual(null, twitProfile.FacebookProfile);
            Assert.AreEqual(IdentityProviders.Twitter, twitProfile.IdProvider);

            // Supposed to be logged in as Twitter user, but this is just the safe way to make sure got the right info instead of looking specifically for user info (Steve etc)
            if (twitProfile.TwitterProfile.Id == null)
            {
                Assert.Fail("FAIL! Profile ID is null!");
            }

            if (twitProfile.TwitterProfile.Name == null)
            {
                Assert.Fail("FAIL! Name is null!");
            }
        }

        /// <summary>
        /// Invalid Token sent to Twitter
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test. Fail if exception</returns>
        [TestMethod]
        public async Task AuthorizationCodeFlow_Twitter_InvalidTokenTest()
        {
            try
            {
                string invalidTwitterRequestedToken = "InvalidAccessTokenHere";

                UserProfile twitProfile = await OAuth.Instance.AuthorizationCodeFlow(IdentityProviders.Twitter, TwitOAuthVerifier, TwitClientID, TwitSecretKey, TwitRedirectURL, invalidTwitterRequestedToken);

                // if gets here we know it failed as it shouldn't get to this line of code as call should pop exception
                Assert.Fail("InvalidTokenTest failed: AuthorizationCodeFlow succeeded with an invalid Twitter token.");
            }
            catch (OAuthException ex)
            {
                if (ex.InnerException.Message.Contains("Unauthorized.") == false)
                {
                    Assert.Fail("InvalidTokenTest failed: Incorrect exception message.  Expected the message 'Unauthorized.' but the actual message is: " + ex.InnerException.Message);
                }
            }
        }
    } // Test Class
}
