// <copyright file="RandUtils.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// Utilities for randomness and entropy needed by SocialPlus
    /// </summary>
    public class RandUtils
    {
        /// <summary>
        /// Crypto strength random number generator.
        /// </summary>
        private static readonly RNGCryptoServiceProvider RandomGenerator = new RNGCryptoServiceProvider();

        /// <summary>
        /// Generates a random ulong by generating random bytes and converting them to an ulong.
        /// </summary>
        /// <returns>random unsigned long</returns>
        public ulong GenerateRandomUlong()
        {
            byte[] randBytes = this.GenerateRandomBytes(sizeof(ulong));
            return BitConverter.ToUInt64(randBytes, 0);
        }

        /// <summary>
        /// Generate a random uint by generating random bytes and converting them to an uint.
        /// </summary>
        /// <returns>random unsigned int</returns>
        public uint GenerateRandomUint()
        {
            byte[] randBytes = this.GenerateRandomBytes(sizeof(uint));
            return BitConverter.ToUInt32(randBytes, 0);
        }

        /// <summary>
        /// Generate a random ushort by generating random bytes and converting them to an ushort.
        /// </summary>
        /// <returns>random unsigned short</returns>
        public ushort GenerateRandomUshort()
        {
            byte[] randBytes = this.GenerateRandomBytes(sizeof(ushort));
            return BitConverter.ToUInt16(randBytes, 0);
        }

        /// <summary>
        /// Generates a random byte array of a size specified by the caller. Input size is deliberately
        /// a "ushort" to restrict the number of bytes of randomness to be created to less than 64KB.
        /// </summary>
        /// <param name="size">number of bytes of randomness</param>
        /// <returns>random byte array</returns>
        public byte[] GenerateRandomBytes(ushort size)
        {
            byte[] randBytes = new byte[size];
            RandomGenerator.GetBytes(randBytes);
            return randBytes;
        }
    }
}
