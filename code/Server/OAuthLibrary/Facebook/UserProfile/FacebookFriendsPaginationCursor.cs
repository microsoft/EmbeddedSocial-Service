// <copyright file="FacebookFriendsPaginationCursor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class describing the fields of "cursors", a field found in "paging", which itself is a field found in the response to a Facebook request for friends.
    /// </summary>
    public class FacebookFriendsPaginationCursor
    {
        /// <summary>
        /// Gets after cursor
        /// </summary>
        [JsonProperty(PropertyName = "after")]
        public string After { get; internal set; }

        /// <summary>
        /// Gets before cursor
        /// </summary>
        [JsonProperty(PropertyName = "before")]
        public string Before { get; internal set; }
    }
}
