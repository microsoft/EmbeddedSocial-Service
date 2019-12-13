// <copyright file="TimeUtils.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;

    /// <summary>
    /// Utilities for time-related constants needed by SocialPlus
    /// </summary>
    public static class TimeUtils
    {
        /// <summary>
        /// Gets the first date in the 21st century Utc
        /// </summary>
        public static DateTime FirstDateOf21Century { get; } = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Gets the beginning of Unix time (1/1/1970), but in a .NET DateTime format Utc
        /// </summary>
        public static DateTime BeginningOfUnixTime { get; } = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Gets the date of the first beta version of our service -- May 1st, 2016 in format Utc
        /// </summary>
        public static DateTime BetaServiceLaunch { get; } = new DateTime(2016, 5, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Gets a timespan equivalent to 24 hours
        /// </summary>
        /// <returns>24-hour timespan</returns>
        public static TimeSpan TwentyFourHours { get; } = new TimeSpan(1, 0, 0, 0);

        /// <summary>
        /// Converts from a .NET DateTime into a .NET Timespan representing Unix time (since 1/1/1970)
        /// </summary>
        /// <param name="timestamp">input time in .NET DateTime format</param>
        /// <returns>.NET Timespan represending Unix time (elapsed time since 1/1/1970)</returns>
        public static TimeSpan DateTime2UnixTime(DateTime timestamp)
        {
            return timestamp.ToUniversalTime().Subtract(BeginningOfUnixTime);
        }

        /// <summary>
        /// Converts from .NET Timespan representing Unix time to .NET DateTime
        /// </summary>
        /// <param name="timestamp">input time in .NET Timespan representing Unix time (elapsed time since 1/1/1970)</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime UnixTime2DateTime(TimeSpan timestamp)
        {
            return BeginningOfUnixTime.Add(timestamp);
        }
    }
}
