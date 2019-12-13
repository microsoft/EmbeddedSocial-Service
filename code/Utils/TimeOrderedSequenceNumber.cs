// <copyright file="TimeOrderedSequenceNumber.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;

    /// <summary>
    /// Generates sequence numbers that are time ordered in a distributed system with no coordination.
    /// Independent nodes can call this class without requiring any coordination among them to obtain
    /// sequence numbers that are "time ordered". Time ordered is within quotes because it is subject
    /// to clock synchronization across nodes. In a distributed system in which all nodes have perfectly
    /// synchronized clocks, the sequence numbers are guaranteed to be time ordered.
    /// Time ordered means that if s2 is generated at a later time than s1, s2 cannot be less than s1.
    /// Notice, that s2 could be equal to s1.
    /// This class offers two APIs that produce sequence numbers with different
    /// guarantees of time ordering and uniqueness:
    ///   1. StronglyOrderedSequenceNumber: generates identical sequence numbers when called within
    ///      the same scheduler timeslice, but strictly time ordered when called in different timeslices.
    ///   2. the sequence numbers are unique (with very high probability) when generated within the same
    ///      scheduler time slice but not necessarily time ordered. The numbers are guaranteed to be
    ///      time ordered when generated in different scheduler timeslices. For more information on
    ///      the probability of the sequence numbers not being unique, see the methods' comments.
    /// The sequence numbers are returned as unsigned 64-bit integers.
    /// </summary>
    public class TimeOrderedSequenceNumber
    {
        /// <summary>
        /// Beginning of time -- start of Unix time
        /// </summary>
        private static readonly DateTime BeginningOfTime = TimeUtils.BeginningOfUnixTime;

        /// <summary>
        /// End of time -- beginning of time + 69 years
        /// </summary>
        private static readonly DateTime EndOfTime = BeginningOfTime.AddYears(69);

        /// <summary>
        /// Number of bits used for time: 41
        /// </summary>
        private static readonly int TimeBitsCount = 41;

        /// <summary>
        /// Number of bits used for random number: 64 - 41
        /// </summary>
        private static readonly int RandomBitsCount = 64 - TimeBitsCount;

        /// <summary>
        /// Class encapsulating code on generating random numbers
        /// </summary>
        private static readonly RandUtils RandUtils = new RandUtils();

        /// <summary>
        /// Whether the sequence numbers are generated in increasing order
        /// </summary>
        private readonly bool increasingOrder;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeOrderedSequenceNumber"/> class.
        /// Sets a flag whether the sequence numbers should be generated in increasing or decreasing order
        /// </summary>
        /// <param name="increasingOrder">order in which sequence number will be generated. Default is decreasing.</param>
        public TimeOrderedSequenceNumber(bool increasingOrder = false)
        {
            this.increasingOrder = increasingOrder;
        }

        /// <summary>
        /// Gets sequence numbers guaranteed to be time ordered, but not necessarily unique. More formally:
        /// If called twice in a row to generate s1 and s2, this call guarantees that s2 >= s1. However,
        /// this method makes no effort to guarantee uniqueness; in fact s2 == s1 when the calls occurred
        /// within the same scheduler timeslice.
        /// A strongly ordered sequence number is formed by encoding the number of milliseconds
        /// since the beginning of time (or end of time if the numbers are genered in descending order)
        /// </summary>
        /// <returns>an ulong representing a strongly ordered sequence number</returns>
        public ulong GenerateStronglyOrderedSequenceNumber()
        {
            return this.GetTimestamp();
        }

        /// <summary>
        /// Gets sequence numbers that are unique with very high probability even when multiple instances
        /// of this class are running on different machines. However, the sequence numbers are not guaranteed
        /// to be ordered when generated within the same schedule timeslice.
        /// Each sequence number is formed of two parts: the bits in its prefix are referred to as TimeBits and
        /// the ones its suffix as RandomBits. The time bits are used to encode the number of milliseconds elapsed
        /// since the beginning of time. The RandomBits are set to a random value.  The number of
        /// TimeBits (T) plus the number of RandomBits (R) is 64.
        /// We set T to 41. This corresponds to 69 years, guaranteeing that time bits will not wrap for 69 years.
        /// We set R to 23 (= 64 - 41).
        /// The probability of collision of two sequence numbers generated on a single machine is:
        ///     - 0 if the numbers are generated in different scheduling quanta because the time bits will be unique
        ///     - less than 1 in 8 million if the numbers are generated in the same scheduling timeslice.
        ///         More precisely, the probability of collision is 1/(2^23).
        /// The probability of generating k unique sequence numbers in a single timeslice is: n!/(n^k * (n - k)!) where n is 2^23.
        /// This means that the probability of collision occurance is:
        ///     - 7 X 10^{-6} for k = 10    - corresponds to a rate of at least 666    requests/second
        ///     - 6 X 10^{-4} for k = 100   - corresponds to a rate of at least 6,666  requests/second
        ///     - 2 X 10^{-3} for k = 200   - corresponds to a rate of at least 13,333 requests/second
        ///     - ~1%         for k = 410   - corresponds to a rate of at least 27,333 requests/second
        /// For more info on birthday paradox probabilities: https://en.wikipedia.org/wiki/Birthday_problem
        /// </summary>
        /// <returns>an ulong representing a pseudo-unique sequence number</returns>
        public ulong GeneratePseudoUniqueSequenceNumber()
        {
            return this.GeneratePseudoUniqueSequenceNumberFromTimestamp(this.GetTimestamp());
        }

        /// <summary>
        /// Gets current timestamp either since the beginning of time (if sequence numbers are to be increasing)
        /// or until end of time (if sequence numbers are to be decreasing)
        /// </summary>
        /// <returns>timestamp in milliseconds (a .NET double cast to ulong)</returns>
        private ulong GetTimestamp()
        {
            TimeSpan span;

            if (this.increasingOrder)
            {
                // Current time since beginning of time in milliseconds
                span = DateTime.Now - BeginningOfTime;
            }
            else
            {
                // Remaining time until end of time in milliseconds
                span = EndOfTime - DateTime.Now;
            }

            return (ulong)span.TotalMilliseconds;
        }

        /// <summary>
        /// Generates sequence numbers as ulong from a timestamp. This number is unique with very high probability.
        /// </summary>
        /// <param name="timestamp">timestamp</param>
        /// <returns>sequence number</returns>
        private ulong GeneratePseudoUniqueSequenceNumberFromTimestamp(ulong timestamp)
        {
            // set the LSB 23 bits to a random value. For this, we generate a random uint and right-shift it 32 - 23 bits
            ulong seqN = RandUtils.GenerateRandomUint() >> (32 - RandomBitsCount);

            // set the MSB 41 bits to the timestamp
            seqN |= timestamp << RandomBitsCount;

            return seqN;
        }
    }
}
