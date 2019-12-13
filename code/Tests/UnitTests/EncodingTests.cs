// <copyright file="EncodingTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Utils;

    /// <summary>
    /// Tests that our encoding and decoding are correct and preserve order
    /// </summary>
    [TestClass]
    public class EncodingTests
    {
        /// <summary>
        /// number of ulongs used by these tests
        /// </summary>
        private const int NumUlongs = 100000;

        /// <summary>
        /// Tests that decode is the inverse of encode on 100,000 ulongs.
        /// </summary>
        [TestMethod]
        public void EncodeDecode()
        {
            RandUtils randUtils = new RandUtils();

            // The Assert is placed in the for-loop so that if we fail, we fail early
            for (int i = 0; i < NumUlongs; i += 1)
            {
                ulong current = randUtils.GenerateRandomUlong();
                char[] ordinalEncoding = Base64OrderPreservingAzureCompatEncoding.Encode(current, Base64OrderPreservingAzureCompatEncoding.SortOrders.Ordinal);
                Assert.AreEqual(current, Base64OrderPreservingAzureCompatEncoding.Decode(ordinalEncoding, Base64OrderPreservingAzureCompatEncoding.SortOrders.Ordinal));
                char[] lexiEncoding = Base64OrderPreservingAzureCompatEncoding.Encode(current, Base64OrderPreservingAzureCompatEncoding.SortOrders.Lexicographic);
                Assert.AreEqual(current, Base64OrderPreservingAzureCompatEncoding.Decode(lexiEncoding, Base64OrderPreservingAzureCompatEncoding.SortOrders.Lexicographic));
            }
        }

        /// <summary>
        /// Tests that encoding is order preserving on 100,000 ulongs
        /// </summary>
        [TestMethod]
        public void EncodingIsOrderPreserving()
        {
            RandUtils randUtils = new RandUtils();

            List<ulong> listOfNumbers = new List<ulong>();
            List<string> listOfEncodings = new List<string>();

            // Generate 100,000 ulongs and insert them in a list
            for (int i = 0; i < NumUlongs; i += 1)
            {
                listOfNumbers.Add(randUtils.GenerateRandomUlong());
            }

            // Sort the list of numbers
            listOfNumbers.Sort();

            // Encode the numbers one by one and insert them in the list in order
            for (int i = 0; i < NumUlongs; i += 1)
            {
                listOfEncodings.Add(new string(Base64OrderPreservingAzureCompatEncoding.Encode(listOfNumbers[i], Base64OrderPreservingAzureCompatEncoding.SortOrders.Ordinal)));
            }

            // Check that the list of encoding numbers is already sorted
            List<string> orderedListOfEncodings = listOfEncodings.OrderBy(x => x, StringComparer.Ordinal).ToList();
            CollectionAssert.AreEqual(listOfEncodings, orderedListOfEncodings);
        }
    }
}
