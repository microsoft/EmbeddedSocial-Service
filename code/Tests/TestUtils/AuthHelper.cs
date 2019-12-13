// <copyright file="AuthHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.TestUtils
{
    using System;

    /// <summary>
    /// Code that helps clients authenticate.
    /// Mostly, it implements the proper format for authorization headers of requests to SocialPlus
    /// </summary>
    public static class AuthHelper
    {
        /// <summary>
        /// Creates the proper value for the authorization field for clients using SocialPlus session token
        /// </summary>
        /// <param name="sessionToken">session token</param>
        /// <returns>authorization value</returns>
        public static string CreateSocialPlusAuth(string sessionToken)
        {
            return "SocialPlus TK=" + sessionToken;
        }

        /// <summary>
        /// Creates the proper value for the auhorization field for clients using AAD S2S
        /// </summary>
        /// <param name="aadToken">AAD token</param>
        /// <param name="appKey">app key</param>
        /// <param name="userHandle">user handle (default value of null)</param>
        /// <returns>authorization value</returns>
        public static string CreateAADS2SAuth(string aadToken, string appKey, string userHandle = null)
        {
            if (string.IsNullOrEmpty(userHandle))
            {
                return "AADS2S AK=" + appKey + "|TK=" + aadToken;
            }

            return "AADS2S AK=" + appKey + "|UH=" + userHandle + "|TK=" + aadToken;
        }

        /// <summary>
        /// Creates the proper value for the authorization field for clients using app key only (anon)
        /// </summary>
        /// <param name="appKey">app key</param>
        /// <returns>authorization value</returns>
        public static string CreateAnonAuth(string appKey)
        {
            return "Anon AK=" + appKey;
        }

        /// <summary>
        /// Helper Utility to parse out Microsoft Access Token when have redirected URL
        /// </summary>
        /// <param name="url"> redirect URL that has the access token in it.</param>
        /// <returns>Returns a token parsed from URL</returns>
        public static string GetMSAAccessTokenFromURL(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("Null or empty url. No Microsoft token found");
            }

            int accessTokenLocation = url.IndexOf("access_token");
            int tokenTypeLocation = url.IndexOf("&token_type");

            if (accessTokenLocation < 0 || tokenTypeLocation < 0)
            {
                throw new InvalidOperationException(string.Format("Malformed url. No Microsoft token found. Access Token Location {0}, Token Type Location {1}", accessTokenLocation, tokenTypeLocation));
            }

            // Get Access Token out of the whole thing
            return url.Substring(accessTokenLocation + 13, tokenTypeLocation - accessTokenLocation - 13); // 13 is for "access_token="
        }

        /// <summary>
        /// Helper Utility to parse out Facebook Access Token when have redirected URL
        /// </summary>
        /// <param name="url"> redirect URL that has the access token in it.</param>
        /// <returns>Returns a token parsed from URL</returns>
        public static string GetFacebookAccessTokenFromURL(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("Null or empty url. No Facebook token found");
            }

            int accessTokenLocation = url.IndexOf("access_token=");
            int tokenExpiresInLocation = url.IndexOf("&expires_in=");

            if (accessTokenLocation < 0 || tokenExpiresInLocation < 0)
            {
                throw new InvalidOperationException(string.Format("Malformed url. No Facebook token found. Access Token Location {0}, Token Expires In Location {1}", accessTokenLocation, tokenExpiresInLocation));
            }

            // Get Access Token out of the whole thing
            return url.Substring(accessTokenLocation + 13, tokenExpiresInLocation - accessTokenLocation - 13); // 13 is for "access_token="
        }
    }
}
