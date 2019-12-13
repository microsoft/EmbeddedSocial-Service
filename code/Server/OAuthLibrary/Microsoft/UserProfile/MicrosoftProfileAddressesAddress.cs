// <copyright file="MicrosoftProfileAddressesAddress.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// Address in a list of addresses in a Microsoft profile
    /// </summary>
    public class MicrosoftProfileAddressesAddress
    {
        /// <summary>
        /// Gets the user's street address, or null if one is not specified
        /// </summary>
        [JsonProperty(PropertyName = "street")]
        public string Street { get; internal set; }

        /// <summary>
        /// Gets the second line of the user's street address, or null if one is not specified.
        /// </summary>
        [JsonProperty(PropertyName = "street_2")]
        public string Street2 { get; internal set; }

        /// <summary>
        /// Gets the city of the user's address, or null if one is not specified.
        /// </summary>
        [JsonProperty(PropertyName = "city")]
        public string City { get; internal set; }

        /// <summary>
        /// Gets the state of the user's address, or null if one is not specified.
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public string State { get; internal set; }

        /// <summary>
        /// Gets the postal code of the user's address, or null if one is not specified.
        /// </summary>
        [JsonProperty(PropertyName = "postal_code")]
        public string PostalCode { get; internal set; }

        /// <summary>
        /// Gets the region of the user's address, or null if one is not specified.
        /// </summary>
        [JsonProperty(PropertyName = "region")]
        public string Region { get; internal set; }
    }
}
