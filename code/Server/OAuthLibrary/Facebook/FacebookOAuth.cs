// <copyright file="FacebookOAuth.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Facebook;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements the <c>OAuth</c> flows for Facebook.
    /// </summary>
    public partial class OAuth
    {
        /// <summary>
        /// Verifies a Facebook access token by asking for the corresponding debug token (Facebook's famous debug_token) from Facebook.
        /// This is better than checking an access token using implicit flow -- unlike implicit flow, this check ensures that the token
        /// was issued to an our app. (this means that the user typed his password in an window that listed our app name). Implicit flow
        /// only guarantees that a user typed his password in a window that listed some app name (not necessarily ours).
        /// </summary>
        /// <param name="userAccessToken">Facebook user access token</param>
        /// <param name="facebookAppID">AppID issued by Facebook to the app that requested the token</param>
        /// <param name="facebookAppSecret">App secret issued by Facebook to the app that requested the token</param>
        /// <returns>Tuple:
        ///     boolean: true if the access token was valid, false otherwise
        ///     <code>OAuthException</code>: exception specific to <c>OAuth</c> encountered during token validation and decoding
        /// </returns>
        private async Task<Tuple<bool, OAuthException>> FacebookDebugUserAccessToken(string userAccessToken, string facebookAppID, string facebookAppSecret)
        {
            // Flag representing whether token has been verified
            bool tokenVerified = false;

            // Exception raised upon verifying and decoding the token
            OAuthException tokenEx = null;

            try
            {
                // Using the nice Facebook library, we start by creating a FacebookClient with our app's credentials
                FacebookClient client = new FacebookClient(facebookAppID + "|" + facebookAppSecret);

                // Conver the token into a debugToken
                dynamic resultDebug = await client.GetTaskAsync<JsonObject>("debug_token", new { input_token = userAccessToken, });

                // The result is a collection of JsonObjects. According to Facebook, there should never be more than one object
                // in the result of a debug_token
                if (resultDebug == null || resultDebug.Values == null || resultDebug.Values.Count != 1)
                {
                    tokenEx = new OAuthException(OAuthErrors.ServiceUnavailable_503_Facebook);
                }
                else
                {
                    // FB access token simply gets decoded into a debugToken
                    FacebookDebugToken debugToken = JsonConvert.DeserializeObject<FacebookDebugToken>(Enumerable.First(resultDebug.Values).ToString());

                    // Token validation tests

                    // Check whether the token is valid
                    if (debugToken.IsValid == false)
                    {
                        // Token invalid. Handle this appropriately.
                        tokenEx = new OAuthException(OAuthErrors.Unauthorized_401_2);
                    }
                    else if (facebookAppID != debugToken.AppId)
                    {
                        // Token stolen by different app. Handle this appropriately.
                        tokenEx = new OAuthException(OAuthErrors.Unauthorized_401_3);
                    }
                    else if (OAuthUtil.BeginningOfTime.AddSeconds(debugToken.ExpiresAt).ToLocalTime() < DateTime.Now)
                    {
                        // Token expired. Handle this appropriately.
                        tokenEx = new OAuthException(OAuthErrors.Unauthorized_401_1);
                    }
                    else if (string.IsNullOrEmpty(debugToken.UserId))
                    {
                        // Token's id doesn't exist. Handle this appropriately.
                        tokenEx = new OAuthException(OAuthErrors.Unauthorized_401_4);
                    }
                    else
                    {
                        // Token is verified. Yay!
                        tokenVerified = true;
                    }
                }
            }
            catch (Exception ex)
            {
                tokenEx = new OAuthException(OAuthErrors.ServiceUnavailable_503_Facebook, ex /* pass out the FacebookOOAuthException also */);
            }

            return new Tuple<bool, OAuthException>(tokenVerified, tokenEx);
        }

        /// <summary>
        /// Calls Facebook with the access token and expects the user profile (aka decodes the token)
        /// </summary>
        /// <param name="userAccessToken">Facebook user access token</param>
        /// <returns>a user profile specific to Facebook</returns>
        private async Task<FacebookProfile> FacebookImplicitFlow(string userAccessToken)
        {
            // Profile received from Facebook
            FacebookProfile userProfile = null;

            try
            {
                // Using the nice Facebook library, we start by creating a FacebookClient
                FacebookClient client = new FacebookClient(userAccessToken);

                // Conver the token into a debugToken
                dynamic resultDebug = await client.GetTaskAsync<JsonObject>("me");

                // If we get a result that's not null, we should deserialize it.
                if (resultDebug == null)
                {
                    throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Facebook);
                }
                else
                {
                    userProfile = JsonConvert.DeserializeObject<FacebookProfile>(resultDebug.ToString());
                }
            }
            catch (Exception ex)
            {
                // This can catch a FacebookOOAuthException or a JsonException or anything else.
                throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Facebook, ex /* pass out the inner also */);
            }

            return userProfile;
        }

        /// <summary>
        /// Gets Facebook friends using OAuth's implicit flow. Note that Facebook's API returns *only* a user's friends who have
        /// logged in to the application that access token belongs to (i.e., was issued on behalf of).
        /// Implicit flow means that the access token acts as a capability.
        /// Whoever is in the possession of the token can query the user's friends. The access token must include the 'user_friends' scope.
        /// Should the access token lack the 'user_friends' scope, the method returns an empty list.
        /// </summary>
        /// <remarks>
        /// NOTE: This code is incomplete for users with large number of friends.
        /// Facebook's API uses pagination (see FacebookFriends/Paging). When the list includes multiple "pages" of friends,
        /// this code retrieves just the first page.
        /// </remarks>
        /// <param name="userAccessToken">access token</param>
        /// <returns>list of Facebook user profiles</returns>
        private async Task<List<FacebookProfile>> GetFacebookFriendsImplicitFlow(string userAccessToken)
        {
            FacebookFriends fbFriendsList = null;

            try
            {
                // Using the nice Facebook library, we start by creating a FacebookClient
                FacebookClient client = new FacebookClient(userAccessToken);

                // Fetch the friends of the user (for the application to which the access token belongs to). Also
                // specify "summary=total_count" to fetch the total count of friends this user has
                dynamic resultDebug = await client.GetTaskAsync<JsonObject>("me/friends?summary=total_count");

                // If we get a result that's not null, we should deserialize it.
                if (resultDebug == null)
                {
                    throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Facebook);
                }
                else
                {
                    fbFriendsList = JsonConvert.DeserializeObject<FacebookFriends>(resultDebug.ToString());
                }
            }
            catch (Exception ex)
            {
                // This can catch a FacebookOOAuthException or a JsonException or anything else.
                throw new OAuthException(OAuthErrors.ServiceUnavailable_503_Facebook, ex /* pass out the inner also */);
            }

            return fbFriendsList.Data;
        }
    }
}