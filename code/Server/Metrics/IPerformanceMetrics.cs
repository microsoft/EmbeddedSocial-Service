// <copyright file="IPerformanceMetrics.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Metrics
{
    using System;
    using System.Net;

    /// <summary>
    /// Interface for logger for performance related metrics
    /// </summary>
    public interface IPerformanceMetrics
    {
        /// <summary>
        /// Logs the end-to-end latency of api calls. The Http method, Uri, Guid, and Http response status are dimensions for this metric
        /// </summary>
        /// <param name="method">request method</param>
        /// <param name="uri">uri</param>
        /// <param name="guid">guid</param>
        /// <param name="status">response status</param>
        /// <param name="ms">elapsed time in ms</param>
        void ApiLatency(string method, string uri, Guid guid, HttpStatusCode status, long ms);
    }
}
