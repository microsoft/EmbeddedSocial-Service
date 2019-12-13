// <copyright file="GoogleProfile.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// Properties of a Google user profile.
    /// </summary>
    public class GoogleProfile
    {
        /// <summary>
        /// Gets a string that identifies this resource as a person in OpenID Connect format
        /// </summary>
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; internal set; }

        /// <summary>
        /// Gets the person's gender. Possible values include, but are not limited to, the following values:
        /// "male" - Male gender.
        /// "female" - Female gender.
        /// "other" - Other.
        /// </summary>
        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; internal set; }

        /// <summary>
        /// Gets the ID of the authenticated user.
        /// </summary>
        [JsonProperty(PropertyName = "sub")]
        public string Sub { get; internal set; }

        /// <summary>
        /// Gets the user's full name.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the user's given (first) name.
        /// </summary>
        [JsonProperty(PropertyName = "given_name")]
        public string GivenName { get; internal set; }

        /// <summary>
        /// Gets the user's family (last) name.
        /// </summary>
        [JsonProperty(PropertyName = "family_name")]
        public string FamilyName { get; internal set; }

        /// <summary>
        /// Gets the URL of the user's profile page.
        /// </summary>
        [JsonProperty(PropertyName = "profile")]
        public string Profile { get; internal set; }

        /// <summary>
        /// Gets the URL of the user's profile picture.
        /// </summary>
        [JsonProperty(PropertyName = "picture")]
        public string Picture { get; internal set; }

        /// <summary>
        /// Gets the user's email address.
        /// </summary>
        [JsonProperty(PropertyName = "email")]
        public string Email { get; internal set; }

        /// <summary>
        /// Gets the boolean flag which is true if the email address is verified. This method returns an email address only if it is verified.
        /// </summary>
        [JsonProperty(PropertyName = "email_verified")]
        public string EmailVerified { get; internal set; }

        /// <summary>
        /// Gets the hosted domain name for the user's Google Apps account. For instance, example.com. The plus.profile.emails.read or email scope is needed to get this domain name.
        /// </summary>
        [JsonProperty(PropertyName = "hd")]
        public string Hd { get; internal set; }

        /// <summary>
        /// Gets the user's preferred locale.
        /// </summary>
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; internal set; }

        /// <summary>
        /// Gets a simpler profile.
        /// </summary>
        public GenericUserProfile GenericUserProfile
        {
            get
            {
                GenericUserProfile authProfile = new GenericUserProfile
                {
                    AccountId = this.Sub,
                    FirstName = this.GivenName,
                    LastName = this.FamilyName,
                };

                if (this.Email != null)
                {
                    authProfile.Emails = new List<string>() { this.Email };
                }

                return authProfile;
            }
        }
    }
}