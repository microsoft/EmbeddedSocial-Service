// <copyright file="SequenceNumbersTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Utils;

    /// <summary>
    /// Tests that sequence numbers are unique in a multi-threaded environment. Also tests that
    /// sequence numbers are sorted in reverse chronological order.
    /// </summary>
    [TestClass]
    public class SequenceNumbersTests
    {
        /// <summary>
        /// number of threads used by this test
        /// </summary>
        private const int NumThreads = 50000;

        /// <summary>
        /// thread-safe bag storing sequence numbers
        /// </summary>
        private static ConcurrentBag<ulong> seqNums = null;

        /// <summary>
        /// Tests that calling the sequence number generator from 50,000 concurrent threads never leads to conflicts
        /// </summary>
        [TestMethod]
        public void PseudoUniqueSequenceNumbersAreUniqueInPractice()
        {
            // reset the bag of strings
            seqNums = new ConcurrentBag<ulong>();

            // create the threads
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < NumThreads; i++)
            {
                threads.Add(new Thread(() => GeneratePseudoUniqueSeqNum()));
            }

            // start the threads and wait for them to finish
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            // look for duplicates
            List<ulong> handles = new List<ulong>(seqNums.ToArray());
            List<ulong> distinctHandles = handles.Distinct<ulong>().ToList<ulong>();
            Assert.AreEqual(handles.Count, NumThreads);
            Assert.AreEqual(handles.Count, distinctHandles.Count);
        }

        /// <summary>
        /// Tests that calling the sequence number generator back-to-back 1 million times always leads to conflicts.
        /// This is normal because sequence numbers only have 23 bits of entropy. The probability of one conflict in
        /// 13,000 random numbers of 23 bits is almost one. However, the sequence number also includes a prefix
        /// guaranteed to be different when called during different scheduler timeslices. Therefore, generating
        /// 13,000 sequence numbers does not always lead to collisions. We found that generating 1 million sequence numbers
        /// always does however.
        /// </summary>
        [TestMethod]
        public void PseudoUniqueSequenceNumbersAreNotUniqueInTheory()
        {
            // Generate pseudounique sequence numbers in decreasing order (since that's the order SocialPlus cares about)
            ulong[] seqNums = GenerateNSequenceNumbersKApart(true, false, 1000000, 0);

            // look for duplicates. They must be there :-)
            List<ulong> distinctSeqNums = seqNums.Distinct<ulong>().ToList<ulong>();
            Assert.AreNotEqual(seqNums.Length, distinctSeqNums.Count);
        }

        /// <summary>
        /// Tests that sequence numbers created chronologically in different scheduling timeslices are
        /// ordered
        /// </summary>
        [TestMethod]
        public void PseudoUniqueSequenceNumbersAreChronoOrdered()
        {
            // Generate 30 pseudo unique sequence numbers 100 milliseconds apart
            ulong[] increasingSeqNums = GenerateNSequenceNumbersKApart(true, true, 30, 100);
            ulong[] decreasingSeqNums = GenerateNSequenceNumbersKApart(true, false, 30, 100);

            // Verify that orderedHandles has the same order as the seqNum lists
            List<ulong> orderedIncreasingSeqNums = increasingSeqNums.OrderBy(x => x).ToList();
            List<ulong> orderedDecreasingSeqNums = decreasingSeqNums.OrderByDescending(x => x).ToList();

            CollectionAssert.AreEqual(orderedIncreasingSeqNums, increasingSeqNums);
            CollectionAssert.AreEqual(orderedDecreasingSeqNums, decreasingSeqNums);
        }

        /// <summary>
        /// Tests that strongly sequence numbers are strongly ordered
        /// ordered
        /// </summary>
        [TestMethod]
        public void StronglyOrderedSequenceNumbersAreChronoOrdered()
        {
            // Generate strongly ordered sequence numbers back-to-back
            ulong[] increasingSeqNums = GenerateNSequenceNumbersKApart(false, true, NumThreads, 0);
            ulong[] decreasingSeqNums = GenerateNSequenceNumbersKApart(false, false, NumThreads, 0);

            // Verify that orderedHandles has the same order as the seqNum lists
            List<ulong> orderedIncreasingSeqNums = increasingSeqNums.OrderBy(x => x).ToList();
            List<ulong> orderedDecreasingSeqNums = decreasingSeqNums.OrderByDescending(x => x).ToList();

            CollectionAssert.AreEqual(orderedIncreasingSeqNums, increasingSeqNums);
            CollectionAssert.AreEqual(orderedDecreasingSeqNums, decreasingSeqNums);
        }

        /// <summary>
        /// Tests that strongly ordered sequence numbers are not unique when generated back-to-back
        /// </summary>
        [TestMethod]
        public void StronglyOrderedSequenceNumbersAreNotUnique()
        {
            // Generate 30 strongly ordered sequence numbers in decreasing order (since that's the order SocialPlus cares about)
            ulong[] seqNums = GenerateNSequenceNumbersKApart(false, false, 30, 0);

            // look for duplicates. They must be there :-)
            List<ulong> distinctSeqNums = seqNums.Distinct<ulong>().ToList<ulong>();
            Assert.AreNotEqual(seqNums.Length, distinctSeqNums.Count);
        }

        /// <summary>
        /// Gets a sequence number and inserts it into the concurrent bag
        /// </summary>
        private static void GeneratePseudoUniqueSeqNum()
        {
            TimeOrderedSequenceNumber seqNumGenerator = new TimeOrderedSequenceNumber();
            seqNums.Add(seqNumGenerator.GeneratePseudoUniqueSequenceNumber());
        }

        /// <summary>
        /// Generates n sequence numbers k milliseconds apart
        /// </summary>
        /// <param name="pseudoUnique">should we call pseudo unique or strongly ordered sequence numbers</param>
        /// <param name="increasingOrder">should sequence numbers be generated in increasing or decreasing order</param>
        /// <param name="n">number of sequence numbers generated</param>
        /// <param name="k">numer of milliseconds apart</param>
        /// <returns>array of sequence numbers</returns>
        private static ulong[] GenerateNSequenceNumbersKApart(bool pseudoUnique, bool increasingOrder, int n, int k)
        {
            TimeOrderedSequenceNumber seqNumGenerator = new TimeOrderedSequenceNumber(increasingOrder);
            ulong[] seqNums = new ulong[n];

            // This method deliberately has repetitive code to ensure it is fast.
            // The faster this code runs, the more likely it is we will find collisions quickly
            if (k <= 0)
            {
                // Fast path
                if (pseudoUnique)
                {
                    for (int i = 0; i < n; i += 1)
                    {
                        seqNums[i] = seqNumGenerator.GeneratePseudoUniqueSequenceNumber();
                    }
                }
                else
                {
                    for (int i = 0; i < n; i += 1)
                    {
                        seqNums[i] = seqNumGenerator.GenerateStronglyOrderedSequenceNumber();
                    }
                }
            }
            else
            {
                // Slow path
                for (int i = 0; i < n; i += 1)
                {
                    if (pseudoUnique)
                    {
                        seqNums[i] = seqNumGenerator.GeneratePseudoUniqueSequenceNumber();
                    }
                    else
                    {
                        seqNums[i] = seqNumGenerator.GenerateStronglyOrderedSequenceNumber();
                    }

                    // No need to call sleep after generating the last sequence number
                    if (i + 1 < n)
                    {
                        Thread.Sleep(k);
                    }
                }
            }

            return seqNums;
        }

        /// <summary>
        /// String comparison class that uses .NET CompareOrdinal. The default .NET string
        /// comparison is done character by character lexicographically, not based on the
        /// Unicode values of each character.  CompareOrdinal does string comparison using
        /// the Unicode character values. In Unicode, "A" (65) is less than "a" (97),
        /// whereas lexicographically "A" > "a".
        /// </summary>
        private class OrdinalComparison : IComparer<string>
        {
            public int Compare(string s1, string s2)
            {
                return string.CompareOrdinal(s1, s2);
            }
        }

        /// <summary>
        /// String comparison class that uses the default .NET Compare. Azure uses ordinal
        /// string comparison using the Unicode character values, not based on their
        /// lexicographical order. In lexicographic order, "A" > "a" whereas in Unicode
        /// "A" (65) is less than "a" (97).
        /// </summary>
        private class LexiComparison : IComparer<string>
        {
            public int Compare(string s1, string s2)
            {
                return string.Compare(s1, s2);
            }
        }
    }
}
