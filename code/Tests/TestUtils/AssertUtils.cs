// <copyright file="AssertUtils.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.TestUtils
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Utility methods for assertions about exceptions in unit tests
    /// </summary>
    public class AssertUtils
    {
        /// <summary>
        /// Utility method to assert that the given async method throws the specified exception
        /// </summary>
        /// <typeparam name="TException">Exception type the method should throw</typeparam>
        /// <param name="action">Async method to be tested</param>
        /// <returns>Task representing the test</returns>
        public static async Task AssertThrowsExceptionAsync<TException>(Func<Task> action)
            where TException : Exception
        {
            try
            {
                await action();
                Assert.Fail("No exception was thrown. Expected " + typeof(TException).Name);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.GetType() == typeof(TException));
            }
        }

        /// <summary>
        /// Utility method to assert that the given synchronous method throws the specified exception
        /// </summary>
        /// <typeparam name="TException">Exception type the method should throw</typeparam>
        /// <param name="action">Synchronous method to test. Works on constructors/void methods.</param>
        public static void AssertThrowsException<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
                Assert.Fail("No exception was thrown. Expected " + typeof(TException).Name);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.GetType() == typeof(TException));
            }
        }
    }
}
