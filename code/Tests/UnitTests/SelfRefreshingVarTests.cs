// <copyright file="SelfRefreshingVarTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Utils;

    /// <summary>
    /// Unit tests for self refreshing variable
    /// </summary>
    [TestClass]
    public class SelfRefreshingVarTests
    {
        /// <summary>
        /// Create an action to fail test with an assertion
        /// </summary>
        private readonly Action<string, Exception> failTest =
            (string msg, Exception ex) => Assert.Fail(msg);

        /// <summary>
        /// Lock used for atomic increments on the counter
        /// </summary>
        private readonly object counterLock = new object();

        /// <summary>
        /// Counter used by the test below
        /// </summary>
        private int counter = 0;

        /// <summary>
        /// A self-refreshing int that increments once every three seconds.
        /// Check that its value is 3 after 10 seconds
        /// </summary>
        [TestMethod]
        public void IncrementSRVTest()
        {
            this.counter = 0;
            SelfRefreshingVar<int> srv = new SelfRefreshingVar<int>(0, TimeSpan.FromSeconds(3), this.AtomicIncrement, this.failTest);

            // sleep for 10 seconds, and check if value is 3
            Thread.Sleep(10 * 1000);
            Assert.AreEqual(3, srv.GetValue());
        }

        /// <summary>
        /// Check that dispose makes the variable not refresh any longer.
        /// Create a self-refreshing int that increments once every two seconds,
        /// sleep for three seconds, dispose it, and sleep for another two seconds.
        /// Check that its value is 1.
        /// </summary>
        [TestMethod]
        public void DisposeSRVTest()
        {
            this.counter = 0;
            SelfRefreshingVar<int> srv = new SelfRefreshingVar<int>(0, TimeSpan.FromSeconds(2), this.AtomicIncrement, this.failTest);

            // sleep for 3 seconds
            Thread.Sleep(3 * 1000);

            // Dispose
            srv.Dispose();

            // sleep for another 2 seconds
            Thread.Sleep(2 * 1000);
            Assert.AreEqual(1, srv.GetValue());
        }

        /// <summary>
        /// Create an un-initialized self-refreshing int that increments once every ten seconds.
        /// Read it right-away, its value should be 0.
        /// Initialize it, and check its value to be 1.
        /// </summary>
        /// <returns>Task</returns>
        [TestMethod]
        public async Task UnInitSRVTest()
        {
            this.counter = 0;
            SelfRefreshingVar<int> srv = new SelfRefreshingVar<int>(TimeSpan.FromSeconds(10), this.AtomicIncrement, this.failTest);

            Assert.AreEqual(0, srv.GetValue());
            await srv.Initialize();
            Assert.AreEqual(1, srv.GetValue());
        }

        /// <summary>
        /// Private function that increments a number atomically
        /// </summary>
        /// <returns>incremented number</returns>
        private Task<int> AtomicIncrement()
        {
            lock (this.counterLock)
            {
                this.counter += 1;
                return Task.FromResult(this.counter);
            }
        }
    }
}
