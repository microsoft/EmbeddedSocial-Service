// <copyright file="HandleGenerator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;

    /// <summary>
    /// This class generates two types of handles: short and long.
    ///  - short: a 11-char string obtained from a pseudo-unique time ordered sequence number generated in decreasing order and Unicode-based encoded
    ///  - long:  a 48-char string obtained from a non-unique time ordered sequence number generated in decreasing order and Unicode-based encode
    ///           concatenated with '-' concatanated with a system guid (36 chars)
    ///
    /// Both handle types provide two properties, but with different guarantees:
    /// 1. Uniqueness: no two different handles collide.
    /// 2. Reverse chronologically:
    ///     if handle h2 is generated later in time than handle h1, h1 > h2 in Unicode-based order
    ///
    /// Short handles are based on PseudoUniqueSequenceNumbers and provide:
    ///     1. uniqueness with very high probability. For more info about the probability of collisions, read PseudoUniqueSequenceNumber description
    ///     2. reverse chronological ordering only when the two handles are generated more than one scheduler timeslice apart (~10ms).
    ///        When two handles are generated within the same scheduler timeslice, their order is random.
    ///
    /// Long handles are based on StronglyOrderedSequenceNumber and provide:
    ///     1. guaranteed uniqueness. (they incorporate a .NET's 128-byte guid)
    ///     2. reverse chronological ordering when the two handles are generated more than one scheduler timeslice apart (~10ms).
    ///        When two handles are generated within the same scheduler timeslice, their order is random.
    ///
    /// Short handles are useful only when handles are required to be short. Otherwise, use long handles.
    /// </summary>
    public class HandleGenerator : IHandleGenerator
    {
        /// <summary>
        /// Sequence number generator set to generate numbers in decreasing order
        /// </summary>
        private static readonly TimeOrderedSequenceNumber SeqNumGenerator = new TimeOrderedSequenceNumber(increasingOrder: false);

        /// <summary>
        /// Generates short handles: 11 characters each
        /// </summary>
        /// <returns>Short time-ordered handle</returns>
        public string GenerateShortHandle()
        {
            char[] handle = Base64OrderPreservingAzureCompatEncoding.Encode(SeqNumGenerator.GeneratePseudoUniqueSequenceNumber(), Base64OrderPreservingAzureCompatEncoding.SortOrders.Ordinal);
            return new string(handle);
        }

        /// <summary>
        /// Generates long handle: a non unique sequence number + "-" + a system guid
        /// </summary>
        /// <returns>Long time-ordered handle</returns>
        public string GenerateLongHandle()
        {
            char[] handle = Base64OrderPreservingAzureCompatEncoding.Encode(SeqNumGenerator.GenerateStronglyOrderedSequenceNumber(), Base64OrderPreservingAzureCompatEncoding.SortOrders.Ordinal);
            return string.Join("-", new string(handle), Guid.NewGuid().ToString());
        }
    }
}
