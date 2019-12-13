// <copyright file="ConcurrentCalls.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.TestUtils
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Utilities for making calls concurrently. These utilities are now used to test the throttle limit,
    /// but they could be extended for other uses
    /// </summary>
    /// <typeparam name="T">Return type for calls</typeparam>
    public static class ConcurrentCalls<T>
    {
        /// <summary>
        /// Fires one async method in parallel <pre>n</pre> times
        /// </summary>
        /// <param name="asyncMethod">Async method</param>
        /// <param name="n">Number of times to fire in parallel</param>
        /// <returns>A list of results corresponding to each call</returns>
        public static async Task<T[]> FireInParallel(Func<Task<T>> asyncMethod, int n)
        {
            var methodArray = new Func<Task<T>>[n];
            var taskArray = new Task<T>[n];
            var resultArray = new T[n];

            for (int i = 0; i < n; i++)
            {
                // Each local assignment creates a new instance of the asyncMethod func on the heap
                methodArray[i] = asyncMethod;
            }

            // Fire n times without an 'await'
            for (int i = 0; i < n; i++)
            {
                taskArray[i] = methodArray[i].Invoke();
            }

            // wait for all to complete
            resultArray = await Task.WhenAll(taskArray);
            return resultArray;
        }

        /// <summary>
        /// Fires an array of async methods in parallel
        /// </summary>
        /// <param name="asyncMethodArray">Async method array</param>
        /// <returns>A list of results corresponding to each call</returns>
        public static async Task<T[]> FireInParallel(Func<Task<T>>[] asyncMethodArray)
        {
            int n = asyncMethodArray.Length;
            var taskArray = new Task<T>[n];
            var resultArray = new T[n];

            // Fire methods without an 'await'
            for (int i = 0; i < n; i++)
            {
                taskArray[i] = asyncMethodArray[i].Invoke();
            }

            // wait for all to complete
            resultArray = await Task.WhenAll(taskArray);
            return resultArray;
        }
    }
}
