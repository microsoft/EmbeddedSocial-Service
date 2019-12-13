// <copyright file="MicrosoftOAuth.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Live;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements the <c>OAuth</c> flows for Microsoft.
    /// </summary>
    public partial class OAuth
    {
        /// <summary>
        /// Implements the logic for verifying and decoding access tokens issued by MSA.
        /// </summary>
        /// <param name="userAccessToken">Microsoft user access token</param>
        /// <returns>MSAProfile: user's profile specific to Microsoft</returns>
        private async Task<MicrosoftProfile> MicrosoftImplicitFlow(string userAccessToken)
        {
            // User profile extracted from the token
            MicrosoftProfile userProfile = null;

            // Response string returned by third party id provider
            string responseString = null;

            // Create the outgoing HTTP request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://apis.live.net/v5.0/me?access_token=" + userAccessToken);
            request.Method = "GET";

            try
            {
                // Parse the incoming response
                WebResponse response = await request.GetResponseAsync();

                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader r = new StreamReader(responseStream))
                {
                    responseString = r.ReadToEnd();
                }

                // Let's parse the response string
                if (string.IsNullOrEmpty(responseString))
                {
                    throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Microsoft);
                }

                userProfile = JsonConvert.DeserializeObject<MicrosoftProfile>(responseString);
            }
            catch (Exception ex)
            {
                // This fires if there's something wrong with our request or parsing the response

                //// Extract the Web exception error message
                //// string errorMsg = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Microsoft, ex);
            }

            return userProfile;
        }

        /// <summary>
        /// TODO: single sign-on is not supported currently.
        /// Implements the logic for verifying and decoding authentication token issued by MSA.
        /// Note that authentication tokens are used for single sign-on only.
        /// </summary>
        /// <param name="userAuthenticationToken">Microsoft authentication token</param>
        /// <param name="appClientID">ClientID issued by Microsoft to the app that requested the token</param>
        /// <param name="appClientSecret">Client secret issued by Microsoft to the app that requested the token</param>
        /// <returns>Tuple:
        ///     boolean: true if the authentication token was valid, false otherwise
        ///     string: the user id in the authentication token. (TODO: return the JWT token instead).
        ///     <c>OAuthException</c>: exception specific to <c>OAuth</c> encountered during token validation and decoding
        /// </returns>
        private Tuple<bool, string, OAuthException> VerifyAndDecodeMicrosoftAuthenticationToken(string userAuthenticationToken, string appClientID, string appClientSecret)
        {
            // Flag representing whether token has been verified
            bool tokenVerified = false;

            // Identity extracted from the token
            string tokenIdentity = null;

            // Exception raised upon verifying and decoding the token
            OAuthException tokenEx = null;

            // MSA authentication token simply gets decoded to a JWT
            JsonWebToken jwtToken = null;

            // Exception raised by LiveID decoding
            LiveAuthException liveEx = null;

            // Decode token. If cannot decode, create appropriate OAuthException.
            // DecodeAuthenticationToken does not appear to throw any errors. Instead, its errors are caught, and it returns false instead.
            if (LiveAuthWebUtility.DecodeAuthenticationToken(userAuthenticationToken, appClientSecret, out jwtToken, out liveEx) == false)
            {
                tokenEx = new OAuthException(OAuthErrors.ServiceUnavailable_503_Microsoft, liveEx /* pass out the LiveException also */);
            }
            else
            {
                //// TOKEN Validation checks

                if (jwtToken.IsExpired == true)
                {
                    // Token expired. Handle this appropriately.
                    tokenEx = new OAuthException(OAuthErrors.Unauthorized_401_1);
                }
                else if (appClientID != jwtToken.Claims.AppId)
                {
                    // Token stolen by different app. Handle this appropriately.
                    tokenEx = new OAuthException(OAuthErrors.Unauthorized_401_3);
                }
                else if (string.IsNullOrEmpty(jwtToken.Claims.UserId))
                {
                    // Token's id doesn't exist. Handle this appropriately.
                    tokenEx = new OAuthException(OAuthErrors.Unauthorized_401_4);
                }
                else
                {
                    // Extract the token's identity
                    tokenIdentity = jwtToken.Claims.UserId;
                    tokenVerified = true;
                }
            }

            // MSA's authentication token contains no information about a user other than it's account id.
            // Note that the account id found in an MSA authentication token is different than the actual account id.
            // Although couldn't find specs, it's likely this is due to privacy reasons. MSA issues a different account
            // id to different application publishers. In this ways, two publishers cannot track a single user accross
            // since the ids found in their auth tokens are different. However, this id is the same across two different
            // apps owned by the same publisher.
            return new Tuple<bool, string, OAuthException>(tokenVerified, tokenIdentity, tokenEx);
        }
    }
}
