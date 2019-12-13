// <copyright file="RedisInstanceType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    /// <summary>
    /// <c>Redis</c> instance type
    /// </summary>
    public enum RedisInstanceType
    {
        /// <summary>
        /// Volatile <c>Redis</c> cache
        /// </summary>
        Volatile,

        /// <summary>
        /// Persistent <c>Redis</c> cache
        /// </summary>
        Persistent
    }
}
