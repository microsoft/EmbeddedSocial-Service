// <copyright file="GuidTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SocialPlus.Utils;

    /// <summary>
    /// Tests that Guid utils work
    /// </summary>
    [TestClass]
    public class GuidTests
    {
        /// <summary>
        /// Tests that Guid xor works
        /// </summary>
        [TestMethod]
        public void XORGuidTest()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            Assert.AreEqual(guid1, guid2.Xor(guid1.Xor(guid2)));
            Assert.AreEqual(guid2, guid1.Xor(guid1.Xor(guid2)));
        }
    }
}
