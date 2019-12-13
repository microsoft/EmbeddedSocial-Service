// <copyright file="GuidExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;

    /// <summary>
    /// Class the implements utils for Guids
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Extendion method to XOR two guids, byte by byte
        /// </summary>
        /// <param name="guid1">guid 1 </param>
        /// <param name="guid2">guid 2</param>
        /// <returns>XOR of guid1 and guid 2</returns>
        public static Guid Xor(this Guid guid1, Guid guid2)
        {
            var guid1Bytes = guid1.ToByteArray();
            var guid2Bytes = guid2.ToByteArray();

            var guidLength = guid1Bytes.Length;
            byte[] result = new byte[guidLength];

            for (int i = 0; i < guidLength; i += 1)
            {
                result[i] = (byte)(guid1Bytes[i] ^ guid2Bytes[i]);
            }

            return new Guid(result);
        }
    }
}
