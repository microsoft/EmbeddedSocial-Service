// <copyright file="Base64Url.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Principal
{
    using System;

    /// <summary>
    /// Base64 url-safe converter
    /// </summary>
    public static class Base64Url
    {
        /// <summary>
        /// Convert byte array to base64 string
        /// </summary>
        /// <param name="input">byte array</param>
        /// <returns>Base64 string</returns>
        public static string Encode(byte[] input)
        {
            string result = System.Convert.ToBase64String(input).TrimEnd(new char[] { '=' }).Replace('+', '-').Replace('/', '_');
            return result;
        }

        /// <summary>
        /// Convert base64 string to byte array
        /// </summary>
        /// <param name="input">Base64 string</param>
        /// <returns>byte array</returns>
        public static byte[] Decode(string input)
        {
            string base64 = input.Replace('_', '/').Replace('-', '+');
            switch (base64.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
            }

            var result = Convert.FromBase64String(base64);
            return result;
        }
    }
}
