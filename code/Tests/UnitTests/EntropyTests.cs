// <copyright file="EntropyTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests RNG collisions in a multithreaded environment. These tests demonstrate that System.Random
    /// lacks sufficient entropy in highly concurrent scenarios whereas RNGCryptoServiceProvider does not.
    /// </summary>
    [TestClass]
    public class EntropyTests
    {
        /// <summary>
        /// number of threads used by this test
        /// </summary>
        private const int NumThreads = 100;

        /// <summary>
        /// maximum random number to generate. These tests are concerned with random numbers not longer than 23 bits
        /// </summary>
        private const int MaxNumber = (1 << 24) - 1;

        /// <summary>
        /// thread-safe bag storing random numbers. A 'uint' is sufficiently large for our random numbers.
        /// </summary>
        private static ConcurrentBag<uint> randomNumbers = null;

        /// <summary>
        /// Tests System.Random to verify that instantiating the RNG multiple times does not have sufficient entropy
        /// </summary>
        [TestMethod]
        public void SystemRandomLacksEnoughEntropy()
        {
            // initializes the bag of numbers
            randomNumbers = new ConcurrentBag<uint>();

            // create the threads
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < NumThreads; i++)
            {
                threads.Add(new Thread(() => GenerateSystemRandomNumber()));
            }

            // start the threads and wait for them to finish
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            // look for duplicates
            List<uint> numbers = new List<uint>(randomNumbers.ToArray());
            List<uint> distinctNumbers = numbers.Distinct<uint>().ToList<uint>();
            Assert.AreEqual(numbers.Count, NumThreads);
            Assert.AreNotEqual(numbers.Count, distinctNumbers.Count);
        }

        /// <summary>
        /// Tests System.Security.Cryptography to verify that instantiating the RNG multiple times does have sufficient entropy
        /// </summary>
        [TestMethod]
        public void CryptoRandomHasEnoughEntropy()
        {
            // reset the bag of numbers
            randomNumbers = new ConcurrentBag<uint>();

            // create the threads
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < NumThreads; i++)
            {
                threads.Add(new Thread(() => GenerateCryptographicRandomNumber()));
            }

            // start the threads and wait for them to finish
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            // look for duplicates
            List<uint> numbers = new List<uint>(randomNumbers.ToArray());
            List<uint> distinctNumbers = numbers.Distinct<uint>().ToList<uint>();
            Assert.AreEqual(numbers.Count, NumThreads);
            Assert.AreEqual(numbers.Count, distinctNumbers.Count);
        }

        /// <summary>
        /// Gets a random number from the System.Random class and inserts it into the concurrent bag
        /// </summary>
        private static void GenerateSystemRandomNumber()
        {
            // Initialize a system-level pseudo-random class with a time-dependent seed value
            Random randomGenerator = new Random();
            randomNumbers.Add((uint)randomGenerator.Next(MaxNumber));
        }

        /// <summary>
        /// Gets a random number from the System.Security.Cryptography class and inserts it into the concurrent bag
        /// </summary>
        private static void GenerateCryptographicRandomNumber()
        {
            // Initialize a system-level crypto-random class with a time-dependent seed value
            RNGCryptoServiceProvider randomGenerator = new RNGCryptoServiceProvider();

            byte[] data = new byte[sizeof(uint)];
            randomGenerator.GetBytes(data);
            randomNumbers.Add(BitConverter.ToUInt32(data, 0) % MaxNumber);
        }
    }
}