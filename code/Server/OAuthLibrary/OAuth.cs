// <copyright file="OAuth.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Main class of <c>OAuth</c> library. This library implements a few common <c>OAuth</c> flows. At the end of each
    /// flow, the caller receives a user's profile.
    /// <para>
    /// The <c>OAuth</c> library has three parties involved:
    ///         -- an identity provider, such as Facebook, Google, or Microsoft. (this is also called an authorization server)
    ///         -- a user (a human) (also called a resource owner)
    ///         -- a client. The client is the caller of this library which must pass the clientID and the clientSecret.
    ///                 These two values are obtained by the developer of the client when it registers the client with
    ///                 the identity provider.
    /// </para>
    /// <para>
    /// The library currently supports two <c>OAuth</c> flows:
    ///         -- implicit flow: client obtains an access token from the user and calls the library to exchange the token
    ///             for the user's profile.
    ///         -- authorization code flow: client obtains a code from the user and calls the library to exchange the code
    ///             for the user's profile.
    /// </para>
    /// <para>
    /// Anybody in the possession of an access token can retrieve a user's profile. On the other hand, only the client can exchange
    /// the code for an access token. The authorization code flow ensures that only the library gets an access token.
    /// </para>
    /// <para>
    /// Each identity provider supports one or more versions of these flows. This library implements at least one flow
    /// for each of the following four identity providers: Microsoft, Google, Facebook, Twitter.
    /// </para>
    /// <para>
    /// The library returns a user profile defined by the <c>UserProfile</c> class. The user profile contains two profiles:
    ///     -- a profile specific to the identity provider (all fields returned by Microsoft, Google, Facebook, or Twitter)
    ///     -- a generic profile. The generic profile is obtained by parsing the specific profile appropriately. In this way
    ///         the caller does not need to be concerned with the details specific to profiles from each identity provider.
    /// </para>
    /// <para>
    /// The library also defines its own exception that can be thrown in case of an error. The exception contains information
    /// about the type of error that could help with debugging. See <c>OOAuthException</c>
    /// </para>
    /// </summary>
    public partial class OAuth
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly OAuth Singleton;

        /// <summary>
        /// Initializes static members of the <see cref="OAuth"/> class. This static constructor also tells C# compiler not to mark type as <code>beforefieldinit</code>.
        /// The static constructor acts as "lock" for the singleton (no two instances can enter a static constructor at the same time).
        /// </summary>
        static OAuth()
        {
            Singleton = new OAuth();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="OAuth"/> class from being created. This is also where all real work is done.
        /// </summary>
        private OAuth()
        {
        }

        /// <summary>
        /// Gets the singleton's instance.
        /// </summary>
        public static OAuth Instance
        {
            get
            {
                return Singleton;
            }
        }

        /// <summary>
        /// Implements the <c>OAuth</c> Implicit Flow. In this flow, a user authorizes the IdentityProvider to issue an access token. The client receives the
        /// access token and uses it to fetch the user profile. For this, it calls the appropriate fetch profile API offered by the identity provider.
        /// The implicit flow is considered an insecure form of authentication because an identity provider allows anyone to retrieve a profile
        /// with the access token.
        /// </summary>
        /// <param name="idProvider">third party identity provider</param>
        /// <param name="userAccessToken">user access token</param>
        /// <returns>the user profile</returns>
        public async Task<UserProfile> ImplicitFlow(IdentityProviders idProvider, string userAccessToken)
        {
            switch (idProvider)
            {
                case IdentityProviders.Microsoft:
                    return new UserProfile
                    {
                        IdProvider = IdentityProviders.Microsoft,
                        MicrosoftProfile = await this.MicrosoftImplicitFlow(userAccessToken),
                    };

                case IdentityProviders.Facebook:
                    return new UserProfile
                    {
                        IdProvider = IdentityProviders.Facebook,
                        FacebookProfile = await this.FacebookImplicitFlow(userAccessToken),
                    };

                case IdentityProviders.Google:
                    return new UserProfile
                    {
                        IdProvider = IdentityProviders.Google,
                        GoogleProfile = await this.GoogleImplicitFlow(userAccessToken),
                    };
            }

            throw new OAuthException(OAuthErrors.NotImplemented_501);
        }

        /// <summary>
        /// Implements the <c>OAuth</c> Authorization Code Flow. In this flow, a user authorizes the IdentityProvider to issue a code. The client receives the code
        /// and can exchange it for an access token. The access token is used to fetch the user profile.
        /// This flow is more secure the Implicit flow. This is because only the client can exchange the code for the access token. This is done by the library
        /// when called by the client (this should be done only by clients running as servers or in the cloud). It should not be done by clients running
        /// on user's mobile devices or browsers.
        /// </summary>
        /// <param name="idProvider">third party identity provider</param>
        /// <param name="userCode">user code</param>
        /// <param name="clientId">client application ID</param>
        /// <param name="clientSecret">client application secret</param>
        /// <param name="clientRedirectURI">client redirection URI; must not be null in case of Google</param>
        /// <param name="userRequestToken">request token; must not be null in case of Twitter</param>
        /// <returns>the user profile</returns>
        public async Task<UserProfile> AuthorizationCodeFlow(IdentityProviders idProvider, string userCode, string clientId, string clientSecret, string clientRedirectURI = null, string userRequestToken = null)
        {
            switch (idProvider)
            {
                case IdentityProviders.Google:
                    if (string.IsNullOrEmpty(clientRedirectURI))
                    {
                        // clientRedirectURI must be present
                        throw new OAuthException(OAuthErrors.BadRequest_400);
                    }

                    return new UserProfile
                    {
                        IdProvider = IdentityProviders.Google,
                        GoogleProfile = await this.GoogleAuthorizationCodeFlow(userCode, clientId, clientSecret, clientRedirectURI),
                    };

                case IdentityProviders.Twitter:
                    if (string.IsNullOrEmpty(userRequestToken))
                    {
                        // userRequestToken must be present
                        throw new OAuthException(OAuthErrors.BadRequest_400);
                    }

                    return new UserProfile
                    {
                        IdProvider = IdentityProviders.Twitter,
                        TwitterProfile = await this.TwitterAuthorizationCodeFlow(userRequestToken, userCode, clientId, clientSecret),
                    };
            }

            throw new OAuthException(OAuthErrors.NotImplemented_501);
        }

        /// <summary>
        /// Obtains third party request tokens. This code needs more revising.
        /// </summary>
        /// <param name="idProvider">external identity provider</param>
        /// <param name="clientId">the id of the application on behalf of which the request token is requested</param>
        /// <param name="clientSecret">the secret of the application on behalf of which the request token is requested</param>
        /// <param name="clientRedirectURI">the <c>oauth</c> callback of the application on behalf of which the request token is requested</param>
        /// <returns>request token</returns>
        public async Task<string> GetRequestToken(IdentityProviders idProvider, string clientId, string clientSecret, string clientRedirectURI)
        {
            // Implements simple dispatcher to private functions depending on the third party provider
            switch (idProvider)
            {
                case IdentityProviders.Twitter:
                    {
                        return await this.GetTwitterRequestToken(clientId, clientSecret, clientRedirectURI);
                    }
            }

            throw new OAuthException(OAuthErrors.NotImplemented_501);
        }

        /// <summary>
        /// Gets friends from a third-party identity provider. The definition of a "friend" depends on the third-party provider.,
        /// "Friends" are friends (for Facebook), following users (for Twitter), and contacts (for Google and Microsoft).
        /// The access token must have the appropriate scopes to retrieve the list of friends.
        /// </summary>
        /// <param name="idProvider">third party identity provider</param>
        /// <param name="userAccessToken">user access token</param>
        /// <returns>the user profile</returns>
        public async Task<List<UserProfile>> GetFriends(IdentityProviders idProvider, string userAccessToken)
        {
            // data field is a list of Facebook profiles. Convert them to our own user profiles
            List<UserProfile> userProfileList = new List<UserProfile>();

            // We only support Facebook for now
            switch (idProvider)
            {
                case IdentityProviders.Facebook:
                    List<FacebookProfile> fbProfiles = await this.GetFacebookFriendsImplicitFlow(userAccessToken);

                    foreach (var p in fbProfiles)
                    {
                        var userProfile = new UserProfile
                        {
                            IdProvider = idProvider,
                            FacebookProfile = p
                        };

                        userProfileList.Add(userProfile);
                    }

                    return userProfileList;
            }

            throw new OAuthException(OAuthErrors.NotImplemented_501);
        }
    }
}
