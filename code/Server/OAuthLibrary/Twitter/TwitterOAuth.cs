// <copyright file="TwitterOAuth.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// Implements the <c>OAuth</c> flows for Twitter.
    /// Twitter-specific implementation of OAuth flows. In particular it implements all steps
    /// described by this page: https://dev.twitter.com/web/sign-in/implementing
    /// </summary>
    public partial class OAuth
    {
        /// <summary>
        /// The Twitter URL where request token is obtained from
        /// </summary>
        private static string twtrRequestTokenURL = "https://api.twitter.com/oauth/request_token";

        /// <summary>
        /// The Twitter URL where access token is validated
        /// </summary>
        private static string twtrAccessTokenURL = "https://api.twitter.com/oauth/access_token";

        /// <summary>
        /// The Twitter URL where user profile is fetched from
        /// </summary>
        private static string twtrGetProfileURL = "https://api.twitter.com/1.1/account/verify_credentials.json";

        /// <summary>
        /// Implements obtaining a request token from Twitter on behalf of a client.
        /// </summary>
        /// <param name="twtrConsumerKey">ConsumerKey issued by Twitter to the app that requested the token.</param>
        /// <param name="twtrConsumerSecret">ConsumerSecret issued by Twitter to the app that requested the token.</param>
        /// <param name="twtrOAuthCallback">Callback registered by the app on their Twitter app portal.</param>
        /// <returns>the request token from Twitter</returns>
        private async Task<string> GetTwitterRequestToken(string twtrConsumerKey, string twtrConsumerSecret, string twtrOAuthCallback)
        {
            // request token obtained from Twitter
            string requestToken = null;

            // Response string returned by third party id provider
            string responseString = null;

            // Construct the request token request to Twitter
            TwitterAuthHeader twitterRequestTokenRequest = new TwitterAuthHeader
            {
                SigningKey = twtrConsumerSecret + "&",
                OauthCallback = twtrOAuthCallback,
                OauthConsumerKey = Uri.EscapeDataString(twtrConsumerKey),   // must escape the consumer key
                OauthNonce = OAuthUtil.GeneratePseudoRandomAlphaNumericString(32),
                OauthSignatureMethod = "HMAC-SHA1",
                OauthTimestamp = OAuthUtil.SecondsSinceBeginningOfTime,
                OauthVersion = "1.0"
            };

            // Create the outgoing HTTP request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(twtrRequestTokenURL);
            request.Method = "POST";
            request.Headers["Authorization"] = twitterRequestTokenRequest.AuthHeader(twtrRequestTokenURL, "POST");

            try
            {
                // Parse the incoming response
                WebResponse response = await request.GetResponseAsync();

                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader r = new StreamReader(responseStream))
                {
                    responseString = r.ReadToEnd();
                }

                var responseValues = System.Web.HttpUtility.ParseQueryString(responseString);
                if (responseValues == null)
                {
                    throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Twitter_1);
                }

                requestToken = responseValues["oauth_token"];
            }
            catch (Exception ex)
            {
                throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Twitter_1, ex);
            }

            return requestToken;
        }

        /// <summary>
        /// Calls Twitter with a request token and a verifier to receive an access token, and then to receive the user profile
        /// </summary>
        /// <param name="userRequestToken">request token obtained from Twitter in the first place (see above) </param>
        /// <param name="userVerifier">corresponds to <c>oauth_verifier</c> in Step 2 of Token sign in</param>
        /// <param name="twtrConsumerKey">ConsumerKey issued by Twitter to the app that requested the token.</param>
        /// <param name="twtrConsumerSecret">ConsumerSecret issued by Twitter to the app that requested the token.</param>
        /// <returns>a user profile specific to Twitter</returns>
        private async Task<TwitterProfile> TwitterAuthorizationCodeFlow(string userRequestToken, string userVerifier, string twtrConsumerKey, string twtrConsumerSecret)
        {
            // First, we try to fetch the access token and the access token secret from Twitter
            Tuple<string, string> result = await this.GetTwitterAccessTokenAndTokenSecret(userRequestToken, userVerifier, twtrConsumerKey, twtrConsumerSecret);

            return await this.TwitterImplicitFlow(result.Item1 /* oauth token */, result.Item2 /* oauth token secret */, twtrConsumerKey, twtrConsumerSecret);
        }

        /// <summary>
        /// Calls Twitter with a request token and a verifier, and receives an access token and an access token secret
        /// </summary>
        /// <param name="userRequestToken">request token obtained from Twitter in the first place (see above) </param>
        /// <param name="userVerifier">corresponds to <c>oauth_verifier</c> in Step 2 of Token sign in</param>
        /// <param name="twtrConsumerKey">ConsumerKey issued by Twitter to the calling app.</param>
        /// <param name="twtrConsumerSecret">ConsumerSecret issued by Twitter to the calling app.</param>
        /// <returns>a tuple where the first item is the access token and the second the access token secret</returns>
        private async Task<Tuple<string, string>> GetTwitterAccessTokenAndTokenSecret(string userRequestToken, string userVerifier, string twtrConsumerKey, string twtrConsumerSecret)
        {
            // access token
            string accessToken = null;

            // access token secret
            string accessTokenSecret = null;

            // Response string returned by third party id provider
            string responseString = null;

            // Construct the request token request to Twitter
            TwitterAuthHeader twitterAccessTokenRequest = new TwitterAuthHeader
            {
                SigningKey = twtrConsumerSecret + "&" + Uri.EscapeDataString(userRequestToken),
                OauthToken = userRequestToken,
                OauthVerifier = userVerifier,
                OauthCallback = twtrAccessTokenURL,
                OauthConsumerKey = Uri.EscapeDataString(twtrConsumerKey),   // must escape the consumer key
                OauthNonce = OAuthUtil.GeneratePseudoRandomAlphaNumericString(32),
                OauthSignatureMethod = "HMAC-SHA1",
                OauthTimestamp = OAuthUtil.SecondsSinceBeginningOfTime,
                OauthVersion = "1.0"
            };

            // Create the outgoing HTTP request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(twtrAccessTokenURL);
            request.Method = "POST";
            request.Headers["Authorization"] = twitterAccessTokenRequest.AuthHeader(twtrAccessTokenURL, "POST");

            try
            {
                // Parse the incoming response
                WebResponse response = await request.GetResponseAsync();

                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader r = new StreamReader(responseStream))
                {
                    responseString = r.ReadToEnd();

                    var responseValues = System.Web.HttpUtility.ParseQueryString(responseString);

                    // Check whether the response is empty
                    if (responseValues == null)
                    {
                        throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Twitter_3);
                    }

                    accessToken = responseValues["oauth_token"];
                    accessTokenSecret = responseValues["oauth_token_secret"];
                }
            }
            catch (Exception ex)
            {
                throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Twitter_3, ex);
            }

            return new Tuple<string, string>(accessToken, accessTokenSecret);
        }

        /// <summary>
        /// Calls Twitter with the access token and access token secret and expects the user profile (aka decodes the token)
        /// </summary>
        /// <param name="accessToken">Twitter user access token</param>
        /// <param name="accessTokenSecret">Twitter user access token secret</param>
        /// <param name="twtrConsumerKey">ConsumerKey issued by Twitter to the calling app.</param>
        /// <param name="twtrConsumerSecret">ConsumerSecret issued by Twitter to the calling app.</param>
        /// <returns>a user profile specific to Twitter</returns>
        private async Task<TwitterProfile> TwitterImplicitFlow(string accessToken, string accessTokenSecret, string twtrConsumerKey, string twtrConsumerSecret)
        {
            // Construct the request token request to Twitter
            TwitterAuthHeader twitterUserProfileRequest = new TwitterAuthHeader
            {
                SigningKey = twtrConsumerSecret + "&" + accessTokenSecret,
                OauthToken = accessToken,
                OauthConsumerKey = Uri.EscapeDataString(twtrConsumerKey),   // must escape the consumer key
                OauthNonce = OAuthUtil.GeneratePseudoRandomAlphaNumericString(32),
                OauthSignatureMethod = "HMAC-SHA1",
                OauthTimestamp = OAuthUtil.SecondsSinceBeginningOfTime,
                OauthVersion = "1.0"
            };

            // User profile extracted from the token
            TwitterProfile tokenProfile = null;

            // Response string returned by third party id provider
            string responseString = null;

            // Create the outgoing HTTP request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(twtrGetProfileURL);
            request.Method = "GET";
            request.Headers["Authorization"] = twitterUserProfileRequest.AuthHeader(twtrGetProfileURL, "GET");

            try
            {
                // Parse the incoming response
                WebResponse response = await request.GetResponseAsync();

                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader r = new StreamReader(responseStream))
                {
                    responseString = r.ReadToEnd();
                }

                if (string.IsNullOrEmpty(responseString))
                {
                    throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Twitter_2);
                }

                tokenProfile = JsonConvert.DeserializeObject<TwitterProfile>(responseString);
            }
            catch (Exception ex)
            {
                throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Twitter_2, ex);
            }

            return tokenProfile;
        }
    }
}
