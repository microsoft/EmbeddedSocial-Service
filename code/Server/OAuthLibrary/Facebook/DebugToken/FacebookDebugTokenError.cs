// <copyright file="FacebookDebugTokenError.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// Entries in a debug token error
    /// </summary>
    public class FacebookDebugTokenError
    {
        /// <summary>
        /// Gets the error code for the error.
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        public int Code { get; internal set; }

        /// <summary>
        /// Gets the error message for the error.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; internal set; }

        /// <summary>
        /// Gets the error <c>subcode</c> for the error.
        /// </summary>
        [JsonProperty(PropertyName = "subcode")]
        public int Subcode { get; internal set; }
    }
}
