// <copyright file="RateLimitingTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Client;
    using SocialPlus.Client.Models;
    using SocialPlus.TestUtils;

    /// <summary>
    /// Tests that rate limiting works.
    /// </summary>
    [TestClass]
    public class RateLimitingTests
    {
        /// <summary>
        /// Estimates the per-minute rate limit. Starts by firing 2K GetBuilds requests and then 1K at a time for a maximum of 10K requests.
        /// Expects to receive an HTTP 429 error code eventually (during any of the 1K batches).
        /// </summary>
        /// <returns>Task</returns>
        [TestMethod]
        public async Task EstimatePerMinuteRateLimitTest()
        {
            int estimate = 0;

            // Set up initial login etc
            SocialPlusClient client = new SocialPlusClient(TestConstants.ServerApiBaseUrl);

            // Start a clock
            var watch = Stopwatch.StartNew();

            // Fire 2000 calls in parallel
            Func<Task<GetBuildInfoResponse>> getBuildInfoFunc = () => client.Config.GetBuildInfoAsync();
            await ConcurrentCalls<GetBuildInfoResponse>.FireInParallel(getBuildInfoFunc, 2000);
            estimate += 2000;

            // Continue to fire calls in parallel, 1K at a time, up to 8K additional calls, or 1 minute whatever takes longer
            try
            {
                // up to 8 more parallel batches
                for (int i = 0; i < 8; i += 1)
                {
                    watch.Stop();
                    if (watch.ElapsedMilliseconds > 1000 * 60)
                    {
                        Console.WriteLine("The rate limit estimate is unavailable but is higher than " + estimate + " reqs/minute");
                        return;
                    }

                    watch.Start();
                    await ConcurrentCalls<GetBuildInfoResponse>.FireInParallel(getBuildInfoFunc, 1000);
                    estimate += 1000;
                }
            }
            catch (Exception e)
            {
                // Check that this is a 429 error, by scanning the exception's message for the string '429'
                Assert.IsTrue(e.Message.Contains("429"));
                Console.WriteLine("The rate limit estimate is " + estimate + " reqs/minute");
                return;
            }

            Console.WriteLine("The rate limit estimate is unavailable but is higher than " + estimate + " reqs/minute");
        }
    }
}
