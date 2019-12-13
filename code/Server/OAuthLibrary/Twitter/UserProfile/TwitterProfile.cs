// <copyright file="TwitterProfile.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System;
    using System.Text.RegularExpressions;

    using Newtonsoft.Json;

    /// <summary>
    /// The fields in a Twitter profile
    /// </summary>
    public class TwitterProfile
    {
        /// <summary>
        /// Gets the id of a Twitter profile
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; internal set; }

        /// <summary>
        /// Gets the full name of a user
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the screen name of a user
        /// </summary>
        [JsonProperty(PropertyName = "screen_name")]
        public string ScreenName { get; internal set; }

        /// <summary>
        /// Gets a simpler profile
        /// </summary>
        public GenericUserProfile GenericUserProfile
        {
            get
            {
                Tuple<string, string> firstAndLastNames = OAuthUtil.SingleName2FirstAndLastNames(this.Name);

                return new GenericUserProfile
                {
                    AccountId = this.Id,
                    FirstName = firstAndLastNames.Item1,
                    LastName = firstAndLastNames.Item2
                };
            }
        }
    }
}
