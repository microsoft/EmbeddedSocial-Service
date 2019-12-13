// <copyright file="GoogleOAuth.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// Implements the <c>OAuth</c> flows for Google.
    /// </summary>
    public partial class OAuth
    {
        /// <summary>
        /// To simplify implementations and increase flexibility, OpenID Connect allows the use of a "Discovery document,"
        /// a JSON document found at a well-known location containing key-value pairs which provide details about the
        /// OpenID Connect provider's configuration, including the URIs of the authorization, token, <c>userinfo</c>, and public-keys endpoints.
        /// The Discovery document for Google's OpenID Connect service may be retrieved from:
        /// </summary>
        private static readonly string GoogleDiscoveryOpenIDConnectURL = "https://accounts.google.com/.well-known/openid-configuration";

        /// <summary>
        /// Internal lock. Together with the flag below, its role is to ensure the method <c>GInit</c> is called only once by this singleton
        /// </summary>
        private static readonly object GoogleLocker = new object();

        /// <summary>
        /// Internal flag. Together with the lock above, its role is to ensure the method <c>GInit</c> is called only once by this singleton
        /// </summary>
        private static bool googleInitStarted = false;

        /// <summary>
        /// Internal flag. Its role is to provide a barrier so that no work gets done until <c>Init</c> is done.
        /// </summary>
        private static ManualResetEvent googleInitDone = new ManualResetEvent(false);

        /// <summary>
        /// This variable gets filled out with information received from Google about their Open ID connect configuration.
        /// </summary>
        private static GoogleOpenIDDiscovery googleOpenIdDiscoveryInfo = null;

        /// <summary>
        /// Google has an OpenID discovery document. This document is at a fixed location that will never change and contains
        /// information about the OpenID discovery URLs. Since the latter could change over time, it's better to query them from
        /// Google then to statically hardcode in our library.
        /// </summary>
        /// <returns>no value, just an empty Task</returns>
        private async Task GInit()
        {
            // Guard that ensures GInit is executed once only
            lock (GoogleLocker)
            {
                if (googleInitStarted == true)
                {
                    return;
                }

                googleInitStarted = true;
            }

            // Create the outgoing HTTP request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GoogleDiscoveryOpenIDConnectURL);

            // Response string returned by Google
            string responseString = null;

            try
            {
                // Parse the incoming response
                WebResponse response = await request.GetResponseAsync();

                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader r = new StreamReader(responseStream))
                {
                    responseString = r.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                //// Extract the Web exception error message
                //// string errorMsg = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                throw new ConfigurationErrorsException("Running into problems with Google OpenID Connect discovery.", ex);
            }

            // Let's parse the response string
            if (string.IsNullOrEmpty(responseString))
            {
                throw new ConfigurationErrorsException("Running into problems with Google OpenID Connect discovery. Returned null or empty response.");
            }

            try
            {
                googleOpenIdDiscoveryInfo = JsonConvert.DeserializeObject<GoogleOpenIDDiscovery>(responseString);
            }
            catch (JsonException ex)
            {
                // If any parse errors, catch them
                throw new ConfigurationErrorsException("Error parsing the response from Google OpenID Connect discovery.", ex);
            }

            // Init done
            googleInitDone.Set();
        }

        /// <summary>
        /// Calls Google with the access token and expects the user profile (aka decodes the token)
        /// </summary>
        /// <param name="userAccessToken">Google user access token</param>
        /// <returns>a user profile specific to Google</returns>
        private async Task<GoogleProfile> GoogleImplicitFlow(string userAccessToken)
        {
            if (googleInitStarted == false)
            {
                await this.GInit();
            }

            // If init not done, wait
            googleInitDone.WaitOne();

            // Profile received from Google
            GoogleProfile userProfile = null;

            // Response string returned by third party id provider
            string responseString = null;

            // Create the outgoing HTTP request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(googleOpenIdDiscoveryInfo.UserinfoEndpoing);
            request.Method = "GET";
            request.Headers["Authorization"] = "Bearer " + userAccessToken;

            try
            {
                // Parse the incoming response
                WebResponse response = await request.GetResponseAsync();

                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader r = new StreamReader(responseStream))
                {
                    responseString = r.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                //// Extract the Web exception error message
                //// string errorMsg = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Google, ex);
            }

            // Let's parse the response string
            if (string.IsNullOrEmpty(responseString))
            {
                throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Google);
            }

            try
            {
                userProfile = JsonConvert.DeserializeObject<GoogleProfile>(responseString);
            }
            catch (JsonException ex)
            {
                // If any parse errors, catch them
                throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Google, ex);
            }

            return userProfile;
        }

        /// <summary>
        /// Calls Google with an authorization code, receives an access token, and then calls the implicit flow.
        /// </summary>
        /// <param name="googleCode">Google user code</param>
        /// <param name="googleAppClientID">clientID issued by Google to the app that requested the code</param>
        /// <param name="googleAppClientSecret">clientSecret issued by Google to the app that requested the code</param>
        /// <param name="googleAppClientRedirectURI">redirect URI registered by the app that requested the code on Google developer portal</param>
        /// <returns>a user profile specific to Google</returns>
        private async Task<GoogleProfile> GoogleAuthorizationCodeFlow(string googleCode, string googleAppClientID, string googleAppClientSecret, string googleAppClientRedirectURI)
        {
            if (googleInitStarted == false)
            {
                await this.GInit();
            }

            // If init not done, wait
            googleInitDone.WaitOne();

            // Format of the response from Google containing the user access token
            GoogleToken userAccessToken = null;

            // Response string returned by third party id provider
            string responseString = null;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    IEnumerable<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>>
                    {
                    new KeyValuePair<string, string>("code", googleCode),
                    new KeyValuePair<string, string>("client_id", googleAppClientID),
                    new KeyValuePair<string, string>("client_secret", googleAppClientSecret),
                    new KeyValuePair<string, string>("redirect_uri", googleAppClientRedirectURI),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    };

                    HttpContent content = new FormUrlEncodedContent(data);

                    // Parse the incoming response
                    HttpResponseMessage response = await client.PostAsync(googleOpenIdDiscoveryInfo.TokenEndpoint, content);

                    // If the Google's response is not ok, throw exception
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Google);
                    }

                    using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                    using (StreamReader r = new StreamReader(responseStream))
                    {
                        responseString = r.ReadToEnd();
                    }

                    userAccessToken = JsonConvert.DeserializeObject<GoogleToken>(responseString);
                }
            }
            catch (Exception ex)
            {
                // If any parse errors, catch them
                throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Google, ex);
            }

            // the last step is similar to an implicit flow step
            return await this.GoogleImplicitFlow(userAccessToken.AccessToken);
        }
    }
}
