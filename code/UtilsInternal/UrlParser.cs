// <copyright file="UrlParser.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// This class implements various utilities for parsing URLs.
    /// </summary>
    public static class UrlParser
    {
        // In SocialPlus, a Url must have a version number. The version number has a major and a minor number.
        // Its format is "vXX.XX" where 'v' can be lower or upper-case. XX must be a number.
        private const string UrlVersionPattern = @"[vV](\d+).(\d+)/";

        /// <summary>
        /// Extracts the major and minor API version number from a SocialPlus Url.
        /// This method can throw a number of exceptions:
        ///   ArgumentNullException if passed in Url is null or not an absolute Url.
        ///   ArgumentException if passed in Url does not have more than 2 segments
        ///   System.OverflowException if the version numbers are too large
        /// </summary>
        /// <param name="url">input url</param>
        /// <param name="major">(out) major version number</param>
        /// <param name="minor">(out)_minor version number</param>
        public static void GetMajorMinorAPIVersion(Uri url, out int major, out int minor)
        {
            // Input checking. Check the Url is not an absolute Url (e.g., a relative Url)
            // Relative Urls do not contain segments
            if (url == null || url.IsAbsoluteUri == false)
            {
                throw new ArgumentNullException(string.Format("Passed in Url {0} is null or malformed.", url));
            }

            if (url.Segments.Length <= 1)
            {
                throw new ArgumentException(string.Format("Passed in Url {0} is malformed.", url));
            }

            // parse out the api version number from the request url
            var firstSegment = url.Segments[1];
            var match = Regex.Match(firstSegment, UrlVersionPattern);

            if (match.Success == false)
            {
                throw new ArgumentException(string.Format("Passed in Url {0} is malformed.", url));
            }

            major = int.Parse(match.Groups[1].Value);
            minor = int.Parse(match.Groups[2].Value);
        }
    }
}
