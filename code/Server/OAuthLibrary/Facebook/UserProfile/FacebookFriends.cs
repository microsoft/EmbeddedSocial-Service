// <copyright file="FacebookFriends.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// Class describing a user profile obtained from Facebook.
    /// C# class corresponding to the fields of a profile in Facebook. These fields
    /// are filled based on the JSON response to a Graph-API call for user profile.
    /// These fields were obtained from: https://developers.facebook.com/docs/graph-api/reference/user
    /// </summary>
    public class FacebookFriends
    {
        /// <summary>
        /// Gets a list of user nodes
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public List<FacebookProfile> Data { get; internal set; }

        /// <summary>
        /// Gets the pagination
        /// </summary>
        [JsonProperty(PropertyName = "paging")]
        public FacebookFriendsPagination Paging { get; internal set; }

        /// <summary>
        /// Gets Aggregated information about the edge, such as counts. Specify the fields to fetch in the summary param (like summary=total_count).
        /// </summary>
        [JsonProperty(PropertyName = "summary")]
        public FacebookFriendsSummary Summary { get; internal set; }
    }
}
