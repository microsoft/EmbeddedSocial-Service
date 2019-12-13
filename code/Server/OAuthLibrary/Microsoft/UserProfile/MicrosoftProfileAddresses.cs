// <copyright file="MicrosoftProfileAddresses.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class describing the fields of addresses in a Microsoft profile
    /// </summary>
    public class MicrosoftProfileAddresses
    {
        /// <summary>
        /// Gets the user's personal postal address.
        /// </summary>
        [JsonProperty(PropertyName = "personal")]
        public MicrosoftProfileAddressesAddress Personal { get; internal set; }

        /// <summary>
        /// Gets the user's business postal address.
        /// </summary>
        [JsonProperty(PropertyName = "business")]
        public MicrosoftProfileAddressesAddress Business { get; internal set; }
    }
}
