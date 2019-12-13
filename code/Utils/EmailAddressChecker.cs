// <copyright file="EmailAddressChecker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Utilities to check whether an email address is properly formed
    /// </summary>
    public static class EmailAddressChecker
    {
        /// <summary>
        /// This code checks whether email is a valid e-mail address.
        /// </summary>
        /// <param name="email">email address to check</param>
        /// <param name="convertUnicodeTimeout">timeout used when converting Unicode domain names (default value is 200ms)</param>
        /// <param name="validateTimeout">timeout used when validating e-mail address (default value is 250ms)</param>
        /// <returns>true if valid</returns>
        public static bool IsValidEmail(string email, double convertUnicodeTimeout = 200, double validateTimeout = 250)
        {
            // This code is based on: https://msdn.microsoft.com/en-us/library/01escwtf%28v=vs.110%29.aspx
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(convertUnicodeTimeout));
            }
            catch (Exception)
            {
                return false;
            }

            // Return true if email is in valid e-mail format.
            try
            {
                return Regex.IsMatch(
                                     email,
                                     @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                                     RegexOptions.IgnoreCase,
                                     TimeSpan.FromMilliseconds(validateTimeout));
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// returns the domain name if it matches
        /// </summary>
        /// <param name="match">domain name to search for</param>
        /// <returns>domain name</returns>
        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            domainName = idn.GetAscii(domainName);          // this might throw an ArgumentException; in that case, IsValidEmail will return false.

            return match.Groups[1].Value + domainName;
        }
    }
}