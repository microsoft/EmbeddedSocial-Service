// <copyright file="FacebookFriendsSummary.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class describing the fields of "summary", a field found in the response to a Facebook request for friends.
    /// </summary>
    public class FacebookFriendsSummary
    {
        /// <summary>
        /// Gets total count of friends
        /// </summary>
        [JsonProperty(PropertyName = "total_count")]
        public int FacebookFriendsCount { get; internal set; }
    }
}
