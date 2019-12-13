// <copyright file="Base64OrderPreservingAzureCompatEncoding.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;

    /// <summary>
    /// Implements encoding utilities that are order-preserving
    /// </summary>
    public static class Base64OrderPreservingAzureCompatEncoding
    {
        /// <summary>
        /// 64 readable characters sorted in lexicographic order
        /// </summary>
        private static readonly char[] CharsInLexicographicOrder =
        {
            '-',
            '_',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H', 'i', 'I', 'j', 'J', 'k', 'K', 'l', 'L', 'm', 'M',
            'n', 'N', 'o', 'O', 'p', 'P', 'q', 'Q', 'r', 'R', 's', 'S', 't', 'T', 'u', 'U', 'v', 'V', 'w', 'W', 'x', 'X', 'y', 'Y', 'z', 'Z'
        };

        /// <summary>
        /// 64 readable characters sorted in ordinal (Unicode-based) order
        /// </summary>
        private static readonly char[] CharsInOrdinalOrder =
        {
            '-',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '_',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        /// <summary>
        /// Types of sort orders
        /// </summary>
        public enum SortOrders
        {
            /// <summary>
            /// Ordinal ordering. It uses Unicode. It is what string.CompareOrdinal uses.
            /// Azure uses Ordinal ordering to sort their row and partition keys.
            /// </summary>
            Ordinal,

            /// <summary>
            /// Lexicographic ordering. It is what string.Compare uses.
            /// </summary>
            Lexicographic
        }

        /// <summary>
        /// Encodes a ulong into a fixed-size char array of length 11.
        /// </summary>
        /// <param name="number">input number</param>
        /// <param name="order">determines type of order (ordinal or lexicographic). Default uses ordinal which is used by Azure</param>
        /// <returns>an array of 11 chars representing the order-preserving encoding of input number</returns>
        public static char[] Encode(ulong number, SortOrders order = SortOrders.Ordinal)
        {
            char[] result = new char[11];
            char[] charsInOrder = CharsInOrdinalOrder;

            if (order == SortOrders.Lexicographic)
            {
                charsInOrder = CharsInLexicographicOrder;
            }

            // Go from LSB to MSB six-bits at a time
            for (int i = 10; i >= 0; i -= 1)
            {
                result[i] = Encode((byte)(number % 64), order);
                number >>= 6;
            }

            return result;
        }

        /// <summary>
        /// Decodes an encoding of a 64-bit unsigned int
        /// </summary>
        /// <param name="encodedChars">an array of 11 characters</param>
        /// <param name="order">determines type of order (ordinal or lexicographic). Default uses ordinal which is used by Azure</param>
        /// <returns>a ulong represending the order-preserving decoding of input char array</returns>
        public static ulong Decode(char[] encodedChars, SortOrders order = SortOrders.Ordinal)
        {
            // Error check
            if (encodedChars.Length != 11)
            {
                throw new ArgumentException(string.Format("The encoding must have 11 bytes. Instead it has {0} bytes.", encodedChars.Length));
            }

            ulong result = 0;

            // Decode one char at a time. Wrap everything into a try in case the input char array contains invalid bytes.
            try
            {
                for (int i = 0; i < 11; i += 1)
                {
                    // Find index of current character in the order array and place it
                    // in the i-th 6-bit segment in the result. For this, left-shift the index (6 * i) bits
                    result += ((ulong)Decode(encodedChars[10 - i], order)) << (6 * i);
                }
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(string.Format("The encoding \"{0}\" contains invalid bytes. Can't decode.", new string(encodedChars)));
            }

            return result;
        }

        /// <summary>
        /// Encodes a number between 0-63 into a char.
        /// </summary>
        /// <param name="number">input number</param>
        /// <param name="order">determines type of order (ordinal or lexicographic). Default uses ordinal which is used by Azure</param>
        /// <returns>a char representing the order-preserving encoding of input number</returns>
        public static char Encode(byte number, SortOrders order = SortOrders.Ordinal)
        {
            // Error check
            if (number > 63)
            {
                throw new ArgumentException(string.Format("The number {0} cannot be represented in 6 bits only. ", number));
            }

            char[] charsInOrder = CharsInOrdinalOrder;

            if (order == SortOrders.Lexicographic)
            {
                charsInOrder = CharsInLexicographicOrder;
            }

            return charsInOrder[number];
        }

        /// <summary>
        /// Decodes a char into a six-bit byte (i.e., the byte is between 0 and 63)
        /// </summary>
        /// <param name="encodedChar">the input char</param>
        /// <param name="order">determines type of order (ordinal or lexicographic). Default uses ordinal which is used by Azure</param>
        /// <returns>a byte represending the order-preserving decoding of input char</returns>
        public static byte Decode(char encodedChar, SortOrders order = SortOrders.Ordinal)
        {
            char[] charsInOrder = CharsInOrdinalOrder;

            if (order == SortOrders.Lexicographic)
            {
                charsInOrder = CharsInLexicographicOrder;
            }

            // Find index of current character in the order array
            int index = Array.IndexOf(charsInOrder, encodedChar);
            if (index < 0)
            {
                throw new ArgumentException(string.Format("The encoding \'{0}\' contains invalid bytes. Can't decode.", encodedChar));
            }

            return (byte)index;
        }
    }
}
