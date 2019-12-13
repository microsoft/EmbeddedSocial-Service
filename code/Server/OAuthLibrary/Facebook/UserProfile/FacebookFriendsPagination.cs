// <copyright file="FacebookFriendsPagination.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class describing the fields of "paging", a field found in the response to a Facebook request for friends.
    /// </summary>
    public class FacebookFriendsPagination
    {
        /// <summary>
        /// Gets cursors
        /// </summary>
        [JsonProperty(PropertyName = "cursors")]
        public FacebookFriendsPaginationCursor Cursor { get; internal set; }

        /// <summary>
        /// Gets previous page
        /// </summary>
        [JsonProperty(PropertyName = "previous")]
        public string Previous { get; internal set; }

        /// <summary>
        /// Gets next page
        /// </summary>
        [JsonProperty(PropertyName = "next")]
        public string Next { get; internal set; }
    }
}
