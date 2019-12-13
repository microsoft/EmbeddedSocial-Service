// <copyright file="MicrosoftProfile.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// The fields in a Microsoft profile
    /// </summary>
    public class MicrosoftProfile
    {
        /// <summary>
        /// Gets the user's ID.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; internal set; }

        /// <summary>
        /// Gets the user's full name.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the user's first name
        /// </summary>
        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; internal set; }

        /// <summary>
        /// Gets the user's last name
        /// </summary>
        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; internal set; }

        /// <summary>
        /// Gets the URL of the user's profile page.
        /// </summary>
        [JsonProperty(PropertyName = "link")]
        public string Link { get; internal set; }

        /// <summary>
        /// Gets the day of the user's birth date, or null if no birth date is specified.
        /// </summary>
        [JsonProperty(PropertyName = "birth_day")]
        public int? BirthDay { get; internal set; }

        /// <summary>
        /// Gets the month of the user's birth date, or null if no birth date is specified.
        /// </summary>
        [JsonProperty(PropertyName = "birth_month")]
        public int? BirthMonth { get; internal set; }

        /// <summary>
        /// Gets the year of the user's birth date, or null if no birth date is specified.
        /// </summary>
        [JsonProperty(PropertyName = "birth_year")]
        public int? BirthYear { get; internal set; }

        /// <summary>
        /// Gets an array that contains the user's work info.
        /// </summary>
        [JsonProperty(PropertyName = "work")]
        public List<string> Work { get; internal set; }

        /// <summary>
        /// Gets the user's email addresses.
        /// </summary>
        [JsonProperty(PropertyName = "emails")]
        public MicrosoftProfileEmails Emails { get; internal set; }

        /// <summary>
        /// Gets the user's postal addresses.
        /// </summary>
        [JsonProperty(PropertyName = "addresses")]
        public MicrosoftProfileAddresses Addresses { get; internal set; }

        /// <summary>
        /// Gets the user's phone numbers.
        /// </summary>
        [JsonProperty(PropertyName = "phones")]
        public MicrosoftProfilePhones Phones { get; internal set; }

        /// <summary>
        /// Gets the user's locale code.
        /// </summary>
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; internal set; }

        /// <summary>
        /// Gets the time, in ISO 8601 format, at which the user last updated the object.
        /// </summary>
        [JsonProperty(PropertyName = "updated_time")]
        public string UpdatedTime { get; internal set; }

        /// <summary>
        ///  Gets a simpler profile
        /// </summary>
        public GenericUserProfile GenericUserProfile
        {
            get
            {
                GenericUserProfile authProfile = new GenericUserProfile
                {
                    AccountId = this.Id,
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                };

                if (this.Emails != null)
                {
                    // MSA has five e-mail fields: preferred, account, personal, business, other.
                    // We extract the last four only. (preferred is a dup).
                    authProfile.Emails = new List<string>();

                    if (!string.IsNullOrEmpty(this.Emails.Account))
                    {
                        authProfile.Emails.Add(this.Emails.Account);
                    }

                    if (!string.IsNullOrEmpty(this.Emails.Personal))
                    {
                        authProfile.Emails.Add(this.Emails.Personal);
                    }

                    if (!string.IsNullOrEmpty(this.Emails.Business))
                    {
                        authProfile.Emails.Add(this.Emails.Business);
                    }

                    if (!string.IsNullOrEmpty(this.Emails.Other))
                    {
                        authProfile.Emails.Add(this.Emails.Other);
                    }
                }

                return authProfile;
            }
        }
    }
}
