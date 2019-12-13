// <copyright file="Base64Utils.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    /// <summary>
    /// Utilities for simple Base64 encoding and decoding.
    /// </summary>
    public class Base64Utils
    {
        /// <summary>
        /// encodes a string into base64-encoding
        /// </summary>
        /// <param name="plainText">input string</param>
        /// <returns>the input string encoded in base 64</returns>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// decodes a string from base64-encoding
        /// </summary>
        /// <param name="base64EncodedData">base64 input string</param>
        /// <returns>decoded input string</returns>
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
