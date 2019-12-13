// <copyright file="UserProfile.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    /// <summary>
    /// This class encapsulates the result of an authentication action. It contains a user profile.
    /// The user profile contained is a third-party profile as well as
    /// a generic user profile (a subset of user attributes common across all third-party profiles).
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// Gets third party identity provider this result was obtained from
        /// </summary>
        public IdentityProviders IdProvider { get; internal set; }

        /// <summary>
        /// Gets the Microsoft user profile
        /// </summary>
        public MicrosoftProfile MicrosoftProfile { get; internal set; }

        /// <summary>
        /// Gets the Facebook user profile
        /// </summary>
        public FacebookProfile FacebookProfile { get; internal set; }

        /// <summary>
        /// Gets the Twitter user profile
        /// </summary>
        public TwitterProfile TwitterProfile { get; internal set; }

        /// <summary>
        /// Gets the Google user profile
        /// </summary>
        public GoogleProfile GoogleProfile { get; internal set; }

        /// <summary>
        /// Gets a generic user profile (a simplified user profile)
        /// </summary>
        public GenericUserProfile GenericUserProfile
        {
            get
            {
                switch (this.IdProvider)
                {
                    case IdentityProviders.Microsoft:
                        return this.MicrosoftProfile.GenericUserProfile;
                    case IdentityProviders.Facebook:
                        return this.FacebookProfile.GenericUserProfile;
                    case IdentityProviders.Google:
                        return this.GoogleProfile.GenericUserProfile;
                    case IdentityProviders.Twitter:
                        return this.TwitterProfile.GenericUserProfile;
                }

                return null;
            }
        }
    }
}
