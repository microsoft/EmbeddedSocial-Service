// <copyright file="ExpirationsWorker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Workers
{
    using System.Threading.Tasks;

    using SocialPlus.Server.Managers;

    /// <summary>
    /// Expirations worker class
    /// </summary>
    public class ExpirationsWorker : IWorker
    {
        /// <summary>
        /// Sleep time in milliseconds between expirations cleaning job
        /// </summary>
        private const int SleepTimeInMs = 1 * 60 * 1000;

        /// <summary>
        /// Boolean indicating whether the run loop should continue
        /// </summary>
        private bool keepRunning;

        /// <summary>
        /// Apps manager
        /// </summary>
        private IPopularTopicsManager popularTopicsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpirationsWorker"/> class
        /// </summary>
        /// <param name="popularTopicsManager">Popular topics manager</param>
        public ExpirationsWorker(IPopularTopicsManager popularTopicsManager)
        {
            this.popularTopicsManager = popularTopicsManager;
        }

        /// <summary>
        /// Main run loop
        /// </summary>
        public async void Run()
        {
            this.keepRunning = true;
            while (this.keepRunning)
            {
                await this.popularTopicsManager.ExpireTopics();
                await Task.Delay(SleepTimeInMs);
            }
        }

        /// <summary>
        /// Stop method
        /// </summary>
        public void Stop()
        {
            this.keepRunning = false;
        }
    }
}
