// <copyright file="UrlParserTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Utils;

    /// <summary>
    /// Unit tests for UrlParser
    /// </summary>
    [TestClass]
    public class UrlParserTests
    {
        /// <summary>
        /// Test trying to parse a null Url
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParsingNullUrlThrowsNullException()
        {
            Uri malformedUrl = null;
            int major, minor;

            UrlParser.GetMajorMinorAPIVersion(malformedUrl, out major, out minor);
        }

        /// <summary>
        /// Test trying to parse a relative Url
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParsingRelativeUrlThrowsNullException()
        {
            Uri malformedUrl = new Uri(@"../relativeUrl", UriKind.Relative);
            int major, minor;

            UrlParser.GetMajorMinorAPIVersion(malformedUrl, out major, out minor);
        }

        /// <summary>
        /// Test trying to parse a Url with no version numbers
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParsingNoVersionNumbersUrlThrowsArgException()
        {
            Uri malformedUrl = new Uri(@"http://sp-dev-stefan.cloudapp.net/");
            int major, minor;

            UrlParser.GetMajorMinorAPIVersion(malformedUrl, out major, out minor);
        }

        /// <summary>
        /// Test trying to parse a Url with negative version numbers
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParsingNegativeVersionNumbersUrlThrowsArgException()
        {
            Uri malformedUrl = new Uri(@"http://sp-dev-stefan.cloudapp.net/v-2.0/users");
            int major, minor;

            UrlParser.GetMajorMinorAPIVersion(malformedUrl, out major, out minor);
        }

        /// <summary>
        /// Test trying to parse a Url that ends just with version numbers.
        /// Such a URL is incorrect -- the server assumes at least a following '/' after the version numbers
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParsingUrlEndingInVersionNumbersThrowsArgException()
        {
            Uri malformedUrl = new Uri(@"http://sp-dev-stefan.cloudapp.net/v0.2");
            int major, minor;

            UrlParser.GetMajorMinorAPIVersion(malformedUrl, out major, out minor);
        }

        /// <summary>
        /// Test trying to parse a Url with huge version numbers
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void ParsingHugeVersionNumbersUrlThrowsOverflowException()
        {
            Uri malformedUrl = new Uri(@"http://sp-dev-stefan.cloudapp.net/v334334433344444433344433333334442321121.0/");
            int major, minor;

            UrlParser.GetMajorMinorAPIVersion(malformedUrl, out major, out minor);
        }

        /// <summary>
        /// Test trying to parse a Url with version number 0.0
        /// </summary>
        [TestMethod]
        public void ParsingWeirdVersionNumbersWorks()
        {
            Uri malformedUrl = new Uri(@"http://sp-dev-stefan.cloudapp.net/v0.0/users");
            int major, minor;

            UrlParser.GetMajorMinorAPIVersion(malformedUrl, out major, out minor);
            Assert.AreEqual(major, 0);
            Assert.AreEqual(minor, 0);
        }

        /// <summary>
        /// Test trying to parse a Url with version number 0.2
        /// </summary>
        [TestMethod]
        public void ParsingVersionNumbersWorks()
        {
            Uri malformedUrl = new Uri(@"http://sp-dev-stefan.cloudapp.net/v0.2/users");
            int major, minor;

            UrlParser.GetMajorMinorAPIVersion(malformedUrl, out major, out minor);
            Assert.AreEqual(major, 0);
            Assert.AreEqual(minor, 2);
        }
    }
}