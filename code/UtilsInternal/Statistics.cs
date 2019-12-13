// <copyright file="Statistics.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Useful routines for calculating performance statistics.
    /// This class is meant for occasional testing, not performance optimized to be used in production code.
    /// </summary>
    public static class Statistics
    {
        /// <summary>
        /// gets the high performance counter value
        /// </summary>
        /// <returns>ticks</returns>
        public static long GetHighPerformanceTimestamp()
        {
            if (!Stopwatch.IsHighResolution)
            {
                throw new NotSupportedException("no high resolution counter");
            }

            return Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// gets the frequency of the high performance counter
        /// </summary>
        /// <returns>ticks per second</returns>
        public static long GetHighPerformanceFrequency()
        {
            if (!Stopwatch.IsHighResolution)
            {
                throw new NotSupportedException("no high resolution counter");
            }

            return Stopwatch.Frequency;
        }

        /// <summary>
        /// Calculates delays in a list of timestamps
        /// </summary>
        /// <param name="timestamps">List of timestamps</param>
        /// <returns>List of delays between adjacent timestamps</returns>
        public static List<long> CalculateDelays(List<long> timestamps)
        {
            if (timestamps == null)
            {
                throw new ArgumentNullException();
            }

            if (timestamps.Count < 2)
            {
                throw new ArgumentException("list is too short");
            }

            List<long> delays = new List<long>();
            for (int i = 1; i < timestamps.Count; i++)
            {
                delays.Add(timestamps[i] - timestamps[i - 1]);
            }

            return delays;
        }

        /// <summary>
        /// Calculates delays in sending to receive of timestamp, sequence #
        /// </summary>
        /// <param name="sent">List of timestamp, sequence number at sent time</param>
        /// <param name="received">List of timestamp, sequence number at receive time</param>
        /// <returns>List of delays from send to receive; will skip any that are sent but not received</returns>
        public static List<long> CalculateDelays(List<Tuple<long, int>> sent, List<Tuple<long, int>> received)
        {
            if (sent == null || received == null)
            {
                throw new ArgumentNullException();
            }

            if (sent.Count < 2 || received.Count < 2)
            {
                throw new ArgumentException("input list is too short");
            }

            List<long> delays = new List<long>();
            foreach (Tuple<long, int> sentItem in sent)
            {
                Tuple<long, int> receivedItem = received.Find(t => t.Item2 == sentItem.Item2);
                if (receivedItem != null)
                {
                    delays.Add(receivedItem.Item1 - sentItem.Item1);
                }
            }

            return delays;
        }

        /// <summary>
        /// Calculates statistics on a list of timestamps
        /// </summary>
        /// <param name="delays">list of delays in ticks</param>
        /// <param name="minDelay">shortest delay in seconds</param>
        /// <param name="avgDelay">average delay in seconds</param>
        /// <param name="medianDelay">median delay in seconds</param>
        /// <param name="maxDelay">longest delay in seconds</param>
        public static void CalculateDelayStatistics(List<long> delays, out double minDelay, out double avgDelay, out double medianDelay, out double maxDelay)
        {
            if (delays == null)
            {
                throw new ArgumentNullException();
            }

            if (delays.Count < 2)
            {
                throw new ArgumentException("list is too short");
            }

            long frequency = GetHighPerformanceFrequency();
            minDelay = Convert.ToDouble(delays.Min()) / frequency;
            avgDelay = Convert.ToDouble(delays.Average()) / frequency;
            maxDelay = Convert.ToDouble(delays.Max()) / frequency;
            delays.Sort();
            medianDelay = (delays.Count % 2 == 0) ? (Convert.ToDouble(delays[delays.Count / 2] + delays[(delays.Count / 2) - 1]) / 2) / frequency :
                                                    Convert.ToDouble(delays[Convert.ToInt32(Math.Floor(Convert.ToDecimal(delays.Count) / 2))]) / frequency;
        }

        /// <summary>
        /// Calculates statistics on a list of sequence numbers
        /// </summary>
        /// <param name="sequence">list of integer sequence numbers in order received</param>
        /// <param name="outOfOrder">count of out of order numbers</param>
        /// <param name="missing">count of missing numbers</param>
        /// <param name="duplicates">count of duplicate numbers</param>
        /// <param name="delivered">count of unique numbers</param>
        public static void CalculateOrderingStatistics(List<int> sequence, out int outOfOrder, out int missing, out int duplicates, out int delivered)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException();
            }

            if (sequence.Count < 2)
            {
                throw new ArgumentException("list is too short");
            }

            int min = sequence.Min();
            int max = sequence.Max();
            int currentMax = min - 1;
            delivered = 0;
            duplicates = 0;
            outOfOrder = 0;
            List<int> missingCandidates = new List<int>();

            for (int i = 0; i < sequence.Count; i++)
            {
                if (sequence[i] == currentMax)
                {
                    duplicates++;
                }
                else if (sequence[i] > currentMax)
                {
                    delivered++;
                    if (sequence[i] > (currentMax + 1))
                    {
                        for (int j = currentMax + 1; j < sequence[i]; j++)
                        {
                            if (!missingCandidates.Contains(j))
                            {
                                missingCandidates.Add(j);
                            }
                        }
                    }

                    currentMax = sequence[i];
                }
                else if (sequence[i] < currentMax)
                {
                    if (missingCandidates.Contains(sequence[i]))
                    {
                        delivered++;
                        outOfOrder++;
                        missingCandidates.Remove(sequence[i]);
                    }
                    else
                    {
                        duplicates++;
                    }
                }
            }

            missing = missingCandidates.Count;
        }

        /// <summary>
        /// calculate throughput
        /// </summary>
        /// <param name="startTime">start time in ticks</param>
        /// <param name="endTime">end time in ticks</param>
        /// <param name="count">number of operations</param>
        /// <returns>throughput measured in operations per ticks</returns>
        public static double CalculateThroughput(long startTime, long endTime, int count)
        {
            if (endTime <= startTime)
            {
                throw new ArgumentException("endTime must be greater than startTime");
            }

            double throughput = Convert.ToDouble(count) / (Convert.ToDouble(endTime - startTime) / Convert.ToDouble(GetHighPerformanceFrequency()));
            return throughput;
        }
    }
}
