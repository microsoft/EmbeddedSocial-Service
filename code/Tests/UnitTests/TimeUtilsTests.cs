// <copyright file="TimeUtilsTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Simple tests for our time util methods
    /// </summary>
    [TestClass]
    public class TimeUtilsTests
    {
        /// <summary>
        /// Tests for time utils public methods
        /// </summary>
        [TestMethod]
        public void AddSubtractTime()
        {
            DateTime timestampNow = DateTime.UtcNow;
            DateTime timestampNowPlusOneSecond = timestampNow.AddSeconds(1);
            DateTime timestampNowMinusOneSecond = timestampNow.Subtract(new TimeSpan(0, 0, 1));

            // Test the reciprocal
            Assert.IsTrue(timestampNow.Equals(Utils.TimeUtils.UnixTime2DateTime(Utils.TimeUtils.DateTime2UnixTime(timestampNow))));

            // Tests that the unix timestamps are off by 1
            Assert.IsTrue(Utils.TimeUtils.DateTime2UnixTime(timestampNow).Add(new TimeSpan(0, 0, 1)) == Utils.TimeUtils.DateTime2UnixTime(timestampNowPlusOneSecond));
            Assert.IsTrue(Utils.TimeUtils.DateTime2UnixTime(timestampNow).Subtract(new TimeSpan(0, 0, 1)) == Utils.TimeUtils.DateTime2UnixTime(timestampNowMinusOneSecond));
        }
    }
}
