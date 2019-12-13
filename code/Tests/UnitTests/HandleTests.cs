// <copyright file="HandleTests.cs" company="Microsoft">
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
    /// Tests that handles are unique in a multi-threaded environment. Also tests that
    /// handles are sorted in reverse chronological order.
    /// </summary>
    [TestClass]
    public class HandleTests
    {
        /// <summary>
        /// number of threads used by this test
        /// </summary>
        private const int NumThreads = 10000;

        /// <summary>
        /// thread-safe bag storing random strings
        /// </summary>
        private static ConcurrentBag<string> randomStrings = null;

        /// <summary>
        /// Tests that calling the long handle generator from 10,000 concurrent threads never leads to conflicts
        /// </summary>
        [TestMethod]
        public void LongHandlesAreUniqueInPractice()
        {
            // reset the bag of strings
            randomStrings = new ConcurrentBag<string>();

            // create the threads
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < NumThreads; i++)
            {
                threads.Add(new Thread(() => GenerateRandomLongHandle()));
            }

            // start the threads and wait for them to finish
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            // look for duplicates
            List<string> handles = new List<string>(randomStrings.ToArray());
            List<string> distinctHandles = handles.Distinct<string>().ToList<string>();
            Assert.AreEqual(handles.Count, NumThreads);
            Assert.AreEqual(handles.Count, distinctHandles.Count);
        }

        /// <summary>
        /// Tests that calling the long handle generator back-to-back 100,000 times never leads to conflicts.
        /// This is normal because long handles have 29 bits of entropy which makes the probability of conflict
        /// very low for handles generated in the same scheduling timeslice.
        /// </summary>
        [TestMethod]
        public void LongHandlesAreUniqueInTheory()
        {
            // Generate handles in decreasing order since that's the order SocialPlus cares about
            string[] handles = GenerateNHandlesKApart(false, false, 100000, 0);

            // look for duplicates. They must be there :-)
            List<string> distinctHandles = handles.Distinct<string>().ToList<string>();
            Assert.AreEqual(handles.Length, distinctHandles.Count);
        }

        /// <summary>
        /// Tests that long handles created chronologically in different scheduling timeslices are
        /// ordered.
        /// </summary>
        [TestMethod]
        public void LongHandlesAreReversedChronoOrdered()
        {
            // Generate 30 handles 10 milliseconds apart
            string[] handles = GenerateNHandlesKApart(false, false, 30, 10);

            // Azure tables use .NET string.CompareOrdinal, which is what our OrdinalComparison class uses
            // However, we also test for lexicographical order expecting mis-orderings

            // Verify that orderedHandles has the same order as the handles list using OrdinalComparison
            // whereas they ordered differently in LexicographicalComparison
            List<string> ordinalOrderedDecreasingHandles = handles.OrderByDescending(x => x, new OrdinalComparison()).ToList();
            List<string> lexiOrderedDecreasingHandles = handles.OrderByDescending(x => x, new LexiComparison()).ToList();

            CollectionAssert.AreEqual(handles, ordinalOrderedDecreasingHandles);
            CollectionAssert.AreNotEqual(handles, lexiOrderedDecreasingHandles);
        }

        /// <summary>
        /// Tests that calling the short handle generator from 10,000 concurrent threads never leads to conflicts
        /// </summary>
        [TestMethod]
        public void ShortHandlesAreUniqueInPractice()
        {
            // reset the bag of strings
            randomStrings = new ConcurrentBag<string>();

            // create the threads
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < NumThreads; i++)
            {
                threads.Add(new Thread(() => GenerateRandomShortHandle()));
            }

            // start the threads and wait for them to finish
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            // look for duplicates
            List<string> handles = new List<string>(randomStrings.ToArray());
            List<string> distinctHandles = handles.Distinct<string>().ToList<string>();
            Assert.AreEqual(handles.Count, NumThreads);
            Assert.AreEqual(handles.Count, distinctHandles.Count);
        }

        /// <summary>
        /// Tests that calling the short handle generator back-to-back 100,000 times always leads to conflicts.
        /// This is normal because short handles only have 23 bits of entropy. The probability of one conflict in
        /// 13,000 handles of 23 bits is almost one. However, the short time handle also includes a prefix
        /// guaranteed to be different when called during different scheduler timeslices. Therefore, generating
        /// 13,000 handles does not always lead to collisions. We found that generating 1 million handles
        /// always does however.
        /// </summary>
        [TestMethod]
        public void ShortHandlesAreNotUniqueInTheory()
        {
            // Generate handles in decreasing order since that's the order SocialPlus cares about
            string[] handles = GenerateNHandlesKApart(true, false, 1000000, 0);

            // look for duplicates. They must be there :-)
            List<string> distinctHandles = handles.Distinct<string>().ToList<string>();
            Assert.AreNotEqual(handles.Length, distinctHandles.Count);
        }

        /// <summary>
        /// Tests that short handles created chronologically in different scheduling timeslices are
        /// ordered.
        /// </summary>
        [TestMethod]
        public void ShortHandlesAreReversedChronoOrdered()
        {
            // Generate 30 handles 10 milliseconds apart
            string[] handles = GenerateNHandlesKApart(true, false, 30, 10);

            // Azure tables use .NET string.CompareOrdinal, which is what our OrdinalComparison class uses
            // However, we also test for lexicographical order expecting mis-orderings

            // Verify that orderedHandles has the same order as the handles list using OrdinalComparison
            // whereas they ordered differently in LexicographicalComparison
            List<string> ordinalOrderedDecreasingHandles = handles.OrderByDescending(x => x, new OrdinalComparison()).ToList();
            List<string> lexiOrderedDecreasingHandles = handles.OrderByDescending(x => x, new LexiComparison()).ToList();

            CollectionAssert.AreEqual(handles, ordinalOrderedDecreasingHandles);
            CollectionAssert.AreNotEqual(handles, lexiOrderedDecreasingHandles);
        }

        /// <summary>
        /// Gets a short handle and inserts it into the concurrent bag
        /// </summary>
        private static void GenerateRandomShortHandle()
        {
            HandleGenerator handleGenerator = new HandleGenerator();
            randomStrings.Add(handleGenerator.GenerateShortHandle());
        }

        /// <summary>
        /// Gets a long handle and inserts it into the concurrent bag
        /// </summary>
        private static void GenerateRandomLongHandle()
        {
            HandleGenerator handleGenerator = new HandleGenerator();
            randomStrings.Add(handleGenerator.GenerateLongHandle());
        }

        /// <summary>
        /// Generates n handles k milliseconds apart
        /// </summary>
        /// <param name="shortHandles">should we generated short or long handles</param>
        /// <param name="increasingOrder">should handles be generated in increasing or decreasing order</param>
        /// <param name="n">number of handles generated</param>
        /// <param name="k">numer of milliseconds apart</param>
        /// <returns>array of sequence numbers</returns>
        private static string[] GenerateNHandlesKApart(bool shortHandles, bool increasingOrder, int n, int k)
        {
            HandleGenerator handleGenerator = new HandleGenerator();
            string[] handles = new string[n];

            // This method deliberately has repetitive code to ensure it is fast.
            // The faster this code runs, the more likely is for collisions to occur
            if (k <= 0)
            {
                // Fast path
                if (shortHandles)
                {
                    for (int i = 0; i < n; i += 1)
                    {
                        handles[i] = handleGenerator.GenerateShortHandle();
                    }
                }
                else
                {
                    for (int i = 0; i < n; i += 1)
                    {
                        handles[i] = handleGenerator.GenerateLongHandle();
                    }
                }
            }
            else
            {
                // Slow path
                for (int i = 0; i < n; i += 1)
                {
                    if (shortHandles)
                    {
                        handles[i] = handleGenerator.GenerateShortHandle();
                    }
                    else
                    {
                        handles[i] = handleGenerator.GenerateLongHandle();
                    }

                    // No need to call sleep after generating the last sequence number
                    if (i + 1 < n)
                    {
                        Thread.Sleep(k);
                    }
                }
            }

            return handles;
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
