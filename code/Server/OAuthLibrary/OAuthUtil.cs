// <copyright file="OAuthUtil.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Implements utils for <c>OAuth</c> library
    /// </summary>
    public static class OAuthUtil
    {
        /// <summary>
        /// Source of pseudo-randomness. The seed is always different.
        /// </summary>
        private static Random pseudoRandom = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// Gets numbers of seconds since the beginning of time
        /// </summary>
        public static string SecondsSinceBeginningOfTime
        {
            get
            {
                TimeSpan ts = DateTime.UtcNow - BeginningOfTime;
                return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets beginning of time (1970).
        /// </summary>
        public static DateTime BeginningOfTime
        {
            get
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// generates random bytes and returns a byte array
        /// </summary>
        /// <param name="lengthOfArray">array's length</param>
        /// <returns><pre>lengthOfArray</pre> of pseudo-random bytes</returns>
        public static byte[] GeneratePseudoRandomByteArray(ushort lengthOfArray)
        {
            byte[] byteArray = new byte[lengthOfArray];
            pseudoRandom.NextBytes(byteArray);
            return byteArray;
        }

        /// <summary>
        /// generates random bytes and returns a string
        /// </summary>
        /// <param name="randomBytesLength">number of random bytes generated</param>
        /// <returns>a random string whose length is <pre>randomBytesLength</pre></returns>
        public static string GeneratePseudoRandomString(ushort randomBytesLength)
        {
            return System.Text.Encoding.UTF8.GetString(GeneratePseudoRandomByteArray(randomBytesLength));
        }

        /// <summary>
        /// Generates random bytes and returns a string that is base64-encoded
        /// </summary>
        /// <param name="randomBytesLength">number of random bytes generated</param>
        /// <returns>a random, base64-encoding of a byte array whose length is <pre>randomBytesLength</pre>.
        /// Note that the returned string might have a different length than randomBytesLength as it is converted to base-64
        /// </returns>
        public static string GeneratePseudoRandomBase64String(ushort randomBytesLength)
        {
            return Convert.ToBase64String(GeneratePseudoRandomByteArray(randomBytesLength));
        }

        /// <summary>
        /// Generate random bytes and returns an alphanumeric string
        /// </summary>
        /// <param name="randomBytesLength">number of random bytes generated</param>
        /// <returns>a random, alphanumeric of a byte array whose length is <pre>randomBytesLength</pre>.
        /// Note that the returned string might have a different length than randomBytesLength as it is converted to base-64
        /// and then stripped of all non-alphanumeric characters
        /// </returns>
        public static string GeneratePseudoRandomAlphaNumericString(ushort randomBytesLength)
        {
            return Regex.Replace(GeneratePseudoRandomBase64String(randomBytesLength), "[^A-Za-z0-9]", string.Empty);
        }

        /// <summary>
        /// Takes an input string and a signing key. Signs the input with the signing key and converts it to a base64-encoded string.
        /// </summary>
        /// <param name="data">input string</param>
        /// <param name="signingKey">signing key</param>
        /// <returns>base64-encoded signature string</returns>
        public static string Base64HMACString(string data, string signingKey)
        {
            return Convert.ToBase64String(HMACByteArray(ASCIIEncoding.ASCII.GetBytes(data), ASCIIEncoding.ASCII.GetBytes(signingKey)));
        }

        /// <summary>
        /// Takes input data as a byte[] and a signing key. Signs the input with the signing key
        /// </summary>
        /// <param name="byteArray">input byte array</param>
        /// <param name="signingKey">signing key</param>
        /// <returns>signature byte array</returns>
        public static byte[] HMACByteArray(byte[] byteArray, byte[] signingKey)
        {
            byte[] oauthSignature;

            using (HMACSHA1 hasher = new HMACSHA1(signingKey))
            {
                oauthSignature = hasher.ComputeHash(byteArray);
            }

            return oauthSignature;
        }

        /// <summary>
        /// Splits a single name into a first and last name using the following algorithm:
        /// 1. Trim all whitespace characters from start and end of single name
        /// 2. Find the rightmost space character in singleName.
        /// 3. If no such character found, firstName is set to a single initial corresponding to the first character
        ///                 of the remaining single name
        /// 4. Else first name is the substring up to the space. Last name is everything else.
        /// Note that we do not change any of the capitalization of the single name.
        /// </summary>
        /// <param name="singleName">single name string</param>
        /// <returns>first and last names as a tuple</returns>
        public static Tuple<string, string> SingleName2FirstAndLastNames(string singleName)
        {
            string firstName;
            string lastName;

            // trim all spaces from stand and end of a string.
            singleName = singleName.Trim();

            int lastSpaceFound = singleName.LastIndexOf(' ');

            // If there is no space, lastname := fullname (without spaces), firstname := firstInitial
            // otherwise, first name is the substring up to space, and last name is everthing else.
            if (lastSpaceFound < 0)
            {
                firstName = singleName[0].ToString();
                lastName = singleName;
            }
            else
            {
                firstName = singleName.Substring(0, lastSpaceFound);
                lastName = singleName.Substring(lastSpaceFound + 1);
            }

            return new Tuple<string, string>(firstName, lastName);
        }
    }
}
