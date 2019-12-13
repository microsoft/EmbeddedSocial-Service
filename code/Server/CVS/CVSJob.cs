// <copyright file="CVSJob.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.CVS
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// CVS job
    /// </summary>
    public class CVSJob : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CVSJob"/> class
        /// </summary>
        /// <param name="serviceUri">CVS service uri</param>
        /// <param name="subscriptionKey">Subscription key</param>
        public CVSJob(Uri serviceUri, string subscriptionKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Query CVS service to get a previously submitted job
        /// </summary>
        /// <param name="jobId">job id</param>
        /// <returns>job</returns>
        public async Task<object> QueryAsyncJob(string jobId)
        {
            // avoid await warning
            await Task.Delay(0);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disposes <see cref="CVSJob"/>
        /// </summary>
        public void Dispose()
        {
        }
    }
}
